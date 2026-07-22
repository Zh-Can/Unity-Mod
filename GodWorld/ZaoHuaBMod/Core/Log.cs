using BepInEx.Logging;

namespace ZaoHuaBMod.Core
{
    public static class Log
    {
        public static ManualLogSource Logger;
        
        public static void Info(object msg)
        {
            Logger?.LogInfo(msg);
        }

        public static void Warning(object msg)
        {
            Logger?.LogWarning(msg);
        }
        
        public static void Error(object msg)
        {
            Logger?.LogError(msg);
        }
    }
}