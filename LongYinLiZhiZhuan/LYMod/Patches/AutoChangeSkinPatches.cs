using HarmonyLib;
using Il2Cpp;
using LYMod.Helpers;

namespace LYMod.Patches;

/// <summary>
/// 自动换指定服装
/// </summary>
public class AutoChangeSkinPatches
{

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HeroData), nameof(HeroData.JoinForce))]
    public static void JoinForce(HeroData __instance)
    {
        var flag = HeroHelper.TryReadPlayer(out var player);
        if (__instance == null || Plugin.Instance.SpecifiedSkinId.Value == 99999 || !flag || player.belongForceID == -1 || __instance.belongForceID != player.belongForceID) return;
        
        __instance.setSkinID = Plugin.Instance.SpecifiedSkinId.Value;
        __instance.setSkinLv = __instance.heroForceLv;
        __instance.skinID = Plugin.Instance.SpecifiedSkinId.Value;
        __instance.skinLv = __instance.heroForceLv;
        
    }
    [HarmonyPostfix]
    [HarmonyPatch(typeof(HeroData), nameof(HeroData.SetHeroForceLv))]
    public static void SetHeroForceLv(HeroData __instance, int _forceLv)
    {
        var flag = HeroHelper.TryReadPlayer(out var player);
        if (__instance == null || Plugin.Instance.SpecifiedSkinId.Value == 99999 || !flag || player.belongForceID == -1 || __instance.belongForceID != player.belongForceID) return;
        
        __instance.setSkinID = Plugin.Instance.SpecifiedSkinId.Value;
        __instance.setSkinLv = _forceLv;
        __instance.skinID = Plugin.Instance.SpecifiedSkinId.Value;
        __instance.skinLv = _forceLv;
        
    }
}