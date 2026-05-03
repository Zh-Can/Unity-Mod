
using HarmonyLib;
using Il2Cpp;

namespace LYMod.Patches;
/// <summary>
/// 天气
/// </summary>
public class WeatherControllerPatches
{

    [HarmonyPrefix]
    [HarmonyPatch(typeof(WeatherController), nameof(WeatherController.ChangeWeather), typeof(int))]
    public static bool WeatherController_ChangeWeather1_Prefix(ref int targetWeatherID)
    {
        if (!Plugin.Instance.WeatherLockSunnyFlag.Value) return true;
        targetWeatherID = 0;
        return true;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(WeatherController), nameof(WeatherController.ChangeWeather), typeof(int), typeof(float))]
    public static bool WeatherController_ChangeWeather2_Prefix(ref int targetWeatherID, float lastTime)
    {
        if (!Plugin.Instance.WeatherLockSunnyFlag.Value) return true;
        targetWeatherID = 0;
        return true;
    }
}