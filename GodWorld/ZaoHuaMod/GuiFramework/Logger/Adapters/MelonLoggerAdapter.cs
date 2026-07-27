using System;

namespace ZaoHuaMod.GuiFramework.Logger.Adapters
{
    /// <summary>
    /// 通过反射调用 MelonLoader.MelonLogger，无需直接引用 MelonLoader.dll。
    /// 构造时把 MethodInfo 缓存为委托，避免每次日志都反射 Invoke。
    /// </summary>
    public class MelonLoggerAdapter : ILogger
    {
        private readonly Action<object> _info;
        private readonly Action<object> _warning;
        private readonly Action<object> _error;

        public MelonLoggerAdapter()
        {
            var melonType = Type.GetType("MelonLoader.MelonLogger, MelonLoader");
            if (melonType == null)
                return;

            _info = CreateDelegate(melonType, "Msg");
            _warning = CreateDelegate(melonType, "Warning");
            _error = CreateDelegate(melonType, "Error");
        }

        private static Action<object> CreateDelegate(Type type, string methodName)
        {
            var method = type.GetMethod(methodName, new[] { typeof(object) });
            if (method == null)
                return null;

            return (Action<object>)Delegate.CreateDelegate(typeof(Action<object>), method);
        }

        public void Info(string msg)
        {
            _info?.Invoke(msg);
        }

        public void Warning(string msg)
        {
            _warning?.Invoke(msg);
        }

        public void Error(string msg)
        {
            _error?.Invoke(msg);
        }
    }
}
