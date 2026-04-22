using System.Collections;
using HarmonyLib;
using Il2Cpp;
using LYMod.Helpers;
using MelonLoader;

namespace LYMod.Patches;

public class ForceTeachNewSkillPlotPatches
{
    private static bool _isMasterPractice = false;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(BuildingUIController), nameof(BuildingUIController.GenerateBuildingButton))]
    public static void BuildingUIController_GenerateBuildingButton_Postfix(BuildingUIController __instance)
    {
        var flag = HeroHelper.TryReadPlayer(out var player);
        if (!flag || player.belongForceID == -1 || player.GetForce().mainAreaID != player.atAreaID) return;
        
        if (__instance.buildingData is not { buildingID: 2 }) return;

        Plugin.LOG.Msg("[掌门演武] 当前是练武场，开始添加按钮");

        try
        {
            // 使用游戏原有的按钮创建方式
            var choice = new AreaBuildingChoice
            {
                text = "掌门演武",
                describe = "掌门领众弟子演武，亲授新武学招式",
                callFuc = "", // 我们会通过按钮名称识别
                callFucParam = "",
                justNeedOneCondition = true,
                mainCondition = new Il2CppSystem.Collections.Generic.List<string>(),
                subCondition = new Il2CppSystem.Collections.Generic.List<string>()
            };

            Plugin.LOG.Msg("[掌门演武] 创建按钮中...");
            __instance.CreateBuildingButton(choice);

            // 找到刚创建的按钮并设置名称
            var buttonGrid = __instance.buildingButtonGrid;
            if (buttonGrid != null)
            {
                var lastChildIndex = buttonGrid.transform.childCount - 1;
                if (lastChildIndex >= 0)
                {
                    var newButton = buttonGrid.transform.GetChild(lastChildIndex).gameObject;
                    if (newButton != null)
                    {
                        newButton.name = "MasterPracticeButton";
                        Plugin.LOG.Msg("[掌门演武] 按钮创建并命名成功");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.LOG.Msg($"[掌门演武] 添加按钮异常: {ex.Message}");
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(BuildingButtonController), nameof(BuildingButtonController.OnClick))]
    public static bool BuildingButtonController_OnClick_Prefix(BuildingButtonController __instance)
    {
        try
        {
            if (__instance.gameObject.name == "MasterPracticeButton")
            {
                OnMasterPracticeButtonClicked();
                return false;
            }
        }
        catch (Exception ex)
        {
            Plugin.LOG.Msg($"[掌门演武] 按钮点击异常: {ex.Message}");
        }

        return true;
    }

    private static void OnMasterPracticeButtonClicked()
    {
        // 检查玩家是否是掌门
        if (!IsPlayerLeader())
        {
            GameController.Instance?.ShowTextOnMouse("只有掌门才能使用此功能");
            return;
        }

        var plotController = PlotController.Instance;
        if (plotController == null)
        {
            return;
        }

        try
        {
            plotController.ChooseQingMingFestivalPlot();
            _isMasterPractice = true;
        }
        catch (Exception ex)
        {
            Plugin.LOG.Msg($"[掌门演武] ChooseQingMingFestivalPlot 异常: {ex.Message}");
            Plugin.LOG.Msg($"[掌门演武] 异常堆栈: {ex.StackTrace}");
        }
    }

    private static bool IsPlayerLeader()
    {
        try
        {
            var flag = HeroHelper.TryReadPlayer(out var player);
            if (!flag)
            {
                Plugin.LOG.Msg("[掌门演武] 无法获取玩家角色数据");
                return false;
            }

            // 检查是否是掌门（isLeader 字段）
            if (player.isLeader)
            {
                Plugin.LOG.Msg($"[掌门演武] 玩家 {player.heroName} 是掌门");
                return true;
            }

            Plugin.LOG.Msg($"[掌门演武] 玩家 {player.heroName} 不是掌门");
            return false;
        }
        catch (Exception ex)
        {
            Plugin.LOG.Msg($"[掌门演武] 检查掌门身份异常: {ex.Message}");
            return false;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlotController), nameof(PlotController.FinishQingMingFestivalPlot))]
    public static bool PlotController_FinishQingMingFestivalPlot_Prefix(string param)
    {
        if (!_isMasterPractice) return true;

        try
        {
            Plugin.LOG.Msg($"[掌门演武] FinishQingMingFestivalPlot 被调用，参数: {param}");

            var chooseController = ChooseController.Instance;
            if (chooseController == null)
            {
                Plugin.LOG.Msg("[掌门演武] ChooseController 为空");
                return false;
            }

            var chooseResult = chooseController.chooseResult;
            if (chooseResult == null)
            {
                Plugin.LOG.Msg("[掌门演武] chooseResult 为空");
                return false;
            }

            var skillIcon = chooseResult.GetComponent<SkillIconController>();
            if (skillIcon?.skillLvData == null)
            {
                Plugin.LOG.Msg("[掌门演武] 无法获取选择的技能");
                return false;
            }

            MelonCoroutines.Start(OnSkillSelected(skillIcon.skillLvData));
        }
        catch (Exception ex)
        {
            Plugin.LOG.Msg($"[掌门演武] FinishQingMingFestivalPlot 处理异常: {ex.Message}");
            Plugin.LOG.Msg($"[掌门演武] 异常堆栈: {ex.StackTrace}");
        }
        finally
        {
            _isMasterPractice = false;
        }

        return false; // 不执行原方法
    }

    private static IEnumerator OnSkillSelected(KungfuSkillLvData skillData)
    {
        Plugin.LOG.Msg($"[掌门演武] 选择了技能: {skillData.skillID}, 等级: {skillData.lv}");
        PlotController.Instance.HideInteractUI();

        var worldData = GameController.Instance?.worldData;
        if (worldData == null || worldData.Forces == null)
        {
            Plugin.LOG.Msg("[掌门演武] 世界数据为空");
            yield break;
        }

        var playerForceId = GlobalData.PlayerForceID;
        Plugin.LOG.Msg($"[掌门演武] 玩家门派 ID: {playerForceId}");

        ForceData playerForce = null;
        foreach (var force in worldData.Forces)
        {
            if (force.forceID == playerForceId)
            {
                playerForce = force;
                break;
            }
        }

        if (playerForce == null)
        {
            Plugin.LOG.Msg("[掌门演武] 未找到玩家门派数据");
            yield break;
        }

        var disciples = playerForce.FindAllHero(noPlayer:true, noLeader:true);
        if (disciples == null)
        {
            Plugin.LOG.Msg("[掌门演武] 获取弟子列表失败");
            yield break;
        }

        var discipleCount = disciples.Count;
        if (discipleCount == 0)
        {
            Plugin.LOG.Msg("[掌门演武] 没有找到弟子");
            yield break;
        }

        Plugin.LOG.Msg($"[掌门演武] 找到 {discipleCount} 名弟子，开始传授技能");

        for (var i = 0; i < discipleCount; i++)
        {
            var disciple = disciples[i];
            if (disciple == null) continue;

            var existingSkill = disciple.FindSkill(skillData.skillID);
            if (existingSkill == null)
            {
                
                disciple.GetSkill(skillData, true);
                Plugin.LOG.Msg($"[掌门演武] [{i + 1}/{discipleCount}] 传授技能 {skillData.skillID} 给弟子 {disciple.heroName}");
            }
            else
            {
                for (var j = 0; j < 2; j++)
                {
                    disciple.UpgradeSkill(skillData);
                }
                Plugin.LOG.Msg($"[掌门演武] [{i + 1}/{discipleCount}] 弟子 {disciple.heroName} 已掌握技能 {skillData.skillID}");
            }

            yield return null;
        }

        Plugin.LOG.Msg("[掌门演武] 所有弟子技能传授完成");
    }
}
