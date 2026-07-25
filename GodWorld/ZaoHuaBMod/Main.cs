using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Unity.Mono;
using HarmonyLib;
using UnityEngine;
using ZaoHuaBMod.GuiFramework.Config;
using ZaoHuaBMod.GuiFramework.Localization;
using ZaoHuaBMod.GuiFramework.Logger;
using ZaoHuaBMod.GuiFramework.Logger.Adapters;

namespace ZaoHuaBMod
{
    [BepInPlugin(ModInfo.Guid, ModInfo.Name, ModInfo.Version)]
    public class Main : BaseUnityPlugin
    {
        public static Main Instance;

        private void Start()
        {
            Instance = this;
            Harmony.CreateAndPatchAll(typeof(Main));
            
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
    }
}
