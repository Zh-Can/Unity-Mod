using System.IO;
using System.Reflection;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
using ZaoHuaBMod;
using ZaoHuaBMod.GuiFramework.Config;
using ZaoHuaBMod.GuiFramework.Localization;
using ZaoHuaBMod.GuiFramework.Logger;
using ZaoHuaBMod.GuiFramework.Logger.Adapters;
using ZaoHuaBMod.GuiFramework.Other;

[assembly: MelonInfo(typeof(ZaoHuaBMod.Main), ModInfo.Name, ModInfo.Version, ModInfo.Author)]
[assembly: MelonGame(ModInfo.Developer, ModInfo.DeveloperName)]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.MONO)]
namespace ZaoHuaBMod
{
    public class Main : MelonMod
    {
        public static Main Instance;
        
        public override void OnInitializeMelon()
        {
            Instance = this;
            var harmony = new HarmonyLib.Harmony("ZHMod1");
            harmony.PatchAll(typeof(Main));
            
            // 初始化日志：
            // 1.不初始化则默认走 UnityDebugLogger（Unity Console）
            // 2.BepInEx 入口用 new Adapters.BepInExLogger(Logger)
            // Log.Initialize(new BepInExLogger(Logger));
            // 3.MelonLoader 入口用 new Adapters.MelonLoggerAdapter()
            Log.Initialize(new MelonLoggerAdapter());
            Log.Info("ZaoHuaBMod Loaded!");

            // 初始化 Mod 目录与配置
            var modDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Loc.ModDirectory = modDir;
            Loc.ScanLanguages();

            BaseConfig.Load();
            BaseConfig.ApplyToManager();
            Loc.TryApplyLanguage(BaseConfig.Language);
            
            ConfinInit();
        }
        private GameObject _uiObj;
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (_uiObj == null)
            {
                _uiObj = new GameObject("ModUI");
                UnityEngine.Object.DontDestroyOnLoad(_uiObj);
                _uiObj.AddComponent<MainView>();
                Log.Info("ModUI 已在场景加载后创建");
            }
        }

        public MelonPreferences_Category MainCategory;
        public MelonPreferences_Entry<bool> TestFlag;
        private void ConfinInit()
        {
            MainCategory = MelonPreferences.CreateCategory("ZaoHuaModConfig", "功能配置");
            MainCategory.SetFilePath(MelonEnvironment.UserDataDirectory + $"\\{ModInfo.Name}.cfg");
            TestFlag = MainCategory.CreateEntry("_testFlag", false,  description: "测试");
        }

        public void SaveConfig()
        {
            MainCategory.SaveToFile();
        }
    }
}

// bepinex

// using System.IO;
// using System.Reflection;
// using BepInEx;
// using BepInEx.Unity.Mono;
// using HarmonyLib;
// using UnityEngine;
// using ZaoHuaBMod.GuiFramework.Config;
// using ZaoHuaBMod.GuiFramework.Localization;
// using ZaoHuaBMod.GuiFramework.Logger;
// using ZaoHuaBMod.GuiFramework.Logger.Adapters;
//
// namespace ZaoHuaBMod
// {
//     [BepInPlugin(ModInfo.Guid, ModInfo.Name, ModInfo.Version)]
//     public class Main : BaseUnityPlugin
//     {
//         public static Main Instance;
//         private void Awake()
//         {
//             ConfinInit();
//         }
//         private void Start()
//         {
//             Instance = this;
//             Harmony.CreateAndPatchAll(typeof(Main));
//             
//             // 初始化日志：
//             // BepInEx 入口用 new Adapters.BepInExLogger(Logger)
//             // MelonLoader 入口用 new Adapters.MelonLoggerAdapter()
//             // 不初始化则默认走 UnityDebugLogger（Unity Console）
//             Log.Initialize(new BepInExLogger(Logger));
//             Log.Info("ZaoHuaBMod Loaded!");
//
//             // 初始化 Mod 目录与配置
//             var modDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
//             Loc.ModDirectory = modDir;
//             Loc.ScanLanguages();
//
//             BaseConfig.Load();
//             BaseConfig.ApplyToManager();
//             Loc.TryApplyLanguage(BaseConfig.Language);
//
//             GameObject uiObj = new GameObject("ModUI");
//             UnityEngine.Object.DontDestroyOnLoad(uiObj);
//             uiObj.AddComponent<MainView>();
//
//         }
//     }
// }
