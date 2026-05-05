using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MelonLoader;
using Il2Cpp;
using HarmonyLib;
using Il2CppConsolation;
using UnityEngine;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(BookOwnMark.Plugin), "BookOwnMark", "4.0", "Can")]
[assembly: MelonGame("TppStudio", "LongYinLiZhiZhuan")]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]

namespace BookOwnMark
{
    public class Plugin : MelonMod
    {
        public static Plugin Instance = null!;
        public static readonly MelonLogger.Instance LOG = Melon<Plugin>.Logger;

        private static HashSet<string> _ownedBookNames = [];
        private static HashSet<string> _speBookNames = [];
        private const string OWNED_MARK = " <color=#33cc86>☑</color>";
        private const string SPE_BOOK_MARK = " <color=#ff3333>☑</color>";
        private static bool _addedMark = false;

        private static int _currentSkillTypeFilter = -1;
        private static int _currentRareLvFilter = -1;
        private static ItemListController _currentItemListController = null;
        private static TradeUIController _currentTradeUI = null;

        private static readonly string[] SkillTypeNames =
        [
            "全部", "内功", "轻功", "绝技", "拳掌", "剑法", "刀法", "长兵", "奇门", "射术"
        ];

        private static readonly string[] RareLvNames =
        [
            "全部", "白", "绿", "蓝", "紫", "金", "红"
        ];

        private static GameObject _filterPanel = null;
        private static Dropdown _skillTypeDropdown = null;
        private static Dropdown _rareLvDropdown = null;
        private static ItemListData _originalItemList = null;
        private static Il2CppSystem.Collections.Generic.List<Il2CppSystem.Collections.Generic.List<ItemData>> _itemTypeList = null;
        private static Il2CppSystem.Collections.Generic.List<ItemData> _bookList = null;

        public override void OnInitializeMelon()
        {
            Instance = this;
            LOG.Msg("[BookOwnMark] Mod已加载");
            var harmony = new HarmonyLib.Harmony("BookOwnMark");
            harmony.PatchAll(typeof(Plugin));
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                var tuc = TradeUIController.Instance;
                var list = tuc.rightList.targetItemList.allItem;
                foreach (var t in list)
                {
                    if (t.name.EndsWith(OWNED_MARK))
                    {
                        t.name = t.name.Replace(OWNED_MARK, "");
                    }
                }

            }
        }
        // 设置 Dropdown 滚轮滚动速度
        private static void SetDropdownScrollSpeed(Dropdown dropdown, float speed)
        {
            if (dropdown == null || dropdown.template == null) return;

            ScrollRect scrollRect = dropdown.template.GetComponent<ScrollRect>();
            if (scrollRect != null)
            {
                scrollRect.scrollSensitivity = speed; 
            }
        }
        private static void CreateFilterUI()
        {
            if (_filterPanel != null) return;
            if (_currentItemListController == null)
            {
                // LOG.Msg("ItemListController is null");
                return;
            }
            
            // LOG.Msg("Creating filter UI...");

            var templateDropdown = _currentItemListController.sortTypeDropDown;
            if (templateDropdown == null)
            {
                // LOG.Msg("sortTypeDropDown not found");
                return;
            }
            
            // LOG.Msg("Found template dropdown");

            _skillTypeDropdown = CreateDropdownFromTemplate(templateDropdown, "SkillTypeDropdown", SkillTypeNames);
            _skillTypeDropdown.transform.SetParent(templateDropdown.transform.parent, false);
            var skillRect = _skillTypeDropdown.GetComponent<RectTransform>();
            skillRect.anchorMin = templateDropdown.GetComponent<RectTransform>().anchorMin;
            skillRect.anchorMax = templateDropdown.GetComponent<RectTransform>().anchorMax;
            skillRect.pivot = templateDropdown.GetComponent<RectTransform>().pivot;
            skillRect.sizeDelta = templateDropdown.GetComponent<RectTransform>().sizeDelta;
            skillRect.anchoredPosition = templateDropdown.GetComponent<RectTransform>().anchoredPosition + new Vector2(90, 0);
            _skillTypeDropdown.value = 0;
            _skillTypeDropdown.onValueChanged.AddListener((Action<int>)(index =>
            {
                _currentSkillTypeFilter = index - 1;
                RefreshCurrentList();
            }));
            SetDropdownScrollSpeed(_skillTypeDropdown, 20f);

            _rareLvDropdown = CreateDropdownFromTemplate(templateDropdown, "RareLvDropdown", RareLvNames);
            _rareLvDropdown.transform.SetParent(templateDropdown.transform.parent, false);
            var rareRect = _rareLvDropdown.GetComponent<RectTransform>();
            rareRect.anchorMin = templateDropdown.GetComponent<RectTransform>().anchorMin;
            rareRect.anchorMax = templateDropdown.GetComponent<RectTransform>().anchorMax;
            rareRect.pivot = templateDropdown.GetComponent<RectTransform>().pivot;
            rareRect.sizeDelta = templateDropdown.GetComponent<RectTransform>().sizeDelta;
            rareRect.anchoredPosition = templateDropdown.GetComponent<RectTransform>().anchoredPosition + new Vector2(180, 0);
            _rareLvDropdown.value = 0;
            _rareLvDropdown.onValueChanged.AddListener((Action<int>)(index =>
            {
                _currentRareLvFilter = index - 1;
                RefreshCurrentList();
            }));
            SetDropdownScrollSpeed(_rareLvDropdown, 20f);

            _filterPanel = new GameObject("BookFilterPanel");
            _filterPanel.transform.SetParent(templateDropdown.transform.parent, false);
            
            // LOG.Msg("Filter UI created successfully");
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

        private static void ShowFilterUI()
        {
            if (_filterPanel == null)
            {
                CreateFilterUI();
            }
            
            if (_skillTypeDropdown != null) _skillTypeDropdown.gameObject.SetActive(true);
            if (_rareLvDropdown != null) _rareLvDropdown.gameObject.SetActive(true);
            _currentSkillTypeFilter = -1;
            _currentRareLvFilter = -1;
            // LOG.Msg("Filter UI shown");
        }

        private static void HideFilterUI()
        {
            if (_skillTypeDropdown != null && _skillTypeDropdown.gameObject != null)
            {
                _skillTypeDropdown.gameObject.SetActive(false);
                _skillTypeDropdown.value = 0;
            }
            if (_rareLvDropdown != null && _rareLvDropdown.gameObject != null)
            {
                _rareLvDropdown.gameObject.SetActive(false);
                _rareLvDropdown.value = 0;
            }
            _currentSkillTypeFilter = -1;
            _currentRareLvFilter = -1;
            // LOG.Msg("Filter UI hidden and reset");
        }

        private static void RefreshCurrentList()
        {
            if (_currentItemListController == null) return;
            
            // 修复方案：不替换 targetItemList，而是通过控制 itemGrid 中子物体的显示/隐藏来实现筛选
            // 首先确保显示的是完整的秘籍列表
            if (_currentItemListController.targetItemList != _originalItemList)
            {
                _currentItemListController.targetItemList = _originalItemList;
                _currentItemListController.RefreshItemList(true);
            }
            
            // 获取所有物品图标
            var itemIcons = _currentItemListController.itemGrid?.GetComponentsInChildren<ItemIconController>(true);
            if (itemIcons == null || itemIcons.Length == 0) return;
            
            // 遍历所有图标，根据筛选条件显示/隐藏
            foreach (var icon in itemIcons)
            {
                if (icon?.itemData == null) continue;
                if (icon.itemData.type != ItemType.Book) continue;
                
                var db = icon.itemData.bookData.DataBase();
                var matchType = (_currentSkillTypeFilter == -1 || db.type == _currentSkillTypeFilter);
                var matchRare = (_currentRareLvFilter == -1 || db.rareLv == _currentRareLvFilter);
                
                // 显示或隐藏图标
                icon.gameObject.SetActive(matchType && matchRare);
            }
        }

        private static void ScanOwnedBooks()
        {
            _ownedBookNames.Clear();
            _speBookNames.Clear();

            var gc = GameController.Instance;
            var player = gc.worldData?.Player();
            if (player == null) return;

            var storageList = player.selfStorage?.allItem;
            if (storageList != null)
            {
                foreach (var item in storageList)
                {
                    if (item is { type: ItemType.Book })
                    {
                        // 去掉可能存在的标记后再添加
                        _ownedBookNames.Add(RemoveOwnedMark(item.Name()));
                    }
                }
            }

            if (player.belongForceID != -1)
            {
                var forceData = gc.worldData.GetHeroForce(0);
                var bookStorage = forceData.bookStorage?.allItem;
                if (bookStorage != null)
                {
                    foreach (var item in bookStorage)
                    {
                        if (item is { type: ItemType.Book })
                        {
                            // 去掉可能存在的标记后再添加
                            _ownedBookNames.Add(RemoveOwnedMark(item.Name()));
                        }
                    }
                }
            }

            // 扫描特殊书籍仓库
            var speBookStorage = gc.worldData?.speBookStorage?.allItem;
            if (speBookStorage != null)
            {
                foreach (var item in speBookStorage)
                {
                    if (item is { type: ItemType.Book })
                    {
                        // 去掉可能存在的标记后再添加
                        _speBookNames.Add(RemoveOwnedMark(item.Name()));
                    }
                }
            }

            // LOG.Msg($"[BookOwnMark] 扫描到 {_ownedBookNames.Count} 本秘籍, {_speBookNames.Count} 本特殊秘籍");
        }

        private static bool IsBookOwned(string itemName)
        {
            return _ownedBookNames.Contains(itemName);
        }

        private static bool IsSpeBook(string itemName)
        {
            return _speBookNames.Contains(itemName);
        }

        private static string RemoveOwnedMark(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            return name.Replace(OWNED_MARK, "").Replace(SPE_BOOK_MARK, "");
        }

        private static IEnumerator MarkOwnedBooksOnTradeUICoroutine(TradeUIController tradeUI)
        {
            if (tradeUI == null) yield break;
            // 只在非仓库界面标记
            if (tradeUI.tradeUIType == TradeUIType.Storage)
            {
                // LOG.Msg("Skipping mark for Storage UI");
                // LOG.Msg(tradeUI.tradeUIType);
                yield break;
            }

            var merchantList = tradeUI.rightList;
            if (merchantList == null || merchantList.itemGrid == null)
            {
                // LOG.Msg("rightList or itemGrid is null");
                yield break;
            }

            var maxWait = 50;
            var waited = 0;

            while (waited < maxWait)
            {
                var icons = merchantList.itemGrid.GetComponentsInChildren<ItemIconController>(true);
                if (icons != null && icons.Length > 0)
                {
                    break;
                }
                waited++;
                yield return null;
            }

            // LOG.Msg($"Marking books on trade UI, waited: {waited}");

            var finalIcons = merchantList.itemGrid.GetComponentsInChildren<ItemIconController>(true);
            // LOG.Msg($"  icons count: {finalIcons?.Length ?? 0}");

            if (finalIcons == null || finalIcons.Length == 0) yield break;

            foreach (var icon in finalIcons)
            {
                if (icon?.itemData == null) continue;

                var itemType = icon.itemData.type;
                if (itemType != ItemType.Book) continue;
                var bookName = icon.itemData.Name();
                var newName = bookName;
                var hasMark = false;

                // 检查普通仓库（绿色标记）
                if (IsBookOwned(bookName) && !bookName.Contains(OWNED_MARK))
                {
                    newName += OWNED_MARK;
                    hasMark = true;
                }
                // 检查特殊仓库（蓝色标记）
                if (IsSpeBook(bookName) && !bookName.Contains(SPE_BOOK_MARK))
                {
                    newName += SPE_BOOK_MARK;
                    hasMark = true;
                }

                if (hasMark)
                {
                    icon.itemData.name = newName;
                    _addedMark = true;
                }
            }
        }

        // 统一的处理逻辑
        private static void HandleShowTradeUI(TradeUIController __instance, TradeUIType targetType, ItemListData rightItemList)
        {
            // LOG.Msg($"ShowTradeUI called: {targetType}");
            if (__instance == null) return;
            _currentTradeUI = __instance;
            ScanOwnedBooks();
            MelonCoroutines.Start(MarkOwnedBooksOnTradeUICoroutine(__instance));
            
            if (targetType == TradeUIType.Storage)
            {
                _currentItemListController = __instance.rightList;
                _originalItemList = rightItemList;
                _itemTypeList = _currentItemListController.targetItemList.itemTypeList;
                _bookList = _currentItemListController.targetItemList.itemTypeList[(int)ItemType.Book];
                HideFilterUI();
            }
        }

        // 重载1: ShowTradeUI(TradeUIType, ItemListData, ItemListData, bool)
        [HarmonyPostfix]
        [HarmonyPatch(typeof(TradeUIController), nameof(TradeUIController.ShowTradeUI), typeof(TradeUIType), typeof(ItemListData), typeof(ItemListData), typeof(bool))]
        public static void ShowTradeUI_Postfix1(TradeUIController __instance, TradeUIType targetType, ItemListData leftItemList, ItemListData rightItemList)
        {
            HandleShowTradeUI(__instance, targetType, rightItemList);
        }

        // 重载2: ShowTradeUI(TradeUIType, ItemListType, ItemListData, ItemListData)
        [HarmonyPostfix]
        [HarmonyPatch(typeof(TradeUIController), nameof(TradeUIController.ShowTradeUI), typeof(TradeUIType), typeof(ItemListType), typeof(ItemListData), typeof(ItemListData))]
        public static void ShowTradeUI_Postfix2(TradeUIController __instance, TradeUIType targetType, ItemListType targetItemListType, ItemListData leftItemList, ItemListData rightItemList)
        {
            HandleShowTradeUI(__instance, targetType, rightItemList);
        }

        // 重载3: ShowTradeUI(TradeUIType, ItemListData, ItemListData, int, int)
        [HarmonyPostfix]
        [HarmonyPatch(typeof(TradeUIController), nameof(TradeUIController.ShowTradeUI), typeof(TradeUIType), typeof(ItemListData), typeof(ItemListData), typeof(int), typeof(int))]
        public static void ShowTradeUI_Postfix3(TradeUIController __instance, TradeUIType targetType, ItemListData leftItemList, ItemListData rightItemList, int _minItemLv, int _maxItemLv)
        {
            HandleShowTradeUI(__instance, targetType, rightItemList);
        }

        // 重载4: ShowTradeUI(TradeUIType, ItemListType, ItemListData, ItemListData, int, int, bool, bool, float, float) - FameExchange 使用
        [HarmonyPostfix]
        [HarmonyPatch(typeof(TradeUIController), nameof(TradeUIController.ShowTradeUI), typeof(TradeUIType), typeof(ItemListType), typeof(ItemListData), typeof(ItemListData), typeof(int), typeof(int), typeof(bool), typeof(bool), typeof(float), typeof(float))]
        public static void ShowTradeUI_Postfix4(TradeUIController __instance, TradeUIType targetType, ItemListType targetItemListType, ItemListData leftItemList, ItemListData rightItemList, int _minItemLv, int _maxItemLv, bool _useAreaItemPrice, bool _noSell, float _speSellValueRate, float _speBuyValueRate)
        {
            HandleShowTradeUI(__instance, targetType, rightItemList);
        }

        private static void ClearAllOwnedMarks(TradeUIController tuc)
        {
            // 清理当前商店的标记
            if (tuc == null) return;
            var list = tuc.rightList.targetItemList.allItem;
            foreach (var item in list)
            {
                if (item is { name: not null } && item.name.Contains(OWNED_MARK))
                {
                    item.name = RemoveOwnedMark(item.name);
                }
            }
            
            // 清理所有已标记的物品名称
            var gc = GameController.Instance;
            if (gc?.worldData != null)
            {
                // 清理玩家仓库
                var player = gc.worldData.Player();
                if (player?.selfStorage?.allItem != null)
                {
                    foreach (var item in player.selfStorage.allItem)
                    {
                        if (item is { name: not null } && item.name.Contains(OWNED_MARK))
                        {
                            item.name = RemoveOwnedMark(item.name);
                        }
                    }
                }
                
                // 清理门派仓库
                if (player?.belongForceID != -1)
                {
                    var forceData = gc.worldData.GetHeroForce(0);
                    if (forceData?.bookStorage?.allItem != null)
                    {
                        foreach (var item in forceData.bookStorage.allItem)
                        {
                            if (item is { name: not null } && item.name.Contains(OWNED_MARK))
                            {
                                item.name = RemoveOwnedMark(item.name);
                            }
                        }
                    }
                }
                
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TradeUIController), nameof(TradeUIController.HideTradeUI))]
        public static void HideTradeUI_Prefix(TradeUIController __instance)
        {
            // 关闭界面时清理所有标记
            ClearAllOwnedMarks(__instance);
            
            _currentItemListController = null;
            _currentTradeUI = null;
            _currentSkillTypeFilter = -1;
            _currentRareLvFilter = -1;
            _originalItemList = null;
            _addedMark = false;
            HideFilterUI();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TradeUIController), nameof(TradeUIController.SureButtonClicked))]
        public static bool SureButtonClicked_Prefix(TradeUIController __instance)
        {
            if (__instance == null) return true;

            var rightOutList = __instance.rightOutList?.itemGrid?.GetComponentsInChildren<ItemIconController>(true);
            var leftOutList = __instance.leftOutList?.itemGrid?.GetComponentsInChildren<ItemIconController>(true);
            
            if (_addedMark)
            {
                if (rightOutList == null || rightOutList.Length == 0) return true;
                foreach (var icon in rightOutList)
                {
                    if (icon?.itemData == null) continue;
                    if (icon.itemData.type != ItemType.Book) continue;

                    var bookName = icon.itemData.name;
                    if (!string.IsNullOrEmpty(bookName) && bookName.Contains(OWNED_MARK))
                    {
                        icon.itemData.name = RemoveOwnedMark(bookName);
                    }
                }
            }
            // 当在秘籍筛选时存取
            if (__instance.tradeUIType == TradeUIType.Storage && __instance.rightList.nowItemListType == ItemListType.BookType &&
                __instance.forceItemListType == ItemListType.None)
            {
                // 修复后：由于 RefreshCurrentList 不再替换 targetItemList，
                // 游戏内部会自动处理从 _originalItemList 中移除/添加物品
                // 我们只需要确保 _bookList 与 _originalItemList 同步
                
                // 重新从 _originalItemList 获取秘籍列表（因为游戏可能已经修改了 allItem）
                _bookList = _originalItemList.itemTypeList[(int)ItemType.Book];
                
                // 重置筛选状态
                if (_skillTypeDropdown != null && _skillTypeDropdown.gameObject != null)
                {
                    _skillTypeDropdown.value = 0;
                }
                if (_rareLvDropdown != null && _rareLvDropdown.gameObject != null)
                {
                    _rareLvDropdown.value = 0;
                }
                _currentSkillTypeFilter = -1;
                _currentRareLvFilter = -1;
            }
            
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemListController), nameof(ItemListController.ChangeListType))]
        public static void ChangeListType_Postfix(ItemListController __instance)
        {
            // LOG.Msg($"ItemListController.ChangeListType called, nowItemListType: {__instance.nowItemListType}");
            
            if (__instance.nowItemListType == ItemListType.BookType)
            {
                if (_currentTradeUI == null || _currentTradeUI.tradeUIType != TradeUIType.Storage) return;
                ShowFilterUI();
                // LOG.Msg("进入个人仓库秘籍分类，筛选功能已激活！");
            }
            else
            {
                // LOG.Msg("触发了变更=============");
                if (_currentItemListController != __instance) return;
                HideFilterUI();
                // LOG.Msg($"allItem:{_currentItemListController.targetItemList.allItem.Count}, bookList:{_currentItemListController.targetItemList.itemTypeList[(int)ItemListType.BookType]}");
                _originalItemList.itemTypeList[(int)ItemListType.BookType] = _bookList;
                _currentItemListController.targetItemList = _originalItemList;
                // LOG.Msg($"_originalItemList:{_originalItemList.Count}, _itemTypeList:{_itemTypeList.Count}, _bookList:{_bookList.Count}");
                // LOG.Msg("=============");
                _currentItemListController.RefreshItemList(true);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemIconController), nameof(ItemIconController.OnClick))]
        public static void ItemIconController_OnClick_Postfix(ItemIconController __instance)
        {
            // 当在秘籍筛选状态下点击物品时，重新应用筛选
            // 因为游戏内部会刷新列表，导致筛选失效
            if (_currentItemListController == null) return;
            if (_currentTradeUI?.tradeUIType != TradeUIType.Storage) return;
            if (_currentItemListController.nowItemListType != ItemListType.BookType) return;
            
            // 如果有筛选条件，重新应用筛选
            if (_currentSkillTypeFilter != -1 || _currentRareLvFilter != -1)
            {
                RefreshCurrentList();
            }
        }
    }
}
