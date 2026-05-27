using System.Collections;
using System.Text;
using UnityEngine;
using Object = Il2CppSystem.Object;
using HarmonyLib;
using Il2Cpp;
using LYMod.Helpers;
using MelonLoader;
using UnityEngine.UI;

namespace LYMod;


public class GameDataControllerPatches
{
   
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

    [HarmonyPrefix]
    [HarmonyPatch(typeof(BattleController), nameof(BattleController.StartBattleButtonClicked))]
    public static void StartBattleButtonClicked_Prefix(BattleController __instance)
    {
        if (__instance == null || !Plugin.Instance.BattleMaxTime999Flag.Value) return;
        
        var newList = new Il2CppSystem.Collections.Generic.List<float>();
        newList.Add(999);
        newList.Add(999);
        newList.Add(999);
        BattleController.BattleMaxTime = newList;
    }
}

public class DrinkUIControllerPatches
{
    /// <summary>
    ///  喝酒一回合胜利
    /// </summary>
    /// <param name="__instance"></param>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(DrinkUIController), nameof(DrinkUIController.NextButtonClicked))]
    public static void DrinkUIController_NextButtonClicked_Prefix(DrinkUIController __instance)
    {
        if (__instance == null || !Plugin.Instance.DrinkOneWinFlag.Value) return;
        __instance.enemyLose = true;
        __instance.playerLose = false;
    }
    /// <summary>
    /// 喝酒自动倒十成
    /// </summary>
    /// <param name="__instance"></param>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(DrinkUIController), nameof(DrinkUIController.FixedUpdate))]
    public static void DrinkUIController_FixedUpdate_Postfix(DrinkUIController __instance)
    {
        if (__instance ==null || !Plugin.Instance.DrinkUiAutoFillFlag.Value) return;
        __instance.SetEnemyFillAmount(1);
    }

}

/// <summary>
/// 轻功训练不受击
/// </summary>
public class StudyDodgePlayerPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(StudyDodgePlayer), nameof(StudyDodgePlayer.OnHit))]
    public static bool StudyDodgePlayer_OnHit_Prefix(StudyDodgePlayer __instance)
    {
        return !Plugin.Instance.DodgeHitFlag.Value;
    }
}


public class GameControllerPatches
{
    #region 时间冻结

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameController), nameof(GameController.ChangeDay), new Type[0])]
    public static bool GameController_ChangeDay_Prefix()
    {
        // if (!Plugin.Instance.TimeFreezeFlag.Value) return true;
        // var wd = GameController.Instance.worldData;
        // wd.worldTime.day -= 1;
        // return true;
        return !Plugin.Instance.TimeFreezeFlag.Value;
    }
    
    #endregion
    /// <summary>
    /// 队友自动离队时间
    /// </summary>
    /// <param name="teamLeader"></param>
    /// <param name="teamMate"></param>
    /// <param name="autoLeaveDay"></param>
    /// <returns></returns>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameController), nameof(GameController.HeroJoinTeam))]
    public static bool GameController_HeroJoinTeam_Prefix(HeroData teamLeader, HeroData teamMate, ref int autoLeaveDay)
    {
        if (Plugin.Instance.TeammateLeaveDay.Value == 30 || ModConfig.HaveNpcMod) return true;
        autoLeaveDay = Plugin.Instance.TeammateLeaveDay.Value;
        return true;
    }
}

public class PoisonPatches
{
    private static readonly List<ItemData> EquipItemData = new();
    
    //给装备附毒时间
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CraftPoisonUIController), nameof(CraftPoisonUIController.GetCostTime))]
    public static void CraftPoisonUIController_GetCostTime_Postfix(CraftPoisonUIController __instance, ref int __result)
    {
        if (__instance == null || !Plugin.Instance.PoisonTime1Flag.Value) return;
        __result = 1;
    }
    //引毒/炼蛊时间
    [HarmonyPostfix]
    [HarmonyPatch(typeof(SpePoisonController), nameof(SpePoisonController.GetCostTime))]
    public static void SpePoisonController_GetCostTime_Postfix(SpePoisonController __instance, ref int __result)
    {
        if (__instance == null || !Plugin.Instance.PoisonTime1Flag.Value) return;
        __result = 1;
    }
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CraftPoisonUIController), nameof(CraftPoisonUIController.GetChangePoisonNum))]
    public static void GetChangePoisonNum_Postfix(ref float __result)
    {
        if (Plugin.Instance.PoisonRate.Value > 1)
        {
            __result *= Plugin.Instance.PoisonRate.Value;
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(BattleController), nameof(BattleController.StartBattleButtonClicked))]
    public static void StartBattleButtonClicked_Prefix(BattleController __instance)
    {
        if (__instance == null || !Plugin.Instance.PoisonNumReduceFlag.Value) return;
        EquipItemData.Clear();
        var flag = HeroHelper.TryReadPlayer(out var player);
        if (!flag || player.itemListData?.allItem == null) return;
        var items = player.itemListData.allItem;
        if (items.Count == 0) return;
        foreach (var item in items)
        {
            if (item == null) continue;
            if (item.Equiped() && item.equipmentData?.equipPoisonData is { poisonNum: > 0 })
            {
                EquipItemData.Add(item);
            }
        }
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(BattleController), nameof(BattleController.BattleRealEnd))]
    public static void BattleRealEnd_Postfix(BattleController __instance)
    {
        if (__instance == null || !Plugin.Instance.PoisonNumReduceFlag.Value) return;
        var flag = HeroHelper.TryReadPlayer(out var player);
        if (!flag || player.itemListData?.allItem == null) return;
        var items = player.itemListData.allItem;
        if (items.Count == 0) return;
        foreach (var item in items)
        {
            if (item == null || !item.Equiped() || item.equipmentData == null) continue;
            foreach (var oldItem in EquipItemData.Where(oldItem => oldItem != null && oldItem.name == item.name))
            {
                if (oldItem.equipmentData?.equipPoisonData != null)
                {
                    item.equipmentData.equipPoisonData = oldItem.equipmentData.equipPoisonData;
                }
            }
        }
    }
}

public class MeditationDataPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(MeditationData), nameof(MeditationData.ChangeExp))]
    public static bool MeditationData_ChangeExp_Prefix(MeditationData __instance, ref float _exp, bool showInfo)
    {
        if (__instance != null && Plugin.Instance.ChanDaoRate.Value > 1)
        {
            _exp *= Plugin.Instance.ChanDaoRate.Value;
        }

        return true;
    }
}

public class ChooseControllerPatches
{
    // 金龙生刷新购买情报
    [HarmonyPostfix]
    [HarmonyPatch(typeof(HeroIconController), nameof(HeroIconController.OnClick))]
    public static void HeroIconController_OnClick_Postfix(HeroIconController __instance)
    {
        if (__instance != null && Plugin.Instance.Interaction.Value)
        {
            var hero = __instance.heroData;
            if (hero != null)
            {
                GameController.Instance.worldData.monthBuyAreaInfoTime = 0;
            }
        }
    }
    // 无限交互判断
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlotController), nameof(PlotController.CheckMeetRequire))]
    public static void PlotController_CheckMeetRequire_Postfix(PlotController __instance,
        ChoiceRequirementType requireType, float requireNum, bool includeTeamMate = true)
    {
        if (__instance != null && Plugin.Instance.Interaction.Value)
        {
            var hero = __instance.targetInteractHero;
            if (hero != null)
            {
                hero.playerInteractionTimeData.ResetTime();
            }
        }
    }

    #region 任意传授  此处代码由 3DM：SaintCirno9 大佬提供

    /// <summary>
    /// 从当前技能选择面板读取玩家刚点中的武功。
    /// </summary>
    private static bool TryGetSelectedSkill(out KungfuSkillLvData selectedSkill)
    {
        selectedSkill = null!;

        var chooseController = ChooseController.Instance;
        if (chooseController == null) return false;

        var chooseResult = chooseController.chooseResult;
        if (chooseResult == null) return false;

        var skillIcon = chooseResult.GetComponent<SkillIconController>();
        if (skillIcon?.skillLvData == null) return false;

        selectedSkill = skillIcon.skillLvData;
        return true;
    }

    /// <summary>
    /// 只在原版会拦住的“额外技能”上接管点击，并直接调用原版 Sure。
    /// </summary>
    private static bool HandleTeachNewSkillChoosen(PlotController plotController, bool forceTeach)
    {
        if (plotController == null || !Plugin.Instance.TeachAnyNewSkill.Value) return true;

        var targetInteractHero = plotController.targetInteractHero;
        if (targetInteractHero == null) return true;

        if (!TryGetSelectedSkill(out var selectedSkill)) return true;

        var skillData = selectedSkill.DataBase();
        if (skillData == null) return true;

        // 原版允许的技能继续交给游戏自己处理。
        if (targetInteractHero.FindSkill(selectedSkill.skillID) != null) return true;
        if (skillData.rareLv != 5 && skillData.rareLv <= targetInteractHero.heroForceLv) return true;

        ChooseController.Instance?.UnshowChoosePanel();
        var skillIdParam = selectedSkill.skillID.ToString();
        if (forceTeach)
            plotController.ForceTeachNewSkillToNPCSure(skillIdParam);
        else
            plotController.TeachNewSkillToNPCSure(skillIdParam);

        return false;
    }

    /// <summary>
    /// 仅对面板补出来的额外技能接管点击
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlotController), nameof(PlotController.TeachNewSkillToNPCChoosen))]
    public static bool PlotController_TeachNewSkillToNPCChoosen_Prefix(PlotController __instance)
    {
        return HandleTeachNewSkillChoosen(__instance, false);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlotController), nameof(PlotController.ForceTeachNewSkillToNPCChoosen))]
    public static bool PlotController_ForceTeachNewSkillToNPCChoosen_Prefix(PlotController __instance)
    {
        return HandleTeachNewSkillChoosen(__instance, true);
    }
    
    /// <summary>
    /// 只补技能选择面板，让原版没有列出的武功也能出现在列表里。
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ChooseController), nameof(ChooseController.ShowChoosePanel),
        typeof(ChooseType),
        typeof(Il2CppSystem.Collections.Generic.List<Object>),
        typeof(GameObject), typeof(string), typeof(string),
        typeof(ChooseFilterType), typeof(HeroData), typeof(string))]
    public static void ChooseController_ShowChoosePanel_Postfix(
        ChooseController __instance, ChooseType _chooseType, Il2CppSystem.Collections.Generic.List<Object> param,
        GameObject _sendResultFucTarget, string _sendResultFuc, string _sendResultParam, ChooseFilterType _filterType,
        HeroData? targetFavorHero, string _cancelFuc)
    {
        if (_sendResultFuc == "SpeRemoveSkillChoosen" && Plugin.Instance.RemoveAnySkill.Value)
        {
            var flag = HeroHelper.TryReadPlayer(out var player);
            if (!flag && player.kungfuSkills == null) return;

            var content = __instance.choosePanel?.transform?.Find("ChoosePanelRoot/ChooseItemList/Viewport/Content");
            if (content == null) return;

            var newObj = __instance.newObj;
            if (newObj == null)
            {
                newObj = GameObjectController.Instance?.skillIconPrefab;
            }
            if (newObj == null) return;

            var existingSkillIds = new HashSet<int>();
            for (var i = 0; i < content.childCount; i++)
            {
                var child = content.GetChild(i);
                if (child != null && child.gameObject != null && child.gameObject.activeSelf)
                {
                    var skillIcon = child.GetComponent<SkillIconController>();
                    if (skillIcon?.skillLvData != null) existingSkillIds.Add(skillIcon.skillLvData.skillID);
                }
            }

            foreach (var skill in player.kungfuSkills)
            {
                if (skill == null || existingSkillIds.Contains(skill.skillID)) continue;

                var skillData = skill.DataBase();
                if (skillData == null) continue;

                var newSkillObj = UnityEngine.Object.Instantiate(newObj.gameObject, content);
                if (newSkillObj == null) continue;

                newSkillObj.SetActive(true);
                var newSkillIcon = newSkillObj.GetComponent<SkillIconController>();
                if (newSkillIcon != null)
                {
                    newSkillIcon.skillLvData = skill;
                    newSkillIcon.skillListID = skill.skillID;
                    newSkillIcon.skillIconType = SkillIconType.Choose;
                }
            }
        }
        
        if (_filterType is ChooseFilterType.ForceTeachNpcNewSkill or ChooseFilterType.TeachNpcNewSkill && Plugin.Instance.TeachAnyNewSkill.Value)
        {
            var flag = HeroHelper.TryReadPlayer(out var player);
            if (!flag || player.kungfuSkills == null) return;
            // 优先使用当前选择流程传入的目标，避免静态缓存串到别的 NPC。
            var currentTargetHero = targetFavorHero ?? PlotController.Instance?.targetInteractHero;
            
            var content = __instance.choosePanel?.transform?.Find("ChoosePanelRoot/ChooseItemList/Viewport/Content");
            if (content == null) return;
            
            var newObj = __instance.newObj;
            if (newObj == null)
            {
                newObj = GameObjectController.Instance?.skillIconPrefab;
            }
            if (newObj == null) return;
            
            var existingSkillIds = new System.Collections.Generic.HashSet<int>();
            for (int i = 0; i < content.childCount; i++)
            {
                var child = content.GetChild(i);
                if (child != null && child.gameObject != null && child.gameObject.activeSelf)
                {
                    var skillIcon = child.GetComponent<SkillIconController>();
                    if (skillIcon?.skillLvData != null)
                    {
                        existingSkillIds.Add(skillIcon.skillLvData.skillID);
                    }
                }
            }
            
            // 使用targetHero获取NPC已有技能列表，但需要做null检查和IL2CPP对象有效性检查
            var npcExistingSkillIds = new System.Collections.Generic.HashSet<int>();
            if (currentTargetHero is { kungfuSkills: not null })
            {
                foreach (var skill in currentTargetHero.kungfuSkills)
                {
                    if (skill != null)
                    {
                       npcExistingSkillIds.Add(skill.skillID);
                    }
                }
            }
            
            foreach (var skill in player.kungfuSkills)
            {
                // 这里不再限制 rareLv，真正做到“任意等级技能”都能补进列表。
                if (skill == null || existingSkillIds.Contains(skill.skillID)) continue;
                if (npcExistingSkillIds.Contains(skill.skillID)) continue;
            
                var skillData = skill.DataBase();
                if (skillData == null) continue;
            
                var newSkillObj = UnityEngine.Object.Instantiate(newObj.gameObject, content);
                if (newSkillObj == null) continue;
            
                newSkillObj.SetActive(true);
                var newSkillIcon = newSkillObj.GetComponent<SkillIconController>();
                if (newSkillIcon != null)
                {
                    newSkillIcon.skillLvData = skill;
                    newSkillIcon.skillListID = skill.skillID;
                    newSkillIcon.skillIconType = SkillIconType.Choose;
                }
            }
        }

        // 传秘籍
        if (_filterType == ChooseFilterType.ForceTeachNpcNewBook && Plugin.Instance.TeachAnyNewSkill.Value)
        {
            var flag = HeroHelper.TryReadPlayer(out var player);
            var content = __instance.choosePanel?.transform?.Find("ChoosePanelRoot/ChooseItemList/Viewport/Content");
            var newObj = __instance.newObj;
            if (newObj == null)
            {
                newObj = GameObjectController.Instance?.itemIconPrefab;
            }
            if (!flag || content == null || newObj == null) return;
            
            var existingSkillIds = new System.Collections.Generic.HashSet<int>();
            for (int i = 0; i < content.childCount; i++)
            {
                var child = content.GetChild(i);
                if (child != null && child.gameObject != null && child.gameObject.activeSelf)
                {
                    var skillIcon = child.GetComponent<SkillIconController>();
                    if (skillIcon?.skillLvData != null)
                    {
                        existingSkillIds.Add(skillIcon.skillLvData.skillID);
                    }
                }
            }
            
            // 使用targetHero获取NPC已有技能列表，但需要做null检查和IL2CPP对象有效性检查
            var npcExistingSkillIds = new System.Collections.Generic.HashSet<int>();
            var currentTargetHero = targetFavorHero ?? PlotController.Instance?.targetInteractHero;
            if (currentTargetHero is { kungfuSkills: not null })
            {
                foreach (var skill in currentTargetHero.kungfuSkills)
                {
                    if (skill != null)
                    {
                        npcExistingSkillIds.Add(skill.skillID);
                    }
                }
            }
            
            HashSet<ItemData> allBookSet = new HashSet<ItemData>();
            // 背包
            var bookList = player.itemListData.itemTypeList[(int)ItemType.Book];
            // 个人仓库
            var bookList1 = player.selfStorage.itemTypeList[(int)ItemType.Book];
            // 藏经阁
            var bookList2 = new Il2CppSystem.Collections.Generic.List<ItemData>();
            if (player.belongForceID != -1)
            {
                bookList2 = player.GetForce().bookStorage.allItem;
            }
            // 存入set中
            foreach (var item in bookList) allBookSet.Add(item);
            foreach (var item in bookList1) allBookSet.Add(item);
            foreach (var item in bookList2) allBookSet.Add(item);
            
            foreach (var item in allBookSet)
            {
                // 这里不再限制 rareLv，真正做到“任意等级技能”都能补进列表。
                if (item == null || existingSkillIds.Contains(item.bookData.skillID)) continue;
                if (npcExistingSkillIds.Contains(item.bookData.skillID)) continue;
            
                var newSkillObj = UnityEngine.Object.Instantiate(newObj.gameObject, content);
                if (newSkillObj == null) continue;
            
                newSkillObj.SetActive(true);
                var newItemIcon = newSkillObj.GetComponent<ItemIconController>();
                if (newItemIcon != null)
                {
                    newItemIcon.itemIconType = ItemIconType.Choose;
                    newItemIcon.itemData = item;
                }
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlotController), nameof(PlotController.ChangePlot), typeof(string))]
    public static void ChangePlot_Postfix(PlotController __instance, string plotID)
    {
        var f = HeroHelper.TryReadPlayer(out var player);
        if (__instance == null || plotID != "0" || !Plugin.Instance.TeachAnyNewSkill.Value || !f 
            || (player.belongForceID != -1 && __instance.targetInteractHero.belongForceID == player.belongForceID) 
            || ModConfig.HaveNpcMod
            ) return;
        
        
        Il2CppSystem.Collections.Generic.List<SinglePlotChoiceData> list = __instance.nowSinglePlot.choices;
        var flag = false;
        foreach (var spcd in list)
        {
            if (spcd.callFuc == "ForceTeachNpcNewBook" || spcd.choiceText == "传授秘籍")
            {
                flag = true;
            }
        }
        if (flag) return;
        
        var singlePlotChoiceData = new SinglePlotChoiceData
        {
            choiceText = "传授秘籍",
            callFuc = "ForceTeachNewBookToNPC",
            requirements = new Il2CppSystem.Collections.Generic.List<PlotChoiceRequirement>(),
            relations = new Il2CppSystem.Collections.Generic.List<RelationRequirementType>(),
            costResource = new Il2CppSystem.Collections.Generic.List<ResourceData>()
        };
        singlePlotChoiceData.requirements.Add(new PlotChoiceRequirement(ChoiceRequirementType.FavorDegree, 60f));
        list.Insert(list.Count - 2, singlePlotChoiceData);
        __instance.nowSinglePlot.choices = list;
    }
    
    // [HarmonyPrefix]
    // [HarmonyPatch(typeof(PlotInteractController), nameof(PlotInteractController.OnClick))]
    // public static void Prefix(PlotInteractController __instance)
    // {
    //     // 获取选项数据
    //     var choiceData = __instance.choiceData;
    //     if (choiceData != null)
    //     {
    //         string callFunc = choiceData.callFuc;  // 调用的函数名
    //         string callParam = choiceData.callParam; // 参数
    //         
    //         Plugin.LOG.Msg($"{callFunc}-{callParam}");
    //     }
    // }
    
   
    #endregion
    
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(BuildingUIController), nameof(BuildingUIController.GetSpeRemoveSkillCost))]
    public static void GetSpeRemoveSkillCost_Postfix(BuildingUIController __instance, ref int __result)
    {
        if (__instance != null && Plugin.Instance.RemoveAnySkill.Value)
        {
            __result = 1;
        }
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(HeroTagDataBase), nameof(HeroTagDataBase.GetCostTime))]
    public static void GetCostTime_Postfix(HeroTagDataBase __instance, ref int __result)
    {
        if (__instance != null && Plugin.Instance.RemoveAnySkill.Value)
        {
            __result = 1;
        }
    }
}

public class IdentifyMatchControllerPatches
{
    private static int _roundVersion;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(IdentifyMatchController), nameof(IdentifyMatchController.StartNewRound))]
    public static void IdentifyMatchController_StartNewRound_Postfix(
        IdentifyMatchController __instance,
        float waitTime)
    {
        if (!Plugin.Instance.AutoJianBaoFlag.Value || __instance == null)
            return;

        var myVersion = ++_roundVersion;

        MelonCoroutines.Start(WaitAndSelect(__instance, myVersion));
    }

    private static IEnumerator WaitAndSelect(
        IdentifyMatchController controller,
        int version)
    {
        if (controller == null)
            yield break;

        // 等待进入可选择状态
        while (controller != null &&
               version == _roundVersion &&
               controller.identifyMatchState != IdentifyMatchState.Choose)
            yield return null;

        if (controller == null || version != _roundVersion)
            yield break;

        // 等两帧，让UI和内部状态同步
        yield return null;
        yield return null;

        if (controller == null || version != _roundVersion)
            yield break;

        AutoSelectHighestValueTreasure(controller);

        if (controller == null || version != _roundVersion)
            yield break;

        // 再等一帧，确保 SetNowChooseTreasure 生效
        yield return null;

        if (controller == null ||
            version != _roundVersion ||
            controller.nowChooseTreasure == null)
            yield break;

        try
        {
            controller.SureButtonClicked();
        }
        catch (Exception ex)
        {
            Plugin.LOG.Error($"AutoJianBao Error: {ex}");
        }
    }

    private static void AutoSelectHighestValueTreasure(IdentifyMatchController controller)
    {
        if (controller?.identifyMatchUIPanel == null)
            return;

        var icons = controller.identifyMatchUIPanel
            .GetComponentsInChildren<ItemIconController>();

        if (icons == null || icons.Length == 0)
            return;

        ItemIconController best = null;
        var maxValue = float.MinValue;

        foreach (var icon in icons)
        {
            if (icon?.itemData == null)
                continue;

            float value = icon.itemData.GetTreasureRealValue();

            if (value > maxValue)
            {
                maxValue = value;
                best = icon;
            }
        }

        if (best != null) controller.SetNowChooseTreasure(best.gameObject);
    }
}



public class LivingSkillPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(HeroData), nameof(HeroData.ChangeLivingSkillExp))]
    public static bool HeroData_ChangeLivingSkillExp_Prefix(HeroData __instance, int id,
        ref float num, bool showText)
    {
        if (__instance != null && num > 0 && Plugin.Instance.LivingSkillExpRate.Value > 1)
        {
            num *= Plugin.Instance.LivingSkillExpRate.Value;
        }
        return true;
    }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(HeroData), nameof(HeroData.ChangeMaxLivingSkill))]
    public static bool HeroData_ChangeMaxLivingSkill_Prefix(HeroData __instance, int id, 
        ref int num, bool showInfo)
    {
        if (__instance != null && num > 0 && Plugin.Instance.MaxLivingSkillExpTimes.Value > 1)
        {
            num *= Plugin.Instance.MaxLivingSkillExpTimes.Value;
        }
        return true;
    }
}


/// <summary>
/// 点天赋无前置要求
/// </summary>
public class ManageTagControllerPatches
{
    public static Il2CppSystem.Collections.Generic.List<HeroTagDataBase> OriginalHeroTagDataBases = new();
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ManageTagController), nameof(ManageTagController.ShowManageTagUI))]
    public static void ManageTagController_ShowManageTagUI_Prefix(ManageTagController __instance, 
        HeroData _targetHero, bool _useMoney)
    {
        // 优先遗忘，如果已开启无前置则关闭
        if (Plugin.Instance.FastRemoveTag.Value && Plugin.Instance.AnyTagFlag.Value)
        {
            Plugin.Instance.AnyTagFlag.Value = false;
        } 
        
        if (Plugin.Instance.AnyTagFlag.Value)
        {
            var list = GameDataController.Instance.heroTagDataBase;
            for (var i = 0; i < list.Count; i++)
            {
                list[i].replaceTag = new Il2CppSystem.Collections.Generic.List<string>();
                list[i].sameMeaning = "";
                list[i].oppositeMeaning = "";
            }
        }
        else
        {
            if (OriginalHeroTagDataBases.Count == 0) return;
            GameDataController.Instance.heroTagDataBase = OriginalHeroTagDataBases;
        }
    }
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ManageTagController), nameof(ManageTagController.CheckMeetCondition))]
    public static void ManageTagController_CheckMeetCondition_Postfix(ManageTagController __instance,
        HeroData checkHero, HeroTagDataBase targetTag, ref bool __result)
    {
        if (__instance != null && Plugin.Instance.AnyTagFlag.Value)
        {
            targetTag.replaceTag = new Il2CppSystem.Collections.Generic.List<string>();
            targetTag.oppositeMeaning = "";
            targetTag.sameMeaning = "";
            __result = true;
        }
    }
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ManageTagController), nameof(ManageTagController.CheckMeetOneCondition))]
    public static void ManageTagController_CheckMeetOneCondition_Postfix(ManageTagController __instance,
        HeroData checkHero, string requirement, ref bool __result)
    {
        if (__instance != null && Plugin.Instance.AnyTagFlag.Value)
        {
            __result = true;
        }
    }
}

public class AreaBuildingDataPatches
{
    #region 门派建筑效果倍数

    // 加成
    [HarmonyPostfix]
    [HarmonyPatch(typeof(AreaBuildingData), nameof(AreaBuildingData.GetBuildingSpeAddData))]
    public static void GetBuildingSpeAddData_Postfix(AreaBuildingData __instance, ref ForceSpeAddData __result)
    {
        if (__instance == null) return;
        var db = __instance.DataBase();
        if (db == null) return;
        
        var area = __instance.GetArea();
        var flag = HeroHelper.TryReadPlayer(out var player);
        if (area == null || !flag || area.belongForceID != player.belongForceID) return;

        if (!UIBuilderExtensions.BuildingTimesMap.TryGetValue(__instance.buildingID, out int times) || times == 1) return;
        var dict = db.GetBuildingSpeAddData(__instance.lv).forceSpeAddData;
        foreach (var ky in dict)
        {
            __result.Set(ky.Key, ky.Value * times);
        }
    }
    // 每月收入
    [HarmonyPostfix]
    [HarmonyPatch(typeof(AreaBuildingData), nameof(AreaBuildingData.GetTotalChangeResource))]
    public static void GetTotalChangeResource_Postfix(AreaBuildingData __instance, 
        Il2CppSystem.Collections.Generic.List<float> __result)
    {
        if (__instance == null) return;
        var area = __instance.GetArea();
        var flag = HeroHelper.TryReadPlayer(out var player);
        if (!flag || area.belongForceID != player.belongForceID) return;
        if (!UIBuilderExtensions.BuildingTimesMap.TryGetValue(__instance.buildingID, out var times) || times == 1) return;
        for (var i = 0; i < __result.Count; i++)
        {
            if (__result[i] > 0) __result[i] *= times;
        }
    }
    // 人口
    [HarmonyPostfix]
    [HarmonyPatch(typeof(AreaBuildingData), nameof(AreaBuildingData.GetChangeMaxPeople))]
    public static void GetChangeMaxPeople_Postfix(AreaBuildingData __instance, ref float __result)
    {
        if (__instance == null) return;
        var area = __instance.GetArea();
        var flag = HeroHelper.TryReadPlayer(out var player);
        if (!flag || area.belongForceID != player.belongForceID) return;
        if (!UIBuilderExtensions.BuildingTimesMap.TryGetValue(__instance.buildingID, out var times) || times == 1) return;
        if (__result > 0) __result *= times;
    }
    // 安全度，支持度，防御，人口
    [HarmonyPostfix]
    [HarmonyPatch(typeof(AreaBuildingData), nameof(AreaBuildingData.GetChangeAreaState))]
    public static void GetChangeAreaState_Postfix(AreaBuildingData __instance, AreaStateType areaStateType, ref float __result)
    {
        if (__instance == null) return;
        var area = __instance.GetArea();
        var flag = HeroHelper.TryReadPlayer(out var player);
        if (!flag || area.belongForceID != player.belongForceID) return;
        if (!UIBuilderExtensions.BuildingTimesMap.TryGetValue(__instance.buildingID, out var times) || times == 1) return;
        if (__result > 0) __result *= times;
    }
    
    #endregion
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameDataController), nameof(GameDataController.GameDataIntoGame))]
    public static bool GameDataController_GameDataIntoGame_Prefix(GameDataController __instance)
    {
        if (__instance == null || !Plugin.Instance.UpgradeDay1.Value) return true;
        var list = __instance.buildingDataBase;
        foreach (var b in list)
        {
            b.buildCostTime = 1;
        }
        return true;
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(AreaRoadData), nameof(AreaRoadData.GetUpgradeTime))]
    public static void AreaRoadData_GetUpgradeTime_Postfix(AreaRoadData? __instance, ref int __result)
    {
        if (__instance == null || !Plugin.Instance.UpgradeDay1.Value) return;
        __result = 1;
    }
    [HarmonyPostfix]
    [HarmonyPatch(typeof(AreaBuildingData), nameof(AreaBuildingData.GetUpgradeTime))]
    public static void AreaBuildingData_GetUpgradeTime_Postfix(AreaBuildingData? __instance, ref int __result)
    {
        if (__instance == null || !Plugin.Instance.UpgradeDay1.Value) return;
        __result = 1;
    }
    [HarmonyPostfix]
    [HarmonyPatch(typeof(AreaBuildingData), nameof(AreaBuildingData.GetMoveTime))]
    public static void AreaBuildingData_GetMoveTime_Postfix(AreaBuildingData? __instance, ref int __result)
    {
        if (__instance == null || !Plugin.Instance.UpgradeDay1.Value) return;
        __result = 1;
    }
    [HarmonyPostfix]
    [HarmonyPatch(typeof(AreaBuildingData), nameof(AreaBuildingData.GetDestroyTime))]
    public static void AreaBuildingData_GetDestroyTime_Postfix(AreaBuildingData? __instance, ref int __result)
    {
        if (__instance == null || !Plugin.Instance.UpgradeDay1.Value) return;
        __result = 1;
    }
    [HarmonyPostfix]
    [HarmonyPatch(typeof(AreaBuildController), nameof(AreaBuildController.GetMaxSpeBuildingNum))]
    public static void AreaBuildController_GetMaxSpeBuildingNum_Postfix(AreaBuildController __instance, ref int __result)
    {
       if (__instance == null || Plugin.Instance.MaxSpeBuildingNum.Value == 5) return;
        __result = Plugin.Instance.MaxSpeBuildingNum.Value;
    }
    
    /// <summary>
    /// 添加特殊建筑
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(AreaBuildController), nameof(AreaBuildController.ShowBuildNewPanel))]
    public static void AreaBuildController_ShowBuildNewPanel_Postfix(AreaBuildController __instance, bool show)
    {
        if (__instance == null || !show || !Plugin.Instance.AddSpeBuildingsFlag.Value) return;
        var buildingIDsToAdd = new List<int> { 10,11,12,13,14,16,17,18,21,22,23,24,25,26,42,43,44,45,46,47,48,49,50,51,52,74,75 };
        foreach (var buildingID in buildingIDsToAdd)
        {
            __instance.GenerateBuildNewButton(buildingID);
        }
    }

    #region 建筑可拆除开关

    // 检查建筑ID是否在排除列表中
    private static List<int> excludeIds = new(){0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,19,20};
    
    /// <summary>
    /// 建筑可拆除开关
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(AreaBuildController), nameof(AreaBuildController.ShowBuildChoiceGrid))]
    public static void AreaBuildController_ShowBuildChoiceGrid_Postfix(
        AreaBuildController __instance,
        bool show
    )
    {
        
        if (!Plugin.Instance.BuildingDestroyFlag.Value || !show || __instance?.buildChoiceGrid == null)
            return;
        MelonCoroutines.Start(AddDestroyNextFrame(__instance));
    }

    private static IEnumerator AddDestroyNextFrame(AreaBuildController controller)
    {
        yield return null;

        if (controller == null ||
            controller.buildChoiceGrid == null ||
            !controller.buildChoiceGrid.activeInHierarchy)
            yield break;

        var grid = controller.buildChoiceGrid.transform;

        // 检查现有按钮状态
        var hasDestroyButton = false;
        var hasCancelDestroyButton = false;
        for (var i = 0; i < grid.childCount; i++)
        {
            var child = grid.GetChild(i);
            if (child == null) continue;
            
            // 获取按钮文本 - 遍历子对象查找 Text 组件
            var buttonText = "";
            for (var j = 0; j < child.childCount; j++)
            {
                var grandChild = child.GetChild(j);
                if (grandChild == null) continue;
                var txt = grandChild.GetComponent<Text>();
                if (txt != null)
                {
                    buttonText = txt.text;
                    break;
                }
            }
            
            if (child.name == "DestroyButton" || buttonText.Contains("拆除"))
            {
                hasDestroyButton = true;
            }
            // 检查是否是"取消拆除"按钮（通过文本判断）
            else if (buttonText.Contains("取消拆除"))
            {
                hasCancelDestroyButton = true;
            }
        }

        // 如果已有拆除按钮或是取消拆除模式，不添加
        if (hasDestroyButton)
        {
            Plugin.LOG.Msg("Destroy button already exists");
            yield break;
        }
        
        if (hasCancelDestroyButton)
        {
            Plugin.LOG.Msg("Cancel destroy mode detected, skip destroy button");
            yield break;
        }

        CreateDestroyButton(controller);
    }

    private static void CreateDestroyButton(AreaBuildController controller)
    {
        var prefab = controller.buildChoiceButtonPrefab;
        var grid = controller.buildChoiceGrid;

        if (prefab == null || grid == null)
            return;

        // 尝试从 buildTargetIcon 获取建筑数据
        var targetData = controller.buildTargetIcon?
            .GetComponent<AreaBuildingIconController>()?
            .buildingData;

        // 如果失败，尝试从 buildTargetObj 获取
        if (targetData == null && controller.buildTargetObj != null)
        {
            targetData = controller.buildTargetObj
                .GetComponent<AreaBuildingIconController>()?
                .buildingData;
        }

        if (targetData == null)
        {
            Plugin.LOG.Msg($"CreateDestroyButton failed: targetData null. buildTargetIcon={controller.buildTargetIcon != null}, buildTargetObj={controller.buildTargetObj != null}");
            return;
        }
        
        if (excludeIds.Contains(targetData.buildingID))
        {
            return;
        }

        var buttonObj = UnityEngine.Object.Instantiate(prefab, grid.transform);
        buttonObj.name = "DestroyButton";
        buttonObj.SetActive(true);

        // 设置文本
        var text = buttonObj.GetComponentInChildren<Text>();
        if (text != null)
        {
            text.text = "拆除";
        }

        // 绑定事件
        var listener = buttonObj.GetComponent<UIEventListener>();
        if (listener == null)
            listener = buttonObj.AddComponent<UIEventListener>();

        listener.onClick = new Action<GameObject>(_ =>
        {
            Plugin.LOG.Msg("Destroy clicked");

            GameController.Instance?.ObstacleDestroyStart(targetData, true);
            controller.ShowBuildChoiceGrid(false);
        });
    }
    #endregion
}
// 指定突破加的什么属性
public class BreakThroughChoiceControllerPatch
{
    private static KungfuSkillLvData? _kungfuSkillLvData;
    [HarmonyPostfix]
    [HarmonyPatch(typeof(KungfuSkillLvData), nameof(KungfuSkillLvData.GetBreakThroughAvailableChoice))]
    public static void KungfuSkillLvData_GetBreakThroughAvailableChoice_Postfix(KungfuSkillLvData __instance,
        Il2CppSystem.Collections.Generic.List<int> __result)
    {
        if (__instance != null && __result != null && Plugin.Instance.BreakChoiceFlag && 
            !string.IsNullOrEmpty(Plugin.Instance.BreakChoiceListStr))
        {
            var list = new List<int>(
                Plugin.Instance.BreakChoiceListStr
                    .Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries) // 过滤空字符串
                    .Select(s => int.TryParse(s.Trim(), out int val) ? val : (int?)null) // 去空格 + 安全解析
                    .Where(val => val.HasValue) // 只保留解析成功的值
                    .Select(val => val.Value)
            );
            __result.Clear();
            list.ForEach(__result.Add);
            Plugin.Instance.BreakChoiceFlag = false;
        }

        _kungfuSkillLvData = __instance;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(BreakThroughChoiceController), nameof(BreakThroughChoiceController.OnClick))]
    public static bool BreakThroughChoiceController_OnClick_Prefix(BreakThroughChoiceController __instance)
    {
        if (__instance != null)
        {
            if (Plugin.Instance.BreakFlag)
            {
                __instance.extraAddData.Set(int.Parse(Plugin.Instance.BreakType),
                    float.Parse(Plugin.Instance.BreakValue));
                Plugin.Instance.BreakFlag = false;
            }
        }
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(BreakThroughController), nameof(BreakThroughController.BreakThroughChoiceClicked))]
    public static void BreakThroughController_BreakThroughChoiceClicked_Postfix(BreakThroughController __instance, BreakThroughChoiceController targetChoice)
    {
        if (Plugin.Instance.PlayerAllBreakThroughFlag.Value && HeroHelper.TryReadPlayer(out var player) && _kungfuSkillLvData !=  null)
        {
            var breakThroughAvailableChoice = _kungfuSkillLvData.GetBreakThroughAvailableChoice();
            var dict = new Il2CppSystem.Collections.Generic.Dictionary<int, float>();
            bool hasForceBonus = player.HaveForceFunction(14);
            var oldData = _kungfuSkillLvData.extraAddData.heroSpeAddData;
            foreach (var id in breakThroughAvailableChoice)
            {
                // 计算倍数
                float multiplier = Mathf.Max(0.5f, (hasForceBonus ? 1 : 0) + targetChoice.rareLv);
                // 获取突破选项基础数据
                var speAddBase = GameDataController.Instance.speAddDataBase[id];
                float addValue = multiplier * speAddBase.speValue;
                // 设置最终数值
                if (oldData != null && oldData.TryGetValue(id, out var value))
                    dict[id] = addValue + value;
                else
                    dict[id] = addValue;
            }
            _kungfuSkillLvData.extraAddData.heroSpeAddData = dict;
        }
    }
}

// 写书1天
public class BookWriterUIControllerPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(BookWriterUIController), nameof(BookWriterUIController.SureButtonClicked))]
    public static bool BookWriterUIController_SureButtonClicked_Prefix(BookWriterUIController __instance, GameObject buttonClick)
    {
        if (__instance == null || !Plugin.Instance.CopyBookFlag.Value) return true;
        var list = __instance.targetBookWriterList;
        foreach (var bwd in list)
        {
            if (bwd != null && bwd.workPercent < 0.99)
                bwd.workPercent = 0.99999f;
        }

        return true;
    }
}

/// <summary>
/// 练功经验倍率
/// </summary>
public class StudySkillPatches
{
    /// <summary>
    /// 轻功经验倍率
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(StudyDodgeSkillController), nameof(StudyDodgeSkillController.FinishStudyDodgeSkill))]
    public static void StudyDodgeSkillController_FinishStudyDodgeSkill_Prefix(StudyDodgeSkillController __instance,
        StudySkillResult studyDodgeResult)
    {
        if (__instance != null && Plugin.Instance.StudyUniqeRate.Value > 1)
        {
            __instance.totalExp *= Plugin.Instance.StudyUniqeRate.Value;
        }
    }
    /// <summary>
    /// 内功经验倍率
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(StudyInternalSkillController), nameof(StudyInternalSkillController.FinishStudyInternalSkill))]
    public static void StudyInternalSkillController_FinishStudyInternalSkill_Prefix(StudyInternalSkillController __instance,
        StudyInternalResult studyInternalResult)
    {
        if (__instance != null && Plugin.Instance.StudyUniqeRate.Value > 1)
        {
            __instance.totalExp *= Plugin.Instance.StudyUniqeRate.Value;
        }
    }
    /// <summary>
    /// 绝技经验倍率
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(StudyUniqueSkillController), nameof(StudyUniqueSkillController.FinishStudyUniqueSkill))]
    public static void StudyUniqueSkillController_FinishStudyUniqueSkill_Prefix(StudyUniqueSkillController __instance,
        StudySkillResult studyUniqueResult)
    {
        if (__instance != null && Plugin.Instance.StudyUniqeRate.Value > 1)
        {
            __instance.totalExp *= Plugin.Instance.StudyUniqeRate.Value;
        }
    }
 
    [HarmonyPrefix]
    [HarmonyPatch(typeof(StudySkillController), nameof(StudySkillController.FinishStudySkill))]
    public static void StudySkillController_FinishStudySkill_Prefix(StudySkillController __instance, ref float expNum)
    {
        if (__instance != null && Plugin.Instance.StudyFightRate.Value > 1)
        {
            expNum *= Plugin.Instance.StudyFightRate.Value;
        }
    }
}


public class HeroDataPatch
{
    /// <summary>
    /// 玩家情侣争风吃醋发生不愉快事件
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(HeroData), nameof(HeroData.CheckPlayerMakeLoverUnhappy))]
    public static bool HeroData_CheckPlayerMakeLoverUnhappy_Patch(HeroData __instance)
    {
        return !Plugin.Instance.PlayerLoverUnHappyEventFlag.Value;
    }
    /// <summary>
    /// 不增恶名
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(HeroData), nameof(HeroData.ChangeBadFame))]
    public static bool HeroData_ChangeBadFame_Prefix(HeroData __instance, ref float num)
    {
        if (__instance is { heroID: 0 } && Plugin.Instance.BzemFlag.Value && num > 0f)
        {
            num = 0f;
        }
        return true; 
    }
    /// <summary>
    /// 设置门派职务cd
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ForceData), nameof(ForceData.SetForceJob))]
    public static void SetForceJob_Postfix(ForceData __instance, int jobType, int jobID, HeroData targetHero)
    { 
        if (Plugin.Instance.EnableChangeForceJobCdZero.Value)
        {
            targetHero.forceJobCD = 0;
        }
    }
    /// <summary>
    /// 装备/技能可操控
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(HeroData), nameof(HeroData.ItemControlable))]
    public static void ItemControlable_Postfix(HeroData __instance, ref bool __result)
    {
        var flag = HeroHelper.TryReadPlayer(out var player);
        if (flag &&
            __instance != null &&
            __instance is { hide: false, dead: false } &&
            Plugin.Instance.EnableNpcEquipAndSkill.Value &&
            (
                player.HaveBrother(__instance.heroID) ||
                player.HaveFriend(__instance.heroID) ||
                player.Lover == __instance.heroID ||
                player.HavePrelover(__instance.heroID) ||
                player.Teacher == __instance.heroID ||
                player.HaveStudent(__instance.heroID)
            ))
        {
            __result = true;
        }
    }
    /// <summary>
    /// 装备/技能 锁
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(HeroData), nameof(HeroData.ItemLockable))]
    public static void ItemLockable_Postfix(HeroData __instance, ref bool __result)
    {
        var flag = HeroHelper.TryReadPlayer(out var player);
        if (flag &&
            __instance != null &&
            __instance.heroID != 0 &&
            __instance is { hide: false, dead: false } &&
            Plugin.Instance.EnableNpcEquipAndSkill.Value &&
            (
                player.HaveBrother(__instance.heroID) ||
                player.HaveFriend(__instance.heroID) ||
                player.Lover == __instance.heroID ||
                player.HavePrelover(__instance.heroID) ||
                player.Teacher == __instance.heroID ||
                player.HaveStudent(__instance.heroID)
            ))
        {
            __result = true;
        }
    }
    /// <summary>
    /// 有关系的可以在私宅修改天赋
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlotController), nameof(PlotController.ChooseManageTagTargetSelfHouse))]
    public static bool PlotController_ChooseManageTagTargetSelfHouse_Prefix(PlotController __instance)
    {
        var gc = GameController.Instance;
        var cc = ChooseController.Instance;

        if (cc == null ||
            !HeroHelper.TryReadPlayer(out var player) ||
            gc?.worldData == null ||
            __instance == null ||
            !Plugin.Instance.EnablePrivateHouseNpcTag.Value)
        {
            return true;
        }
        
        var list = new Il2CppSystem.Collections.Generic.List<HeroData>();

        void AddHero(int heroId)
        {
            if (heroId <= 0) return;

            var hero = gc.worldData.GetHero(heroId);
            if (hero is { dead: false, hide: false })
                list.Add(hero);
        }

        void AddHeroes(Il2CppSystem.Collections.Generic.List<int> heroIds)
        {
            foreach (var id in heroIds)
            {
                AddHero(id);
            }
        }

        AddHero(player.Lover);
        AddHero(player.Teacher);
        AddHeroes(player.Friends);
        AddHeroes(player.Brothers);
        AddHeroes(player.PreLovers);
        AddHeroes(player.teamMates);

        if (list.Count == 0) return true;
        
        cc.ShowChoosePanel(
            ChooseType.Hero,
            list,
            __instance.gameObject,
            "ChooseManageTagTargetResult"
        );
        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlotController), nameof(PlotController.ChooseManageTagTargetSelfHouse))]
    public static void PlotController_ChooseManageTagTargetSelfHouse_Postfix(PlotController __instance)
    {
        __instance.HideInteractUI();
    }

    // /// <summary>
    // /// 晋升要求不受武学限制数量修改后影响影响
    // /// </summary>
    // /// <param name="__instance"></param>
    // /// <param name="__result"></param>
    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(HeroData), nameof(HeroData.GetUpgradeForceLvNeedSkillNum))]
    // public static void HeroData_GetUpgradeForceLvNeedSkillNum_Postfix(HeroData __instance, ref int __result)
    // {
    //     if (ModConfig.HaveNpcMod) return;
    //     __result /= Plugin.Instance.KungFuMaxLimitTimes.Value;
    // }
    
    /// <summary>
    /// 玩家/Npc 最大天赋数量设置
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(HeroData), nameof(HeroData.GetMaxTagNum))]
    public static void GetMaxTagNum_Postfix(HeroData __instance, ref int __result)
    {
        if (ModConfig.HaveNpcMod) return;
        
        if (__instance.heroID == 0 && Plugin.Instance.PlayerMaxTagNum.Value != 9)
        {
            __result = Plugin.Instance.PlayerMaxTagNum.Value;
        }
        if (__instance.heroID != 0 && Plugin.Instance.NpcMaxTagNum.Value != 9)
        {
            __result = Plugin.Instance.NpcMaxTagNum.Value;
        }
    }
    
    #region 新档相关
    [HarmonyPostfix]
    [HarmonyPatch(typeof(StartGameSettingController), nameof(StartGameSettingController.Update))]
    public static void StartGameSettingController_Update_Postfix(StartGameSettingController __instance)
    {
        if (__instance != null && Plugin.Instance.NewGameTagNumFlag.Value) __instance.Player.heroTagPoint = 999;
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(StartMenuController), nameof(StartMenuController.CheckMeetCondition))]
    public static void StartMenuController_CheckMeetCondition_Postfix(StartMenuController __instance, HeroData checkHero, 
        HeroTagDataBase targetTag, ref bool __result)
    {
        if (__instance != null && Plugin.Instance.NewGameAnyTagFlag.Value)
        { 
            targetTag.oppositeMeaning = "";
            targetTag.sameMeaning = "";
            __result = true;
        }
    }
    [HarmonyPostfix]
    [HarmonyPatch(typeof(StartMenuController), nameof(StartMenuController.CheckMeetOneCondition))]
    public static void StartMenuController_CheckMeetOneCondition_Postfix(StartMenuController __instance, HeroData checkHero, 
        string requirement, ref bool __result)
    {
        if (__instance != null && Plugin.Instance.NewGameAnyTagFlag.Value)
        { 
            __result = true;
        }
    }
    [HarmonyPostfix]
    [HarmonyPatch(typeof(HeroTagData), nameof(HeroTagData.StartChooseAble))]
    public static void HeroTagData_StartChooseAble_Postfix(ref bool __result)
    {
        if (Plugin.Instance.NewGameAnyTagFlag.Value)
        { 
            __result = true;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(StartMenuController), nameof(StartMenuController.Start))]
    public static void StartMenuController_Start_Postfix(StartMenuController __instance)
    {
        Slider[] sliders = __instance.customDifficultyRoot.GetComponentsInChildren<Slider>(true);
        
        // 只修改前8个滑动条（0-7），保留最后两个（8-9）不变
        for (int i = 0; i < sliders.Length && i < 6; i++)
        {
            Slider slider = sliders[i];
            
            // 修改范围为 -5 到 5
            slider.minValue = Plugin.Instance.NewSaveSliderMin.Value;
            slider.maxValue = Plugin.Instance.NewSaveSliderMax.Value;
            
            // 如果需要整数步进
            slider.wholeNumbers = true;
            
        }
    }
    #endregion
    
    # region 人物潜力限制开关

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HeroData), nameof(HeroData.ChangeAttri))]
    public static void HeroData_ChangeAttri_Postfix(HeroData __instance, int id, float num, 
        bool showText, bool skillUpgrade)
    {
        if (__instance == null || !skillUpgrade || num <= 0 || !Plugin.Instance.BreakMaxLimitFlag.Value) return;
        if (__instance.heroID == 0 && Plugin.Instance.BreakMaxLimitNotForPlayerFlag.Value) return;
        var baseAttri = __instance.baseAttri;
        var maxAttri = __instance.maxAttri;
        if (baseAttri == null || maxAttri == null) return;
        if (id < 0 || id >= baseAttri.Count || id >= maxAttri.Count) return;
        
        var currentVal = baseAttri[id];
        var maxVal = maxAttri[id];
        if (currentVal > maxVal)
        {
            maxAttri[id] = currentVal;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HeroData), nameof(HeroData.ChangeFightSkill))]
    public static void HeroData_ChangeFightSkill_Postfix(HeroData __instance, int id, float num, 
        bool showText, bool skillUpgrade)
    {
        if (__instance == null || !skillUpgrade || num <= 0 || !Plugin.Instance.BreakMaxLimitFlag.Value) return;
        if (__instance.heroID == 0 && Plugin.Instance.BreakMaxLimitNotForPlayerFlag.Value) return;
        var baseFightSkill = __instance.baseFightSkill;
        var maxFightSkill = __instance.maxFightSkill;
        if (baseFightSkill == null || maxFightSkill == null) return;
        if (id < 0 || id >= baseFightSkill.Count || id >= maxFightSkill.Count) return;
        
        var currentVal = baseFightSkill[id];
        var maxVal = maxFightSkill[id];
        if (currentVal > maxVal)
        {
            maxFightSkill[id] = currentVal;
        }
    }
    
    #endregion
    
    
    /// <summary>
    /// 游戏难度倍率默认最高难度1.6
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="__result"></param>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(HeroData), nameof(HeroData.GetGameDifficultyExpRate))]
    public static void Postfix(HeroData __instance, ref float __result)
    {
        if (Mathf.Approximately(Plugin.Instance.ExpRateMultiplier.Value, 1)) return;
        var flag = HeroHelper.TryReadPlayer(out var player);

        if (__instance == null || !flag) return;
        
        var playerForceId = player.belongForceID;
        //玩家无门派时，除了玩家所有人都修改倍率
        if (playerForceId == -1)
        {
            if (__instance.heroID != 0) __result = Plugin.Instance.ExpRateMultiplier.Value;
        }
        else//玩家有门派时，不和玩家一个门派的人物倍率修改
        {
            if (Plugin.Instance.ExpRateMultiplierSelfForceFlag.Value || __instance.belongForceID != playerForceId) __result = Plugin.Instance.ExpRateMultiplier.Value;
        }
    }
    /// <summary>
    /// 门派功绩倍率
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(HeroData), nameof(HeroData.ChangeForceContribution))]
    public static bool HeroData_ChangeForceContribution_Prefix(HeroData __instance, ref float num, 
        bool showInfo, int targetForce = -1)
    {
        var flag = HeroHelper.TryReadPlayer(out var player);
        if (!flag || __instance is not { heroID: 0 }) return true;
        var forceId = __instance.belongForceID;
        var playerForceId = player.belongForceID;
        if (forceId != playerForceId && Plugin.Instance.ForceContributionRate.Value > 1 && num > 0)
        {
            num *= Plugin.Instance.ForceContributionRate.Value;
        }
        return true;
    }
    /// <summary>
    /// 官府功绩
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(HeroData), nameof(HeroData.ChangeGovernContribution))]
    public static bool HeroData_ChangeGovernContribution_Prefix(HeroData __instance, ref float num)
    {
        if (__instance is not { heroID: 0 } || num < 0) return true;
        num *= Plugin.Instance.GovernContributionRate.Value;
        return true;
    }
    
    // 所有门派特性生效
    [HarmonyPrefix]
    [HarmonyPatch(typeof(HeroData), nameof(HeroData.HaveForceFunction))]
    public static bool HeroData_HaveForceFunction_Prefix(HeroData __instance,int forceID, ref bool __result)
    {
        if (UIBuilderExtensions.EnabledForceIDs.Count == 0)
        {
            UIBuilderExtensions.RefreshForceList();
        }

        if (!HeroHelper.TryReadPlayer(out var player))
        {
            __result = false;
            return false;
        }
        
        // 1. 玩家本人：所有启用的门派特性都可用
        if (__instance.heroID == 0)
        {
            __result = UIBuilderExtensions.EnabledForceIDs.Contains(forceID);
            return false;
        }
         
        // 2. 同门派 NPC：会有玩家的所有门派特性
        if (player.belongForceID != -1 && __instance.IsPlayerSameForce())
        {
            __result = UIBuilderExtensions.EnabledForceIDs.Contains(forceID);
            return false;
        }
        // // 3. 其他 NPC：只有自己的门派特性（这个竟然会让喝酒那个门派提示获得饮胜状态）
        // __result = __instance.belongForceID == forceID;
     
        return false;
    }
    
    /// <summary>
    /// 战斗获得经验倍率
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(HeroData), nameof(HeroData.BattleChangeSkillFightExp))]
    public static bool HeroData_BattleChangeSkillFightExp_Prefix(HeroData __instance, ref float num, 
        KungfuSkillLvData targetSkill, bool showInfo)
    {
        if (__instance is { heroID: 0 } && Plugin.Instance.BattleChangeSkillFightRate.Value > 1)
        {
            num *= Plugin.Instance.BattleChangeSkillFightRate.Value;
        }
        
        return true;
    }
    /// <summary>
    /// 最大好感度
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(HeroData), nameof(HeroData.GetMaxFavor))]
    public static void HeroData_GetMaxFavor_Postfix(HeroData __instance, float maxFavor, 
        ref float __result)
    {
        if (__instance != null && Mathf.Approximately(__result, 100) && Plugin.Instance.FavorMax.Value > 100) 
            __result = Plugin.Instance.FavorMax.Value;
    }
    /// <summary>
    /// 金钱倍数
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(HeroData), nameof(HeroData.ChangeMoney))]
    public static bool HeroData_ChangeMoney_Prefix(HeroData __instance, ref int num, bool showInfo)
    {
        if (__instance != null && num > 0 && __instance.heroID == 0 && Plugin.Instance.MoneyTimes.Value > 1)
        {
            num *= Plugin.Instance.MoneyTimes.Value;
        }
        return true;
    }
    /// <summary>
    /// 装备重量
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(HeroData), nameof(HeroData.EquipItem))]
    public static bool HeroData_EquipItem_Prefix(ItemData itemData, bool playSound = false, bool showInfo = false)
    {
        if (Plugin.Instance.EquipmentWeight.Value < 1)
            itemData.weight *= Plugin.Instance.EquipmentWeight.Value;
        return true;
    }

    /// <summary>
    /// 好感不减
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(HeroData), nameof(HeroData.ChangeFavor))]
    public static bool HeroData_ChangeFavor_Prefix(ref float num)
    {
        if (Plugin.Instance.Hgbj.Value && num < 0f) 
            num = 0f;
        
        if (num > 0 && Plugin.Instance.FavorTimes.Value > 1)
            num *= Plugin.Instance.FavorTimes.Value;
        
        return true;
    }
     /// <summary>
     /// 忠诚度
     /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(HeroData), nameof(HeroData.ChangeLoyal))]
    public static bool Prefix(HeroData __instance, ref float num, bool showInfo)
    {
        var flag = HeroHelper.TryReadPlayer(out var player);
        if (!Plugin.Instance.LoyalLockFlag.Value || !flag || __instance == null || player.belongForceID != __instance.belongForceID || num >= 0) return true;
        num = 0;
        return true;
    }
}

public class CraftingPatches
{
    [HarmonyPatch(typeof(CraftUIController), "GetCraftRate")]
    [HarmonyPostfix]
    public static void GetCraftRate_Postfix(int costID, ref float __result)
    {
        if (Plugin.Instance.Pzqh.Value > 1)
            __result *= Plugin.Instance.Pzqh.Value;
    }
    
}

public class PlotControllerPatches
{
    [HarmonyPatch(typeof(PlotController), nameof(PlotController.GetStealNpcSkillSuccessRate))]
    [HarmonyPostfix]
    public static void PlotController_GetStealNpcSkillSuccessRate_Postfix(PlotController __instance,
        ref float __result)
    {
        if (__instance != null && Plugin.Instance.StealRate.Value 
                               && __instance.targetInteractHero.heroID != 0)
        {
            __result = 1f;
        }
    }
    
    [HarmonyPatch(typeof(PlotController), nameof(PlotController.GetStealNpcSuccessRate))]
    [HarmonyPostfix]
    public static void PlotController_GetStealNpcSuccessRate_Postfix(PlotController __instance,
        ref float __result)
    {
        if (__instance != null && Plugin.Instance.StealRate.Value 
                               && __instance.targetInteractHero.heroID != 0)
        {
            __result = 1f;
        }
    }
    
    
    /// <summary>
    /// 统一处理“传授后直接升满”，顺便兜住目标技能不存在的情况。
    /// </summary>
    private static void UpgradeTargetSkillToFull(HeroData targetHero, string skillIDParam)
    {
        if (targetHero == null || string.IsNullOrWhiteSpace(skillIDParam)) return;

        var targetSkill = targetHero.FindSkill(int.Parse(skillIDParam));
        if (targetSkill == null) return;

        for (int i = 0; i < 10; i++)
        {
            targetHero.UpgradeSkill(targetSkill);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlotController), nameof(PlotController.TeachNewSkillToNPCSure))]
    public static void PlotController_TeachNewSkillToNPCSure_Postfix(PlotController __instance, string skillIDParam)
    {
        if (__instance != null && Plugin.Instance.TeachNewSkillToNpc.Value)
        {
            UpgradeTargetSkillToFull(__instance.targetInteractHero, skillIDParam);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlotController), nameof(PlotController.TeachNPCSure))]
    public static void PlotController_TeachNPCSure_Postfix(PlotController __instance, string skillIDParam)
    {
        if (__instance != null && Plugin.Instance.TeachNpc.Value)
        {
            UpgradeTargetSkillToFull(__instance.targetInteractHero, skillIDParam);
        }
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlotController), nameof(PlotController.ForceTeachNPCSure))]
    public static void PlotController_ForceTeachNPCSure_Postfix(PlotController __instance, string skillIDParam)
    {
        if (__instance != null && Plugin.Instance.TeachNpc.Value)
        {
            UpgradeTargetSkillToFull(__instance.targetInteractHero, skillIDParam);
        }
    }
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlotController), nameof(PlotController.ForceTeachNewSkillToNPCSure))]
    public static void PlotController_ForceTeachNewSkillToNPCSure_Postfix(PlotController __instance, string skillIDParam)
    {
        if (__instance != null && Plugin.Instance.TeachNewSkillToNpc.Value)
        {
            UpgradeTargetSkillToFull(__instance.targetInteractHero, skillIDParam);
            if (Plugin.Instance.Interaction.Value) 
                __instance.targetInteractHero?.playerInteractionTimeData?.ResetTime();
        }
    }
}

public class ReadBookControllerPatches
{
    /// <summary>
    /// 读书耐心扣减1
    /// </summary>
    /// <param name="__instance"></param>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ReadBookTextController), nameof(ReadBookTextController.OnClick))]
    public static void ReadBookTextController_OnClick_Prefix(ReadBookTextController __instance)
    {
        if (__instance == null || !Plugin.Instance.ReadBookChangePatient1Flag.Value) return;
        __instance.textData.costPatient = __instance.textData.costPatient > 0 ? 1 : 0;
    }
    /// <summary>
    /// 读书倍率
    /// </summary>
    /// <param name="__instance"></param>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ReadBookController), nameof(ReadBookController.FinishRead))]
    public static void ReadBookController_FinishRead_Prefix(ReadBookController __instance)
    {
        if (__instance != null && Plugin.Instance.ReadBook.Value > 1)
        {
            __instance.totalExp *= Plugin.Instance.ReadBook.Value;
        }
    }
}
/// <summary>
/// 探险相关
/// </summary>
public class ExploreControllerPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ExploreController), nameof(ExploreController.PlayerFinishMove))]
    public static void ExploreController_PlayerFinishMove_Postfix(ExploreController __instance)
    {
        if (__instance == null || !Plugin.Instance.Explore.Value) return;
        __instance.leftPower = 1000;
    }
    // 自动去除迷雾补丁
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ExploreController), nameof(ExploreController.GenerateExploreMap))]
    public static void GenerateExploreMap_Postfix(ExploreController __instance)
    {
        if (__instance == null || !Plugin.Instance.ExploreSeeAllFlag.Value) return;
        __instance.SeeAllTile();
    }
    // 随意移动
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ExploreTileUnitController), nameof(ExploreTileUnitController.OnClick))]
    public static void ExploreTileUnitController_OnClick_Postfix(ExploreTileUnitController __instance)
    {
        if  (__instance == null || !Plugin.Instance.ExploreFreeMoveFlag.Value) return;
        var ec = ExploreController.Instance;
        if (ec == null) return;
        var exploreTileData = __instance.exploreTileData;
        var column = exploreTileData.column;
        var row = exploreTileData.row;
        ec.PlayerEnterGrid(column, row);
    }
    
    
}

public class ForceDataPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ForceData), nameof(ForceData.GetNowResearchTech))]
    public static void ForceData_GetNowResearchTech_Postfix(ForceData? __instance, ForceTechLvData? __result)
    {
        if (__instance != null && __result != null && Plugin.Instance.ReasearchFlag.Value)
        {
            var flag = HeroHelper.TryReadPlayer(out var player);
            
            if (flag && player.belongForceID != -1 && __instance.forceID == player.GetForce()?.forceID)
            {
                __result.researchPercent = 1f;
            }
        }
    }
    
    // 0钱1粮2木3矿4药5威望
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ForceData), nameof(ForceData.CostResource),
        typeof(Il2CppSystem.Collections.Generic.List<float>), typeof(bool))]
    public static bool ForceData_CostResource_Prefix(Il2CppSystem.Collections.Generic.List<float> resourceList,
        bool showInfo = false)
    {
        if (Plugin.Instance.Cost0.Value)
        {
            for (var i = 0; i < resourceList.Count; i++) resourceList[i] = 0f;
        }
       
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ForceData), nameof(ForceData.CostResource),
        typeof(Il2CppSystem.Collections.Generic.List<ResourceData>), typeof(bool))]
    public static bool ForceData_CostResource_Prefix1(Il2CppSystem.Collections.Generic.List<ResourceData> resourceList,
        bool showInfo)
    {
        if (Plugin.Instance.Cost0.Value)
        {
            var list = new Il2CppSystem.Collections.Generic.List<ResourceData>();
            foreach (var t in resourceList) list.Add(new ResourceData(t.resourceType, 0));
            resourceList = list;
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ForceData), nameof(ForceData.CostResource), typeof(ResourceData), typeof(bool))]
    public static bool ForceData_CostResource_Prefix2(ref ResourceData resource, bool showInfo)
    {
        if (Plugin.Instance.Cost0.Value)
        {
            resource = new ResourceData(resource.resourceType, 0);
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ForceData), nameof(ForceData.CostResource), typeof(int), typeof(float), typeof(bool))]
    public static bool ForceData_CostResource_Prefix3(int id, ref float num, bool showInfo)
    {
        if (Plugin.Instance.Cost0.Value)
        {
            num = 0;
        }
        return true;
    }
    // 门派特性显示在门派总览的信息里
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ForceDetailController), nameof(ForceDetailController.ShowForceDetail))]
    public static void ShowForceDetail_Postfix(ForceDetailController __instance, int targetForceID)
    {
        var worldData = GameController.Instance?.worldData;
        if (worldData == null || __instance.baseDetailText == null) return;
        
        var playerForceID = GlobalData.PlayerForceID;
        var originalText = __instance.baseDetailText.text;
        
        if (worldData.gameMode == GameMode.Plot)
        {
            if (targetForceID == playerForceID)
            {
                var sb = new StringBuilder();
                foreach (var forceId in UIBuilderExtensions.EnabledForceIDs)
                {
                    var forceData = GetForceDataById(forceId);
                    if (forceData != null)
                    {
                        var speFunc = forceData.speFunctionDescribe;
                        if (!string.IsNullOrEmpty(speFunc))
                        {
                            sb.Append($"<color=#BE8100>{speFunc}</color>\n\n");
                        }
                    }
                }
                
                if (sb.Length > 0)
                {
                    __instance.baseDetailText.text = originalText + "\n\n<color=#BE8100><b>门派特性</b></color>\n" + sb.ToString();
                }
            }
            else
            {
                var forceData = GetForceDataById(targetForceID);
                if (forceData != null)
                {
                    var speFunc = forceData.speFunctionDescribe;
                    if (!string.IsNullOrEmpty(speFunc))
                    {
                        __instance.baseDetailText.text = originalText + "\n\n<color=#BE8100><b>门派特性</b>\n" + speFunc + "</color>";
                    }
                }
            }
        }
        else if (worldData.gameMode == GameMode.Free)
        {
            if (targetForceID != playerForceID) return;
            
            var featureIndex = originalText.IndexOf("<b>门派特性</b>");
            if (featureIndex < 0) return;
            
            var sb = new StringBuilder();
            foreach (var forceId in UIBuilderExtensions.EnabledForceIDs)
            {
                var forceData = GetForceDataById(forceId);
                if (forceData != null)
                {
                    var speFunc = forceData.speFunctionDescribe;
                    if (!string.IsNullOrEmpty(speFunc))
                    {
                        sb.Append($"<color=#BE8100>{speFunc}</color>\n\n");
                    }
                }
            }
            
            if (sb.Length > 0)
            {
                var newText = originalText.Substring(0, featureIndex) + "<color=#BE8100><b>门派特性</b></color>\n" + sb.ToString();
                __instance.baseDetailText.text = newText;
            }
        }
    }
    private static ForceData GetForceDataById(int forceId)
    {
        var worldData = GameController.Instance?.worldData;
        if (worldData == null || worldData.Forces == null) return null;
            
        foreach (var force in worldData.Forces)
        {
            if (force.forceID == forceId)
                return force;
        }
        return null;
    }
}



public class ItemListDataPatches
{
    /// <summary>
    /// 马和马鞍的负重倍数
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ItemData), nameof(ItemData.GetHorseMaxWeightAdd))]
    public static void ItemData_GetHorseMaxWeightAdd_Postfix(ItemData __instance, ref float __result)
    {
        var flag = HeroHelper.TryReadPlayer(out var player);
        if (!flag || Plugin.Instance.HorseMaxWeightTimes.Value == 1 || !__instance.IsHeroEquip(player)) return;
        __result *= Plugin.Instance.HorseMaxWeightTimes.Value;
    }
    /// <summary>
    /// 马和马鞍视野范围加成倍数
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ItemData), nameof(ItemData.GetHorseSeeRange))]
    public static void ItemData_GetHorseSeeRange_Postfix(ItemData __instance, ref float __result)
    {
        var flag = HeroHelper.TryReadPlayer(out var player);
        if (!flag || Plugin.Instance.HorseMaxSeeRangeTimes.Value == 1 || !__instance.IsHeroEquip(player))return;
        __result *= Plugin.Instance.HorseMaxSeeRangeTimes.Value;
    }
    /// <summary>
    /// 马和马鞍探险耐力加成倍数
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ItemData), nameof(ItemData.GetHorseStepAddRate))]
    public static void ItemData_GetHorseStepAddRate_Postfix(ItemData __instance, ref float __result)
    {
        var flag = HeroHelper.TryReadPlayer(out var player);
        if (!flag || Plugin.Instance.HorseStepAddRateTimes.Value == 1 || !__instance.IsHeroEquip(player)) return;
        __result *= Plugin.Instance.HorseStepAddRateTimes.Value;
    }
    
    
    /// <summary>
    /// 一眼鉴宝
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(QuickDetail), nameof(QuickDetail.ShowTreasureQuickDetail))]
    public static bool QuickDetail_ShowTreasureQuickDetail_Prefix(QuickDetail __instance, GameObject target, 
        ItemData treasureData)
    {
        if (__instance == null || !Plugin.Instance.JianBaoFlag.Value || treasureData.treasureData.fullIdentified) return true;
        var list = treasureData.treasureData.treasureLv;
        var list1 =  treasureData.treasureData.playerGuessTreasureLv;
        for (int i = 0; i < 4; i++)
        {
            list1[i].Clear();
            list1[i].Add(list[i]);
        }
        return true;
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ItemListData), nameof(ItemListData.GetItem), typeof(ItemData), typeof(bool))]
    public static void ItemListData_GetItem_Postfix(ItemListData? __instance, ItemData targetItem, bool showPopInfo)
    {
        if (__instance?.GetHero() != null && __instance.GetHero().heroID == 0)
        {
            // 所有是红品质
            if (Plugin.Instance.RedQuality.Value)
            {
                var newDict = new Il2CppSystem.Collections.Generic.Dictionary<int, float>();
                if (targetItem.type == ItemType.Book)
                {
                    targetItem = targetItem.SetBookData(targetItem.bookData.skillID, 5);
                }
                else if (targetItem.type == ItemType.Treasure)
                {
                    var list = targetItem.treasureData.treasureLv;
                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i] = 5;
                    }
                    targetItem.rareLv = 5;
                    targetItem.value = targetItem.GetTreasureRealValue();
                }
                else if (targetItem.type == ItemType.Equip)
                {
                    // 基础属性
                    var oldDict = targetItem.equipmentData.baseAddData.heroSpeAddData;
                    foreach (var dict in oldDict)
                    {
                        if (dict.Value > 0)
                        {
                            newDict[dict.Key] = dict.Value;
                        }
                        else
                        {
                            newDict[dict.Key] = dict.Value * -1;
                        }
                        targetItem.equipmentData.baseAddData.heroSpeAddData = newDict;
                    }
                    // 额外属性
                    oldDict = targetItem.equipmentData.extraAddData.heroSpeAddData;
                    foreach (var dict in oldDict)
                    {
                        if (dict.Value > 0)
                        {
                            newDict[dict.Key] = dict.Value;
                        }
                        else
                        {
                            newDict[dict.Key] = dict.Value * -1;
                        }
                    }
                    targetItem.equipmentData.extraAddData.heroSpeAddData = newDict;
                }
                else if (targetItem.type == ItemType.Material)
                {
                    var oldDict = targetItem.materialData.extraAddData.heroSpeAddData;
                    foreach (var dict in oldDict)
                    {
                        if (dict.Value > 0)
                        {
                            newDict[dict.Key] = dict.Value;
                        }
                        else
                        {
                            newDict[dict.Key] = dict.Value * -1;
                        }
                    }
                    targetItem.materialData.extraAddData.heroSpeAddData = newDict;
                }
                else if (targetItem.type == ItemType.Med)
                {
                    targetItem.rareLv = 5;
                    var baseItem = GameDataController.Instance.medDataBase[targetItem.itemID];
                    
                    float multiplier = targetItem.rareLv / Mathf.Clamp(targetItem.itemLv * 5f, 5f, 20f) + 1.0f;
                    targetItem.medFoodData.changeHeroState = baseItem.medFoodData.changeHeroState * multiplier;
                    
                    targetItem.CountValueAndWeight();
                }
                else if (targetItem.type == ItemType.Food)
                {
                    targetItem.rareLv = 5;
                    var baseItem = GameDataController.Instance.foodDataBase[targetItem.itemID];
                    float multiplier = targetItem.rareLv / Mathf.Clamp(targetItem.itemLv * 5f, 5f, 20f) + 1.0f;
                    targetItem.medFoodData.changeHeroState = baseItem.medFoodData.changeHeroState * multiplier;
                    // 生成额外属性加成（如果配置了随机加成值）
                    int randomSpeAddValue = targetItem.medFoodData.randomSpeAddValue;
                    if (randomSpeAddValue > 0)
                    {
                        // 计算总值：2 * (稀有度 + 5 * 随机加成基础值)
                        int totalValue = 2 * (targetItem.rareLv + 5 * randomSpeAddValue);

                        targetItem.medFoodData.extraAddData.heroSpeAddData.Clear();
                        // 生成属性加成
                        GameController.Instance.GenerateSpeAddByValue(
                            totalValue,
                            targetItem.medFoodData.extraAddData,
                            1,      
                            0.0f, 
                            1      
                        );
                    }
                    targetItem.CountValueAndWeight();
                }
                else
                {
                    targetItem.rareLv = 5;
                }
            }
            
            // 获得的武学书都是红色品质
            if (targetItem.type == ItemType.Book && Plugin.Instance.RedBook.Value)
            {
                targetItem = targetItem.SetBookData(targetItem.bookData.skillID, 5);
            }

            // 获得材料
            if (targetItem.type == ItemType.Material && Plugin.Instance.RedMaterial)
            {
                targetItem.itemLv = 5;
                targetItem.rareLv = 5;
                
                var inputBox = OtherHelper.ParseInputBox(Plugin.Instance.MaterialAttr);
                if (inputBox == null)
                    return;
                var il2CppDictionary = OtherHelper.ToIl2CppDictionary(inputBox);
                if (il2CppDictionary == null)
                    return;
                targetItem.materialData.extraAddData.heroSpeAddData = il2CppDictionary; 
                var tempItem = new ItemData(ItemType.Material).SetMaterialData(targetItem.subType, 5, 5);
                targetItem.name = tempItem.name;
                targetItem.value = (int)targetItem.CountValueAndWeight();
            }
               
            if (targetItem.type == ItemType.Treasure)
            {
                if (Plugin.Instance.RedTreasure.Value)
                {
                    var tempItem = new ItemData(ItemType.Treasure).SetTreasureData(targetItem.subType, 5, 5);
                    var list = targetItem.treasureData.treasureLv;
                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i] = 5;
                    }
                    targetItem.itemLv = 5;
                    targetItem.value = targetItem.GetTreasureRealValue();
                    targetItem.rareLv = 5;
                    targetItem.name = tempItem.name;
                    targetItem.subType = tempItem.subType;
                }
                if (Plugin.Instance.GoodTreasure.Value)
                {
                    var list = targetItem.treasureData.treasureLv;
                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i] = 5;
                    }
                    targetItem.rareLv = 5;
                }
            }
            
        }
    }
}

public class BreakThroughControllerPatches
{
    [HarmonyPatch(typeof(BreakThroughController), nameof(BreakThroughController.GetScoreRate))]
    [HarmonyPostfix]
    public static void BreakThroughController_GetScoreRate_Postfix(BreakThroughController __instance,
        ref float __result)
    {
        if (__instance != null && Plugin.Instance.RedBreak.Value > 1)
        {
            __result *= Plugin.Instance.RedBreak.Value;
        }
    }
}

/// <summary>
/// 剑池天工修改
/// </summary>
public class SpeEnhanceEquipControllerPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(SpeEnhanceEquipController), nameof(SpeEnhanceEquipController.GetTimeNeed))]
    public static void SpeEnhanceEquipController_GetTimeNeed_Postfix(SpeEnhanceEquipController __instance, ref int __result)
    {
        if (__instance == null || !Plugin.Instance.SwordPoolEasyFlag.Value) return;
        __result = 1;
    }
    [HarmonyPostfix]
    [HarmonyPatch(typeof(SpeEnhanceEquipController), nameof(SpeEnhanceEquipController.GetStoneNeed))]
    public static void SpeEnhanceEquipController_GetStoneNeed_Postfix(SpeEnhanceEquipController __instance, ref int __result)
    {
        if (__instance == null || !Plugin.Instance.SwordPoolEasyFlag.Value) return;
        __result = 1;
    }
}

    
