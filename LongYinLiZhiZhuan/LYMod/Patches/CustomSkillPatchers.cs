using System;
using HarmonyLib;
using Il2Cpp;

namespace LYMod.Patches;

public class CustomSkillPatchers
{
    // 特效条数上限 / 武学点数预算（预算要远大于 特效数 × 单个特效消耗，否则完成时会被预算校验拦住）
    private const int SpeAddNumMaxValue = 999;
    private const int BudgetValue = 999999;

    private static bool _backedUp;
    private static int[] _speOrigin = null!;
    private static int[] _budIntOrigin = null!;
    private static int[] _budExtOrigin = null!;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CustomSkillCreateController), nameof(CustomSkillCreateController.ShowCreatePanel))]
    public static void CustomSkillCreateController_ShowCreatePanel_Prefix(CustomSkillCreateController __instance, int _templateSkillID)
    {
        try
        {
            var maxSpe = CustomSkillCreateController.SpeAddNumMax;
            var budInt = CustomSkillCreateController.EffectBudget;
            var budExt = CustomSkillCreateController.ExternalEffectBudget;
            if (maxSpe == null || budInt == null || budExt == null) return;

            var n = Math.Min(maxSpe.Length, Math.Min(budInt.Length, budExt.Length));
            if (n <= 0) return;

            // 这三个是静态数组，会一直活到进程结束，所以第一次进来先把游戏原始值备份下来，
            // 开关关掉时才能还原，而不是"写进去就再也回不去了"。
            if (!_backedUp)
            {
                _speOrigin = new int[n];
                _budIntOrigin = new int[n];
                _budExtOrigin = new int[n];
                for (var i = 0; i < n; i++)
                {
                    _speOrigin[i] = maxSpe[i];
                    _budIntOrigin[i] = budInt[i];
                    _budExtOrigin[i] = budExt[i];
                }

                _backedUp = true;
            }

            // 前缀在面板初始化之前执行，所以显示和校验读到的都是这里的最终值
            var on = Plugin.Instance.CustomSkillPoint999Flag.Value;
            for (var i = 0; i < n; i++)
            {
                maxSpe[i] = on ? SpeAddNumMaxValue : _speOrigin[i];   // 特效条数
                budInt[i] = on ? BudgetValue : _budIntOrigin[i];     // 内功类点数预算
                budExt[i] = on ? BudgetValue : _budExtOrigin[i];     // 外功类点数预算
            }
        }
        catch (Exception ex)
        {
            Plugin.LOG.Warning("[LYMod] 应用自创武功上限失败: " + ex.Message);
        }
    }

    // 自创武学每个品质限制次数解开（每次调用即时读开关，切换后下一次自创就生效）
    [HarmonyPrefix, HarmonyPatch(typeof(WorldData), nameof(WorldData.HavePlayerCustomSkill))]
    public static bool WorldData_HavePlayerCustomSkill_Prefix(ref bool __result, int rareLv)
    {
        if (!Plugin.Instance.CustomSkillNumLimitFlag.Value) return true;
        __result = false; return false;
    }
}
