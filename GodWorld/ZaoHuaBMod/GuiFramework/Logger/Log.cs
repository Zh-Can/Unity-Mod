namespace ZaoHuaBMod.GuiFramework.Logger
{
    /// <summary>
    /// 通用日志入口，默认使用 Unity Debug，也可由外部传入 ILogger 实现。
    /// </summary>
    public static class Log
    {
        
        private static ILogger _logger = new UnityDebugLogger();
        
        public static void Initialize(ILogger logger)
        {
            if (logger != null) _logger = logger;
        }

        public static void Info(string msg)
        {
            _logger.Info(msg);
        }
        
        public static void Warning(string msg)
        {
            _logger.Warning(msg);
        }
        
        public static void Error(string msg)
        {
            _logger.Error(msg);
        }
    }
}
