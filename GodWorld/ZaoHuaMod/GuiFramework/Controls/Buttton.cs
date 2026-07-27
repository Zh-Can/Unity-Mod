using System;
using UnityEngine;
using ZaoHuaMod.GuiFramework.Localization;
using ZaoHuaMod.GuiFramework.Style;

namespace ZaoHuaMod.GuiFramework.Controls
{
    /// <summary>
    ///     按钮相关控件，提供链式调用支持。
    /// </summary>
    public static partial class UI
    {
        /// <summary>按钮链式构造器入口，text 会自动繁化。</summary>
        public static ButtonBuilder Button(string text)
        {
            return new ButtonBuilder(Loc.Get(text));
        }

        /// <summary>
        ///     按钮构造器，支持 Btn/Add/Del/Label/Style/Tooltip/OnClick 后接 Draw。
        ///     Draw 返回是否被点击。
        /// </summary>
        public class ButtonBuilder
        {
            private string _text;
            private string _tooltip;
            private GUIStyle _style;
            private Action _onClick;

            internal ButtonBuilder(string text)
            {
                _text = text;
                _style = DarkSkin.SBtn;
            }

            /// <summary>普通按钮样式（默认）。</summary>
            public ButtonBuilder Btn()
            {
                _style = DarkSkin.SBtn;
                return this;
            }
            /// <summary>添加按钮样式。</summary>
            public ButtonBuilder Add()
            {
                _style = DarkSkin.SBtnAdd;
                return this;
            }
            /// <summary>删除按钮样式。</summary>
            public ButtonBuilder Del()
            {
                _style = DarkSkin.SBtnDel;
                return this;
            }
            /// <summary>文本按钮样式。</summary>
            public ButtonBuilder Label()
            {
                _style = DarkSkin.SNameBtn;
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
                _tooltip = Loc.Get(tooltip);
                return this;
            }

            /// <summary>设置点击回调。</summary>
            public ButtonBuilder OnClick(Action action)
            {
                _onClick = action;
                return this;
            }

            /// <summary>绘制按钮。</summary>
            public bool Draw(params GUILayoutOption[] options)
            {
                bool clicked = GUILayout.Button(new GUIContent(_text, _tooltip), _style, options);
                if (clicked)
                    _onClick?.Invoke();
                return clicked;
            }
        }
    }
}
