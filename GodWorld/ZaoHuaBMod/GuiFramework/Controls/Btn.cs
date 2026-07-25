using System;
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
        public static ButtonBuilder Btn()
        {
            return new ButtonBuilder();
        }

        /// <summary>
        ///     按钮构造器，支持 Text/Style/Tooltip/OnClick 后接 Draw。
        ///     Draw 返回是否被点击。
        /// </summary>
        public class ButtonBuilder
        {
            private string _text;
            private string _tooltip;
            private GUIStyle _style = DarkSkin.SBtn;
            private Action _onClick;

            /// <summary>普通按钮</summary>
            public ButtonBuilder Text(string text)
            {
                _text = text;
                return this;
            }
            /// <summary>添加按钮</summary>
            public ButtonBuilder Add(string text)
            {
                _text = text;
                _style =  DarkSkin.SBtnAdd;
                return this;
            }
            /// <summary>删除按钮</summary>
            public ButtonBuilder Del(string text)
            {
                _text = text;
                _style =  DarkSkin.SBtnDel;
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

            /// <summary>设置点击回调。</summary>
            public ButtonBuilder OnClick(Action action)
            {
                _onClick = action;
                return this;
            }

            /// <summary>绘制按钮。</summary>
            public bool Draw()
            {
                bool clicked = GUILayout.Button(new GUIContent(_text, _tooltip), _style);
                if (clicked)
                    _onClick?.Invoke();
                return clicked;
            }
        }
    }
}
