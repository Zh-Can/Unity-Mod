using LunHuiShop;
using Il2CppInterop.Runtime;
using LunHuiShop.GuiFramework.Config;
using LunHuiShop.GuiFramework.Localization;
using LunHuiShop.GuiFramework.Logger;
using LunHuiShop.GuiFramework.Logger.Adapters;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(LunHuiShop.LunHuiShop), ModInfo.Name, ModInfo.Version, ModInfo.Author)]
[assembly: MelonGame(ModInfo.Developer, ModInfo.DeveloperName)]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]

namespace LunHuiShop;

public class LunHuiShop : MelonMod
{
    public static LunHuiShop Instance;
    // 窗体对象
    private GameObject _uiObj;

    private static MelonPreferences_Category _mainCategory;

    public override void OnInitializeMelon()
    {
        Instance = this;

        InitConfig();
        var harmony = new HarmonyLib.Harmony("LunHuiShop");
        harmony.PatchAll();

        BaseConfig.Load();
        BaseConfig.ApplyToManager();

        // 初始化 Mod 目录与配置
        Loc.ModDirectory = Path.GetDirectoryName(MelonAssembly.Assembly.Location);
        Loc.ScanLanguages();
        Loc.TryApplyLanguage(BaseConfig.Language);
        Log.Initialize(new MelonLoggerAdapter());
        Log.Info("轮回商店 加载完成！~");
    }

    private void InitConfig()
    {
        
    }

    public static void SaveConfig()
    {
        _mainCategory.SaveToFile();
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (_uiObj == null)
        {
            _uiObj = new GameObject("LunHuiShopUI");
            UnityEngine.Object.DontDestroyOnLoad(_uiObj);
            _uiObj.AddComponent(Il2CppType.Of<MainView>());
            Log.Info("LunHuiShopUI 已在场景加载后创建");
        }
    }

    
}