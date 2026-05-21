using HarmonyLib;
using Il2Cpp;
using LYMod.Helpers;
using MelonLoader;

namespace LYMod.Patches;
/// <summary>
/// 初始化数据
/// </summary>
public class LoadSavePostPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameDataController), nameof(GameDataController.Awake))]
    public static void GameDataController_Awake_Postfix(GameDataController __instance)
    {
        // GameDataController初始化后 保存原始天赋数据
        ManageTagControllerPatches.OriginalHeroTagDataBases = GameDataController.Instance.heroTagDataBase;

        if (Plugin.Instance.Relation999Flag.Value)
        {
            GlobalData.MaxLoverNum = 99;
            GlobalData.MaxFriendNum = 99;
            GlobalData.MaxBrotherNum = 99;
        }
        else
        {
            GlobalData.MaxLoverNum = 4;
            GlobalData.MaxBrotherNum = 4;
            GlobalData.MaxFriendNum = 16;
        }
    }
    
    /// <summary>
    /// 读档后触发
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameController), nameof(GameController.GameStartTeleportPlayer))]
    public static void GameController_GameStartTeleportPlayer_Postfix(GameController __instance)
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
                    Plugin.LOG.Msg("【LYMod】由于加载了 NPC管理Mod，LYMod的【天赋上限设置】，【入队时间修改】失效");
                    ModConfig.HaveNpcMod = true;
                    break;
                case "ReadBookPlus":
                    Plugin.LOG.Msg("【LYMod】由于加载了 HaveReadBookPlus，LYMod的 读书显示所有格子 失效");
                    ModConfig.HaveReadBookPlus = true;
                    break;
                case "BookOwnMark":
                    ModConfig.HaveBookOwnMark = true;
                    break;
            }
        }
        Plugin.LOG.Msg("===================================================");
        
        // 游戏进入时自动加载建筑倍率配置
        UIBuilderExtensions.RefreshBuildingList();
        // 修改武学修炼数量限制倍数
        // OtherHelper.ChaneMaxNum();
        
        // 进入存档后自动修改玩家门派衣服为指定衣服
        var flag = HeroHelper.TryReadPlayer(out var player);
        if (Plugin.Instance.SpecifiedSkinId.Value != 99999 && flag && player.belongForceID != -1)
        {
            Plugin.LOG.Msg($"自动设置玩家门派服装为：{Plugin.Instance.SpecifiedSkinId.Value}");
            var skinDataBase = GameDataController.Instance.skinDataBase;
            var skinIdFlag = true;
            foreach (var skin in skinDataBase)
            {
                if (skin.skinID == Plugin.Instance.SpecifiedSkinId.Value)
                {
                    skinIdFlag = false;
                }
            }

            if (skinIdFlag)
            {
                Plugin.LOG.Msg($"写入了错误的皮肤ID:{Plugin.Instance.SpecifiedSkinId.Value}，已被恢复为默认值 99999");
                Plugin.Instance.SpecifiedSkinId.Value = 99999;
                return;
            }
            
            var heros = player.GetForce().GetOwnHeros();
            foreach (var hero in heros)
            {
                hero.setSkinID = Plugin.Instance.SpecifiedSkinId.Value;
                hero.setSkinLv = hero.heroForceLv;
                hero.skinID = Plugin.Instance.SpecifiedSkinId.Value;
                hero.skinLv = hero.heroForceLv;
            }
        }
    }
}