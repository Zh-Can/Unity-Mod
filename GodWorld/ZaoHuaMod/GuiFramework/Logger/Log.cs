using System;
using System.Reflection;

namespace ZaoHuaMod.GuiFramework.Logger
{
    /// <summary>
    /// 通用日志入口，默认使用 Unity Debug，也可由外部传入 ILogger 实现。
    /// 前缀策略：MelonLoader 控制台会自动为每条日志加上 "[ModName]" 前缀（包括
    /// UnityEngine.Debug 的输出也会被它捕获加前缀），所以 MelonLoader 环境下前缀留空，
    /// 避免重复；非 MelonLoader 环境（如纯 Unity）才自动检测 ModInfo.Name 作为前缀。
    /// 如需自定义前缀，可直接给 Prefix 赋值。
    /// </summary>
    public static class Log
    {
        /// <summary>日志前缀，按运行环境自动决定（MelonLoader 下为空）。</summary>
        public static string Prefix { get; set; } = InitPrefix();

        private static ILogger _logger = new UnityDebugLogger();

        /// <summary>检测运行环境，决定是否使用自带前缀。</summary>
        private static string InitPrefix()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                if (asm.GetName().Name == "MelonLoader")
                    return ""; // MelonLoader 已带 "[ModName]" 前缀，这里不再重复
            return AutoDetectPrefix();
        }

        /// <summary>非 MelonLoader 环境：自动检测 ModInfo.Name 作为前缀。</summary>
        private static string AutoDetectPrefix()
        {
            try
            {
                var modInfoType = typeof(Log).Assembly.GetType("ZaoHuaMod.ModInfo");
                if (modInfoType != null)
                {
                    var nameField = modInfoType.GetField("Name", BindingFlags.Public | BindingFlags.Static);
                    if (nameField?.GetValue(null) is string name && !string.IsNullOrEmpty(name))
                        return $"[{name}] ";
                }
            }
            catch { }
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
