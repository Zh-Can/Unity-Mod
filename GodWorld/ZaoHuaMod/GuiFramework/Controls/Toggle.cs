using System;
using UnityEngine;
using ZaoHuaMod.GuiFramework.Localization;
using ZaoHuaMod.GuiFramework.Style;

namespace ZaoHuaMod.GuiFramework.Controls
{
    /// <summary>
    ///     开关相关控件，提供链式调用支持。
    /// </summary>
    public static partial class UI
    {
        /// <summary>开关链式构造器入口，text 会自动繁化。</summary>
        public static ToggleBuilder Toggle(string text)
        {
            return new ToggleBuilder(Loc.Get(text));
        }

        /// <summary>
        ///     开关构造器，支持 Value/Style/Tooltip 后接 Draw。
        ///     Draw 返回新的开关状态。
        /// </summary>
        public class ToggleBuilder
        {
            private bool _value;
            private string _text;
            private string _tooltip;
            private GUIStyle _style;
            private Action _onChange;
            private Action<bool> _onChangeWithValue;

            internal ToggleBuilder(string text)
            {
                _text = text;
                _style = DarkSkin.SToggle;
            }

            /// <summary>设置开关状态。</summary>
            public ToggleBuilder Value(bool value)
            {
                _value = value;
                return this;
            }

            /// <summary>指定自定义样式。</summary>
            public ToggleBuilder Style(GUIStyle style)
            {
                _style = style;
                return this;
            }

            /// <summary>设置 Tooltip 文本。</summary>
            public ToggleBuilder Tooltip(string tooltip)
            {
                _tooltip = Loc.Get(tooltip);
                return this;
            }

            /// <summary>开关状态变化时触发（无参数）。</summary>
            public ToggleBuilder OnChange(Action onChange)
            {
                _onChange = onChange;
                return this;
            }

            /// <summary>开关状态变化时触发（带新值）。</summary>
            public ToggleBuilder OnChange(Action<bool> onChange)
            {
                _onChangeWithValue = onChange;
                return this;
            }

            /// <summary>绘制开关。</summary>
            public bool Draw()
            {
                var newValue = GUILayout.Toggle(_value, new GUIContent(_text, _tooltip), _style);
                if (newValue != _value)
                {
                    _onChange?.Invoke();
                    _onChangeWithValue?.Invoke(newValue);
                }
                return newValue;
            }
        }
    }
}
