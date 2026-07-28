using HarmonyLib;
using Il2Cpp;
using LunHuiShop.GuiFramework.Logger;

namespace LunHuiShop;

public class GamePatches
{
    /// <summary>
    /// GameDataController 初始化完成后填充 ShopItems
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameDataController), nameof(GameDataController.Start))]
    public static void GameDataController_Awake_Postfix(GameDataController __instance)
    {
        var list = new List<ShopItem>(100);
        var i = 0;
        foreach (var armor in __instance.armorDataBase.Values)
        {
            var iconName = armor.GetItemIconName();
            Log.Info($"装备: {armor.Name()}, IconName='{iconName}'");
            list.Add(new ShopItem
            {
                Id = armor.itemID,
                Name = armor.Name(),
                Type = armor.type.ToString(),
                Level = armor.itemLv.ToString(),
                Quality = armor.rareLv.ToString(),
                Price = armor.value,
                Fame = 0,
                IconName = $"1_0_{i++}"
            });
        }
        MainView.ShopItems = list;
    }
}