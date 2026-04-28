using HarmonyLib;
using Il2Cpp;

namespace LYMod.Patches;
/// <summary>
/// 自定义游戏难度、
/// 仅针对V1.0.1f3的存档
/// </summary>
public class GameCustomDifficultyPatches
{

    // public static string[] 
    
    /// <summary>
    /// 不管什么难度都返回可以解锁成就
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CustomDifficultyData), nameof(CustomDifficultyData.CanUnlockAchievement))]
    public static void CanUnlockAchievement_Postfix(ref bool __result)
    {
        if (!Plugin.Instance.AnyDifficultUnlockAchFlag.Value) return;
        __result = true;
    }
    
}