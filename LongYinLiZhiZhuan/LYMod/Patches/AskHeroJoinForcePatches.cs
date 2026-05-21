using HarmonyLib;
using Il2Cpp;

namespace LYMod.Patches;

/// <summary>
/// 对话招募人物，提示是否收为徒弟
/// </summary>
public class AskHeroJoinForcePatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlotController), nameof(PlotController.AskHeroJoinForce))]
    public static void AskHeroJoinForce_Postfix(PlotController __instance)
    {
        if (Plugin.Instance.AskHeroJoinForceFlag.Value)
        {
            var flag = false;
            var choices = __instance.nowSinglePlot.choices;
            
            foreach (var c in choices)
            {
                if (c.choiceText == "欢迎之至")
                {
                    c.choiceText = "收为徒弟";
                }
                if (c.choiceText.Contains("唐突")) flag = true;
            }
            if (flag) return; 
            var choice = new SinglePlotChoiceData
            {
                choiceText = "收入门派",
                callParam = __instance.targetInteractHero.heroID.ToString(),
                inited = true
            };
            choices.Insert(1, choice);
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlotInteractController), nameof(PlotInteractController.OnClick))]
    public static bool Prefix(PlotInteractController __instance)
    {
        // 获取选项数据
        var choiceData = __instance.choiceData;
        if (choiceData != null && choiceData.choiceText == "收入门派")
        {
            var id = int.Parse(choiceData.callParam);
            var hero = GameController.Instance.worldData.GetHero(id);
            var player = GameController.Instance.worldData.Player();
            hero.JoinForce(player.belongForceID, hero.heroForceLv, setTeacherToLeader: false);
            PlotController.Instance.HideInteractUI();
            return false;
        }
        return true; 
    }
}