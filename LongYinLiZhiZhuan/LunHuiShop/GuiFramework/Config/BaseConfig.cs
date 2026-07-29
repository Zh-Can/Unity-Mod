using UnityEngine;
using LunHuiShop.GuiFramework.Controls;
using LunHuiShop.GuiFramework.Localization;

namespace LunHuiShop.GuiFramework.Config
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


        // ---------- 窗口位置/尺寸 ----------
        private static string WindowKey(int id, string suffix) => $"_Mod_Window_{id}_{suffix}";

        public static Rect? LoadWindowRect(int id)
        {
            var xKey = WindowKey(id, "X");
            if (!PlayerPrefs.HasKey(xKey)) return null;

            return new Rect(
                PlayerPrefs.GetFloat(WindowKey(id, "X"), 100f),
                PlayerPrefs.GetFloat(WindowKey(id, "Y"), 100f),
                PlayerPrefs.GetFloat(WindowKey(id, "W"), 600f),
                PlayerPrefs.GetFloat(WindowKey(id, "H"), 400f)
            );
        }

        public static void SaveWindowRect(int id, Rect rect)
        {
            PlayerPrefs.SetFloat(WindowKey(id, "X"), rect.x);
            PlayerPrefs.SetFloat(WindowKey(id, "Y"), rect.y);
            PlayerPrefs.SetFloat(WindowKey(id, "W"), rect.width);
            PlayerPrefs.SetFloat(WindowKey(id, "H"), rect.height);
            PlayerPrefs.Save();
        }

        /// <summary>仅保存窗口位置（不保存尺寸）</summary>
        public static void SaveWindowPosition(int id, Vector2 pos)
        {
            PlayerPrefs.SetFloat(WindowKey(id, "X"), pos.x);
            PlayerPrefs.SetFloat(WindowKey(id, "Y"), pos.y);
            PlayerPrefs.Save();
        }

        /// <summary>仅加载窗口位置（返回 null 表示无保存记录）</summary>
        public static Vector2? LoadWindowPosition(int id)
        {
            var xKey = WindowKey(id, "X");
            if (!PlayerPrefs.HasKey(xKey)) return null;
            return new Vector2(
                PlayerPrefs.GetFloat(WindowKey(id, "X"), 100f),
                PlayerPrefs.GetFloat(WindowKey(id, "Y"), 100f)
            );
        }
    }
}