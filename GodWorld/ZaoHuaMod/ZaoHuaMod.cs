using System.Collections.Generic;
using HarmonyLib;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
using ZaoHuaMod;
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
        
        // 窗体对象
        private GameObject _uiObj;
        
        public override void OnInitializeMelon()
        {
            Instance = this;

            InitConfig();
            
            var harmony = new HarmonyLib.Harmony("ZHMod");
            harmony.PatchAll(typeof(ZaoHuaMod));
            
            Log.Initialize(new MelonLoggerAdapter());
            Log.Info("ZaoHuaMod 加载完成！~");
            
        }

        private void InitConfig()
        {
            _mainCategory = MelonPreferences.CreateCategory("ZaoHuaModConfig", "功能配置");
            _mainCategory.SetFilePath(MelonEnvironment.UserDataDirectory + "\\ZaoHuaMod.cfg");
            ChooseCountFlag = _mainCategory.CreateEntry("chooseCountFlag", false,  description: "开局选择点数修改99开关");
            ZhCountFlag = _mainCategory.CreateEntry("zhCountFlag", false,  description: "轮回商店9999点数");
            AllSkillFlag = _mainCategory.CreateEntry("allSkillFlag", false,  description: "炼丹有能解锁两列的技能");
            MaxPlotCountFlag = _mainCategory.CreateEntry("maxPlotCountFlag", false,  description: "神器鼎地块扩增至100");
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
        /// 预览建筑影响范围
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PastureImpl), "GetTargetPlotPosList")]
        public static void PastureImpl_GetTargetPlotPosList_Prefix(TbPastureBuildCfg buildCfg, MyVector2Int curCellPos)
        {
            if (buildCfg.effectRangeType != 0)
            {
                buildCfg.effectRangeType = 2;
            }
        }
        /// <summary>
        /// 建筑影响范围修改
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PastureImpl), "GetTargetBuildStoList")]
        public static void PastureImpl_GetTargetBuildStoList_Prefix(TbPastureBuildSto buildSto, ref int effectRangeType)
        {
            // 只要有范围效果，就改成全场
            if (effectRangeType != 0)
            {
                effectRangeType = 2;
            }
        }
    }
}