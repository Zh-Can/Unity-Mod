using System.Reflection;

namespace LunHuiShop.GuiFramework.Logger
{
    /// <summary>
    /// 通用日志入口，默认使用 Unity Debug，也可由外部传入 ILogger 实现。
    /// 自动检测 ModInfo.Name 作为前缀，不存在则用 "[Mod] "。
    /// </summary>
    public static class Log
    {
        /// <summary>日志前缀，自动取 ModInfo.Name，不存在则用 "[Mod] "。</summary>
        public static string Prefix { get; set; } = AutoDetectPrefix();

        private static ILogger _logger = new UnityDebugLogger();

        private static string AutoDetectPrefix()
        {
            try
            {
                var modInfoType = typeof(Log).Assembly.GetType("LunHuiShop.ModInfo");
                if (modInfoType != null)
                {
                    var nameField = modInfoType.GetField("Name", BindingFlags.Public | BindingFlags.Static);
                    if (nameField?.GetValue(null) is string name && !string.IsNullOrEmpty(name))
                        return $"[{name}] ";
                }
            }
            catch { }
            UnityEngine.Debug.LogWarning(
                "[Log] 未检测到 ModInfo.Name（LunHuiShop.ModInfo），日志前缀使用默认 \"[Mod] \"。\n" +
                "建议创建 ModInfo.cs：public static class ModInfo { public const string Name = \"你的Mod名\"; }");
            return "[Mod] ";
        }

        public static void Initialize(ILogger logger)
        {
            if (logger != null) _logger = logger;
        }

        public static void Info(string msg)
        {
            _logger.Info($"{Prefix}{msg}");
        }

        public static void Warning(string msg)
        {
            _logger.Warning($"{Prefix}{msg}");
        }

        public static void Error(string msg)
        {
            _logger.Error($"{Prefix}{msg}");
        }
    }
}
