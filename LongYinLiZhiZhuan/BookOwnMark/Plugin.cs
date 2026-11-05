using System;
using System.Collections.Generic;
using MelonLoader;
using Il2Cpp;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(BookOwnMark.Plugin), "BookOwnMark", "5.0", "Can")]
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

        private static ItemListController _storageList = null!;   // 仓库界面右侧（玩家仓库）列表
        private static TradeUIController _currentTradeUI = null!;
        // 单独记录类型，避免在 Update 高频回调里访问可能已销毁的 _currentTradeUI
        private static TradeUIType _markTradeType = TradeUIType.None;

        public override void OnInitializeMelon()
        {
            Instance = this;
            var harmony = new HarmonyLib.Harmony("BookOwnMark");
            harmony.PatchAll(typeof(Plugin));
            LOG.Msg("[BookOwnMark] v5.0 已加载（适配虚拟列表）");
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
            if (__instance.nowItemListType != ItemListType.BookType) return;
            if (_skillTypeFilter < 0 && _rareLvFilter < 0) return;

            ApplyFilter(__instance);
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
            if (template == null) return;

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

        private static void ShowFilterUI()
        {
            if (!_uiBuilt) CreateFilterUI();
            if (!_uiBuilt) return;
            if (_skillTypeDropdown != null) _skillTypeDropdown.gameObject.SetActive(true);
            if (_rareLvDropdown != null) _rareLvDropdown.gameObject.SetActive(true);
        }

        private static void HideFilterUI()
        {
            if (_skillTypeDropdown != null && _skillTypeDropdown.gameObject != null)
                _skillTypeDropdown.gameObject.SetActive(false);
            if (_rareLvDropdown != null && _rareLvDropdown.gameObject != null)
                _rareLvDropdown.gameObject.SetActive(false);
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
            finally
            {
                _resetting = false;
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
            HideFilterUI();
            ResetFilterState();
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
            ResetFilterState();
            HideFilterUI();
        }

        // 只有玩家自己的仓库、且切到「秘籍」分类时才显示筛选下拉
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemListController), nameof(ItemListController.ChangeListType))]
        public static void ChangeListType_Postfix(ItemListController __instance)
        {
            if (__instance == null || __instance != _storageList) return;

            if (__instance.nowItemListType == ItemListType.BookType)
            {
                CreateFilterUI();
                ShowFilterUI();
            }
            else
            {
                HideFilterUI();
                ResetFilterState();
            }
        }

        #endregion

        #region 已有秘籍标记

        // 图标每次绑定后（inited 被重置）由 Update 重新写名字文本，这里在之后补上标记，
        // 不修改 itemData.name，因此不会污染物品数据，也不受虚拟化影响。
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemIconController), nameof(ItemIconController.Update))]
        public static void ItemIcon_Update_Postfix(ItemIconController __instance)
        {
            if (_markTradeType == TradeUIType.None || _markTradeType == TradeUIType.Storage) return;
            if (_ownedBookNames.Count == 0 && _speBookNames.Count == 0) return;

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
