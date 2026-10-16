using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using SMR2Mod.GuiFramework.Config;
using SMR2Mod.GuiFramework.Localization;
using SMR2Mod.GuiFramework.Logger;
using SMR2Mod.GuiFramework.Logger.Adapters;
using UnityEngine;

namespace SMR2Mod
{
    [BepInPlugin(ModInfo.Name, ModInfo.Name, ModInfo.Version)]
    public class Main : BaseUnityPlugin
    {
        public static Main Instance;

        //配置
        public static ConfigEntry<bool> CombatGodMode;
        public static ConfigEntry<bool> InfiniteCombatStamina;
        public static ConfigEntry<bool> InfiniteEnergy;
        public static ConfigEntry<bool> LockInsanity;
        public static ConfigEntry<bool> FullHappiness;
        public static ConfigEntry<bool> OneHitWork;


        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            Harmony.CreateAndPatchAll(typeof(GamePatches)); 
            // 初始化日志：
            // BepInEx 入口用 new Adapters.BepInExLogger(Logger)
            // MelonLoader 入口用 new Adapters.MelonLoggerAdapter()
            // 不初始化则默认走 UnityDebugLogger（Unity Console）
            Log.Initialize(new BepInExLogger(Logger));
            Log.Info("ZaoHuaBMod Loaded!");
            
            // 初始化 Mod 目录与配置
            var modDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Loc.ModDirectory = modDir;
            Loc.ScanLanguages();
            
            BaseConfig.Load();
            BaseConfig.ApplyToManager();
            Loc.TryApplyLanguage(BaseConfig.Language);

            GameObject uiObj = new GameObject("ModUI");
            UnityEngine.Object.DontDestroyOnLoad(uiObj);
            uiObj.AddComponent<MainView>();
            
            ConfigInit();
        }
        
        private void ConfigInit()
        {
            CombatGodMode = Config.Bind("Toggles", "CombatGodMode", false, "战斗时血量无敌锁血");
            InfiniteCombatStamina = Config.Bind("Toggles", "InfiniteCombatStamina", false, "战斗时体力无限");
            InfiniteEnergy = Config.Bind("Toggles", "InfiniteEnergy", false, "体力无限");
            LockInsanity = Config.Bind("Toggles", "LockInsanity", false, "疯狂锁定");
            FullHappiness = Config.Bind("Toggles", "FullHappiness", false, "小镇谢礼总是满值");
            OneHitWork = Config.Bind("Toggles", "OneHitWork", false, "一次完成工作内容");
        }
        
    }
}