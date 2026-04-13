using System.Collections;
using System.Text;
using UnityEngine;
using Object = Il2CppSystem.Object;
using HarmonyLib;
using Il2Cpp;
using LYMod.Helpers;
using MelonLoader;

namespace LYMod;


public class GameDataControllerPatches
{
    // 读取存档后
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameDataController), nameof(GameDataController.GameDataIntoGame))]
    public static void GameDataController_GameDataIntoGame_Postfix(GameDataController __instance)
    {
        var allMods = MelonBase.RegisteredMelons.OfType<MelonMod>();
        Plugin.LOG.Msg("===================================================");
        foreach (var mod in allMods)
        {
            switch (mod.Info.Name)
            {
                case "Refresh Auction":
                    ModConfig.HaveAucRoll = true;
                    OtherHelper.AddInfoTab("【LYMod】由于加载了 <color=FF8C06>Refresh Auction</color> Mod，LYMod的<color=#FF8C06>按R键重Roll拍卖会</color><color=#FF0000>失效</color>", lastTime:20f);
                    Plugin.LOG.Msg("【LYMod】由于加载了 Refresh Auction Mod，LYMod的按R键重Roll拍卖会失效");
                    break;
                case "SelfHouseLover":
                    OtherHelper.AddInfoTab("【LYMod】由于加载了 <color=FF8C06>SelfHouseLover</color> Mod，LYMod的<color=#9A7CFF>按R键重Roll黄鹤楼招贤</color><color=#FF0000>失效</color>", lastTime:20f);
                    Plugin.LOG.Msg("【LYMod】由于加载了 SelfHouseLover Mod，LYMod的按R键重Roll黄鹤楼招贤失效");
                    ModConfig.HaveRecruitReRoll = true;
                    break;
                case "NPC管理Mod" or "TeammateManagerMod":
                    OtherHelper.AddInfoTab("【LYMod】由于加载了 <color=#FF8C06>NPC管理Mod</color>，LYMod的<color=#9A7CFF>【天赋上限设置】</color>，<color=#9A7CFF>【武学修习数量上限】</color>，<color=#9A7CFF>【入队时间修改】</color><color=#FF0000>失效</color>", lastTime:20f);
                    Plugin.LOG.Msg("【LYMod】由于加载了 NPC管理Mod，LYMod的【天赋上限设置】，【武学修习数量上限】，【入队时间修改】失效");
                    ModConfig.HaveNpcMod = true;
                    break;
                case "ReadBookPlus":
                    Plugin.LOG.Msg("【LYMod】由于加载了 HaveReadBookPlus，LYMod的 读书显示所有格子 失效");
                    ModConfig.HaveReadBookPlus = true;
                    break;
            }
        }
        Plugin.LOG.Msg("===================================================");
        
        // 游戏进入时自动加载建筑倍率配置
        UIBuilderExtensions.RefreshBuildingList();
        // 修改武学修炼数量限制倍数
        OtherHelper.ChaneMaxNum();
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

    [HarmonyPostfix]
    [HarmonyPatch(typeof(BattleController), nameof(BattleController.StartBattleButtonClicked))]
    public static void StartBattleButtonClicked_Postfix(BattleController __instance)
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
    private static readonly Dictionary<string, float> PoisonValuesBeforeBattle = new();
    
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
        if (!Plugin.Instance.PoisonNumReduceFlag.Value) return;
        PoisonValuesBeforeBattle.Clear();
        var gc = GameController.Instance;
        if (gc == null) return;
        var player = gc.worldData.Player();
        var items = player.itemListData.allItem;
        if (items == null || items.Count == 0) return;
        foreach (var item in items)
        {
            if (item.Equiped() && item.equipmentData?.equipPoisonData is { poisonNum: > 0 })
            {
                PoisonValuesBeforeBattle[item.name] = item.equipmentData.equipPoisonData.poisonNum;
                Plugin.LOG.Msg($"[Poison] Before Battle - Item: {item.name}, poisonNum: {item.equipmentData.equipPoisonData.poisonNum}");
            }
        }
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(BattleController), nameof(BattleController.BattleRealEnd))]
    public static void BattleRealEnd_Postfix(BattleController __instance)
    {
        if (__instance == null || !Plugin.Instance.PoisonNumReduceFlag.Value) return;
        var gc = GameController.Instance;
        if (gc == null) return;
        var player = gc.worldData.Player();
        var items = player.itemListData.allItem;
        if (items == null || items.Count == 0) return;
        foreach (var item in items)
        {
            if (item.Equiped() && item.equipmentData?.equipPoisonData != null)
            {
                var beforeValue = PoisonValuesBeforeBattle.GetValueOrDefault(item.name, 0);
                var afterValue = item.equipmentData.equipPoisonData.poisonNum;
                var diff = afterValue - beforeValue;
                //Plugin.LOG.Msg($"[Poison] After Battle - Item: {item.name}, Before: {beforeValue}, After: {afterValue}, Diff: {diff}");
                if (beforeValue > 0 && diff < 0)
                {
                    item.equipmentData.equipPoisonData.poisonNum = beforeValue;
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

#region 抄书和默写获得结果逻辑修改

public class BookWriterDataPatches
{

    private static readonly string[] RareColors = { "#00B400", "#78BE00", "#0080FF", "#9A7CFF", "#FF8C06", "#FF0000" };

    private static string ColorizeText(string text, int lv)
    {
        string color = RareColors[lv];
        return $"<color={color}>{text}</color>";
    }
    /// <summary>
    /// 抄书计算
    /// </summary>
    /// <param name="hero"></param>
    /// <param name="originalBook"></param>
    /// <returns></returns>
    private static int DetermineCopyResult(HeroData hero, ItemData originalBook)
    {
        // 技能类别
        var skillType = originalBook.bookData.DataBase().type;
        // 技能稀有度
        var skillLv = originalBook.bookData.DataBase().rareLv;
        // 书完整度
        var bookRareLv = originalBook.rareLv;
        Plugin.LOG.Msg($"[LYMod3.9.1 抄书] 技能类别：{skillType}, 技能稀有度：{skillLv}, 抄书原本完整度：{bookRareLv}");
        
        // 智力 学识 抄书人对应技能值
        var zhiLi = hero.totalAttri[2];
        var xueShi = hero.totalLivingSkill[2];
        var fightSkill = hero.totalFightSkill[skillType];
        Plugin.LOG.Msg($"[LYMod3.9.1 抄书] 智力：{zhiLi}, 学识：{xueShi}, 相关技能数值：{fightSkill}");

        var text = ColorizeText(GlobalData.AttriRatioString[skillLv + 1], skillLv);
        var needValue = skillLv switch
        {
            4 => 110,
            5 => 120,
            _ => GlobalData.AttriLvNum[skillLv + 1]
        };

        Plugin.LOG.Msg($"[LYMod3.9.1 抄书] 需求数值：{needValue}, text：{text}");
        
        // 检测属性是否达标
        if (zhiLi < needValue)
        {
            OtherHelper.AddInfoTab($"智力属性未达到{text}，只能抄到残本",lastTime:10f);
            return 0;
        }
        // 生活属性最大按120
        if (xueShi < needValue)
        {
            text = skillLv switch
            {
                5 => "<color=#FF0000>120</color>",
                4 => "<color=#FF8C06>110</color>",
                _ => text
            };
            OtherHelper.AddInfoTab($"学识属性未达到{text}，只能抄到残本",lastTime:10f);
            return 0;
        }
        if (fightSkill < needValue)
        {
            OtherHelper.AddInfoTab($"{GlobalData.FightSkillName[skillType]}属性未达到{text}，只能抄到残本",lastTime:10f);
            return 0;
        }
        // 天赋
        var tagValue1 = hero.HaveTag(222) ? 5 : 0;
        var tagValue2 = hero.HaveTag(81) ? 7 : 0;
        var tagValue3 = hero.HaveTag(82) ? 8 : 0;
        var tagValue4 = hero.HaveTag(83) ? 10 : 0;
        var tagScore = tagValue1 + tagValue2 + tagValue3 + tagValue4;

        
        // ====================== 权重计算（核心）======================
        var wBook = Mathf.Min(bookRareLv, 4) * 12f;    // 原本质量（影响最大）
        var wZhiLi = Mathf.Clamp(zhiLi, 0, 150) * 0.4f;     // 智力权重
        var wXueShi = Mathf.Clamp(xueShi, 0, 120) * 0.5f;   // 学识权重（比智力高）
        var wFight = Mathf.Clamp(fightSkill, 0, 150) * 0.6f;// 对应技能（最重要）
        var wTag = Mathf.Clamp(tagScore, 0, 30) * 1.0f;    // 天赋权重
        
        // 最终总分（0~200区间）
        var totalScore = wBook + wZhiLi + wXueShi + wFight + wTag;

        return totalScore switch
        {
            // ====================== 分数 → 等级 ======================
            >= 180 => 4,// 珍本
            >= 140 => 3,// 古本
            >= 100 => 2,// 善本
            >= 60 => 1,// 仿本 
            _ => 0 // 残本
        };
    }

    /// <summary>
    /// 默写计算
    /// </summary>
    /// <param name="hero"></param>
    /// <param name="ksld"></param>
    /// <returns></returns>
    private static int DetermineMemoryResult(HeroData hero, KungfuSkillLvData ksld)
    {
        // 技能类别
        var skillType = ksld.Type();
        // 技能稀有度
        var skillLv = ksld.DataBase().rareLv;
        // 技能修炼到的等级
        var lv = ksld.lv;
        Plugin.LOG.Msg($"[LYMod3.9.1 默写] 技能类别：{skillType}, 技能稀有度：{skillLv}, 技能修炼到的等级：{lv}");
        
        // 智力 学识 抄书人对应技能值
        var zhiLi = hero.totalAttri[2];
        var xueShi = hero.totalLivingSkill[2];
        var fightSkill = hero.totalFightSkill[skillType];
        Plugin.LOG.Msg($"[LYMod3.9.1 默写] 智力：{zhiLi}, 学识：{xueShi}, 相关技能数值：{fightSkill}");
        
        var text = ColorizeText(GlobalData.AttriRatioString[skillLv+1], skillLv);
        var needValue = skillLv switch
        {
            4 => 110,
            5 => 120,
            _ => GlobalData.AttriLvNum[skillLv + 1]
        };
        Plugin.LOG.Msg($"[LYMod3.9.1 默写] 需求数值：{needValue}, text：{text}");
        
        // 检测属性是否达标
        if (zhiLi < needValue)
        {
            OtherHelper.AddInfoTab($"智力属性未达到{text}，只能抄到残本",lastTime:10f);
            return 0;
        }
        // 生活属性最大按120算
        if (xueShi < (skillLv == 5 ? 120 : needValue))
        {
            text = skillLv switch
            {
                5 => "<color=#FF0000>120</color>",
                4 => "<color=#FF8C06>110</color>",
                _ => text
            };
            OtherHelper.AddInfoTab($"学识属性未达到{text}，只能抄到残本",lastTime:10f);
            return 0;
        }
        if (fightSkill < needValue)
        {
            OtherHelper.AddInfoTab($"{GlobalData.FightSkillName[skillType]}属性未达到{text}，只能抄到残本",lastTime:10f);
            return 0;
        }
        
        // 天赋分数
        var tagValue1 = hero.HaveTag(222) ? 5 : 0;
        var tagValue2 = hero.HaveTag(81) ? 7 : 0;
        var tagValue3 = hero.HaveTag(82) ? 8 : 0;
        var tagValue4 = hero.HaveTag(83) ? 10 : 0;
        var tagScore = tagValue1 + tagValue2 + tagValue3 + tagValue4;

        // ========== 权重计算（核心） ==========
        var wLv = Mathf.Clamp(lv, 0, 10) * 8f;          // 武学等级（核心）
        var wFight = Mathf.Clamp(fightSkill, 0, 150) * 0.7f; // 战斗技能（关键）
        var wXueShi = Mathf.Clamp(xueShi, 0, 120) * 0.6f;   // 学识（重要）
        var wZhiLi = Mathf.Clamp(zhiLi, 0, 150) * 0.4f;     // 智力（辅助）
        var wTag = Mathf.Clamp(tagScore, 0, 30) * 1.2f;     // 天赋（增幅）

        // 最终得分
        var total = wLv + wFight + wXueShi + wZhiLi + wTag;

        return total switch
        {
            // ========== 分数 → 完整度等级 ==========
            >= 190 => 4, // 珍本
            >= 150 => 3, // 古本
            >= 110 => 2, // 善本
            >= 70 => 1, // 仿本
            _ => 0
        };
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(BookWriterData), nameof(BookWriterData.GetWorkResult))]
    public static void BookWriterData_GetWorkResult_Postfix(BookWriterData __instance, ItemData __result)
    {
        if (__instance == null || __result == null || !Plugin.Instance.BookWriteChangeFlag.Value) return;
        
        var flag = HeroHelper.TryGetHeroByID(__instance.bookWriterHeroID, out var heroData);
        if (!flag) return;
        switch (__instance.bookWriterType)
        {
            case BookWriterType.Copy:
            {
                var targetBook = __instance.targetBookData;
                if (targetBook == null) return;
                __result.rareLv = DetermineCopyResult(heroData, __instance.targetBookData);
                break;
            }
            case BookWriterType.Memory:
            {
                var targetSkillData = __instance.targetSkillData;
                if (targetSkillData == null) return;
                __result.rareLv = DetermineMemoryResult(heroData, targetSkillData);
                break;
            }
        }
    }
}


#endregion

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
        HeroData targetFavorHero, string _cancelFuc)
    {
        if (_sendResultFuc == "SpeRemoveSkillChoosen" && Plugin.Instance.RemoveAnySkill.Value)
        {
            var player = GameDataController.Instance?.gameSaveData?.WorldData?.Player();
            if (player == null || player.kungfuSkills == null) return;

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
            Plugin.LOG.Msg(_filterType);
            var player = GameDataController.Instance?.gameSaveData?.WorldData?.Player();
            if (player == null || player.kungfuSkills == null) return;
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
    }
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
     [HarmonyPrefix]
     [HarmonyPatch(typeof(IdentifyMatchController), nameof(IdentifyMatchController.SureButtonClicked))]
     public static bool IdentifyMatchController_SureButtonClicked_Prefix(IdentifyMatchController __instance)
     {
         if (__instance != null && __instance.identifyMatchUIPanel != null && Plugin.Instance.AutoJianBaoFlag.Value)
         {
             List<float> list = new List<float>();
             var il2CppArrayBase = __instance.identifyMatchUIPanel.GetComponentsInChildren<ItemIconController>();
             if (il2CppArrayBase is { Length: > 0 })
             {
                 foreach (ItemIconController itemIconController in il2CppArrayBase)
                 {
                     if (itemIconController != null && itemIconController.itemData != null)
                     {
                         list.Add(itemIconController.itemData.GetTreasureRealValue());
                     }
                 }
                 int index = list.IndexOf(list.Max());
                 __instance.SetNowChooseTreasure(il2CppArrayBase[index].gameObject);
             }
         }
         return true;
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


public class HeroTagIconControllerPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ManageTagController), nameof(ManageTagController.CheckMeetCondition))]
    public static void ManageTagController_CheckMeetCondition_Postfix(ManageTagController __instance,
        HeroData checkHero, HeroTagDataBase targetTag, ref bool __result)
    {
        if (__instance != null && Plugin.Instance.AnyTagFlag.Value)
        {
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
    #region 建筑效果倍数

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
            __result.Set(ky.Key, __result.Get(ky.Key) * times);
        }
    }
    // 每月收入
    [HarmonyPostfix]
    [HarmonyPatch(typeof(AreaBuildingData), nameof(AreaBuildingData.GetTotalChangeResource))]
    public static void GetTotalChangeResource_Postfix(AreaBuildingData __instance, 
        Il2CppSystem.Collections.Generic.List<float> __result)
    {
        var area = __instance.GetArea();
        var flag = HeroHelper.TryReadPlayer(out var player);
        if (__instance == null || area.belongForceID != player.belongForceID) return;
        if (!UIBuilderExtensions.BuildingTimesMap.TryGetValue(__instance.buildingID, out int times) || times == 1) return;
        for (var i = 0; i < __result.Count; i++)
        {
            if (__result[i] > 0)
            {
                __result[i] *= times;
            }
        }
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
            b.buildCostTime = 2f;
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
    /// <param name="__instance"></param>
    /// <param name="show"></param>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(AreaBuildController), nameof(AreaBuildController.ShowBuildNewPanel))]
    public static void AreaBuildController_ShowBuildNewPanel_Postfix(AreaBuildController __instance, bool show)
    {
        if (__instance == null || !show || !Plugin.Instance.AddSpeBuildingsFlag.Value) return;
        var buildingIDsToAdd = new List<int> { 42,43,44,45,46,47,48,49,50,51,52,16,18,21,24,26,74,75 };
        foreach (var buildingID in buildingIDsToAdd)
        {
            __instance.GenerateBuildNewButton(buildingID);
        }
    }
    
   
}
// 指定突破加的什么属性
public class BreakThroughChoiceControllerPatch
{
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
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(BreakThroughChoiceController), nameof(BreakThroughChoiceController.OnClick))]
    public static bool BreakThroughChoiceController_OnClick_Postfix(BreakThroughChoiceController __instance)
    {
        if (__instance != null)
        {
            var heroSpeAddData = __instance.extraAddData.heroSpeAddData;
            
            if (Plugin.Instance.BreakFlag)
            {
                heroSpeAddData.Clear();
                heroSpeAddData[int.Parse(Plugin.Instance.BreakType)] = float.Parse(Plugin.Instance.BreakValue);
                Plugin.Instance.BreakFlag = false;
            }
        }
        return true;
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

public class StudySkillPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(StudyDodgeSkillController), nameof(StudyDodgeSkillController.FinishStudyDodgeSkill))]
    public static void StudyDodgeSkillController_FinishStudyDodgeSkill_Postfix(StudyDodgeSkillController __instance,
        StudySkillResult studyDodgeResult)
    {
        if (__instance != null && Plugin.Instance.StudyUniqeRate.Value > 1)
        {
            __instance.totalExp *= Plugin.Instance.StudyUniqeRate.Value;
        }
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(StudyInternalSkillController), nameof(StudyInternalSkillController.FinishStudyInternalSkill))]
    public static void StudyInternalSkillController_FinishStudyInternalSkill_Postfix(StudyInternalSkillController __instance,
        StudyInternalResult studyInternalResult)
    {
        if (__instance != null)
        {
            __instance.totalExp *= Plugin.Instance.StudyUniqeRate.Value;
        }
    }
    [HarmonyPostfix]
    [HarmonyPatch(typeof(StudyUniqueSkillController), nameof(StudyUniqueSkillController.FinishStudyUniqueSkill))]
    public static void StudyUniqueSkillController_FinishStudyUniqueSkill_Postfix(StudyUniqueSkillController __instance,
        StudySkillResult studyUniqueResult)
    {
        if (__instance != null)
        {
            __instance.totalExp *= Plugin.Instance.StudyUniqeRate.Value;
        }
    }
    [HarmonyPostfix]
    [HarmonyPatch(typeof(StudyAttackSkillController), nameof(StudyAttackSkillController.FinishStudyFightSkill))]
    public static void StudyAttackSkillController_FinishStudyFightSkill_Postfix(StudyAttackSkillController __instance,
        StudySkillResult studyDodgeResult)
    {
        if (__instance != null && Plugin.Instance.StudyFightRate.Value > 1)
        {
            __instance.totalExp *= Plugin.Instance.StudyFightRate.Value;
        }
    }
}


public class HeroDataPatch
{
    /// <summary>
    /// 晋升要求不受武学限制数量修改后影响影响
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="__result"></param>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(HeroData), nameof(HeroData.GetUpgradeForceLvNeedSkillNum))]
    public static void HeroData_GetUpgradeForceLvNeedSkillNum_Postfix(HeroData __instance, ref int __result)
    {
        if (ModConfig.HaveNpcMod) return;
        __result /= Plugin.Instance.KungFuMaxLimitTimes.Value;
    }
    
    /// <summary>
    /// 玩家/Npc 最大天赋数量设置
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(HeroData), "GetMaxTagNum")]
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
    #endregion
    
    # region 人物潜力限制开关

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HeroData), nameof(HeroData.ChangeAttri))]
    public static void HeroData_ChangeAttri_Postfix(HeroData __instance, int id, float num, 
        bool showText, bool skillUpgrade)
    {
        if (__instance == null || !skillUpgrade || num <= 0 || __instance.heroID != 0 
            || !Plugin.Instance.BreakMaxLimitFlag.Value) return;
        
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
        if (__instance == null || !skillUpgrade || num <= 0 || __instance.heroID != 0
            || !Plugin.Instance.BreakMaxLimitFlag.Value) return;
        
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
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(HeroData), nameof(HeroData.ChangeLivingSkill))]
    public static void HeroData_ChangeLivingSkill_Postfix(HeroData __instance, int id, float num, 
        bool showText, bool skillUpgrade)
    {
        if (__instance == null || !skillUpgrade || num <= 0 || __instance.heroID != 0
            || !Plugin.Instance.BreakMaxLimitFlag.Value) return;
        
        var baseLivingSkill = __instance.baseLivingSkill;
        var maxLivingSkill = __instance.maxLivingSkill;
        if (baseLivingSkill == null || maxLivingSkill == null) return;
        if (id < 0 || id >= baseLivingSkill.Count || id >= maxLivingSkill.Count) return;
        
        var currentVal = baseLivingSkill[id];
        var maxVal = maxLivingSkill[id];
        if (currentVal > maxVal)
        {
            maxLivingSkill[id] = currentVal;
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
            if (__instance.belongForceID != playerForceId) __result = Plugin.Instance.ExpRateMultiplier.Value;
        }
    }
    /// <summary>
    /// 门派功绩倍率
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="num"></param>
    /// <param name="showInfo"></param>
    /// <param name="targetForce"></param>
    /// <returns></returns>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(HeroData), nameof(HeroData.ChangeForceContribution))]
    public static bool HeroData_ChangeForceContribution_Prefix(HeroData __instance, ref float num, 
        bool showInfo, int targetForce = -1)
    {
        var gc = GameController.Instance;
        if (__instance == null || gc == null || __instance.heroID != 0) return true;
        var forceId = __instance.belongForceID;
        if (forceId != targetForce && Plugin.Instance.ForceContributionRate.Value > 1 && num > 0)
        {
            num *= Plugin.Instance.ForceContributionRate.Value;
        }
        return true;
    }
    
    // 所有门派特性生效
    [HarmonyPostfix]
    [HarmonyPatch(typeof(HeroData), nameof(HeroData.HaveForceFunction))]
    public static void HeroData_HaveForceFunction_Postfix(HeroData __instance,int forceID, ref bool __result)
    {
        if (__instance is { heroID: 0 })
        {
            if (UIBuilderExtensions.EnabledForceIDs.Count == 0)
            {
                UIBuilderExtensions.RefreshForceList();
            }
            if (UIBuilderExtensions.EnabledForceIDs.Contains(forceID)) 
                __result = true;
        }
    }
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
    [HarmonyPostfix]
    [HarmonyPatch(typeof(HeroData), nameof(HeroData.GetMaxFavor))]
    public static void HeroData_GetMaxFavor_Postfix(HeroData __instance, float maxFavor, 
        ref float __result)
    {
        if (__instance != null && Mathf.Approximately(__result, 100) && Plugin.Instance.FavorMax.Value > 100) 
            __result = Plugin.Instance.FavorMax.Value;
    }
    
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
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(HeroData), nameof(HeroData.EquipItem))]
    public static bool HeroData_EquipItem_Prefix(ItemData itemData, bool playSound = false, bool showInfo = false)
    {
        if (Plugin.Instance.EquipmentWeight.Value < 1)
            itemData.weight *= Plugin.Instance.EquipmentWeight.Value;
        return true;
    }

    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(HeroData), "ChangeFavor")]
    public static bool HeroData_ChangeFavor_Prefix(ref float num)
    {
        if (Plugin.Instance.Hgbj.Value && num < 0f)
        {
            num = 0f;
        }

        if (num > 0 && Plugin.Instance.FavorTimes.Value > 1)
        {
            num *= Plugin.Instance.FavorTimes.Value;
        }

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HeroDetailController), nameof(HeroDetailController.ClothChoiceButtonClicked))]
    public static void HeroDetailController_ClothChoiceButtonClicked_Postfix(HeroDetailController __instance, int skinID, int skinLv)
    {
        if (__instance == null) return;
        var n = __instance.nowShowHero;
        if (n == null) return;
        
        int[] cdTable = { 30, 20, 15, 10, 5, 1, 0 };
        var lv = Mathf.Clamp(n.heroForceLv, 0, 6);
        n.changeSkinCd = cdTable[lv];
        
    }

    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(HeroDetailController), nameof(HeroDetailController.Update))]
    public static void HeroDetailController_Update_Postfix(HeroDetailController __instance)
    {
        if (__instance == null || __instance.nowShowHero is not { changeSkinCd: > 0 }) return;
        int[] cdTable = { 30, 15, 10, 5, 3, 0};
        var lv = Mathf.Clamp(__instance.nowShowHero.heroForceLv, 0, 5);
        __instance.nowShowHero.changeSkinCd = cdTable[lv];
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(HeroData), "ChangeBadFame")]
    public static bool HeroData_ChangeBadFame_Prefix(ref float num)
    {
        if (Plugin.Instance.Hgbj.Value && num > 0f)
        {
            num = 0f;
        }
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
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlotController), nameof(PlotController.GenerateAuctionItem))]
    public static bool PlotController_GenerateAuctionItem_Prefix(PlotController __instance,ItemListData targetItemList, 
        ref float shopLv, List<int> itemTypeLimit, ref int itemNum)
    {
        if (__instance != null)
        {
            if (Plugin.Instance.ShopLvRate.Value > 1)
            {
                if (shopLv < 1)
                    shopLv = 1;
                shopLv *= Plugin.Instance.ShopLvRate.Value;
                
            }
            if (Plugin.Instance.ItemNum.Value > -1)
                itemNum = Plugin.Instance.ItemNum.Value;
        }
        return true;
    }
    
    
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
            var heroData = GameDataController.Instance.gameSaveData.WorldData.Player();
            if (__instance.forceID == heroData.GetForce()?.forceID)
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
        bool showInfo = false)
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
    public static bool ForceData_CostResource_Prefix2(ref ResourceData resource, bool showInfo = false)
    {
        if (Plugin.Instance.Cost0.Value)
        {
            resource = new ResourceData(resource.resourceType, 0);
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ForceData), nameof(ForceData.CostResource), typeof(int), typeof(float), typeof(bool))]
    public static bool ForceData_CostResource_Prefix3(int id, ref float num, bool showInfo = false)
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
        __result *= Plugin.Instance.HorseMaxWeightTimes.Value;
    }
    /// <summary>
    /// 马和马鞍视野范围加成倍数
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ItemData), nameof(ItemData.GetHorseSeeRange))]
    public static void ItemData_GetHorseSeeRange_Postfix(ItemData __instance, ref float __result)
    {
        __result *= Plugin.Instance.HorseMaxSeeRangeTimes.Value;
    }
    /// <summary>
    /// 马和马鞍探险耐力加成倍数
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ItemData), nameof(ItemData.GetHorseStepAddRate))]
    public static void ItemData_GetHorseStepAddRate_Postfix(ItemData __instance, ref float __result)
    {
        __result *= Plugin.Instance.HorseStepAddRateTimes.Value;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ItemIconController), nameof(ItemIconController.Update))]
    public static bool ItemIconController_Update_Prefix(ItemIconController __instance)
    {
        if (__instance != null && __instance.itemData.type == ItemType.Treasure 
                               && Plugin.Instance.JianBaoFlag.Value)
        {
            var list = __instance.itemData.treasureData.treasureLv;
            var list1 =  __instance.itemData.treasureData.playerGuessTreasureLv;
            for (int i = 0; i < 4; i++)
            {
                list1[i].Clear();
                list1[i].Add(list[i]);
            }
        }
        return true;
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ItemListData), nameof(ItemListData.GetItem), typeof(ItemData), typeof(bool))]
    public static void ItemListData_GetItem_Postfix(ItemListData? __instance, ItemData targetItem, bool showPopInfo = false)
    {
        if (__instance?.GetHero() != null && __instance.GetHero().heroID == 0)
        {
            // 所有是红品质
            if (Plugin.Instance.RedQuality.Value)
            {
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
                targetItem.value = 9999;
                
                var inputBox = OtherHelper.ParseInputBox(Plugin.Instance.MaterialAttr);
                if (inputBox == null)
                    return;
                var il2CppDictionary = OtherHelper.ToIl2CppDictionary(inputBox);
                if (il2CppDictionary == null)
                    return;
                targetItem.materialData.extraAddData.heroSpeAddData = il2CppDictionary; 
            }
               
            if (targetItem.type == ItemType.Treasure)
            {
                if (Plugin.Instance.RedTreasure.Value)
                {
                    var list = targetItem.treasureData.treasureLv;
                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i] = 5;
                    }
                    targetItem.itemLv = 5;
                    targetItem.value = targetItem.GetTreasureRealValue();
                    targetItem.rareLv = 5;
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

