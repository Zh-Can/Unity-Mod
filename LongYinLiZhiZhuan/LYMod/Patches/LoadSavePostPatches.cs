using HarmonyLib;
using Il2Cpp;
using LYMod.Helpers;
using MelonLoader;

namespace LYMod.Patches;

public class LoadSavePostPatches
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
        
        // 进入存档保存原始天赋数据
        ManageTagControllerPatches.OriginalHeroTagDataBases = GameDataController.Instance.heroTagDataBase;
        
        // 进入存档后自动修改玩家门派衣服为指定衣服
        var flag = HeroHelper.TryReadPlayer(out var player);
        if (Plugin.Instance.SpecifiedSkinId.Value != 99999 && flag && player.belongForceID != -1)
        {
            Plugin.LOG.Msg($"自动设置玩家门派服装为：{Plugin.Instance.SpecifiedSkinId.Value}");
            var heros = player.GetForce().GetOwnHeros();
            for (var i = 0; i < heros.Count; i++)
            {
                heros[i].setSkinID = Plugin.Instance.SpecifiedSkinId.Value;
                heros[i].setSkinLv = heros[i].heroForceLv;
                heros[i].skinID = Plugin.Instance.SpecifiedSkinId.Value;
                heros[i].skinLv = heros[i].heroForceLv;
            }
        }
        
        // 读取自定义难度数据
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
    }
    private static int GetValue(Il2CppSystem.Collections.Generic.Dictionary<int, int> dict, CustomDifficultyType type)
    {
        return dict.TryGetValue((int)type, out var v) ? v : 0;
    }
}