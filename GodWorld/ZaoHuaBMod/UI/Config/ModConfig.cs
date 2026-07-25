using UnityEngine;
using ZaoHuaBMod.UI.Core;

namespace ZaoHuaBMod.UI.Config
{
    /// <summary>
    /// 管理 Mod 持久化配置（使用 Unity PlayerPrefs）。
    /// </summary>
    public static class ModConfig
    {
        private const string ScaleKey = "_Mod_Scale";
        private const string LanguageKey = "_Mod_Language";

        public static float Scale { get; set; } = 1f;
        public static string Language { get; set; } = Localization.Loc.Chinese;


        public static void Load()
        {
            Scale = PlayerPrefs.GetFloat(ScaleKey, 1f);
            Language = PlayerPrefs.GetString(LanguageKey, Localization.Loc.Chinese);
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
            Localization.Loc.CurrentLanguage = Language;
        }
    }
}
