using UnityEngine;
using ZaoHuaBMod.UI.Framework;

namespace ZaoHuaBMod.Core
{
    /// <summary>
    /// 管理 Mod 持久化配置（使用 Unity PlayerPrefs）。
    /// </summary>
    public static class ModConfig
    {
        private const string ScaleKey = "_Mod_Scale";
        private const string LanguageKey = "_Mod_Language";

        public static float Scale { get; set; } = 1f;
        public static string Language { get; set; } = Localization.Chinese;


        public static void Load()
        {
            Scale = PlayerPrefs.GetFloat(ScaleKey, 1f);
            Language = PlayerPrefs.GetString(LanguageKey, Localization.Chinese);
        }


        public static void Save()
        {
            PlayerPrefs.SetFloat(ScaleKey, Scale);
            PlayerPrefs.SetString(LanguageKey, Language);
            PlayerPrefs.Save();
        }


        public static void ApplyToManager()
        {
            GUIManager.Instance.SetScale(Scale);
            Localization.CurrentLanguage = Language;
        }
    }
}
