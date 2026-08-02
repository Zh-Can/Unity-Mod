using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using HarmonyLib;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
using ZaoHuaMod;
using ZaoHuaMod.GuiFramework.Config;
using ZaoHuaMod.GuiFramework.Controls;
using ZaoHuaMod.GuiFramework.Localization;
using ZaoHuaMod.GuiFramework.Logger;
using ZaoHuaMod.GuiFramework.Logger.Adapters;

[assembly: MelonInfo(typeof(ZaoHuaMod.ZaoHuaMod), ModInfo.Name, ModInfo.Version, ModInfo.Author)]
[assembly: MelonGame(ModInfo.Developer, ModInfo.DeveloperName)]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.MONO)]

namespace ZaoHuaMod
{
    public class ZaoHuaMod : MelonMod
    {

        public static ZaoHuaMod Instance;

        internal static string RefreshType = "";

        private static MelonPreferences_Category _mainCategory;
        internal static MelonPreferences_Entry<bool> ChooseCountFlag;
        internal static MelonPreferences_Entry<bool> ZhCountFlag;
        internal static MelonPreferences_Entry<bool> AllSkillFlag;
        internal static MelonPreferences_Entry<bool> MaxPlotCountFlag;
        internal static MelonPreferences_Entry<bool> BuildStoFlag;
        internal static MelonPreferences_Entry<bool> DrugProfitLabelFlag;

        // 小世界建筑效果倍率
        internal static MelonPreferences_Entry<int> GrowSpeedMultiplier;  // 灵泉/火炼池
        internal static MelonPreferences_Entry<int> CountMultiplier;      // 灵枢台
        internal static MelonPreferences_Entry<int> JuLingMultiplier;     // 聚灵台
        internal static MelonPreferences_Entry<int> LingChiMultiplier;    // 灵池

        // 灵泉/火炼池描述原始文本备份（避免倍率改回后描述无法还原）
        private static string _originEffDes4Chinese;
        private static string _originEffDes6Chinese;
        private static string _originEffDes4Traditional;
        private static string _originEffDes6Traditional;
        private static string _originEffDes4English;
        private static string _originEffDes6English;

        // 灵枢台描述原始文本备份（effDes key = 2_5）
        private static string _originEffDes5Chinese;
        private static string _originEffDes5Traditional;
        private static string _originEffDes5English;

        // 聚灵台描述原始文本备份（effDes key = 2_10）
        private static string _originEffDes10Chinese;
        private static string _originEffDes10Traditional;
        private static string _originEffDes10English;

        // 窗体对象
        private GameObject _uiObj;
        
        public override void OnInitializeMelon()
        {
            Instance = this;

            InitConfig();
            
            var harmony = new HarmonyLib.Harmony("ZHMod");
            harmony.PatchAll(typeof(ZaoHuaMod));
            harmony.PatchAll(typeof(GameInputBlocker));
            
            BaseConfig.Load();
            BaseConfig.ApplyToManager();
            
            // 初始化 Mod 目录与配置
            var modDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Loc.ModDirectory = modDir;
            Loc.ScanLanguages();
            Loc.TryApplyLanguage(BaseConfig.Language);
            Log.Initialize(new MelonLoggerAdapter());
            Log.Info("ZaoHuaMod 加载完成！更新日期：2026-07-31");
        }

        private void InitConfig()
        {
            _mainCategory = MelonPreferences.CreateCategory("ZaoHuaModConfig", "功能配置");
            _mainCategory.SetFilePath(MelonEnvironment.UserDataDirectory + "\\ZaoHuaMod.cfg");
            ChooseCountFlag = _mainCategory.CreateEntry("chooseCountFlag", false,  description: "开局选择点数修改99开关");
            ZhCountFlag = _mainCategory.CreateEntry("zhCountFlag", false,  description: "轮回商店9999点数");
            AllSkillFlag = _mainCategory.CreateEntry("allSkillFlag", false,  description: "炼丹有能解锁两列的技能");
            MaxPlotCountFlag = _mainCategory.CreateEntry("maxPlotCountFlag", false,  description: "神器鼎地块扩增至100");
            BuildStoFlag = _mainCategory.CreateEntry("buildStoFlag", false,  description: "神器鼎地块建筑范围全覆盖开关");
            DrugProfitLabelFlag = _mainCategory.CreateEntry("drugProfitLabelFlag", true, description: "显示炼丹售价");
            GrowSpeedMultiplier = _mainCategory.CreateEntry("growSpeedMultiplier", 1, description: "灵泉/火炼池生长速度倍率（1=原生+100%，2=+200%，以此类推）");
            CountMultiplier = _mainCategory.CreateEntry("countMultiplier", 1, description: "灵枢台产量倍率");
            JuLingMultiplier = _mainCategory.CreateEntry("juLingMultiplier", 1, description: "聚灵台增幅倍率");
            LingChiMultiplier = _mainCategory.CreateEntry("lingChiMultiplier", 1, description: "灵池灵鱼成长倍率");
        }
        
        public static void SaveConfig()
        {
            _mainCategory.SaveToFile();
        }
        
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (_uiObj == null)
            {
                _uiObj = new GameObject("ZaoHuaModUI");
                UnityEngine.Object.DontDestroyOnLoad(_uiObj);
                _uiObj.AddComponent<MainView>();
                Log.Info("ZaoHuaModUI 已在场景加载后创建");
            }
        }

        #region 刷新商店/交易，悬赏

        /// <summary>
        /// 商店打开
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(TradePanel), nameof(TradePanel.ShowMe))]
        public static void TradePanel_ShowMe_Postfix(TradePanel __instance)
        {
            RefreshType = "Trade";
            MainView.RefreshButtonWindow?.Show();
        }
        /// <summary>
        /// 商店关闭
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(TradePanel), nameof(TradePanel.HideMe))]
        public static void TradePanel_HideMe_Postfix(TradePanel __instance)
        {
            MainView.RefreshButtonWindow?.Hide();
            RefreshType = "";
        }
        /// <summary>
        /// 刷新商店
        /// </summary>
        internal void RefreshTrades()
        {
            int curShopId = Singleton<TbShopImpl>.Instance.curShopId;
            
            TbShopSto shopSto = Singleton<TbShopImpl>.Instance.GetShopSto(curShopId);
            // log.Msg($"curShopId:{curShopId}，npcStoId:{shopSto.npcStoId}");
            if (shopSto.npcStoId != 0)
            {
                Singleton<TbNpcImpl>.Instance.RefresNpcTradeItem(shopSto.npcStoId, true);
            }
            TradePanel tradePanel = MonoSingleton<UIMgr>.Instance.GetPanel<TradePanel>("TradePanel");
            Singleton<TbShopImpl>.Instance.SetShopItemStos(shopSto);
            tradePanel.RefreshShopItems();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(DeaconTaskPanel), nameof(DeaconTaskPanel.ShowMe))]
        public static void DeaconTaskPanel_ShowMe_Postfix(DeaconTaskPanel __instance)
        {
            RefreshType = "DeaconTask";
            MainView.RefreshButtonWindow?.Show();
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(DeaconTaskPanel), nameof(DeaconTaskPanel.HideMe))]
        public static void DeaconTaskPanel_HideMe_Postfix(DeaconTaskPanel __instance)
        {
            RefreshType = "";
            MainView.RefreshButtonWindow?.Hide();
        }
        
        private static int _lastBuildId;
        private static int _lastFunctionId;
        private static int _lastNameId;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(TbTaskImpl), nameof(TbTaskImpl.UpdateDeaconTask))]
        public static void TbTaskImpl_UpdateDeaconTask_Postfix(TbTaskImpl __instance, int buildId, 
            int functionId, int nameId)
        {
            _lastBuildId = buildId;
            _lastFunctionId = functionId;
            _lastNameId = nameId;
        }
        /// <summary>
        /// 刷新悬赏
        /// </summary>
        internal void RefreshDeaconTasks()
        {
            DeaconTaskPanel panel = MonoSingleton<UIMgr>.Instance.GetPanel<DeaconTaskPanel>("DeaconTaskPanel");
            
            string mapId = Singleton<TbPlayerImpl>.Instance.GetMapStoId();
            MyVector2Int cellPosition = Singleton<TbPlayerImpl>.Instance.GetMapInfoStoId(true);

            // 找出当前建筑/功能下未接受的悬赏任务并删除
            List<TbDeaconTaskSto> toRemove = BsSaveDataImpl.NowActor.deaconTaskStoList.FindAll(deaSto =>
                deaSto.mapStoId == mapId
                && deaSto.cellPosition == cellPosition
                && deaSto.buildId == _lastBuildId
                && deaSto.functionId == _lastFunctionId
                && BsSaveDataImpl.NowActor.playerTaskStoList.Find(pt => pt.deaconTaskId == deaSto.id) == null);

            foreach (TbDeaconTaskSto task in toRemove)
            {
                BsSaveDataImpl.NowActor.deaconTaskStoList.Remove(task);
            }

            // 重新生成一批
            Singleton<TbTaskImpl>.Instance.UpdateDeaconTask(_lastBuildId, _lastFunctionId, _lastNameId);

            // 刷新面板 UI
            panel.UpdateShowTask();
        }
        #endregion
        

        /// <summary>
        ///     开局选择点
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CreateRolePanel), "Switchboard")]
        public static void CreateRolePanel_Switchboard_Postfix(CreateRolePanel __instance, int index)
        {
            if (__instance != null && ChooseCountFlag.Value)
                Traverse.Create(__instance)
                    .Field("chooseCount")
                    .SetValue(99);
        }

        /// <summary>
        ///     造化商店
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(AchieveImpl), "GetendRewardCoins")]
        public static void AchieveImpl_GetendRewardCoins_Postfix(AchieveImpl __instance, ref int __result)
        {
            if (ZhCountFlag.Value) __result = 9999;
        }
        
        /// <summary>
        /// 炼丹两个都列技能都解锁
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SideTalentSmallCell), "SetState")]
        public static bool SideTalentSmallCell_SetState_Prefix(SideTalentSmallCell __instance, ref int states)
        {
            if (!AllSkillFlag.Value) return true;
            
            // 只拦截炼丹分支天赋被设为“锁定”的情况
            if (states != -1
                || __instance.treeCfg == null
                || __instance.treeCfg.sub != 1
                || __instance.treeCfg.group == 0)
            {
                return true;
            }

            // 已经是可解锁/已解锁，禁止被同行另一个已解锁的顶回锁定
            if (__instance.State == 1 || __instance.State == 2)
            {
                return false; // 跳过原始 SetState，保持当前状态
            }

            // 旧存档里被锁成 -1 的，改成可解锁
            if (__instance.State == -1)
            {
                states = 1;
                return true;
            }

            // 其他情况（比如等级不够是 0）保持原状
            return false;
        }
        
        /// <summary>
        /// 神器鼎-地块扩至100
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PastureImpl), "GetMaxPlotCount")]
        public static void PastureImpl_GetMaxPlotCount_Postfix(PastureImpl __instance, ref int __result)
        {
            if (MaxPlotCountFlag.Value) __result = 100;
        }

        /// <summary>
        /// 预览建筑影响范围（BuildStoFlag 开启时全场覆盖）
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PastureImpl), "GetTargetPlotPosList")]
        public static void PastureImpl_GetTargetPlotPosList_Prefix(TbPastureBuildCfg buildCfg, MyVector2Int curCellPos)
        {
            // Log.Info($"{buildCfg.id} - {buildCfg.GetName} - {buildCfg.effectRangeType} - {buildCfg.effDes}");
            if (buildCfg.effectRangeType <= 1) return;
            buildCfg.effectRangeType = BuildStoFlag.Value ? 2 : 11;
        }
        /// <summary>
        /// 建筑影响范围修改（BuildStoFlag 开启时全场覆盖）
        /// - ``1`` ：只影响自身- ``2`` ：全图所有建筑- ``11~20`` ：菱形范围（num = effectRangeType - 10），找到该建筑占据的地块，扩展菱形- ``21~30`` ：方形范围
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PastureImpl), "GetTargetBuildStoList")]
        public static void PastureImpl_GetTargetBuildStoList_Prefix(TbPastureBuildSto buildSto, ref int effectRangeType)
        {
            if (effectRangeType <= 1) return;
            effectRangeType = BuildStoFlag.Value ? 2 : 11;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PastureImpl), nameof(PastureImpl.RefreshPastureEffect))]
        public static void PastureImpl_RefreshPastureEffect_Postfix()
        {
            if (BsSaveDataImpl.nowActor?.pastureBuildStoList == null) return;

            var langDic = MonoSingleton<TbLanguageImpl>.Instance.GetLanguageDic("TbPastureBuildCfgLocal");
            
            // TbPastureBuildCfg pastureBuildCfg = Singleton<TbDataImpl>.Instance.GetPastureBuildCfg(5);
            // Log.Info($"{pastureBuildCfg.effDes} - {pastureBuildCfg.GetEffDes}");
            
            // 局部函数：备份并替换某个 effDes key 的三语言描述
            void ApplyEffDes(string key, string oldText, string newText, ref string originChinese, ref string originTraditional, ref string originEnglish)
            {
                if (originChinese == null) originChinese = langDic[key].Chinese;
                if (originTraditional == null) originTraditional = langDic[key].TraditionalChinese;
                if (originEnglish == null) originEnglish = langDic[key].English;

                langDic[key].Chinese = originChinese.Replace(oldText, newText);
                langDic[key].TraditionalChinese = originTraditional.Replace(oldText, newText);
                langDic[key].English = originEnglish.Replace(oldText, newText);
            }

            // 灵泉/火炼池：+1 → +N
            ApplyEffDes("2_4", "1", GrowSpeedMultiplier.Value.ToString(), ref _originEffDes4Chinese, ref _originEffDes4Traditional, ref _originEffDes4English);
            ApplyEffDes("2_6", "1", GrowSpeedMultiplier.Value.ToString(), ref _originEffDes6Chinese, ref _originEffDes6Traditional, ref _originEffDes6English);

            // 灵枢台：+1 → +N
            ApplyEffDes("2_5", "1", CountMultiplier.Value.ToString(), ref _originEffDes5Chinese, ref _originEffDes5Traditional, ref _originEffDes5English);

            // 聚灵台：50 → 50 * N
            int juLingBonus = JuLingMultiplier.Value * 50;
            Log.Info($"聚灵台描述修改: multiplier={JuLingMultiplier.Value}, bonus={juLingBonus}, " +
                     $"origin={_originEffDes10Chinese ?? langDic["2_10"].Chinese}, " +
                     $"after={langDic["2_10"].Chinese.Replace("50", juLingBonus.ToString())}");
            ApplyEffDes("2_10", "50", juLingBonus.ToString(), ref _originEffDes10Chinese, ref _originEffDes10Traditional, ref _originEffDes10English);

            foreach (var buildSto in BsSaveDataImpl.nowActor.pastureBuildStoList)
            {
                if (buildSto.updateGrowSpeed > 0)
                {
                    buildSto.updateGrowSpeed = GrowSpeedMultiplier.Value * 100;
                }
                if (buildSto.updateGrowCount > 0)
                {
                    buildSto.updateGrowCount = CountMultiplier.Value;
                }
            }
        }

        /// <summary>
        /// 修炼界面聚灵台增幅倍率修改
        /// 原逻辑：GetBuildPlacedCount(10) 返回实际数量，每个 +50
        /// 改为：对 buildId=10 返回 数量 * JuLingMultiplier，让修炼界面自己算出正确数值
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PastureImpl), nameof(PastureImpl.GetBuildPlacedCount))]
        public static void PastureImpl_GetBuildPlacedCount_Postfix(int buildId, ref int __result)
        {
            if (buildId == 10 && __result > 0)
                __result *= JuLingMultiplier.Value;
        }
    }
}
