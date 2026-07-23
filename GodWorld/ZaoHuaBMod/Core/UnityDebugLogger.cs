using UnityEngine;

namespace ZaoHuaBMod.Core
{
    public class UnityDebugLogger : ILogger
    {
        public void Info(object msg) => Debug.Log($"[ZaoHuaBMod] {msg}");
        public void Warning(object msg) => Debug.LogWarning($"[ZaoHuaBMod] {msg}");
        public void Error(object msg) => Debug.LogError($"[ZaoHuaBMod] {msg}");
    }
}
