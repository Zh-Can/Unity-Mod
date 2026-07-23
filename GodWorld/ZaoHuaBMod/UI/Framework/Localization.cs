using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ZaoHuaBMod.Core;

namespace ZaoHuaBMod.UI.Framework
{
    /// <summary>
    /// 简易多语言支持类，语言包使用 key=value 纯文本格式。
    /// </summary>
    public static class Localization
    {
        public const string Chinese = "zh-CN";

        private static readonly Dictionary<string, Dictionary<string, string>> _languages
            = new Dictionary<string, Dictionary<string, string>>();

        private static string _currentLanguage = Chinese;

        private static readonly HashSet<string> _missing = new HashSet<string>();


        /// <summary>Mod 根目录，由外部初始化。</summary>
        public static string ModDirectory { get; set; }


        /// <summary>已扫描到的可用语言列表（按文件名排序）。</summary>
        public static List<LanguageInfo> AvailableLanguages { get; } = new List<LanguageInfo>();


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
        ///     获取文本。
        ///     当前语言是中文时直接返回 key 作为原文。
        /// </summary>
        public static string Get(string key)
        {
            if (string.IsNullOrEmpty(key))
                return "";

            if (CurrentLanguage == Chinese)
                return key;

            if (_languages.TryGetValue(CurrentLanguage, out var lang) &&
                lang.TryGetValue(key, out var value))
                return value;

            _missing.Add($"{CurrentLanguage}:{key}");
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
            if (!_languages.TryGetValue(language, out var lang))
            {
                lang = new Dictionary<string, string>();
                _languages[language] = lang;
            }

            lang[key] = text;
        }


        /// <summary>批量注册某个语言的所有翻译。</summary>
        public static void RegisterLanguage(string language, Dictionary<string, string> entries)
        {
            if (entries == null)
                return;

            if (!_languages.TryGetValue(language, out var lang))
            {
                lang = new Dictionary<string, string>();
                _languages[language] = lang;
            }

            foreach (var kv in entries)
                lang[kv.Key] = kv.Value;
        }


        /// <summary>从 key=value 格式文件加载语言包。</summary>
        public static void LoadLanguage(string language, string file)
        {
            if (!File.Exists(file))
            {
                Log.Warning($"[Localization] 语言文件不存在: {file}");
                return;
            }

            var data = new Dictionary<string, string>();
            var lines = File.ReadAllLines(file, Encoding.UTF8);

            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (string.IsNullOrEmpty(line))
                    continue;
                if (line.StartsWith("#") || line.StartsWith("//"))
                    continue;

                var sepIndex = line.IndexOf('=');
                if (sepIndex < 0)
                    sepIndex = line.IndexOf(':');

                if (sepIndex < 0)
                    continue;

                var key = line.Substring(0, sepIndex).Trim();
                var value = line.Substring(sepIndex + 1).Trim();

                if (string.IsNullOrEmpty(key))
                    continue;

                data[key] = value;
            }

            _languages[language] = data;
        }


        /// <summary>
        /// 扫描 ModDirectory/languages 目录下的 .cfg 语言文件。
        /// 返回是否扫描到除简中外的新语言。
        /// </summary>
        public static bool ScanLanguages()
        {
            AvailableLanguages.Clear();
            AvailableLanguages.Add(new LanguageInfo(Chinese, "简中", null));

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

                AvailableLanguages.Add(new LanguageInfo(fileName, fileName, file));
            }

            return AvailableLanguages.Count > 1;
        }


        /// <summary>
        /// 按顺序切换下一个可用语言，并加载对应 .cfg 文件。
        /// 切换后保存配置到注册表。
        /// </summary>
        public static void CycleLanguage()
        {
            if (AvailableLanguages.Count <= 1)
                return;

            var index = AvailableLanguages.FindIndex(l => l.Code == CurrentLanguage);
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
                LoadLanguage(info.Code, info.FilePath);

            CurrentLanguage = info.Code;

            ModConfig.Language = info.Code;
            ModConfig.Save();
        }


        /// <summary>尝试应用指定语言代码，若对应 cfg 不存在则回退到简中。</summary>
        public static void TryApplyLanguage(string code)
        {
            if (string.IsNullOrEmpty(code) || code == Chinese)
            {
                CurrentLanguage = Chinese;
                return;
            }

            var info = AvailableLanguages.FirstOrDefault(l => l.Code == code);
            if (info == null || string.IsNullOrEmpty(info.FilePath) || !File.Exists(info.FilePath))
            {
                Log.Warning($"[Localization] 语言文件 {code}.cfg 不存在，回退到简中");
                CurrentLanguage = Chinese;
                ModConfig.Language = Chinese;
                ModConfig.Save();
                return;
            }

            ApplyLanguage(info);
        }


        /// <summary>保存缺失的翻译条目到文件，每行格式为 language:key。</summary>
        public static void SaveMissing(string file)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Missing translations");
            foreach (var item in _missing)
                sb.AppendLine(item);

            File.WriteAllText(file, sb.ToString(), Encoding.UTF8);
        }


        /// <summary>清空缺失记录。</summary>
        public static void ClearMissing()
        {
            _missing.Clear();
        }


        public static IEnumerable<string> MissingKeys => _missing;


        public class LanguageInfo
        {
            public readonly string Code;
            public readonly string DisplayName;
            public readonly string FilePath;

            public LanguageInfo(string code, string displayName, string filePath)
            {
                Code = code;
                DisplayName = displayName;
                FilePath = filePath;
            }
        }
    }
}
