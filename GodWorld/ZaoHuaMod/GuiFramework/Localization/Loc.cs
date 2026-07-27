using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ZaoHuaMod.GuiFramework.Config;
using ZaoHuaMod.GuiFramework.Logger;

namespace ZaoHuaMod.GuiFramework.Localization
{
    /// <summary>
    /// 简易多语言支持类，语言包使用 key=value 纯文本格式。
    /// 语言直接用文件名显示名（如"en-US.cfg，显示的就是en-US"）作为唯一标识。
    /// </summary>
    public static class Loc
    {
        public const string Chinese = "简中";

        private static readonly Dictionary<string, Dictionary<string, string>> Languages
            = new Dictionary<string, Dictionary<string, string>>();

        private static string _currentLanguage = Chinese;


        /// <summary>Mod 根目录，由外部初始化。</summary>
        public static string ModDirectory { get; set; }


        /// <summary>已扫描到的可用语言列表（按文件名排序）。</summary>
        public static List<LanguageInfo> AvailableLanguages { get; } = new List<LanguageInfo>();


        /// <summary>当前语言名称。</summary>
        public static string CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (string.IsNullOrEmpty(value) || _currentLanguage == value)
                    return;

                _currentLanguage = value;
                LanguageChanged?.Invoke(value);
            }
        }


        public static event Action<string> LanguageChanged;


        /// <summary>
        /// 获取文本。
        /// 如果当前语言没有对应翻译，则返回 key 作为默认文本。
        /// 重要提醒：开头不要有空格
        /// </summary>
        public static string Get(string key)
        {
            if (string.IsNullOrEmpty(key))
                return "";

            if (Languages.TryGetValue(CurrentLanguage, out var lang)
                && lang.TryGetValue(key, out var value))
            {
                return value;
            }
            return key;
        }


        /// <summary>获取文本并格式化参数。</summary>
        public static string Format(string key, params object[] args)
        {
            var text = Get(key);
            try
            {
                return string.Format(text, args);
            }
            catch (FormatException)
            {
                return text;
            }
        }


        /// <summary>在代码中直接注册一条翻译。</summary>
        public static void Register(string language, string key, string text)
        {
            if (!Languages.TryGetValue(language, out var lang))
            {
                lang = new Dictionary<string, string>();
                Languages[language] = lang;
            }

            lang[key] = text;
        }


        /// <summary>批量注册某个语言的所有翻译。</summary>
        public static void RegisterLanguage(string language, Dictionary<string, string> entries)
        {
            if (entries == null)
                return;

            if (!Languages.TryGetValue(language, out var lang))
            {
                lang = new Dictionary<string, string>();
                Languages[language] = lang;
            }

            foreach (var kv in entries)
                lang[kv.Key] = kv.Value;
        }


        /// <summary>从 key=value 格式文件加载语言包。</summary>
        public static void LoadLanguage(string language, string file)
        {
            if (!File.Exists(file))
            {
                Log.Warning($"语言文件不存在: {file}");
                return;
            }


            var data = new Dictionary<string, string>();

            var text = DecodeText(
                File.ReadAllBytes(file)
            );


            var lines = text.Split(
                new[] { "\r\n", "\n" },
                StringSplitOptions.None
            );


            foreach (var raw in lines)
            {
                var line = raw.Trim();

                if (string.IsNullOrEmpty(line))
                    continue;

                if (line.StartsWith("#") ||
                    line.StartsWith("//"))
                    continue;


                var index = line.IndexOf('=');

                if (index < 0)
                    index = line.IndexOf(':');


                if (index < 0)
                    continue;


                var key = line.Substring(0, index).Trim();

                var value = line.Substring(index + 1).Trim();


                if (!string.IsNullOrEmpty(key))
                    data[key] = value;
            }


            Languages[language] = data;
        }


        /// <summary>
        /// 扫描 ModDirectory/languages 目录下的 .cfg 语言文件。
        /// 返回是否扫描到除简中外的新语言。
        /// </summary>
        public static bool ScanLanguages()
        {
            AvailableLanguages.Clear();
            AvailableLanguages.Add(new LanguageInfo(Chinese, null));

            if (string.IsNullOrEmpty(ModDirectory))
                return false;

            var langDir = Path.Combine(ModDirectory, "languages");
            if (!Directory.Exists(langDir))
                return false;

            var cfgFiles = Directory.GetFiles(langDir, "*.cfg")
                .OrderBy(f => f)
                .ToArray();

            foreach (var file in cfgFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);

                if (fileName == Chinese)
                    continue;

                AvailableLanguages.Add(new LanguageInfo(fileName, file));
            }

            return AvailableLanguages.Count > 1;
        }


        /// <summary>
        /// 按顺序切换下一个可用语言，并加载对应 .cfg 文件。
        /// 切换后保存配置到 PlayerPrefs。
        /// </summary>
        public static void CycleLanguage()
        {
            if (AvailableLanguages.Count <= 1)
                return;

            var index = AvailableLanguages.FindIndex(l => l.DisplayName == CurrentLanguage);
            if (index < 0)
                index = 0;

            var nextIndex = (index + 1) % AvailableLanguages.Count;
            var next = AvailableLanguages[nextIndex];

            ApplyLanguage(next);
        }


        /// <summary>应用指定语言，并加载对应 cfg 文件（如存在）。</summary>
        public static void ApplyLanguage(LanguageInfo info)
        {
            if (!string.IsNullOrEmpty(info.FilePath) && File.Exists(info.FilePath))
                LoadLanguage(info.DisplayName, info.FilePath);

            CurrentLanguage = info.DisplayName;

            BaseConfig.Language = info.DisplayName;
            BaseConfig.Save();
        }


        /// <summary>尝试应用指定语言，若对应 cfg 不存在则回退到简中。</summary>
        public static void TryApplyLanguage(string language)
        {
            if (string.IsNullOrEmpty(language) || language == Chinese)
            {
                CurrentLanguage = Chinese;
                return;
            }

            var info = AvailableLanguages.FirstOrDefault(l => l.DisplayName == language);
            if (info == null || string.IsNullOrEmpty(info.FilePath) || !File.Exists(info.FilePath))
            {
                Log.Warning($"[Loc] 语言文件 {language}.cfg 不存在，回退到简中");
                CurrentLanguage = Chinese;
                BaseConfig.Language = Chinese;
                BaseConfig.Save();
                return;
            }

            ApplyLanguage(info);
        }


        public class LanguageInfo
        {
            public readonly string DisplayName;
            public readonly string FilePath;

            public LanguageInfo(string displayName, string filePath)
            {
                DisplayName = displayName;
                FilePath = filePath;
            }
        }


        /// <summary>
        /// 自动检测文本编码
        /// UTF8 -> GBK
        /// </summary>
        private static string DecodeText(byte[] bytes)
        {
            if (HasUtf8Bom(bytes))
            {
                return Encoding.UTF8.GetString(
                    bytes,
                    3,
                    bytes.Length - 3
                );
            }

            try
            {
                var utf8 = new UTF8Encoding(false, true);
                return utf8.GetString(bytes);
            }
            catch
            {

            }

            return Encoding.GetEncoding(936).GetString(bytes);
        }



        private static bool HasUtf8Bom(byte[] bytes)
        {
            return bytes.Length >= 3 &&
                   bytes[0] == 0xEF &&
                   bytes[1] == 0xBB &&
                   bytes[2] == 0xBF;
        }
    }
}
