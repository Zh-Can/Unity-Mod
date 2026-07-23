using System;

namespace ZaoHuaBMod.Core.Adapters
{
    /// <summary>
    /// 通过反射调用 BepInEx.Logging.ManualLogSource，不直接引用 BepInEx 程序集。
    /// 构造时把实例方法绑定为委托缓存，避免每次日志反射 Invoke。
    /// </summary>
    public class BepInExLogger : ILogger
    {
        private readonly Action<object> _info;
        private readonly Action<object> _warning;
        private readonly Action<object> _error;


        public BepInExLogger(object source)
        {
            if (source == null)
                return;

            var sourceType = source.GetType();
            _info = CreateDelegate(source, sourceType, "LogInfo");
            _warning = CreateDelegate(source, sourceType, "LogWarning");
            _error = CreateDelegate(source, sourceType, "LogError");
        }


        private static Action<object> CreateDelegate(object target, Type type, string methodName)
        {
            var method = type.GetMethod(methodName, new[] { typeof(object) });
            if (method == null)
                return null;

            return (Action<object>)Delegate.CreateDelegate(typeof(Action<object>), target, method);
        }


        public void Info(object msg)
        {
            _info?.Invoke(msg);
        }


        public void Warning(object msg)
        {
            _warning?.Invoke(msg);
        }


        public void Error(object msg)
        {
            _error?.Invoke(msg);
        }
    }
}
