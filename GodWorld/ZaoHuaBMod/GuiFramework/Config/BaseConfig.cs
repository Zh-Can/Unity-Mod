using UnityEngine;
using ZaoHuaBMod.GuiFramework.Controls;
using ZaoHuaBMod.GuiFramework.Localization;

namespace ZaoHuaBMod.GuiFramework.Config
{
    /// <summary>
    ///     管理 Mod 持久化配置（使用 Unity PlayerPrefs）。
    /// </summary>
    public static class BaseConfig
    {
        private const string ScaleKey = "_Mod_Scale";
        private const string LanguageKey = "_Mod_Language";

        public static float Scale { get; set; } = 1f;
        public static string Language { get; set; } = Loc.Chinese;


        public static void Load()
        {
            Scale = PlayerPrefs.GetFloat(ScaleKey, 1f);
            Language = PlayerPrefs.GetString(LanguageKey, Loc.Chinese);
        }


        public static void Save()
        {
            PlayerPrefs.SetFloat(ScaleKey, Scale);
            PlayerPrefs.SetString(LanguageKey, Language);
            PlayerPrefs.Save();
        }


        public static void ApplyToManager()
        {
            UI.WindowControls.SetScale(Scale);
            Loc.CurrentLanguage = Language;
        }
    }
}