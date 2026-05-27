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
        // Plugin.ExpRate = GetValue(dict, CustomDifficultyType.expRate);
        // Plugin.FameRate = GetValue(dict, CustomDifficultyType.fameRate);
        // Plugin.MaxweightRate = GetValue(dict, CustomDifficultyType.maxweightRate);
        // Plugin.SelfforceExpRate = GetValue(dict, CustomDifficultyType.selfforceExpRate);
        // Plugin.OtherforceExpRate = GetValue(dict, CustomDifficultyType.otherforceExpRate);
        // Plugin.RandomEnemyStrength = GetValue(dict, CustomDifficultyType.randomEnemyStrength);
        // Plugin.RandomEnemyNum = GetValue(dict, CustomDifficultyType.randomEnemyNum);
        // Plugin.BadfameRate = GetValue(dict, CustomDifficultyType.badfameRate);
        // Plugin.MaxSkillNum = GetValue(dict, CustomDifficultyType.maxSkillNum);
        // Plugin.TeammateLimit = GetValue(dict, CustomDifficultyType.teammateLimit);
        // Plugin.AiForceDevelopSpeed = GetValue(dict, CustomDifficultyType.aiForceDevelopSpeed);
        Plugin.ExpRate = GetValue(dict, 0);
        Plugin.FameRate = GetValue(dict, 1);
        Plugin.MaxweightRate = GetValue(dict, 2);
        Plugin.SelfforceExpRate = GetValue(dict, 3);
        Plugin.OtherforceExpRate = GetValue(dict, 4);
        Plugin.RandomEnemyStrength = GetValue(dict, 5);
        Plugin.RandomEnemyNum = GetValue(dict, 6);
        Plugin.BadfameRate = GetValue(dict, 7);
        Plugin.MaxSkillNum = GetValue(dict, 8);
        Plugin.TeammateLimit = GetValue(dict, 9);
        Plugin.AiForceDevelopSpeed = GetValue(dict, 10);
        Plugin.TimeDifficulty = GameController.Instance.worldData.TimeDifficulty;
    }
    private static int GetValue(Il2CppSystem.Collections.Generic.Dictionary<int, int> dict, int type)
    {
        return dict.TryGetValue(type, out var v) ? v : 0;
    }
}