using HarmonyLib;
using Il2Cpp;

namespace LYMod.Patches;
/// <summary>
/// 自定义游戏难度
/// </summary>
public class GameCustomDifficultyPatches
{
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

    public static void GetCustomDifficultyData()
    {
        // 读取自定义难度数据
        if (GameController.Instance == null) return;
        if (GameController.Instance.worldData.customDifficultyData == null) return;
        var dict = GameController.Instance.worldData.customDifficultyData.customDifficultyLv;
        Plugin.ExpRate = GetValue(dict, CustomDifficultyType.expRate);
        Plugin.FameRate = GetValue(dict, CustomDifficultyType.fameRate);
        Plugin.MaxweightRate = GetValue(dict, CustomDifficultyType.maxweightRate);
        Plugin.SelfforceExpRate = GetValue(dict, CustomDifficultyType.selfforceExpRate);
        Plugin.OtherforceExpRate = GetValue(dict, CustomDifficultyType.otherforceExpRate);
        Plugin.RandomEnemyStrength = GetValue(dict, CustomDifficultyType.randomEnemyStrength);
        Plugin.RandomEnemyNum = GetValue(dict, CustomDifficultyType.randomEnemyNum);
        Plugin.BadfameRate = GetValue(dict, CustomDifficultyType.badfameRate);
        Plugin.MaxSkillNum = GetValue(dict, CustomDifficultyType.maxSkillNum);
        Plugin.TeammateLimit = GetValue(dict, CustomDifficultyType.teammateLimit);
        Plugin.AiForceDevelopSpeed = GetValue(dict, CustomDifficultyType.aiForceDevelopSpeed);
        Plugin.TimeDifficulty = GameController.Instance.worldData.TimeDifficulty;
    }
    private static int GetValue(Il2CppSystem.Collections.Generic.Dictionary<int, int> dict, CustomDifficultyType type)
    {
        return dict.TryGetValue((int)type, out var v) ? v : 0;
    }
}