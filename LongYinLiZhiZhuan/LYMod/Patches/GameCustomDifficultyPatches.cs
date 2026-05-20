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

    [HarmonyPostfix]
    [HarmonyPatch(typeof(AreaBuildingData), nameof(AreaBuildingData.GetBuildSpeedRate))]
    public static void GetBuildSpeedRate_Postfix(AreaBuildingData __instance, ref float __result)
    {
        if (!Plugin.Instance.ForceDevelopSpeedFlag.Value) return;
        
        // 获取所属区域
        AreaData area = __instance.GetArea();
        if (area == null)
            throw new NullReferenceException();
    
        // 基础建造速度 = 区域特殊加成(13) + 1.0
        float buildSpeedRate = area.areaSpeAddData.Get(13) + 1.0f;
    
        // 如果区域有所属势力
        if (area.belongForceID >= 0)
        {
            area = __instance.GetArea();
            if (area == null)
            {
                Plugin.LOG.Msg("area == null");
                throw new NullReferenceException();
            }
        
            ForceData force = area.GetForce();
            if (force == null)
            {
                Plugin.LOG.Msg("force == null");
                throw new NullReferenceException();
            }
        
            // 加上势力的建造速度加成(12)
            buildSpeedRate += force.forceSpeAddData.Get(12);
        
            // 应用AI发展速度加成
            float aiDevelopSpeed = GameController.Instance.worldData.GetAIForceDevelopSpeed();
            buildSpeedRate += aiDevelopSpeed * 0.05f;
            
        }
        __result = buildSpeedRate;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ForceData), nameof(ForceData.GetBuildCostRate))]
    public static void GetBuildCostRate_Postfix(ForceData __instance, ref float __result)
    {
        if (!Plugin.Instance.ForceDevelopSpeedFlag.Value) return;
        
        // 应用AI发展速度，减少建造成本
        float aiDevelopSpeed = GameController.Instance.worldData.GetAIForceDevelopSpeed();
        float costRate = aiDevelopSpeed * -0.05f;
    
        // 最小值为0.05，即最大减少95%成本
        __result = Math.Max(0.05f, costRate + 1.0f);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ForceData), nameof(ForceData.GetRealSalaryCost))]
    public static void GetRealSalaryCost_Postfix(ForceData __instance, ref int __result)
    {
        if (!Plugin.Instance.ForceDevelopSpeedFlag.Value) return;
        
        int totalSalary = __instance.totalSalary;
        int totalPopulation = __instance.totalPopulation;
    
        // 基础薪资倍率
        float salaryRate = 1.0f;
    
        // 人口超过100时，每多1人增加1%薪资
        if (totalPopulation > 100)
        {
            salaryRate = (totalPopulation - 100) * 0.01f + 1.0f;
        }
    
        // 应用AI发展速度减少薪资
        float aiDevelopSpeed = GameController.Instance.worldData.GetAIForceDevelopSpeed();
        salaryRate *= 1.0f - aiDevelopSpeed * 0.025f;
        
        // 返回四舍五入后的实际薪资成本
        __result = (int)Math.Round(totalSalary * salaryRate);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ForceData), nameof(ForceData.GetResearchCostRate))]
    public static void GetResearchCostRate_Postfix(ForceData __instance, ref float __result)
    {
        if (!Plugin.Instance.ForceDevelopSpeedFlag.Value) return;
        
        // 获取AI发展速度，减少研究成本
        float aiDevelopSpeed = GameController.Instance.worldData.GetAIForceDevelopSpeed();
        float costRate = aiDevelopSpeed * -0.05f;
        
        // 最小值为0.05，即最大减少95%成本
        __result = Math.Max(0.05f, costRate + 1.0f);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ForceData), nameof(ForceData.GetResearchSpeedRate))]
    public static void GetResearchSpeedRate_Postfix(ForceData __instance, ref float __result)
    {
        if (!Plugin.Instance.ForceDevelopSpeedFlag.Value) return;
        
        // 获取势力的研究速度加成(4)
        float researchSpeedRate = __instance.forceSpeAddData.Get(4);
        
        // 应用AI发展速度加成
        float aiDevelopSpeed = GameController.Instance.worldData.GetAIForceDevelopSpeed();
        float aiBonus = aiDevelopSpeed * 0.05f;
        
        // 总研究速度 = 基础加成 + 1.0 + AI加成
        __result = researchSpeedRate + 1.0f + aiBonus;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ForceData), nameof(ForceData.GetSalaryRate))]
    public static void GetSalaryRate_Postfix(ForceData __instance, ref float __result)
    {
        if (!Plugin.Instance.ForceDevelopSpeedFlag.Value) return;
        
        int totalPopulation = __instance.totalPopulation;
    
        // 基础薪资倍率
        float salaryRate = 1.0f;
    
        // 人口超过100时，每多1人增加1%薪资
        if (totalPopulation > 100)
        {
            salaryRate = ((totalPopulation - 100) * 0.01f) + 1.0f;
        }
    
        // 应用AI发展速度减少薪资倍率
        float aiDevelopSpeed = GameController.Instance.worldData.GetAIForceDevelopSpeed();
        salaryRate *= 1.0f - aiDevelopSpeed * 0.025f;
        
        __result = Math.Clamp(salaryRate, 0, salaryRate);
    }
}