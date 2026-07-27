using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using ZaoHuaMod.GuiFramework.Localization;
using ZaoHuaMod.GuiFramework.Logger;

namespace ZaoHuaMod
{
    // ============================================================
    // 共享工具方法
    // ============================================================
    internal static class DrugProfitHelper
    {
        private static PropertyInfo _textProp;
        private static FieldInfo _priceField;
        private static bool _priceFieldChecked;

        public static string GetText(object tmp)
        {
            if (tmp == null) return "";
            var prop = _textProp ?? (_textProp = tmp.GetType().GetProperty("text",
                BindingFlags.Instance | BindingFlags.Public));
            if (prop != null)
                return (string)prop.GetValue(tmp, null) ?? "";
            return "";
        }

        public static void SetText(object tmp, string text)
        {
            if (tmp == null) return;
            var prop = _textProp ?? (_textProp = tmp.GetType().GetProperty("text",
                BindingFlags.Instance | BindingFlags.Public));
            prop?.SetValue(tmp, text, null);
        }

        public static BlendId MakeItemBlendId(int itemId)
        {
            return new BlendId(BlendEnum.ItemId, itemId);
        }

        public static TbItemCfg GetItemCfg(int itemId)
        {
            try { return Singleton<TbItemImpl>.Instance.GetItemCfg(MakeItemBlendId(itemId)); }
            catch { return null; }
        }

        public static int GetItemPrice(int itemId)
        {
            var cfg = GetItemCfg(itemId);
            if (cfg == null) return 0;

            if (!_priceFieldChecked)
            {
                _priceFieldChecked = true;
                foreach (var name in new[] { "sellPrice", "price", "buyPrice", "salePrice", "sellPriceValue" })
                {
                    var field = cfg.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public);
                    if (field != null && (field.FieldType == typeof(int) || field.FieldType == typeof(long)))
                    {
                        _priceField = field;
                        break;
                    }
                }
            }

            if (_priceField == null) return 0;
            try { return Convert.ToInt32(_priceField.GetValue(cfg)); }
            catch { return 0; }
        }
    }




    // ============================================================
    // 丹药详情面板 —— 追加售价 [X灵石/颗]
    // ============================================================
    [HarmonyPatch(typeof(CraftingDrugRecipeInfoCell), "SetInfo")]
    public static class DrugProfitInfoCellPatch
    {
        private static FieldInfo _txtField;

        private static FieldInfo TxtField =>
            _txtField ?? (_txtField = typeof(CraftingDrugRecipeInfoCell)
                .GetField("txtRecipeInfo", BindingFlags.Instance | BindingFlags.Public));

        private static void Postfix(CraftingDrugRecipeInfoCell __instance, TbDrugRecipeSto recipeSto,
            bool isShowDeleteButton, bool isShowFollowButton)
        {
            if (!ZaoHuaMod.DrugProfitLabelFlag.Value)
                return;

            if (recipeSto == null) return;
            var txtObj = TxtField.GetValue(__instance);
            if (txtObj == null) return;

            try
            {
                var cfg = Singleton<TbDataImpl>.Instance.GetDrugRecipeCfg(recipeSto.recipeId);
                if (cfg == null || cfg.itemId <= 0) return;

                var price = DrugProfitHelper.GetItemPrice(cfg.itemId);
                if (price <= 0) return;

                var currentText = DrugProfitHelper.GetText(txtObj);
                var append = $"\n<color=#1943a6>【{Loc.Get("丹药售价")}：{price}{Loc.Get("灵石/颗")}】</color>";

                var baseText = currentText;
                var markerIdx = currentText.IndexOf("【丹药售价", StringComparison.Ordinal);
                if (markerIdx > 0)
                    baseText = currentText.Substring(0, markerIdx).TrimEnd('\n', '\r');

                DrugProfitHelper.SetText(txtObj, baseText + append);
            }
            catch (Exception ex)
            {
                Log.Warning($"[DrugProfit] InfoCell error: {ex.Message}");
            }
        }
    }
}
