using System;
using System.Reflection;

namespace SMR2Mod
{
    public class GameHelpers
    {
        public static float GetMax(object obj)
        {
            Type type = obj.GetType();
            while (type != null && type != typeof(object))
            {
                PropertyInfo property = type.GetProperty("Max", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                bool flag2 = property != null;
                if (flag2)
                {
                    return Convert.ToSingle(property.GetValue(obj));
                }
                FieldInfo field = type.GetField("max", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                bool flag3 = field != null;
                if (flag3)
                {
                    return Convert.ToSingle(field.GetValue(obj));
                }
                type = type.BaseType;
            }
            return 9999;
        }
    }
}