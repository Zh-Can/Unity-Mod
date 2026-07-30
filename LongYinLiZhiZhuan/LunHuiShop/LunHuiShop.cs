using LunHuiShop;
using Il2CppInterop.Runtime;
using LunHuiShop.GuiFramework.Config;
using LunHuiShop.GuiFramework.Localization;
using LunHuiShop.GuiFramework.Controls;
using LunHuiShop.GuiFramework.Logger;
using LunHuiShop.GuiFramework.Logger.Adapters;
using MelonLoader;
using UnityEngine;
using System.Reflection;
using Il2Cpp;

[assembly: MelonInfo(typeof(LunHuiShop.LunHuiShop), ModInfo.Name, ModInfo.Version, ModInfo.Author)]
[assembly: MelonGame(ModInfo.Developer, ModInfo.DeveloperName)]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]

namespace LunHuiShop;

public class LunHuiShop : MelonMod
{
    public static LunHuiShop Instance = null!;
    // 窗体对象
    private GameObject _uiObj = null!;

    private static readonly MelonPreferences_Category MainCategory = null!;

    public override void OnInitializeMelon()
    {
        Instance = this;

        // 尽早初始化日志，确保后续 Log.Warning/Info 能输出
        Log.Initialize(new MelonLoggerAdapter());

        InitConfig();
        var harmony = new HarmonyLib.Harmony("LunHuiShop");
        harmony.PatchAll(typeof(UIPatches));
        harmony.PatchAll(typeof(GamePatches));

        // 手工注册运行时补丁（运行时类型查找，避免 TypeLoadException）
        RegisterBuildQuickButtonPatch(harmony);
        RegisterMouseControllerPatch(harmony);
        RegisterAreaControllerPatch(harmony);

        BaseConfig.Load();
        BaseConfig.ApplyToManager();

        // 初始化 Mod 目录与配置
        Loc.ModDirectory = Path.GetDirectoryName(MelonAssembly.Assembly.Location)!;
        Loc.ScanLanguages();
        Loc.TryApplyLanguage(BaseConfig.Language);
        
        // 初始化
        
        Log.Info("加载完成！~");
    }

    
    private void InitConfig()
    {
        
    }

    public static void SaveConfig()
    {
        MainCategory.SaveToFile();
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

    /// <summary>
    ///     每帧清理 MouseController 实例的 hoveredObject 状态。
    ///     注意：不清理 UICamera.hoveredObject（清理它会导致 MouseController.ProcessEvents
    ///     调用 ProcessMouse，适得其反）。
    /// </summary>
    public override void OnUpdate()
    {
        if (UI.WindowControls.ShouldBlockGamePointerInput())
        {
            UIPatches.ClearHoveredState();
        }

        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            foreach (var a in GlobalData.DecorationTypeName)
            {
                Log.Info(a);
            }
            foreach (var a in GlobalData.HorseTypeName)
            {
                Log.Info(a);
            }
        }
    }


    #region 防止点击穿透处理

    private static void RegisterBuildQuickButtonPatch(HarmonyLib.Harmony harmony)
    {
        try
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "Assembly-CSharp");
            if (asm == null)
            {
                Log.Warning("Assembly-CSharp 未找到，跳过建筑追踪拦截");
                return;
            }

            // Assembly.GetType 按名称查找类型（仅元数据操作），不触发全程序集扫描
            var type = asm.GetType("Il2Cpp.BuildQuickButtonController")
                       ?? asm.GetType("BuildQuickButtonController");
            if (type == null)
            {
                Log.Warning("BuildQuickButtonController 类型未找到，跳过建筑追踪拦截");
                return;
            }

            // 1) 注册 Update 前缀
            var updateMethod = type.GetMethod("Update",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (updateMethod != null)
            {
                var prefixMethod = typeof(UIPatches).GetMethod(
                    nameof(UIPatches.BuildQuickButtonController_Update_Prefix),
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (prefixMethod != null)
                {
                    harmony.Patch(updateMethod, prefix: new HarmonyLib.HarmonyMethod(prefixMethod));
                    Log.Info("已注册 BuildQuickButtonController.Update 防穿透 Patch");
                }
            }

            // 2) 注册 OnHover 前缀——NGUI 在悬停时会发 OnHover(bool) 消息，
            //    这个方法在 Mod 激活时跳过它，阻止 onHover 被设置。
            var onHoverMethod = type.GetMethod("OnHover",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (onHoverMethod != null)
            {
                var onHoverPrefix = typeof(UIPatches).GetMethod(
                    nameof(UIPatches.BuildQuickButtonController_OnHover_Prefix),
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (onHoverPrefix != null)
                {
                    harmony.Patch(onHoverMethod, prefix: new HarmonyLib.HarmonyMethod(onHoverPrefix));
                    Log.Info("已注册 BuildQuickButtonController.OnHover 防穿透 Patch");
                }
            }
        }
        catch (Exception ex)
        {
            Log.Warning($"注册 BuildQuickButtonController Patch 失败: {ex.Message}");
        }
    }

    private static void RegisterMouseControllerPatch(HarmonyLib.Harmony harmony)
    {
        try
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "Assembly-CSharp");
            if (asm == null)
            {
                Log.Warning("Assembly-CSharp 未找到，跳过 MouseController 拦截");
                return;
            }

            var type = asm.GetType("Il2Cpp.MouseController")
                       ?? asm.GetType("MouseController");
            if (type == null)
            {
                Log.Warning("MouseController 类型未找到，跳过 MouseController 拦截");
                return;
            }

            var updateMethod = type.GetMethod("Update",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (updateMethod == null)
            {
                Log.Warning("MouseController.Update 方法未找到，跳过 MouseController 拦截");
                return;
            }

            var prefixMethod = typeof(UIPatches).GetMethod(
                nameof(UIPatches.MouseController_Update_Prefix),
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            if (prefixMethod == null)
            {
                Log.Warning("MouseController_Update_Prefix 方法未找到");
                return;
            }

            harmony.Patch(updateMethod, prefix: new HarmonyLib.HarmonyMethod(prefixMethod));
            Log.Info("已注册 MouseController.Update 防穿透 Patch");
        }
        catch (Exception ex)
        {
            Log.Warning($"注册 MouseController.Update Patch 失败: {ex.Message}");
        }
    }

    private static void RegisterAreaControllerPatch(HarmonyLib.Harmony harmony)
    {
        try
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "Assembly-CSharp");
            if (asm == null) return;

            var type = asm.GetType("Il2Cpp.AreaController")
                       ?? asm.GetType("AreaController");
            if (type == null)
            {
                Log.Warning("AreaController 类型未找到，跳过 FocusOnTarget 拦截");
                return;
            }

            // IDA 显示签名：FocusOnTarget(GameObject target, float maxScale)
            // 使用 GetMethods + 按参数数过滤，避免重载歧义
            var focusMethod = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(m => m.Name == "FocusOnTarget" && m.GetParameters().Length == 2);
            if (focusMethod == null)
            {
                Log.Warning("AreaController.FocusOnTarget(GameObject, float) 方法未找到，跳过");
                return;
            }

            var prefixMethod = typeof(UIPatches).GetMethod(
                nameof(UIPatches.AreaController_FocusOnTarget_Prefix),
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (prefixMethod == null)
            {
                Log.Warning("AreaController_FocusOnTarget_Prefix 方法未找到");
                return;
            }

            harmony.Patch(focusMethod, prefix: new HarmonyLib.HarmonyMethod(prefixMethod));
            Log.Info("已注册 AreaController.FocusOnTarget 防穿透 Patch");
        }
        catch (Exception ex)
        {
            Log.Warning($"注册 AreaController.FocusOnTarget Patch 失败: {ex.Message}");
        }
    }

    #endregion
}