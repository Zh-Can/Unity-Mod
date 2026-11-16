using System;
using HarmonyLib;
using Il2Cpp;

namespace LYMod.Patches;

// 藏宝阁（ExternalStorage）禁止放入「自创武学秘籍」。
// 游戏判定条件：item.type == Book && BookData.skillID >= GlobalData.CustomSkillIDStart
//（自创武学的 id 正是从 CustomSkillIDStart 开始往后分配的）。
//
// 做法：在判定执行期间把 GlobalData.CustomSkillIDStart 临时抬到 int.MaxValue，
// 让"skillID >= 起始 id"恒为 false；方法结束（含异常）立刻还原。
// 物品自身的 skillID 完全不动，存档里保存的仍是真实 id，
// 不会出现"复制进去以后武学变成别的/空"的问题。
public class ExternalStoragePatches
{
    private static int _savedIdStart = -1;
    private static int _raiseDepth;

    /// <summary>抬高起始 id，让自创秘籍通过检查。可重入：嵌套调用只抬一次、只在最外层还原。</summary>
    private static void Raise()
    {
        try
        {
            if (!Plugin.Instance.CustomSkillIntoShowRoomFlag.Value) return;

            _raiseDepth++;
            if (_savedIdStart >= 0) return;   // 已经抬过了，只记层数

            _savedIdStart = GlobalData.CustomSkillIDStart;
            GlobalData.CustomSkillIDStart = int.MaxValue;
        }
        catch (Exception ex)
        {
            Plugin.LOG.Warning("[LYMod] 藏宝阁自创武学限制处理失败: " + ex.Message);
        }
    }

    /// <summary>把起始 id 还原成游戏原值（只在最外层调用结束时执行）。</summary>
    private static void Restore()
    {
        try
        {
            if (_raiseDepth > 0) _raiseDepth--;
            if (_raiseDepth > 0) return;      // 外层还没返回，继续维持抬高状态
            if (_savedIdStart < 0) return;

            GlobalData.CustomSkillIDStart = _savedIdStart;
            _savedIdStart = -1;
        }
        catch (Exception ex)
        {
            // 还原失败也不能影响主流程，只记日志
            Plugin.LOG.Warning("[LYMod] 还原 CustomSkillIDStart 失败: " + ex.Message);
        }
    }

    // 选中物品后、弹"确认要将这#name#复制到藏宝阁内吗？"之前的那道判定
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlotController), nameof(PlotController.ItemIntoExternalStorageChoosen))]
    public static void PlotController_ItemIntoExternalStorageChoosen_Prefix() => Raise();

    [HarmonyFinalizer]
    [HarmonyPatch(typeof(PlotController), nameof(PlotController.ItemIntoExternalStorageChoosen))]
    public static Exception PlotController_ItemIntoExternalStorageChoosen_Finalizer(Exception __exception)
    {
        Restore();
        return __exception;
    }

    // 真正写入藏宝阁数据时的判定（原版命中只打日志然后 return，不写入）
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameDataController), nameof(GameDataController.ItemIntoExternalStorage))]
    public static void GameDataController_ItemIntoExternalStorage_Prefix() => Raise();

    [HarmonyFinalizer]
    [HarmonyPatch(typeof(GameDataController), nameof(GameDataController.ItemIntoExternalStorage))]
    public static Exception GameDataController_ItemIntoExternalStorage_Finalizer(Exception __exception)
    {
        Restore();
        return __exception;
    }
    
    /// <summary>
    /// 藏宝阁容量
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameDataController), nameof(GameDataController.GetExternalStorageMaxValue))]
    public static void GetExternalStorageMaxValue_Postfix(GameDataController __instance, ref int __result)
    {
        if (__instance == null || !Plugin.Instance.ExternalStorageFlag.Value) return;
        __result = 100000000;
    }
}
