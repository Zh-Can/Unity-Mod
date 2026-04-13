using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HarmonyLib;
using Il2Cpp;
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
            if (_currentTradeUI == null || _autoBuyButton == null || _autoSellButton == null)
                return;

            if (!_currentTradeUI.tradeUI.activeInHierarchy)
                return;

            if (!Input.GetMouseButtonDown(0))
                return;

            if (Time.unscaledTime - _lastClickTime < 0.2f)
                return;

            CheckButtonClick(_autoBuyButton, OnAutoBuyClicked);
            CheckButtonClick(_autoSellButton, OnAutoSellClicked);
        }

        private static async void CheckButtonClick(GameObject buttonObj, Func<Task> callback)
        {
            var rect = buttonObj.GetComponent<RectTransform>();
            if (rect == null) return;

            var canvas = buttonObj.GetComponentInParent<Canvas>();
            Camera camera = null;
            if (canvas != null && (int)canvas.renderMode != 0) camera = canvas.worldCamera;

            if (RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition, camera))
            {
                _lastClickTime = Time.unscaledTime;
                await callback();
            }
        }

        public static void CreateButtons(TradeUIController tradeUIController)
        {
            DestroyButtons();

            if (!HasTreasureInShop(tradeUIController))
            {
                return;
            }

            var tradeUI = tradeUIController.tradeUI;
            var sureButton = FindButton(tradeUI, "SureButton");
            var cancelButton = FindButton(tradeUI, "CancelButton");

            if (sureButton == null)
            {
                Plugin.LOG.Warning("[SmartTrade] 未找到SureButton");
                return;
            }

            _autoBuyButton = CreateButtonFromTemplate(sureButton, "AutoBuyButton", "自动买入");
            _autoSellButton = CreateButtonFromTemplate(sureButton, "AutoSellButton", "自动卖出");

            if (_autoBuyButton != null && _autoSellButton != null)
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
                var merchantList = tradeUIController.rightList;
                if (merchantList == null || merchantList.itemGrid == null)
                {
                    Plugin.LOG.Warning("[SmartTrade] 商人物品列表为空");
                    return false;
                }

                var icons = merchantList.itemGrid.GetComponentsInChildren<ItemIconController>(true);
                foreach (var icon in icons)
                {
                    if (icon == null) continue;

                    var itemData = icon.itemData;
                    if (itemData == null) continue;

                    if (itemData.treasureData != null)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Plugin.LOG.Error($"[SmartTrade] 检查珍宝时出错: {ex.Message}");
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
            var buyRect = buyButton.GetComponent<RectTransform>();
            var sellRect = sellButton.GetComponent<RectTransform>();

            if (sureRect == null || buyRect == null || sellRect == null) return;

            var smallerSize = new Vector2(sureRect.sizeDelta.x * 0.7f, sureRect.sizeDelta.y * 0.85f);

            buyRect.sizeDelta = smallerSize;
            buyRect.anchorMin = sureRect.anchorMin;
            buyRect.anchorMax = sureRect.anchorMax;
            buyRect.pivot = sureRect.pivot;

            sellRect.sizeDelta = smallerSize;
            sellRect.anchorMin = sureRect.anchorMin;
            sellRect.anchorMax = sureRect.anchorMax;
            sellRect.pivot = sureRect.pivot;

            var gap = 30f;
            var startX = sureRect.anchoredPosition.x - smallerSize.x - gap;
            var y = sureRect.anchoredPosition.y;

            if (cancelButton != null)
            {
                var cancelRect = cancelButton.GetComponent<RectTransform>();
                if (cancelRect != null) y = Math.Min(sureRect.anchoredPosition.y, cancelRect.anchoredPosition.y);
            }

            sellRect.anchoredPosition = new Vector2(startX, y);
            buyRect.anchoredPosition = new Vector2(startX - smallerSize.x - gap, y);
        }

        #endregion

        #region 自动买卖逻辑

        /**
         * 买入按钮
         */
        private static async Task OnAutoBuyClicked()
        {
            Other.ShowInfo("开始自动买入");
            var icons = _currentTradeUI.rightList.itemGrid.GetComponentsInChildren<ItemIconController>(true);
            if (icons.Length <= 0) return;
            
            var player = GameController.Instance.worldData.Player();
            var area = player.GetArea();
            var list = area.areaTreasurePriceData;
            var expensiveList = new List<int>();
            foreach (var atpd in list)
            {
                if (atpd.expensive)
                {
                    expensiveList.Add(atpd.treasureType);
                }
            }
            
            foreach (var icon in icons)
            {
                var item = icon.itemData;
                if (item.type != ItemType.Treasure) continue;

                var realValue = item.GetTreasureRealValue();
                var price = icon.GetItemPrice(true);
                var netIncome = NetIncome(realValue, Plugin.AreaRate, price);
                
                if (expensiveList.Contains(icon.itemData.subType)) continue;
                
                if (netIncome > 1) icon.OnClick();
                
            }
        }
        
        public static void SetCurrentTradeUI(TradeUIController controller)
        {
            _currentTradeUI = controller;
        }
        /**
         * 卖出按钮
         */
        private static async Task OnAutoSellClicked()
        {
            Other.ShowInfo("开始自动卖出");
            if (Plugin.PurchaseItems is not {Count: > 0}) return;
            if (_currentTradeUI == null) return;
            
            var leftList = _currentTradeUI.leftList;
            if (leftList == null) return;
            
            var icons = leftList.itemGrid?.GetComponentsInChildren<ItemIconController>(true);
            if (icons == null) return;
            
            var shopMoney = _currentTradeUI.rightList?.targetItemList?.money ?? 0;
            var totalSellPrice = 0;

            var player = GameController.Instance.worldData.Player();
            var area = player.GetArea();
            var list = area == null ? new Il2CppSystem.Collections.Generic.List<AreaTreasurePriceData>() : area.areaTreasurePriceData;
            var cheapTypeList = new List<int>();
            var expensiveTypeList = new List<int>();
            foreach (var atpd in list)
            {
                if (!atpd.expensive) 
                    cheapTypeList.Add(atpd.treasureType);
                else
                    expensiveTypeList.Add(atpd.treasureType);
            }
            var expensiveList = new List<ItemIconController>(); 
            var normalList = new List<ItemIconController>();    
            foreach (var icon in icons)
            {
                if (icon.itemData.treasureData == null) continue;
                if (expensiveTypeList.Contains(icon.itemData.subType))
                    expensiveList.Add(icon);
                else if (!cheapTypeList.Contains(icon.itemData.subType))
                    normalList.Add(icon);
            }

            foreach (var purchaseItem in Plugin.PurchaseItems)
            {
                if (purchaseItem?.ItemData == null) continue;

                foreach (var icon in expensiveList)
                {
                    if (!ReferenceEquals(icon.itemData, purchaseItem.ItemData) ||
                        !icon.itemData.treasureData.fullIdentified) continue;
                    if (icon.itemData.itemLv >= 3 && !Plugin.SellHighQuality) continue;

                    var sellPrice = icon.GetItemPrice(false);

                    if (totalSellPrice + sellPrice > shopMoney) break;

                    icon.OnClick();
                    totalSellPrice += sellPrice;
                }
            }
            foreach (var purchaseItem in Plugin.PurchaseItems)
            {
                if (purchaseItem?.ItemData == null) continue;

                foreach (var icon in normalList)
                {
                    if (!ReferenceEquals(icon.itemData, purchaseItem.ItemData) || !icon.itemData.treasureData.fullIdentified) continue;
                    if (icon.itemData.itemLv >= 3 && !Plugin.SellHighQuality) continue;
                    
                    var sellPrice = icon.GetItemPrice(false);
                
                    if (totalSellPrice + sellPrice > shopMoney) break;
                
                    icon.OnClick();
                    totalSellPrice += sellPrice;
                }
            }
        }

        private static float NetIncome(float realValue, float areaRate, int price)
        {
            return realValue * Plugin.SellRate * areaRate - price;
        }

        public static void RefreshData()
        {
            var gc = GameController.Instance;
            if (gc == null) return;

            var player = gc.worldData.Player();
            
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
            }
        }
        // 卖出商品时
        if (leftOutList is { Count: > 0 })
        {
            foreach (var icon in leftOutList)
            {
                if (icon?.itemData == null || icon.itemData.type != ItemType.Treasure) continue;
                Plugin.PurchaseItems.Remove(new PurchaseItem(icon.itemData, 0));
            }
        }

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(TradeUIController.SureButtonClicked))]
    public static void SureButtonClicked_Postfix(TradeUIController __instance)
    {
        if (__instance == null) return;
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

        var leftText = __instance.leftDiscount.GetComponent<SimpleDetailText>().text;
        var areaText = __instance.areaDiscount.GetComponent<SimpleDetailText>().text;
        var sell = int.Parse(Regex.Match(leftText, @"出售价格(\d+)%").Groups[1].Value);
        var area = int.Parse(Regex.Match(areaText, @"买卖价格x(\d+)%").Groups[1].Value);
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
        var money = ShopMoneyHelper.OpenChanged();
        if (money > 0)
            rightItemList.money = money;
        return true;
    }
}