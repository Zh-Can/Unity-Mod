using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace SmartTrade
{
    [HarmonyPatch]
    public static class UIPatches
    {
        #region 窗口开启屏蔽游戏内操作
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameController), nameof(GameController.Update))]
        public static bool GameController_Update(GameController __instance)
        {
            return Plugin.IsDisableOperate();
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GlobalData), nameof(GlobalData.GetKeyDown))]
        public static bool GlobalData_GetKeyDown(GlobalData __instance)
        {
            return Plugin.IsDisableOperate();
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GlobalData), nameof(GlobalData.GetKey))]
        public static bool GlobalData_GetKey(GlobalData __instance)
        {
            return Plugin.IsDisableOperate();
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GlobalData), nameof(GlobalData.GetKeyUp))]
        public static bool GlobalData_GetKeyUp(GlobalData __instance)
        {
            return Plugin.IsDisableOperate();
        } 
        #endregion
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameController), nameof(GameController.ChangeMonth))]
        public static void GameController_ChangeMonth_Postfix()
        {
            if (!Plugin.Instance.ShopModify) return;
            ShopMoneyHelper.OnMonthChanged();
        }
    }
}
