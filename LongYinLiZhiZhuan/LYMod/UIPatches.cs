using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace LYMod;

[HarmonyPatch]
public static class UIPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(BuildQuickButtonController), nameof(BuildQuickButtonController.Update))]
    public static bool BuildQuickButtonController_Update(BuildQuickButtonController __instance)
    {
        if (AllowGamePointerInput())
        {
            return true;
        }

        if (__instance != null)
        {
            __instance.onHover = false;
            __instance.hoverTime = 0f;
        }

        return false;
    }

    // Game/UI click paths vary, so pointer blocking stays at the Unity input layer.
    private static bool AllowGamePointerInput()
    {
        if (Plugin.Instance == null || !Plugin.Instance.ShouldBlockGamePointerInput())
        {
            return true;
        }

        UICamera.hoveredObject = null;
        MouseController.hoveredObject = null;
        return false;
    }

    // 鼠标按键类输入统一返回 false，避免点击穿透到游戏
    private static bool TryBlockMouseButton(ref bool result)
    {
        if (AllowGamePointerInput())
        {
            return true;
        }

        result = false;
        return false;
    }

    // 只拦鼠标轴，其他轴仍交给游戏
    private static bool TryBlockMouseAxis(string axisName, ref float result)
    {
        if (AllowGamePointerInput())
        {
            return true;
        }

        if (axisName is "Mouse X" or "Mouse Y" or "Mouse ScrollWheel")
        {
            result = 0f;
            return false;
        }

        return true;
    }

    // 一些界面会把鼠标左/右/中键映射成 Fire 按钮
    private static bool TryBlockFireButton(string buttonName, ref bool result)
    {
        if (AllowGamePointerInput())
        {
            return true;
        }

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

    // 部分逻辑走 GetKey(Mouse0/1/2...)，这里一起兜住
    private static bool TryBlockMouseKey(KeyCode keyCode, ref bool result)
    {
        if (AllowGamePointerInput())
        {
            return true;
        }

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
        {
            return true;
        }

        __result = Vector2.zero;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SendMouseEvents), nameof(SendMouseEvents.DoSendMouseEvents))]
    public static bool SendMouseEvents_DoSendMouseEvents_Prefix()
    {
        return AllowGamePointerInput();
    }
}
