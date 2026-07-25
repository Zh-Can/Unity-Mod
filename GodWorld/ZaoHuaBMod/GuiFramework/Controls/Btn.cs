using UnityEngine;
using ZaoHuaBMod.GuiFramework.Style;

namespace ZaoHuaBMod.GuiFramework.Controls
{
    /// <summary>
    ///     按钮相关控件，提供链式调用支持。
    /// </summary>
    public static partial class UI
    {
        /// <summary>按钮链式构造器入口。</summary>
        public static ButtonBuilder Button => new ButtonBuilder();

        /// <summary>
        ///     按钮构造器，支持 Text/Style/Tooltip 后接 Draw。
        ///     Draw 返回是否被点击。
        /// </summary>
        public class ButtonBuilder
        {
            private string _text;
            private string _tooltip;
            private GUIStyle _style = DarkSkin.SBtn;

            /// <summary>设置按钮文本。</summary>
            public ButtonBuilder Text(string text)
            {
                _text = text;
                return this;
            }

            /// <summary>指定自定义样式。</summary>
            public ButtonBuilder Style(GUIStyle style)
            {
                _style = style;
                return this;
            }

            /// <summary>设置 Tooltip 文本。</summary>
            public ButtonBuilder Tooltip(string tooltip)
            {
                _tooltip = tooltip;
                return this;
            }

            /// <summary>绘制按钮。</summary>
            public bool Draw()
            {
                return GUILayout.Button(new GUIContent(_text, _tooltip), _style);
            }
        }
    }
}
