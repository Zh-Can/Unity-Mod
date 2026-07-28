using HarmonyLib;
using UnityEngine;

namespace ZaoHuaBMod.GuiFramework.Controls
{
    /// <summary>
    ///     通用 Unity Input 防点击穿透补丁。
    ///     通过 Harmony Patch 拦截 Input.GetMouseButton / GetAxis 等方法，
    ///     在 Mod IMGUI 窗口激活时阻止鼠标输入穿透到游戏。
    ///     
    ///     使用方式（在 Mod 入口处注册）：
    ///     <code>
    ///     var harmony = new HarmonyLib.Harmony("MyMod");
    ///     harmony.PatchAll(typeof(GameInputBlocker));
    ///     </code>
    ///     
    ///     不依赖任何游戏特定类型，所有 Unity 游戏通用。
    /// </summary>
    [HarmonyPatch]
    public static class GameInputBlocker
    {
        private static bool IsModWindowActive()
        {
            return UI.WindowControls.ShouldBlockGamePointerInput();
        }

        private static bool AllowGamePointerInput()
        {
            return !IsModWindowActive();
        }

        // ---- Mouse Button ----

        private static bool TryBlockMouseButton(ref bool result)
        {
            if (AllowGamePointerInput())
                return true;

            result = false;
            return false;
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

        // ---- Axis ----

        private static bool TryBlockMouseAxis(string axisName, ref float result)
        {
            if (AllowGamePointerInput())
                return true;

            if (axisName == "Mouse X" || axisName == "Mouse Y" || axisName == "Mouse ScrollWheel")
            {
                result = 0f;
                return false;
            }

            return true;
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

        // ---- Fire Buttons ----

        private static bool TryBlockFireButton(string buttonName, ref bool result)
        {
            if (AllowGamePointerInput())
                return true;

            if (buttonName == "Fire1" || buttonName == "Fire2" || buttonName == "Fire3")
            {
                result = false;
                return false;
            }

            return true;
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

        // ---- Key (mouse keys only) ----

        private static bool IsMouseKeyCode(KeyCode keyCode)
        {
            return keyCode == KeyCode.Mouse0 || keyCode == KeyCode.Mouse1 || keyCode == KeyCode.Mouse2
                || keyCode == KeyCode.Mouse3 || keyCode == KeyCode.Mouse4 || keyCode == KeyCode.Mouse5
                || keyCode == KeyCode.Mouse6;
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

        // ---- Mouse Scroll Delta ----

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Input), nameof(Input.mouseScrollDelta), MethodType.Getter)]
        public static bool Input_mouseScrollDelta_Prefix(ref Vector2 __result)
        {
            if (AllowGamePointerInput())
                return true;

            __result = Vector2.zero;
            return false;
        }
    }
}
