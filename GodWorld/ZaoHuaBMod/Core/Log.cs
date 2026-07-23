namespace ZaoHuaBMod.Core
{
    /// <summary>
    /// 通用日志入口，默认使用 Unity Debug，也可由外部传入 ILogger 实现。
    /// </summary>
    public static class Log
    {
        private static ILogger _logger = new UnityDebugLogger();


        public static void Initialize(ILogger logger)
        {
            _logger = logger ?? new UnityDebugLogger();
        }


        public static void Info(object msg)
        {
            _logger?.Info(msg);
        }


        public static void Warning(object msg)
        {
            _logger?.Warning(msg);
        }


        public static void Error(object msg)
        {
            _logger?.Error(msg);
        }
    }
}
