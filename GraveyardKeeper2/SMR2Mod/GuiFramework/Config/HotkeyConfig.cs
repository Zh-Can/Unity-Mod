using System.Text;
using UnityEngine;

namespace SMR2Mod.GuiFramework.Config
{
    /// <summary>
    ///     快捷键配置，支持单个按键或组合键（Ctrl/Alt/Shift）。
    /// </summary>
    public class HotkeyConfig
    {
        public KeyCode Key = KeyCode.BackQuote;
        public bool Ctrl;
        public bool Alt;
        public bool Shift;

        public bool IsPressed()
        {
            if (Key == KeyCode.None) return false;
            if (!Input.GetKeyDown(Key)) return false;

            if (Ctrl && !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
                return false;
            if (Alt && !Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt))
                return false;
            if (Shift && !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                return false;

            return true;
        }

        public string GetDisplayName()
        {
            var sb = new StringBuilder();
            if (Ctrl) sb.Append("Ctrl+");
            if (Alt) sb.Append("Alt+");
            if (Shift) sb.Append("Shift+");
            sb.Append(Key);
            return sb.ToString();
        }

        public override string ToString() => $"{Key}|{Ctrl}|{Alt}|{Shift}";

        public static HotkeyConfig Parse(string s)
        {
            var cfg = new HotkeyConfig();
            if (string.IsNullOrEmpty(s)) return cfg;

            var parts = s.Split('|');
            if (parts.Length >= 1 && System.Enum.TryParse(parts[0], out KeyCode key))
                cfg.Key = key;
            if (parts.Length >= 2 && bool.TryParse(parts[1], out var ctrl))
                cfg.Ctrl = ctrl;
            if (parts.Length >= 3 && bool.TryParse(parts[2], out var alt))
                cfg.Alt = alt;
            if (parts.Length >= 4 && bool.TryParse(parts[3], out var shift))
                cfg.Shift = shift;

            return cfg;
        }

        public HotkeyConfig Clone() => new HotkeyConfig()
        {
            Key = Key,
            Ctrl = Ctrl,
            Alt = Alt,
            Shift = Shift
        };
    }
}
