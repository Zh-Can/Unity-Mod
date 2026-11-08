using System;
using System.Collections.Generic;
using MelonLoader;
using Il2Cpp;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(BookOwnMark.Plugin), "BookOwnMark", "5.1", "Can")]
[assembly: MelonGame("TppStudio", "LongYinLiZhiZhuan")]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]

namespace BookOwnMark
{
    /// <summary>
    /// v5.0 适配新版虚拟列表（ItemListController 使用 currentDataList + 图标池）
    /// 核心变更：
    ///   1. 筛选不再靠隐藏 GameObject（会造成虚空格 + 滚动后被回收重绑而失效），
    ///      改为在 RefreshItemList 之后直接裁剪 currentDataList（显示数据快照），
    ///      再调用 CalculateLayout + UpdateVisibleCells(true) 重建布局与可视单元。
    ///   2. 已拥有标记不再改写 itemData.name（虚拟化下只能扫到可视图标），
    ///      改为在 ItemIconController.Update 之后直接改写图标上的名字文本。
    /// v5.1 新增拍卖会标记：
    ///   拍卖会的「展品列表」走 ChooseController 的物品选择面板（ChooseType.HeroItem），
    ///   不经过 TradeUIController，所以单独记一个面板开关；拍卖大会竞价界面的拍品
    ///   也复用 ItemIconController，只要 auctionPanel 可见就一并标记。标记逻辑与交易界面完全一致。
    /// </summary>
    public class Plugin : MelonMod
    {
        public static Plugin Instance = null!;
        public static readonly MelonLogger.Instance LOG = Melon<Plugin>.Logger;

        private const string OWNED_MARK = " <color=#33cc86>☑</color>";
        private const string SPE_BOOK_MARK = " <color=#ff3333>☑</color>";
        private const string CHECK_CHAR = "☑";

        private static readonly HashSet<string> _ownedBookNames = [];
        private static readonly HashSet<string> _speBookNames = [];

        // -1 = 全部
        private static int _skillTypeFilter = -1;
        private static int _rareLvFilter = -1;

        private static readonly string[] SkillTypeNames =
        [
            "全部", "内功", "轻功", "绝技", "拳掌", "剑法", "刀法", "长兵", "奇门", "射术"
        ];

        private static readonly string[] RareLvNames =
        [
            "全部", "白", "绿", "蓝", "紫", "金", "红"
        ];

        private static Dropdown _skillTypeDropdown = null!;
        private static Dropdown _rareLvDropdown = null!;
        private static bool _uiBuilt;
        private static bool _resetting;
        private static bool _warnedNoTemplate;

        private static ItemListController _storageList = null!;   // 仓库界面右侧（玩家仓库）列表
        private static TradeUIController _currentTradeUI = null!;
        // 单独记录类型，避免在 Update 高频回调里访问可能已销毁的 _currentTradeUI
        private static TradeUIType _markTradeType = TradeUIType.None;

        // 拍卖会「展品列表」面板是否打开（由 PlotController.ShowAuctionItem 打开，
        // 走的是 ChooseController 的物品选择面板，不会触发 ShowTradeUI）
        private static bool _auctionItemPanelOpen;

        public override void OnInitializeMelon()
        {
            Instance = this;
            var harmony = new HarmonyLib.Harmony("BookOwnMark");
            harmony.PatchAll(typeof(Plugin));
            LOG.Msg("[BookOwnMark] v5.1 已加载（适配虚拟列表 + 拍卖会标记）");
        }

        public override void OnUpdate()
        {
            // 调试用：输出当前筛选状态
            if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                LOG.Msg($"[BookOwnMark] 筛选: skill={_skillTypeFilter}, rare={_rareLvFilter}, " +
                        $"storageList={(_storageList != null)}, 已拥有={_ownedBookNames.Count}, 特殊={_speBookNames.Count}");
                if (_storageList != null)
                {
                    var d = _storageList.currentDataList;
                    LOG.Msg($"[BookOwnMark] 显示条目={d?.Count ?? 0}, 类型={_storageList.nowItemListType}");
                }
            }
        }

        #region 已拥有秘籍扫描

        private static void ScanOwnedBooks()
        {
            _ownedBookNames.Clear();
            _speBookNames.Clear();

            var gc = GameController.Instance;
            var player = gc?.worldData?.Player();
            if (player == null) return;

            CollectBooks(player.selfStorage?.allItem, _ownedBookNames);

            if (player.belongForceID != -1)
            {
                var forceData = gc!.worldData.GetHeroForce(0);
                CollectBooks(forceData?.bookStorage?.allItem, _ownedBookNames);
            }

            CollectBooks(gc!.worldData?.speBookStorage?.allItem, _speBookNames);
        }

        private static void CollectBooks(Il2CppSystem.Collections.Generic.List<ItemData> list, HashSet<string> target)
        {
            if (list == null) return;
            var count = list.Count;
            for (var i = 0; i < count; i++)
            {
                var item = list[i];
                if (item == null || item.type != ItemType.Book) continue;
                var n = item.Name();
                if (!string.IsNullOrEmpty(n)) target.Add(n);
            }
        }

        #endregion

        #region 筛选：裁剪 currentDataList

        // 所有 RefreshItemList 重载最终都会走 (ItemListData, ItemListInteractType, bool)
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemListController), nameof(ItemListController.RefreshItemList),
            [typeof(ItemListData), typeof(ItemListInteractType), typeof(bool)])]
        public static void RefreshItemList_Postfix(ItemListController __instance)
        {
            if (__instance == null || __instance != _storageList) return;

            try
            {
                // 每次刷新都同步一次下拉，覆盖"打开仓库时本来就停在秘籍分类"的情况
                SyncFilterUI(__instance);

                if (__instance.nowItemListType != ItemListType.BookType) return;
                if (_skillTypeFilter < 0 && _rareLvFilter < 0) return;

                ApplyFilter(__instance);
            }
            catch (System.Exception ex)
            {
                LOG.Warning("[BookOwnMark] 应用筛选失败: " + ex.Message);
            }
        }

        private static void ApplyFilter(ItemListController controller)
        {
            var data = controller.currentDataList;
            if (data == null) return;

            var total = data.Count;
            if (total <= 0) return;

            var keep = new List<ItemData>(total);
            for (var i = 0; i < total; i++)
            {
                var item = data[i];
                if (item == null) continue;
                if (Matches(item)) keep.Add(item);
            }

            if (keep.Count == total) return;

            data.Clear();
            for (var i = 0; i < keep.Count; i++) data.Add(keep[i]);

            // 内容高度依赖 currentDataList.Count，必须重算；随后强制重绑可视单元
            controller.CalculateLayout();
            controller.UpdateVisibleCells(true);
        }

        private static bool Matches(ItemData item)
        {
            if (item.type != ItemType.Book) return true; // 秘籍分类里理论上只有书，兜底保留

            var bookData = item.bookData;
            if (bookData == null) return true;

            var db = bookData.DataBase();
            if (db == null) return true;

            if (_skillTypeFilter >= 0 && db.type != _skillTypeFilter) return false;
            if (_rareLvFilter >= 0 && db.rareLv != _rareLvFilter) return false;
            return true;
        }

        #endregion

        #region 筛选 UI

        private static void CreateFilterUI()
        {
            if (_uiBuilt) return;
            if (_storageList == null) return;

            var template = _storageList.sortTypeDropDown;
            if (template == null)
            {
                if (!_warnedNoTemplate)
                {
                    _warnedNoTemplate = true;
                    LOG.Warning("[BookOwnMark] 仓库列表没有 sortTypeDropDown，筛选下拉暂时建不出来");
                }
                return;
            }

            try
            {
                var parent = template.transform.parent;
                var templateRect = template.GetComponent<RectTransform>();

                _skillTypeDropdown = CreateDropdownFromTemplate(template, "SkillTypeDropdown", SkillTypeNames);
                _skillTypeDropdown.transform.SetParent(parent, false);
                SetupRect(_skillTypeDropdown, templateRect, 90f);
                _skillTypeDropdown.onValueChanged.AddListener((Action<int>)(index =>
                {
                    _skillTypeFilter = index - 1;
                    RefreshCurrentList();
                }));
                SetDropdownScrollSpeed(_skillTypeDropdown, 20f);

                _rareLvDropdown = CreateDropdownFromTemplate(template, "RareLvDropdown", RareLvNames);
                _rareLvDropdown.transform.SetParent(parent, false);
                SetupRect(_rareLvDropdown, templateRect, 180f);
                _rareLvDropdown.onValueChanged.AddListener((Action<int>)(index =>
                {
                    _rareLvFilter = index - 1;
                    RefreshCurrentList();
                }));
                SetDropdownScrollSpeed(_rareLvDropdown, 20f);

                _uiBuilt = true;
                LOG.Msg("[BookOwnMark] 筛选下拉已创建");
            }
            catch (System.Exception ex)
            {
                LOG.Warning("[BookOwnMark] 创建筛选下拉失败: " + ex.Message);
                ResetFilterUI();
            }
        }

        private static void SetupRect(Dropdown dropdown, RectTransform templateRect, float offsetX)
        {
            var rect = dropdown.GetComponent<RectTransform>();
            rect.anchorMin = templateRect.anchorMin;
            rect.anchorMax = templateRect.anchorMax;
            rect.pivot = templateRect.pivot;
            rect.sizeDelta = templateRect.sizeDelta;
            rect.anchoredPosition = templateRect.anchoredPosition + new Vector2(offsetX, 0);
            dropdown.value = 0;
        }

        private static Dropdown CreateDropdownFromTemplate(Dropdown template, string name, string[] options)
        {
            var newDropdown = UnityEngine.Object.Instantiate(template.gameObject).GetComponent<Dropdown>();
            newDropdown.name = name;
            newDropdown.ClearOptions();

            var optionDataList = new Il2CppSystem.Collections.Generic.List<Dropdown.OptionData>();
            foreach (var option in options)
            {
                optionDataList.Add(new Dropdown.OptionData(option));
            }
            newDropdown.AddOptions(optionDataList);
            newDropdown.value = 0;
            return newDropdown;
        }

        private static void SetDropdownScrollSpeed(Dropdown dropdown, float speed)
        {
            if (dropdown == null || dropdown.template == null) return;
            var scrollRect = dropdown.template.GetComponent<ScrollRect>();
            if (scrollRect != null) scrollRect.scrollSensitivity = speed;
        }

        /// <summary>下拉对象是否还活着：交易界面可能被销毁重建，旧引用会失效</summary>
        private static bool FilterUIAlive()
        {
            try
            {
                return _skillTypeDropdown != null && _skillTypeDropdown.gameObject != null
                       && _rareLvDropdown != null && _rareLvDropdown.gameObject != null;
            }
            catch
            {
                return false;
            }
        }

        private static void ResetFilterUI()
        {
            _uiBuilt = false;
            _skillTypeDropdown = null!;
            _rareLvDropdown = null!;
        }

        private static void ShowFilterUI()
        {
            try
            {
                // 界面被销毁重建过 → 旧引用失效，直接重建
                if (!FilterUIAlive()) ResetFilterUI();
                if (!_uiBuilt) CreateFilterUI();
                if (!_uiBuilt) return;

                if (_skillTypeDropdown != null) _skillTypeDropdown.gameObject.SetActive(true);
                if (_rareLvDropdown != null) _rareLvDropdown.gameObject.SetActive(true);
            }
            catch (System.Exception ex)
            {
                LOG.Warning("[BookOwnMark] 显示筛选下拉失败: " + ex.Message);
                ResetFilterUI();
            }
        }

        private static void HideFilterUI()
        {
            try
            {
                if (_skillTypeDropdown != null && _skillTypeDropdown.gameObject != null)
                    _skillTypeDropdown.gameObject.SetActive(false);
                if (_rareLvDropdown != null && _rareLvDropdown.gameObject != null)
                    _rareLvDropdown.gameObject.SetActive(false);
            }
            catch
            {
                // 界面已经销毁：清掉引用，下次重新创建
                ResetFilterUI();
            }
        }

        private static void ResetFilterState()
        {
            _resetting = true;
            try
            {
                _skillTypeFilter = -1;
                _rareLvFilter = -1;
                if (_skillTypeDropdown != null && _skillTypeDropdown.gameObject != null) _skillTypeDropdown.value = 0;
                if (_rareLvDropdown != null && _rareLvDropdown.gameObject != null) _rareLvDropdown.value = 0;
            }
            catch
            {
                ResetFilterUI();
            }
            finally
            {
                _resetting = false;
            }
        }

        /// <summary>
        /// 按当前分类同步筛选下拉：秘籍分类显示、其它分类隐藏。
        /// 打开界面 / 列表刷新 / 点击分类 三个入口都走这里，避免只依赖 ChangeListType 的时机。
        /// </summary>
        private static void SyncFilterUI(ItemListController list)
        {
            if (list == null || list != _storageList) return;

            try
            {
                if (list.nowItemListType == ItemListType.BookType)
                {
                    ShowFilterUI();
                }
                else
                {
                    HideFilterUI();
                    ResetFilterState();
                }
            }
            catch (System.Exception ex)
            {
                LOG.Warning("[BookOwnMark] 同步筛选下拉失败: " + ex.Message);
            }
        }

        /// <summary>重新走一遍游戏的刷新流程，postfix 会自动套用筛选</summary>
        private static void RefreshCurrentList()
        {
            if (_resetting) return;
            if (_storageList == null) return;
            _storageList.RefreshItemList(true);
        }

        #endregion

        #region 交易界面挂钩

        private static void HandleShowTradeUI(TradeUIController instance, TradeUIType type)
        {
            if (instance == null) return;

            _currentTradeUI = instance;
            _markTradeType = type;
            ScanOwnedBooks();

            if (type != TradeUIType.Storage)
            {
                _storageList = null!;
                HideFilterUI();
                return;
            }

            _storageList = instance.rightList;

            try
            {
                LOG.Msg($"[BookOwnMark] 打开仓库: 左列表分类={instance.leftList?.nowItemListType} " +
                        $"右列表分类={instance.rightList?.nowItemListType}");
            }
            catch
            {
                // 诊断日志失败不影响功能
            }

            HideFilterUI();
            ResetFilterState();

            // 如果界面一打开就停在「秘籍」分类，此时没有 ChangeListType 事件，这里补一次
            SyncFilterUI(_storageList);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TradeUIController), nameof(TradeUIController.ShowTradeUI),
            [typeof(TradeUIType), typeof(ItemListData), typeof(ItemListData), typeof(bool)])]
        public static void ShowTradeUI_Postfix1(TradeUIController __instance, TradeUIType targetType)
            => HandleShowTradeUI(__instance, targetType);

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TradeUIController), nameof(TradeUIController.ShowTradeUI),
            [typeof(TradeUIType), typeof(ItemListType), typeof(ItemListData), typeof(ItemListData)])]
        public static void ShowTradeUI_Postfix2(TradeUIController __instance, TradeUIType targetType)
            => HandleShowTradeUI(__instance, targetType);

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TradeUIController), nameof(TradeUIController.ShowTradeUI),
            [typeof(TradeUIType), typeof(ItemListData), typeof(ItemListData), typeof(int), typeof(int)])]
        public static void ShowTradeUI_Postfix3(TradeUIController __instance, TradeUIType targetType)
            => HandleShowTradeUI(__instance, targetType);

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TradeUIController), nameof(TradeUIController.ShowTradeUI),
            [typeof(TradeUIType), typeof(ItemListType), typeof(ItemListData), typeof(ItemListData),
             typeof(int), typeof(int), typeof(bool), typeof(bool), typeof(float), typeof(float)])]
        public static void ShowTradeUI_Postfix4(TradeUIController __instance, TradeUIType targetType)
            => HandleShowTradeUI(__instance, targetType);

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TradeUIController), nameof(TradeUIController.HideTradeUI))]
        public static void HideTradeUI_Prefix()
        {
            ClearState();
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            ClearState();
        }

        private static void ClearState()
        {
            _currentTradeUI = null!;
            _storageList = null!;
            _markTradeType = TradeUIType.None;
            _auctionItemPanelOpen = false;
            ResetFilterState();
            HideFilterUI();
        }

        // 只有玩家自己的仓库、且切到「秘籍」分类时才显示筛选下拉
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemListController), nameof(ItemListController.ChangeListType))]
        public static void ChangeListType_Postfix(ItemListController __instance)
        {
            if (__instance == null || __instance != _storageList) return;

            SyncFilterUI(__instance);
        }

        #endregion

        #region 拍卖会挂钩

        // 拍卖会「展品列表」：PlotController.ShowAuctionItem -> ChooseController.ShowChoosePanel(HeroItem, {-100})
        // 这条路不经过 TradeUIController，所以单独记一个开关，标记逻辑照旧复用。
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlotController), nameof(PlotController.ShowAuctionItem))]
        public static void ShowAuctionItem_Postfix()
        {
            _auctionItemPanelOpen = true;
            ScanOwnedBooks();

            try
            {
                LOG.Msg($"[BookOwnMark] 查看拍卖会展品: 已拥有 {_ownedBookNames.Count} 本, 特殊 {_speBookNames.Count} 本");
            }
            catch
            {
                // 日志失败不影响功能
            }
        }

        // 选择面板关闭：拍卖展品面板和其它选择面板共用，统一清掉开关
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ChooseController), nameof(ChooseController.HideChoosePanel))]
        public static void HideChoosePanel_Prefix()
        {
            _auctionItemPanelOpen = false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ChooseController), nameof(ChooseController.UnshowChoosePanel))]
        public static void UnshowChoosePanel_Prefix()
        {
            _auctionItemPanelOpen = false;
        }

        // 拍卖大会开始时重扫一次：展品里可能有刚入手的秘籍
        [HarmonyPostfix]
        [HarmonyPatch(typeof(AuctionController), nameof(AuctionController.RestartAuction))]
        public static void RestartAuction_Postfix()
        {
            ScanOwnedBooks();
        }

        /// <summary>是否需要给物品图标补标记：交易界面 或 拍卖会界面</summary>
        private static bool ShouldMark(ItemIconController icon)
        {
            // 交易界面（原有逻辑）
            if (_markTradeType != TradeUIType.None && _markTradeType != TradeUIType.Storage) return true;

            try
            {
                // 拍卖会「展品列表」：ChooseController 的物品选择面板
                if (_auctionItemPanelOpen)
                {
                    var choose = ChooseController.Instance;
                    var inChoosePanel = choose != null && ChoosePanelVisible(choose);
                    // 面板根节点字段万一没接上，退回判断图标是否挂在 ChooseController 下面
                    if (inChoosePanel || (choose != null && icon.GetComponentInParent<ChooseController>() != null))
                        return true;

                    // 面板可能被别的路径直接关掉了，没走到 Hide/Unshow，这里兜底复位
                    _auctionItemPanelOpen = false;
                }

                // 拍卖大会竞价界面：拍品同样用 ItemIconController 显示
                var auction = AuctionController.Instance;
                return auction != null && auction.auctionPanel != null && auction.auctionPanel.activeInHierarchy;
            }
            catch
            {
                return false;
            }
        }

        private static bool ChoosePanelVisible(ChooseController choose)
        {
            if (choose == null) return false;
            if (choose.choosePanel != null && choose.choosePanel.activeInHierarchy) return true;
            return choose.chooseRoot != null && choose.chooseRoot.activeInHierarchy;
        }

        #endregion

        #region 已有秘籍标记

        // 图标每次绑定后（inited 被重置）由 Update 重新写名字文本，这里在之后补上标记，
        // 不修改 itemData.name，因此不会污染物品数据，也不受虚拟化影响。
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemIconController), nameof(ItemIconController.Update))]
        public static void ItemIcon_Update_Postfix(ItemIconController __instance)
        {
            if (_ownedBookNames.Count == 0 && _speBookNames.Count == 0) return;
            if (!ShouldMark(__instance)) return;

            var item = __instance.itemData;
            if (item == null || item.type != ItemType.Book) return;

            var nameText = __instance.nameText;
            if (nameText == null) return;

            var text = nameText.text;
            if (string.IsNullOrEmpty(text) || text.Contains(CHECK_CHAR)) return;

            var bookName = item.Name();
            if (string.IsNullOrEmpty(bookName)) return;

            if (_speBookNames.Contains(bookName))
                nameText.text = text + SPE_BOOK_MARK;
            else if (_ownedBookNames.Contains(bookName))
                nameText.text = text + OWNED_MARK;
        }

        #endregion
    }
}
