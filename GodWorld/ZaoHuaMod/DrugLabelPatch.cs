using System;
using System.Reflection;
using HarmonyLib;
using ZaoHuaMod.GuiFramework.Logger;

namespace ZaoHuaMod
{
    /// <summary>
    ///     丹药列表项：在丹药名称后追加耐药性标注
    /// </summary>
    [HarmonyPatch(typeof(CraftingDrugRecipeCell), "SetInfo")]
    public static class CraftingDrugRecipeCellSetInfoPatch
    {
        private static FieldInfo _txtNameField;
        private static PropertyInfo _textProp;

        private static FieldInfo TxtNameField =>
            _txtNameField ?? (_txtNameField = typeof(CraftingDrugRecipeCell)
                .GetField("txtName", BindingFlags.Instance | BindingFlags.Public));

        private static PropertyInfo GetTextProperty(object obj)
        {
            if (_textProp != null) return _textProp;
            return _textProp = obj.GetType().GetProperty("text",
                BindingFlags.Instance | BindingFlags.Public);
        }

        private static void Postfix(CraftingDrugRecipeCell __instance, TbDrugRecipeSto recipeSto,
            bool isShowDeleteButton, bool isShowFollowButton, bool isShowInfo)
        {
            if (!ZaoHuaMod.DrugResistLabelFlag.Value)
                return;

            if (recipeSto == null)
                return;

            var txtName = TxtNameField.GetValue(__instance);
            if (txtName == null)
                return;

            try
            {
                var drugRecipeCfg = Singleton<TbDataImpl>.Instance.GetDrugRecipeCfg(recipeSto.recipeId);
                if (drugRecipeCfg == null)
                    return;

                var itemId = drugRecipeCfg.itemId;
                if (itemId <= 0)
                    return;

                var blendId = new BlendId(BlendEnum.ItemId, itemId);
                var suffix = "";

                if (!Singleton<TbItemImpl>.Instance.IsDrugResistant(blendId))
                {
                    var drugMaxSto = Singleton<TbItemImpl>.Instance.GetDrugMaxSto(blendId);
                    var drugMax = Singleton<TbItemImpl>.Instance.GetDrugMax(blendId);
                    var used = drugMaxSto?.drugUse ?? 0;

                    if (drugMax > 0)
                    {
                        suffix += $" <color=#1943a6>[{used}/{drugMax}]</color>";
                    }
                }

                var prop = GetTextProperty(txtName);
                var currentText = (string)prop.GetValue(txtName, null);
                var baseText = currentText;
                var idx = currentText.IndexOf(" <color=", StringComparison.Ordinal);
                if (idx > 0)
                    baseText = currentText.Substring(0, idx);

                if (string.IsNullOrWhiteSpace(baseText))
                    baseText = drugRecipeCfg.name;

                prop.SetValue(txtName, baseText.Trim() + suffix, null);
            }
            catch (System.Exception ex)
            {
                Log.Warning($"[DrugLabel] RecipeCell error: {ex.Message}");
            }
        }
    }
}
