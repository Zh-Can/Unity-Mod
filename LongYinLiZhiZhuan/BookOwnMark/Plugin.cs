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

[assembly: MelonInfo(typeof(BookOwnMark.Plugin), "BookOwnMark", "2.2", "Can")]
[assembly: MelonGame("TppStudio", "LongYinLiZhiZhuan")]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]

namespace BookOwnMark
{
    public class Plugin : MelonMod
    {
        public static Plugin Instance = null!;
        public static readonly MelonLogger.Instance LOG = Melon<Plugin>.Logger;

        private static HashSet<string> _ownedBookNames = [];
        private const string OWNED_MARK = " <color=#33cc86>☑</color>";
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
            // LOG.Msg($"type:{_currentSkillTypeFilter}, lv:{_currentRareLvFilter}");
            var tempList = new Il2CppSystem.Collections.Generic.List<ItemData>();
            // LOG.Msg($"tempItemList:{_bookList.Count}");
            foreach (var item in _bookList)
            {
                // LOG.Msg($"name:{item.name}, type:{item.bookData.DataBase().type}, lv:{item.bookData.DataBase().rareLv}, {item.bookData.DataBase().TypeDescribe()}");

                var db = item.bookData.DataBase();
                // LOG.Msg($"_currentSkillTypeFilter:{_currentSkillTypeFilter}, lv:{_currentRareLvFilter}");
                var matchType = (_currentSkillTypeFilter == -1 || db.type == _currentSkillTypeFilter);
                
                var matchRare = (_currentRareLvFilter == -1 || db.rareLv == _currentRareLvFilter);

                // LOG.Msg($"matchRare:{matchRare}, matchType:{matchType}");
                if (matchRare && matchType)
                {
                    tempList.Add(item);
                }
            }
            // LOG.Msg($"tempList:{tempList.Count}");
            
          
            _currentItemListController.targetItemList = new ItemListData
            {
                allItem = tempList,
                itemTypeList =
                {
                    [(int)ItemListType.BookType] = tempList
                }
            };
            _currentItemListController.RefreshItemList(true);
        }

        private static void ScanOwnedBooks()
        {
            _ownedBookNames.Clear();

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
                        _ownedBookNames.Add(item.Name());
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
                            _ownedBookNames.Add(item.Name());
                        }
                    }
                }
            }

            // LOG.Msg($"[BookOwnMark] 扫描到 {_ownedBookNames.Count} 本秘籍");
        }

        private static bool IsBookOwned(string itemName)
        {
            return _ownedBookNames.Contains(itemName);
        }

        private static string RemoveOwnedMark(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            return name.Replace(OWNED_MARK, "");
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

                if (IsBookOwned(bookName) && !bookName.EndsWith("☑</color>"))
                {
                    var newName = bookName + OWNED_MARK;
                    icon.itemData.name = newName;
                    _addedMark = true;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TradeUIController), nameof(TradeUIController.ShowTradeUI), typeof(TradeUIType), typeof(ItemListData), typeof(ItemListData), typeof(bool))]
        public static void ShowTradeUI_Postfix(TradeUIController __instance, TradeUIType targetType, ItemListData leftItemList, ItemListData rightItemList)
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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TradeUIController), nameof(TradeUIController.HideTradeUI))]
        public static void HideTradeUI_Prefix()
        {
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
            LOG.Msg($"111{__instance.forceItemListType}");
            // 当在秘籍筛选时存取
            if (__instance.tradeUIType == TradeUIType.Storage && __instance.rightList.nowItemListType == ItemListType.BookType &&
                __instance.forceItemListType == ItemListType.None)
            {
                // 如果有取出
                if (rightOutList != null && rightOutList.Length != 0)
                {
                    foreach (var icon in rightOutList)
                    {
                        var target = icon.itemData;
                        var findIndex = -1;

                        for (var i = 0; i < _originalItemList.allItem.Count; i++)
                        {
                            var item = _originalItemList.allItem[i];
                            if (item.name == target.name && item.value == target.value)
                            {
                                findIndex = i;
                                break;
                            }
                        }
                        if (findIndex != -1)
                        {
                            _originalItemList.allItem.RemoveAt(findIndex);
                        }

                        findIndex = -1;
                        
                        for (var i = 0; i < _bookList.Count; i++)
                        {
                            var item = _bookList[i];
                            if (item.name == target.name && item.value == target.value)
                            {
                                findIndex = i;
                                break;
                            }
                        }
                        if (findIndex != -1)
                        {
                            _bookList.RemoveAt(findIndex);
                        }
                    }
                }
                // 如果有存入
                if (leftOutList != null && leftOutList.Length != 0)
                {
                    foreach (var icon in leftOutList)
                    {
                        _originalItemList.allItem.Add(icon.itemData);
                        _bookList.Add(icon.itemData);
                    }
                }
                // 刷新数据
                _originalItemList.itemTypeList[(int)ItemType.Book] = _bookList;
                __instance.rightList.targetItemList = _originalItemList;
                // 重置选择
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
            LOG.Msg($"fromStorage:{__instance.fromStorage}, tradeIconType:{__instance.tradeIconType}");
            
            if (__instance == null) return;
            
        }
    }
}
