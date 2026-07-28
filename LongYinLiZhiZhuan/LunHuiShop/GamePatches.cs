using HarmonyLib;
using Il2Cpp;
using LunHuiShop.GuiFramework.Logger;

namespace LunHuiShop;

public class GamePatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameDataController), nameof(GameDataController.Start))]
    public static void GameDataController_Awake_Postfix(GameDataController __instance)
    {
        MainView.ShopItems = new List<ShopItem>();
        
        var gdc = GameDataController.Instance;
        foreach (var armor in gdc.armorDataBase.Values)
        {
            Log.Info(armor.Name());
            
        }

        
        

    }
}