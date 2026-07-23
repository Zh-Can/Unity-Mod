using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Unity.Mono;
using HarmonyLib;
using UnityEngine;
using ZaoHuaBMod.Core;
using ZaoHuaBMod.Core.Adapters;
using ZaoHuaBMod.UI.Core;

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
            Localization.ModDirectory = modDir;
            Localization.ScanLanguages();

            ModConfig.Load();
            ModConfig.ApplyToManager();
            Localization.TryApplyLanguage(ModConfig.Language);

            GameObject uiObj = new GameObject("ModUI");
            UnityEngine.Object.DontDestroyOnLoad(uiObj);
            uiObj.AddComponent<MainView>();

        }
    }
}
