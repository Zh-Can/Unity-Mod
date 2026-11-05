using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using SmartTrade;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SmartTrade
{
    public static class TradeFuncModule
    {
        #region 商店按钮

        private static GameObject _autoBuyButton;
        private static GameObject _autoSellButton;
        private static TradeUIController _currentTradeUI;
        private static float _lastClickTime;

        #endregion

        #region 商店按钮处理

        public static void HandleTradeButtons()
        {
            if (_currentTradeUI == null || _currentTradeUI.tradeUI == null)
                return;

            if (_autoBuyButton == null && _autoSellButton == null)
                return;

            if (!_currentTradeUI.tradeUI.activeInHierarchy)
                return;

            if (!Input.GetMouseButtonDown(0))
                return;

            if (Time.unscaledTime - _lastClickTime < 0.2f)
                return;

            if (_autoBuyButton != null) CheckButtonClick(_autoBuyButton, OnAutoBuyClicked);
            if (_autoSellButton != null) CheckButtonClick(_autoSellButton, OnAutoSellClicked);
        }

        private static void CheckButtonClick(GameObject buttonObj, Func<IEnumerator> callback)
        {
            var rect = buttonObj.GetComponent<RectTransform>();
            if (rect == null) return;

            var canvas = buttonObj.GetComponentInParent<Canvas>();
            Camera camera = null;
            if (canvas != null && (int)canvas.renderMode != 0) camera = canvas.worldCamera;

            if (RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition, camera))
            {
                _lastClickTime = Time.unscaledTime;
                MelonCoroutines.Start(callback());
            }
        }

        public static void CreateButtons(TradeUIController tradeUIController)
        {
            DestroyButtons();
            MelonCoroutines.Start(CreateButtonsCoroutine(tradeUIController));
        }

        private static IEnumerator CreateButtonsCoroutine(TradeUIController tradeUIController)
        {
            if (tradeUIController == null) yield break;

            var tradeUI = tradeUIController.tradeUI;
            if (tradeUI == null) yield break;

            var sureButton = FindButton(tradeUI, "SureButton");
            var cancelButton = FindButton(tradeUI, "CancelButton");

            if (sureButton == null)
            {
                Plugin.LOG.Warning("[SmartTrade] 未找到SureButton");
                yield break;
            }

            var maxWait = 50;
            var waited = 0;

            while (waited < maxWait)
            {
                var merchantList = tradeUIController.rightList;
                var allItem = merchantList?.targetItemList?.allItem;
                if (merchantList?.itemGrid != null && allItem != null && allItem.Count > 0)
                {
                    break;
                }
                waited++;
                yield return null;
            }

            if (HasTreasureInShop(tradeUIController))
            {
                _autoBuyButton = CreateButtonFromTemplate(sureButton, "AutoBuyButton", "自动买入");
            }

            if (ShopHasMoney(tradeUIController))
            {
                _autoSellButton = CreateButtonFromTemplate(sureButton, "AutoSellButton", "自动卖出");
            }

            if (_autoBuyButton != null || _autoSellButton != null)
            {
                PositionButtons(_autoBuyButton, _autoSellButton, sureButton, cancelButton);
            }
        }

        private static void DestroyButtons()
        {
            if (_autoBuyButton != null)
            {
                Object.Destroy(_autoBuyButton);
                _autoBuyButton = null;
            }

            if (_autoSellButton != null)
            {
                Object.Destroy(_autoSellButton);
                _autoSellButton = null;
            }
        }

        private static bool HasTreasureInShop(TradeUIController tradeUIController)
        {
            try
            {
                var allItem = tradeUIController?.rightList?.targetItemList?.allItem;
                if (allItem == null) return false;

                foreach (var item in allItem)
                {
                    if (item == null) continue;
                    if (item.type == ItemType.Treasure) return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Plugin.LOG.Msg($"[SmartTrade] 检查珍宝时出错: {ex.Message}");
                return false;
            }
        }

        private static bool ShopHasMoney(TradeUIController tradeUIController)
        {
            try
            {
                var money = tradeUIController?.rightList?.targetItemList?.money ?? 0;
                return money > 0;
            }
            catch
            {
                return false;
            }
        }

        private static Button FindButton(GameObject tradeUI, string buttonName)
        {
            var buttons = tradeUI.GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
                if (btn != null && btn.gameObject.name.Equals(buttonName, StringComparison.OrdinalIgnoreCase))
                    return btn;

            return null;
        }

        private static GameObject CreateButtonFromTemplate(Button template, string buttonName, string buttonText)
        {
            var newButton = Object.Instantiate(template.gameObject, template.transform.parent);
            newButton.name = buttonName;

            StripUnsafeBehaviours(newButton);
            DisableNativeClick(newButton);

            foreach (var text in newButton.GetComponentsInChildren<Text>(true)) text.text = buttonText;

            return newButton;
        }

        private static void StripUnsafeBehaviours(GameObject root)
        {
            foreach (var mb in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb == null) continue;
                if (mb is Text || mb is Image) continue;

                var typeName = mb.GetType().FullName ?? "";
                if (typeName.Contains("DOTween", StringComparison.OrdinalIgnoreCase) ||
                    typeName.Contains("Tween", StringComparison.OrdinalIgnoreCase) ||
                    typeName.Contains("Animation", StringComparison.OrdinalIgnoreCase))
                    Object.Destroy(mb);
            }

            var animator = root.GetComponent("Animator");
            if (animator != null) Object.Destroy(animator);
        }

        private static void DisableNativeClick(GameObject root)
        {
            var button = root.GetComponent<Button>();
            if (button != null) button.enabled = false;

            foreach (var image in root.GetComponentsInChildren<Image>(true)) image.raycastTarget = false;

            foreach (var text in root.GetComponentsInChildren<Text>(true)) text.raycastTarget = false;
        }

        private static void PositionButtons(GameObject buyButton, GameObject sellButton, Button sureButton,
            Button cancelButton)
        {
            var sureRect = sureButton.GetComponent<RectTransform>();
            if (sureRect == null) return;

            var smallerSize = new Vector2(sureRect.sizeDelta.x * 0.7f, sureRect.sizeDelta.y * 0.85f);
            var gap = 30f;
            var x = sureRect.anchoredPosition.x - smallerSize.x - gap;
            var y = sureRect.anchoredPosition.y;

            if (cancelButton != null)
            {
                var cancelRect = cancelButton.GetComponent<RectTransform>();
                if (cancelRect != null) y = Math.Min(sureRect.anchoredPosition.y, cancelRect.anchoredPosition.y);
            }

            if (sellButton != null)
            {
                var sellRect = sellButton.GetComponent<RectTransform>();
                if (sellRect != null)
                {
                    sellRect.sizeDelta = smallerSize;
                    sellRect.anchorMin = sureRect.anchorMin;
                    sellRect.anchorMax = sureRect.anchorMax;
                    sellRect.pivot = sureRect.pivot;
                    sellRect.anchoredPosition = new Vector2(x, y);
                    x -= smallerSize.x + gap;
                }
            }

            if (buyButton != null)
            {
                var buyRect = buyButton.GetComponent<RectTransform>();
                if (buyRect != null)
                {
                    buyRect.sizeDelta = smallerSize;
                    buyRect.anchorMin = sureRect.anchorMin;
                    buyRect.anchorMax = sureRect.anchorMax;
                    buyRect.pivot = sureRect.pivot;
                    buyRect.anchoredPosition = new Vector2(x, y);
                }
            }
        }

        #endregion

        #region 自动买卖逻辑

        private static bool _automating;

        /**
         * 买入按钮
         */
        private static IEnumerator OnAutoBuyClicked()
        {
            if (_automating) yield break;
            if (_currentTradeUI == null) yield break;

            var rightList = _currentTradeUI.rightList;
            if (rightList == null || rightList.itemGrid == null) yield break;

            var srcData = rightList.targetItemList;
            if (srcData?.allItem == null) yield break;

            var gc = GameController.Instance;
            if (gc == null) yield break;

            var player = gc.worldData?.Player();
            if (player == null) yield break;

            var area = player.GetArea();
            var expensiveTypes = new HashSet<int>();
            var treasurePriceList = area?.areaTreasurePriceData;
            if (treasurePriceList != null)
            {
                foreach (var atpd in treasurePriceList)
                {
                    if (atpd != null && atpd.expensive)
                    {
                        expensiveTypes.Add(atpd.treasureType);
                    }
                }
            }

            var plugin = Plugin.Instance;
            var buyAll = plugin != null && (plugin.RedQuality || plugin.GoodTreasure);

            // 待购区已有物品的快照：已在待购里的绝不能再点，否则会再 GetItem 一份（虚空物品）
            var cartList = _currentTradeUI.rightOutList;
            var cartData = cartList?.targetItemList;
            var cartSet = new HashSet<IntPtr>();
            if (cartData?.allItem != null)
            {
                var cartCount = cartData.allItem.Count;
                for (var i = 0; i < cartCount; i++)
                {
                    var cartItem = cartData.allItem[i];
                    if (cartItem != null) cartSet.Add(ItemPtr(cartItem));
                }
            }

            Other.ShowInfo("开始自动买入");

            // 注意：同款珍宝的物品数据可能完全一样，用 TreasureKey 去重会把"第二件同款"当重复跳过，
            // 所以这里按 ItemData 实例去重（每件物品只点一次）
            var clicked = new HashSet<IntPtr>();
            var failed = new HashSet<IntPtr>();
            var scroll = rightList.itemGrid.GetComponentInParent<ScrollRect>();

            // 虚拟列表：把整张表的图标都租出来，不再靠滚动去"刷"物品
            var bulk = BulkIconBinder.CanBind(rightList);
            var staleLimit = bulk ? 6 : 20;

            _automating = true;
            var stale = 0;
            try
            {
                for (var pass = 0; pass < 3000 && stale < staleLimit; pass++)
                {
                    var ui = _currentTradeUI;
                    if (ui == null || ui.tradeUI == null || !ui.tradeUI.activeInHierarchy) yield break;

                    // 列表每次刷新后虚拟化只会留下可视区间的格子，这里补齐
                    if (bulk) BulkIconBinder.EnsureAllBound(rightList);

                    ItemIconController target = null;
                    ItemData targetItem = null;
                    var targetKey = IntPtr.Zero;

                    var icons = rightList.itemGrid?.GetComponentsInChildren<ItemIconController>(true);
                    if (icons != null)
                    {
                        foreach (var icon in icons)
                        {
                            if (icon == null || icon.gameObject == null || !icon.gameObject.activeInHierarchy) continue;

                            var item = icon.itemData;
                            if (item == null || item.type != ItemType.Treasure || item.treasureData == null) continue;

                            var key = ItemPtr(item);
                            if (clicked.Contains(key) || failed.Contains(key) || cartSet.Contains(key)) continue;

                            // 图标指向的物品必须真的还在商店列表里
                            if (!ContainsItem(srcData.allItem, key)) continue;

                            if (!buyAll)
                            {
                                if (area != null && expensiveTypes.Contains(item.subType)) continue;
                                if (NetIncome(item.GetTreasureRealValue(), Plugin.AreaRate, icon.GetItemPrice(true)) <= 1) continue;
                            }

                            target = icon;
                            targetItem = item;
                            targetKey = key;
                            break;
                        }
                    }

                    if (target != null)
                    {
                        var ok = TryBuy(target, targetItem, targetKey, srcData, cartData, cartSet, failed);
                        if (ok)
                        {
                            clicked.Add(targetKey);
                            cartSet.Add(targetKey);
                        }

                        stale = 0;
                        yield return null;
                        continue;
                    }

                    // 无法批量绑定时退回滚动方案
                    if (!bulk && scroll != null && scroll.verticalNormalizedPosition > 0.001f)
                    {
                        scroll.verticalNormalizedPosition = Mathf.Max(0f, scroll.verticalNormalizedPosition - 0.25f);
                        stale = 0;
                        yield return null;
                        continue;
                    }

                    stale++;
                    yield return null;
                }

                // 扫完后统计一下剩下的，方便确认"为什么没买"
                LogBuySummary(rightList, clicked, buyAll, area, expensiveTypes);
            }
            finally
            {
                if (bulk) BulkIconBinder.ReleaseExtra(rightList);
                _automating = false;
            }
        }

        /// <summary>点一次“买入”，并校验物品是否真的从商店列表消失，没消失就把待购里那份撤掉</summary>
        private static bool TryBuy(ItemIconController icon, ItemData item, IntPtr key,
            ItemListData srcData, ItemListData cartData, HashSet<IntPtr> cartSet, HashSet<IntPtr> failed)
        {
            if (icon == null || item == null) return false;

            try
            {
                icon.OnClick();
            }
            catch (Exception ex)
            {
                Plugin.LOG.Warning("[SmartTrade] 买入点击异常: " + ex.Message);
                failed.Add(key);
                return false;
            }

            if (!ContainsItem(srcData.allItem, key))
            {
                cartSet.Add(key);
                return true;
            }

            failed.Add(key);
            try
            {
                if (cartData != null && ContainsItem(cartData.allItem, key))
                {
                    cartData.LoseItem(item);
                    _currentTradeUI?.rightOutList?.RefreshItemList(true);
                }

                Plugin.LOG.Warning($"[SmartTrade] 买入未生效，已回滚: {item.Name()}");
            }
            catch (Exception ex)
            {
                Plugin.LOG.Warning("[SmartTrade] 回滚待购物品失败: " + ex.Message);
            }

            cartSet.Remove(key);
            return false;
        }

        private static void LogBuySummary(ItemListController list, HashSet<IntPtr> bought, bool buyAll,
            AreaData area, HashSet<int> expensiveTypes)
        {
            try
            {
                var lowProfit = 0;
                var expensive = 0;
                var unknown = 0;

                // 把剩下的条目也全部绑出来，统计才完整
                if (BulkIconBinder.CanBind(list)) BulkIconBinder.EnsureAllBound(list);

                var icons = list?.itemGrid?.GetComponentsInChildren<ItemIconController>(true);
                if (icons != null)
                {
                    foreach (var icon in icons)
                    {
                        if (icon == null || icon.itemData == null) continue;
                        var item = icon.itemData;
                        if (item.type != ItemType.Treasure) continue;

                        if (buyAll) { unknown++; continue; }
                        if (area != null && expensiveTypes.Contains(item.subType)) { expensive++; continue; }
                        if (NetIncome(item.GetTreasureRealValue(), Plugin.AreaRate, icon.GetItemPrice(true)) <= 1)
                        {
                            lowProfit++;
                            continue;
                        }

                        unknown++;
                    }
                }

                Other.ShowInfo($"买入 {bought.Count} 件，剩余：利润不足 {lowProfit}、贵品类 {expensive}、未处理 {unknown}");
                Plugin.LOG.Msg($"[SmartTrade] 自动买入结束：买入 {bought.Count} 件，剩余 利润不足={lowProfit} 贵品类={expensive} 未处理={unknown}");
            }
            catch (Exception ex)
            {
                Plugin.LOG.Warning("[SmartTrade] 统计剩余物品失败: " + ex.Message);
            }
        }
        
        public static void SetCurrentTradeUI(TradeUIController controller)
        {
            _currentTradeUI = controller;
        }
        /**
         * 卖出按钮
         */
        private static IEnumerator OnAutoSellClicked()
        {
            if (_automating) yield break;
            if (_currentTradeUI == null) yield break;

            var leftList = _currentTradeUI.leftList;
            if (leftList == null) yield break;

            if (leftList.itemGrid == null) yield break;

            var srcData = leftList.targetItemList;
            if (srcData?.allItem == null) yield break;

            ScanPlayerTreasures();

            var shopMoney = _currentTradeUI.rightList?.targetItemList?.money ?? 0;
            var totalSellPrice = 0;

            var gc = GameController.Instance;
            if (gc == null) yield break;

            var player = gc.worldData?.Player();
            if (player == null) yield break;

            var plugin = Plugin.Instance;
            var sellHighQuality = plugin != null && plugin.SellHighQuality;

            var area = player.GetArea();
            var treasurePriceList = area?.areaTreasurePriceData;

            var cheapTypes = new HashSet<int>();
            var expensiveTypes = new HashSet<int>();

            if (treasurePriceList != null)
            {
                foreach (var atpd in treasurePriceList)
                {
                    if (atpd == null) continue;
                    if (atpd.expensive) expensiveTypes.Add(atpd.treasureType);
                    else cheapTypes.Add(atpd.treasureType);
                }
            }

            var validSellItemIds = new HashSet<int>(Plugin.PlayerTreasures.Where(t => t != null).Select(t => t.itemID));

            // UI 上勾选的卖出类别（空 = 不按类别，全卖）
            var sellTypeFilter = Plugin.SellTypeFilter;

            // 待售区已有物品的快照：已经在待售里的东西绝不能再点，否则 GetItem 会再 Add 一份（虚空物品）
            var cartList = _currentTradeUI.leftOutList;
            var cartData = cartList?.targetItemList;
            var cartSet = new HashSet<IntPtr>();
            if (cartData?.allItem != null)
            {
                var cartCount = cartData.allItem.Count;
                for (var i = 0; i < cartCount; i++)
                {
                    var cartItem = cartData.allItem[i];
                    if (cartItem != null) cartSet.Add(ItemPtr(cartItem));
                }
            }

            try
            {
                var invAll = player.itemListData?.allItem;
                var same = invAll != null && srcData.allItem != null && invAll.Pointer == srcData.allItem.Pointer;
                Plugin.LOG.Msg($"[SmartTrade] 卖出源列表: allItem={srcData.allItem?.Count} " +
                               $"背包allItem={invAll?.Count} 同一列表={same} 待售={cartData?.allItem?.Count ?? 0}");
            }
            catch (Exception ex)
            {
                Plugin.LOG.Warning("[SmartTrade] 诊断卖出列表失败: " + ex.Message);
            }

            Other.ShowInfo("开始自动卖出");

            var sold = new HashSet<IntPtr>();
            var failed = new HashSet<IntPtr>();
            var scroll = leftList.itemGrid.GetComponentInParent<ScrollRect>();

            var bulk = BulkIconBinder.CanBind(leftList);
            var staleLimit = bulk ? 6 : 20;

            _automating = true;
            var stale = 0;
            try
            {
                for (var pass = 0; pass < 3000 && stale < staleLimit; pass++)
                {
                    var ui = _currentTradeUI;
                    if (ui == null || ui.tradeUI == null || !ui.tradeUI.activeInHierarchy) yield break;

                    if (bulk) BulkIconBinder.EnsureAllBound(leftList);

                    ItemIconController target = null;
                    ItemData targetItem = null;
                    var targetKey = IntPtr.Zero;
                    var targetPrice = 0;

                    var icons = leftList.itemGrid?.GetComponentsInChildren<ItemIconController>(true);
                    if (icons != null)
                    {
                        foreach (var wantExpensive in new[] { true, false })
                        {
                            foreach (var icon in icons)
                            {
                                if (icon == null || icon.gameObject == null || !icon.gameObject.activeInHierarchy) continue;

                                var item = icon.itemData;
                                if (item == null || item.type != ItemType.Treasure || item.treasureData == null) continue;
                                if (!validSellItemIds.Contains(item.itemID)) continue;
                                if (!item.treasureData.fullIdentified) continue;
                                if (item.itemLv >= 3 && !sellHighQuality) continue;
                                if (sellTypeFilter.Count > 0 && !sellTypeFilter.Contains(item.subType)) continue;

                                if (wantExpensive)
                                {
                                    if (!expensiveTypes.Contains(item.subType)) continue;
                                }
                                else
                                {
                                    if (expensiveTypes.Contains(item.subType)) continue;
                                    if (cheapTypes.Contains(item.subType)) continue;
                                }

                                var key = ItemPtr(item);
                                if (sold.Contains(key) || failed.Contains(key) || cartSet.Contains(key)) continue;

                                // 图标指向的物品必须真的还在源列表里，否则点下去只会往待售里凭空塞一份
                                if (!ContainsItem(srcData.allItem, key)) continue;

                                var price = icon.GetItemPrice(false);
                                // deltaMoney = 已经在待售/待购里的金额，游戏确认后才真正结算，
                                // 所以要把它算进店家预算，否则每次点都以为店家还有满额的钱
                                var pending = _currentTradeUI?.deltaMoney ?? 0;
                                if (pending + price > shopMoney) continue;

                                target = icon;
                                targetItem = item;
                                targetKey = key;
                                targetPrice = price;
                                break;
                            }

                            if (target != null) break;
                        }
                    }

                    if (target != null)
                    {
                        var ok = TrySell(target, targetItem, targetKey, srcData, cartData, cartSet, failed);
                        if (ok)
                        {
                            sold.Add(targetKey);
                            cartSet.Add(targetKey);
                            totalSellPrice += targetPrice;
                        }

                        stale = 0;
                        yield return null;
                        continue;
                    }

                    if (!bulk && scroll != null && scroll.verticalNormalizedPosition > 0.001f)
                    {
                        scroll.verticalNormalizedPosition = Mathf.Max(0f, scroll.verticalNormalizedPosition - 0.25f);
                        stale = 0;
                        yield return null;
                        continue;
                    }

                    stale++;
                    yield return null;
                }

                LogSellSummary(leftList, sold.Count, totalSellPrice, shopMoney, validSellItemIds, cheapTypes,
                    sellHighQuality, sellTypeFilter);
            }
            finally
            {
                if (bulk) BulkIconBinder.ReleaseExtra(leftList);
                _automating = false;
            }
        }

        /// <summary>
        /// 点一次“卖出”，并校验物品是否真的从源列表消失。
        /// 若没消失（图标失效/数据不同步），把刚才塞进待售的那一份撤掉，避免虚空物品累积。
        /// </summary>
        private static bool TrySell(ItemIconController icon, ItemData item, IntPtr key,
            ItemListData srcData, ItemListData cartData, HashSet<IntPtr> cartSet, HashSet<IntPtr> failed)
        {
            if (icon == null || item == null) return false;

            try
            {
                icon.OnClick();
            }
            catch (Exception ex)
            {
                Plugin.LOG.Warning("[SmartTrade] 卖出点击异常: " + ex.Message);
                failed.Add(key);
                return false;
            }

            // 正常情况下 LoseItem 已经把它从 allItem / itemTypeList 移除
            if (!ContainsItem(srcData.allItem, key))
            {
                cartSet.Add(key);
                return true;
            }

            // 还在源列表里 → 这次点击没生效，但 GetItem 可能已经往待售里加了一份
            failed.Add(key);
            try
            {
                if (cartData != null && ContainsItem(cartData.allItem, key))
                {
                    cartData.LoseItem(item);
                    _currentTradeUI?.leftOutList?.RefreshItemList(true);
                }

                Plugin.LOG.Warning($"[SmartTrade] 卖出未生效，已回滚: {item.Name()}");
            }
            catch (Exception ex)
            {
                Plugin.LOG.Warning("[SmartTrade] 回滚待售物品失败: " + ex.Message);
            }

            cartSet.Remove(key);
            return false;
        }

        private static void LogSellSummary(ItemListController leftList, int soldCount, int totalSellPrice, int shopMoney,
            HashSet<int> validIds, HashSet<int> cheapTypes, bool sellHighQuality, HashSet<int> typeFilter)
        {
            try
            {
                var notIdentified = 0;
                var levelBlocked = 0;
                var notOwned = 0;
                var cheap = 0;
                var typeSkipped = 0;
                var noMoney = 0;
                var other = 0;
                var moneyLeft = shopMoney - Math.Max(0, _currentTradeUI?.deltaMoney ?? 0);

                if (BulkIconBinder.CanBind(leftList)) BulkIconBinder.EnsureAllBound(leftList);

                var icons = leftList?.itemGrid?.GetComponentsInChildren<ItemIconController>(true);
                if (icons != null)
                {
                    foreach (var icon in icons)
                    {
                        if (icon == null || icon.itemData == null) continue;
                        var item = icon.itemData;
                        if (item.type != ItemType.Treasure) continue;

                        if (!validIds.Contains(item.itemID)) { notOwned++; continue; }
                        if (item.treasureData == null) { other++; continue; }
                        if (!item.treasureData.fullIdentified) { notIdentified++; continue; }
                        if (item.itemLv >= 3 && !sellHighQuality) { levelBlocked++; continue; }
                        if (typeFilter.Count > 0 && !typeFilter.Contains(item.subType)) { typeSkipped++; continue; }
                        if (cheapTypes.Contains(item.subType)) { cheap++; continue; }
                        if (icon.GetItemPrice(false) > moneyLeft) { noMoney++; continue; }

                        other++;
                    }
                }

                Other.ShowInfo($"卖出 {soldCount} 件/{totalSellPrice}文，剩余：未鉴定 {notIdentified}、" +
                               $"等级限制 {levelBlocked}、类别未选 {typeSkipped}、本地便宜 {cheap}、" +
                               $"店家钱不足 {noMoney}、未处理 {other}");
                Plugin.LOG.Msg($"[SmartTrade] 自动卖出结束：卖出 {soldCount} 件 共{totalSellPrice}文，" +
                               $"剩余 未鉴定={notIdentified} 等级限制={levelBlocked} 不在背包={notOwned} " +
                               $"类别未选={typeSkipped} 本地便宜={cheap} 店家钱不足={noMoney} 未处理={other}");
            }
            catch (Exception ex)
            {
                Plugin.LOG.Warning("[SmartTrade] 统计剩余物品失败: " + ex.Message);
            }
        }

        private static bool ContainsItem(Il2CppSystem.Collections.Generic.List<ItemData> list, IntPtr key)
        {
            if (list == null) return false;

            var count = list.Count;
            for (var i = 0; i < count; i++)
            {
                var item = list[i];
                if (item != null && ItemPtr(item) == key) return true;
            }

            return false;
        }

        private static float NetIncome(float realValue, float areaRate, int price)
        {
            return realValue * Plugin.SellRate * areaRate - price;
        }

        /// <summary>物品实例身份（同一件物品的稳定标识，用于"这件已经点过了"判重）</summary>
        private static IntPtr ItemPtr(ItemData item)
        {
            if (item == null) return IntPtr.Zero;
            try
            {
                var ptr = item.Pointer;
                if (ptr != IntPtr.Zero) return ptr;
            }
            catch
            {
                // 拿不到原生指针时退回引用哈希，绝不能让判重键变成 0 而跳过物品
            }

            return new IntPtr(System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(item));
        }

        private static string TreasureKey(ItemData item)
        {
            return $"{item.type},{item.itemID},{item.subType},{item.itemLv},{item.rareLv},{item.value},{item.GetTreasureRealValue()}";
        }

        public static void RefreshData()
        {
            var gc = GameController.Instance;
            if (gc == null) return;

            var player = gc.worldData.Player();

            ScanPlayerTreasures();

            Plugin.KouCai = player.totalLivingSkill[3];
            var calculatedSellRate = 0.25f + Plugin.KouCai * 0.0025f;
            if (player.HaveTag(235)) calculatedSellRate += 0.05f;
            if (player.HaveTag(236)) calculatedSellRate += 0.1f;
            if (player.HaveTag(237)) calculatedSellRate += 0.15f;
            if (player.HaveTag(381)) calculatedSellRate += 0.2f;
            Plugin.SellRate = calculatedSellRate;
            
            Plugin.TableDatas.Clear();
            var list = gc.worldData.Areas;
            var nameList = GlobalData.TreasureTypeName;
            foreach (var area in list) 
            {
                if (area.areaType is 0 or 1)
                {
                    var data = new TableListEntity
                    {
                        AreaId = area.areaID,
                        AreaName = area.areaName,
                        AreaSpePriceRate = CalculateAreaRateByZhiAn(area.safe),
                    };
                    var treasureDatas = area.areaTreasurePriceData;
                    if (treasureDatas?.Count > 0)
                    {
                        foreach (var treasureData in treasureDatas)
                        {
                            data.SpeTreasureType += treasureData.treasureType + ",";
                            data.TreasurePriceInfo[treasureData.treasureType] = treasureData.expensive;
                            if (treasureData.expensive)
                            {
                                data.HasExpensive = true;
                                data.SpeTreasureTypeName +=
                                    "<color=red>" + nameList[treasureData.treasureType] + "</color>，";
                            }
                            else
                            {
                                data.HasCheap = true;
                                data.SpeTreasureTypeName +=
                                    "<color=green>" + nameList[treasureData.treasureType] + "</color>，";
                            }
                        }

                        data.SpeTreasureType = data.SpeTreasureType.TrimEnd(',');
                        data.SpeTreasureTypeName = data.SpeTreasureTypeName.TrimEnd('，');
                    }

                    data.CalculateIncome();
                    Plugin.TableDatas.Add(data);
                }
            }
        }

        // 计算地区买卖价格
        private static float CalculateAreaRateByZhiAn(float zhiAn)
        {
            return 1.25f - zhiAn * 0.005f;
        }

        public static void ScanPlayerTreasures()
        {
            var gc = GameController.Instance;
            if (gc == null) return;

            var player = gc.worldData?.Player();
            if (player == null) return;

            Plugin.PlayerTreasures.Clear();

            try
            {
                var itemList = player.itemListData?.allItem;
                if (itemList == null) return;

                foreach (var item in itemList)
                {
                    if (item == null) continue;
                    if (item.type != ItemType.Treasure) continue;

                    Plugin.PlayerTreasures.Add(item);
                }
            }
            catch (Exception ex)
            {
                Plugin.LOG.Warning("[SmartTrade] 扫描珍宝时出错: " + ex.Message);
            }
        }

        #endregion
    }
}

[HarmonyPatch(typeof(TradeUIController))]
public static class TradeUIControllerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(TradeUIController.SureButtonClicked))]
    public static bool SureButtonClicked_Prefix(TradeUIController __instance)
    {
        if (__instance == null) return true;

        // 待买卖列表同样是虚拟列表，先补齐图标再统计，否则看不到的格子会漏记
        if (__instance.rightOutList != null) BulkIconBinder.EnsureAllBound(__instance.rightOutList);
        if (__instance.leftOutList != null) BulkIconBinder.EnsureAllBound(__instance.leftOutList);

        var rightOutList =__instance.rightOutList?.itemGrid?.GetComponentsInChildren<ItemIconController>(true);
        var leftOutList =__instance.leftOutList?.itemGrid?.GetComponentsInChildren<ItemIconController>(true);

        // 购入商品时
        if (rightOutList is { Count: > 0 })
        {
            foreach (var icon in rightOutList)
            {
                if (icon?.itemData == null || icon.itemData.type != ItemType.Treasure) continue;
                var price = icon.GetItemPrice(true);
                Plugin.PurchaseItems.Add(new PurchaseItem(icon.itemData, price));
                if (!Plugin.PlayerTreasures.Contains(icon.itemData))
                {
                    Plugin.PlayerTreasures.Add(icon.itemData);
                }
            }
        }
        // 卖出商品时
        if (leftOutList is { Count: > 0 })
        {
            foreach (var icon in leftOutList)
            {
                if (icon?.itemData == null || icon.itemData.type != ItemType.Treasure) continue;
                Plugin.PurchaseItems.Remove(new PurchaseItem(icon.itemData, 0));
                Plugin.PlayerTreasures.Remove(icon.itemData);
            }
        }

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(TradeUIController.SureButtonClicked))]
    public static void SureButtonClicked_Postfix(TradeUIController __instance)
    {
        if (__instance == null) return;
        if (!Plugin.Instance.ShopModify) return;
        ShopMoneyHelper.OnAreaChanged(__instance.rightList.targetItemList.money);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(TradeUIController.ShowTradeUI), typeof(TradeUIType), typeof(ItemListData),
        typeof(ItemListData), typeof(bool))]
    public static void ShowTradeUI_Postfix(TradeUIController __instance, TradeUIType targetType, ItemListData leftItemList, ItemListData rightItemList)
    {
        if (targetType != TradeUIType.Shop) return;

        if (__instance.tradeUI == null)
        {
            return;
        }

        TradeFuncModule.SetCurrentTradeUI(__instance);
        TradeFuncModule.CreateButtons(__instance);

        var leftText = __instance.leftDiscount.GetComponent<SimpleDetailText>()?.text;
        var areaText = __instance.areaDiscount.GetComponent<SimpleDetailText>()?.text;
        if (leftText == null || areaText == null) return;
        var sellMatch = Regex.Match(leftText, @"出售价格(\d+)%");
        var areaMatch = Regex.Match(areaText, @"买卖价格x(\d+)%");
        if (!sellMatch.Success || !areaMatch.Success) 
        {
            Plugin.SellRate = 1f;
            Plugin.AreaRate = 1f;
            return;
        }
        var sell = int.Parse(sellMatch.Groups[1].Value);
        var area = int.Parse(areaMatch.Groups[1].Value);
        Plugin.SellRate = sell / 100f;
        Plugin.AreaRate = area / 100f;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(TradeUIController.ShowTradeUI), typeof(TradeUIType), typeof(ItemListData),
        typeof(ItemListData), typeof(bool))]
    public static bool ShowTradeUI_Prefix(TradeUIController __instance, TradeUIType targetType,
        ItemListData leftItemList, ItemListData rightItemList)
    {
        if (__instance == null) return true;
        if (Plugin.Instance.ShopModify)
        {
            var money = ShopMoneyHelper.OpenChanged();
            if (money > 0)
                rightItemList.money = money;
        }
        return true;
    }
}