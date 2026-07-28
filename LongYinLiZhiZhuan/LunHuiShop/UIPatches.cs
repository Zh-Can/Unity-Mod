using System.Reflection;
using HarmonyLib;
using UnityEngine;
using LunHuiShop.GuiFramework.Controls;

namespace LunHuiShop;

/// <summary>
///     游戏专用防点穿 Patch：当 Mod IMGUI 窗口激活时，阻止鼠标输入穿透到游戏。
/// </summary>
[HarmonyPatch]
internal static class UIPatches
{
    private static bool IsModWindowActive()
    {
        return UI.WindowControls.ShouldBlockGamePointerInput();
    }

    /// <summary>
    ///     若 Mod 窗口激活则返回 false，并清除游戏内置 hover 状态防止穿透。
    /// </summary>
    private static bool AllowGamePointerInput()
    {
        if (IsModWindowActive())
        {
            ClearHoveredState();
            return false;
        }

        return true;
    }

    /// <summary>
    ///     防止点穿时清理游戏悬停状态。
    ///     注意：不清理 UICamera.hoveredObject——因为 MouseController.ProcessEvents 中，
    ///     当 UICamera.hoveredObject 非 null 且不是 "MouseUICamera" 时会提前返回而不调用 ProcessMouse，
    ///     清理它反而会导致 ProcessMouse 被调用。
    /// </summary>
    internal static void ClearHoveredState()
    {
        // 只清理 MouseController.hoveredObject（静态），不碰 UICamera.hoveredObject
        ClearStaticFieldOrSetter("MouseController", "hoveredObject");
    }

    /// <summary>
    ///     按优先级尝试：属性 setter → 直接字段名 → 带 m 前缀的 NGUI 风格字段名。
    /// </summary>
    private static void ClearStaticFieldOrSetter(string typeName, string propertyName)
    {
        try
        {
            var type = Type.GetType($"Il2Cpp.{typeName}, Assembly-CSharp")
                       ?? Type.GetType(typeName);
            if (type == null) return;

            // 1) 优先调用属性 setter（最可靠，如 set_hoveredObject）
            var setter = type.GetMethod($"set_{propertyName}",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (setter != null)
            {
                setter.Invoke(null, new object[] { null });
                return;
            }

            // 2) 尝试直接字段名
            var field = type.GetField(propertyName,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(null, null);
                return;
            }

            // 3) 尝试 NGUI 风格后台字段 mHoveredObject
            var nguiName = $"m{propertyName.Substring(0, 1).ToUpperInvariant()}{propertyName.Substring(1)}";
            field = type.GetField(nguiName,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            field?.SetValue(null, null);
        }
        catch
        {
            // 安全忽略
        }
    }

    private static bool TryBlockMouseButton(ref bool result)
    {
        if (AllowGamePointerInput())
            return true;

        result = false;
        return false;
    }

    private static bool TryBlockMouseAxis(string axisName, ref float result)
    {
        if (AllowGamePointerInput())
            return true;

        if (axisName is "Mouse X" or "Mouse Y" or "Mouse ScrollWheel")
        {
            result = 0f;
            return false;
        }

        return true;
    }

    private static bool TryBlockFireButton(string buttonName, ref bool result)
    {
        if (AllowGamePointerInput())
            return true;

        if (buttonName is "Fire1" or "Fire2" or "Fire3")
        {
            result = false;
            return false;
        }

        return true;
    }

    private static bool IsMouseKeyCode(KeyCode keyCode)
    {
        return keyCode is KeyCode.Mouse0 or KeyCode.Mouse1 or KeyCode.Mouse2 or KeyCode.Mouse3 or KeyCode.Mouse4
            or KeyCode.Mouse5 or KeyCode.Mouse6;
    }

    private static bool TryBlockMouseKey(KeyCode keyCode, ref bool result)
    {
        if (AllowGamePointerInput())
            return true;

        if (IsMouseKeyCode(keyCode))
        {
            result = false;
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Input), nameof(Input.GetMouseButton))]
    public static bool Input_GetMouseButton_Prefix(ref bool __result)
    {
        return TryBlockMouseButton(ref __result);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Input), nameof(Input.GetMouseButtonDown))]
    public static bool Input_GetMouseButtonDown_Prefix(ref bool __result)
    {
        return TryBlockMouseButton(ref __result);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Input), nameof(Input.GetMouseButtonUp))]
    public static bool Input_GetMouseButtonUp_Prefix(ref bool __result)
    {
        return TryBlockMouseButton(ref __result);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Input), nameof(Input.GetAxis))]
    public static bool Input_GetAxis_Prefix(string axisName, ref float __result)
    {
        return TryBlockMouseAxis(axisName, ref __result);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Input), nameof(Input.GetAxisRaw))]
    public static bool Input_GetAxisRaw_Prefix(string axisName, ref float __result)
    {
        return TryBlockMouseAxis(axisName, ref __result);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Input), nameof(Input.GetButton))]
    public static bool Input_GetButton_Prefix(string buttonName, ref bool __result)
    {
        return TryBlockFireButton(buttonName, ref __result);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Input), nameof(Input.GetButtonDown))]
    public static bool Input_GetButtonDown_Prefix(string buttonName, ref bool __result)
    {
        return TryBlockFireButton(buttonName, ref __result);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Input), nameof(Input.GetButtonUp))]
    public static bool Input_GetButtonUp_Prefix(string buttonName, ref bool __result)
    {
        return TryBlockFireButton(buttonName, ref __result);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Input), nameof(Input.GetKey), typeof(KeyCode))]
    public static bool Input_GetKey_Prefix(KeyCode key, ref bool __result)
    {
        return TryBlockMouseKey(key, ref __result);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Input), nameof(Input.GetKeyDown), typeof(KeyCode))]
    public static bool Input_GetKeyDown_Prefix(KeyCode key, ref bool __result)
    {
        return TryBlockMouseKey(key, ref __result);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Input), nameof(Input.GetKeyUp), typeof(KeyCode))]
    public static bool Input_GetKeyUp_Prefix(KeyCode key, ref bool __result)
    {
        return TryBlockMouseKey(key, ref __result);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Input), nameof(Input.mouseScrollDelta), MethodType.Getter)]
    public static bool Input_mouseScrollDelta_Prefix(ref Vector2 __result)
    {
        if (AllowGamePointerInput())
            return true;

        __result = Vector2.zero;
        return false;
    }

    /// <summary>
    ///     BuildQuickButtonController.Update 的前缀：在 Mod 窗口激活时将 onHover 置 false，
    ///     同时清除 hoverTime 定时器。双重保障阻止 AreaController.FocusOnTarget 调用。
    /// </summary>
    public static bool BuildQuickButtonController_Update_Prefix(object __instance)
    {
        if (UI.WindowControls.ShouldBlockGamePointerInput())
        {
            try
            {
                var type = __instance.GetType();
                // 清除 onHover 标志
                var onHoverField = type.GetField("onHover",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    ?? type.GetField("mOnHover",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                onHoverField?.SetValue(__instance, false);

                // 同时清除 hoverTime 定时器，确保即使 onHover 被再次设置，
                // 定时器也从 0 重新开始，永远达不到 0.3s 阈值
                var hoverTimeField = type.GetField("hoverTime",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    ?? type.GetField("mHoverTime",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (hoverTimeField != null)
                    hoverTimeField.SetValue(__instance, 0f);
            }
            catch { }
        }
        return true;
    }

    /// <summary>
    ///     MouseController.Update 的前缀：Mod 窗口激活时跳过更新，从根源阻止鼠标事件处理。
    ///     注意：通过运行时反射注册，不依赖编译时类型引用。
    /// </summary>
    public static bool MouseController_Update_Prefix()
    {
        return !UI.WindowControls.ShouldBlockGamePointerInput();
    }

    /// <summary>
    ///     BuildQuickButtonController.OnHover 的前缀：Mod 窗口激活时跳过，
    ///     阻止 NGUI 将 onHover 字段置为 true。
    ///     NGUI 的 UICamera 会在鼠标悬停时发送 OnHover(bool) 消息，
    ///     这个方法拦截它，从源头阻止 onHover 被设置。
    /// </summary>
    public static bool BuildQuickButtonController_OnHover_Prefix()
    {
        return !UI.WindowControls.ShouldBlockGamePointerInput();
    }

    /// <summary>
    ///     AreaController.FocusOnTarget 的前缀：Mod 窗口激活时跳过，
    ///     阻止所有调用路径触发的建筑追踪聚焦。
    ///     这是最终防线——无论从哪个代码路径调用 FocusOnTarget，都会被拦截。
    /// </summary>
    public static bool AreaController_FocusOnTarget_Prefix()
    {
        return !UI.WindowControls.ShouldBlockGamePointerInput();
    }
}
