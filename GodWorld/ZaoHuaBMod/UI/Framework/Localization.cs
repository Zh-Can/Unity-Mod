using System;
using System.Collections.Generic;
using System.IO;
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
        public const string TraditionalChinese = "zh-Hant";
        public const string English = "en-US";

        private static readonly Dictionary<string, Dictionary<string, string>> _languages
            = new Dictionary<string, Dictionary<string, string>>();

        private static string _currentLanguage = Chinese;

        private static readonly HashSet<string> _missing = new HashSet<string>();


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
    }
}
