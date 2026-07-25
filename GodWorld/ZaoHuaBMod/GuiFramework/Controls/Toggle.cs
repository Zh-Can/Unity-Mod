using UnityEngine;
using ZaoHuaBMod.GuiFramework.Style;

namespace ZaoHuaBMod.GuiFramework.Controls
{
    /// <summary>
    ///     开关相关控件，提供链式调用支持。
    /// </summary>
    public static partial class UI
    {
        /// <summary>开关链式构造器入口。</summary>
        public static ToggleBuilder Toggle => new ToggleBuilder();

        /// <summary>
        ///     开关构造器，支持 Value/Style/Tooltip 后接 Draw。
        ///     Draw 返回新的开关状态。
        /// </summary>
        public class ToggleBuilder
        {
            private bool _value;
            private string _text;
            private string _tooltip;
            private GUIStyle _style = DarkSkin.SToggle;

            /// <summary>设置当前值和文本。</summary>
            public ToggleBuilder Text(string text, bool value = false)
            {
                _value = value;
                _text = text;
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
                _tooltip = tooltip;
                return this;
            }

            /// <summary>绘制开关。</summary>
            public bool Draw()
            {
                return GUILayout.Toggle(_value, new GUIContent(_text, _tooltip), _style);
            }
        }
    }
}
