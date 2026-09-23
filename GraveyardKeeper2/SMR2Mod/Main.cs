using System.IO;
using System.Reflection;
using BepInEx;
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

        private void Awake()
        {
            Instance = this;
            ConfinInit();
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
            
        }
        
        private void ConfinInit()
        {
            
        }
    }
}