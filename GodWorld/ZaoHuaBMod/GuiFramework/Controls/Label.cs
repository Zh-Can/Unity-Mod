using System.Collections.Generic;
using UnityEngine;
using ZaoHuaBMod.GuiFramework.Style;

namespace ZaoHuaBMod.GuiFramework.Controls
{
    /// <summary>
    ///     Label 相关控件，提供链式调用支持。
    /// </summary>
    public static partial class UI
    {
        
        /// <summary>Label 链式构造器入口。</summary>
        public static LabelBuilder Label()
        {
            return new LabelBuilder();
        }
        
        /// <summary>
        ///     Label 构造器，支持 Title/Text 后接 Tooltip 或 Draw。
        /// </summary>
        public class LabelBuilder
        {
            private string _text;
            private string _tooltip;
            private GUIStyle _style = DarkSkin.SLabel;

            /// <summary>标题样式</summary>
            public LabelBuilder Title(string text)
            {
                _text = text;
                _tooltip = null;
                _style = DarkSkin.STitle;
                return this;
            }
            /// <summary>详情标题 样式。</summary>
            public LabelBuilder DetailHead(string text)
            {
                _text = text;
                _tooltip = null;
                _style = DarkSkin.SDetailHead;
                return this;
            }
            /// <summary>详情内容 样式。</summary>
            public LabelBuilder DetailText(string text)
            {
                _text = text;
                _tooltip = null;
                _style = DarkSkin.SBonus;
                return this;
            }
            
            /// <summary>靠右侧提示灰色小字样式</summary>
            public LabelBuilder AsHint(string text)
            {
                _text = text;
                _tooltip = null;
                _style = DarkSkin.SHint;
                return this;
            }
            /// <summary>灰色小字样式</summary>
            public LabelBuilder AsMuted(string text)
            {
                _text = text;
                _tooltip = null;
                _style = DarkSkin.SMuted;
                return this;
            }
            /// <summary>数字样式</summary>
            public LabelBuilder AsCount(string text)
            {
                _text = text;
                _tooltip = null;
                _style = DarkSkin.SCount;
                return this;
            }

            /// <summary>普通文本 样式。</summary>
            public LabelBuilder Text(string text)
            {
                _text = text;
                _tooltip = null;
                _style = DarkSkin.SLabel;
                return this;
            }
            /// <summary>功能名称 样式。</summary>
            public LabelBuilder FeatureName(string text)
            {
                _text = text;
                _tooltip = null;
                _style = DarkSkin.SFeatureName;
                return this;
            }
            /// <summary>蓝色小字标签 样式。</summary>
            public LabelBuilder Tag(string text)
            {
                _text = text;
                _tooltip = null;
                _style = DarkSkin.STag;
                return this;
            }
            
            /// <summary>金色小字标签 样式。</summary>
            public LabelBuilder TagGold(string text)
            {
                _text = text;
                _tooltip = null;
                _style = DarkSkin.STagHidden;
                return this;
            }
           

            /// <summary>指定自定义样式。</summary>
            public LabelBuilder Style(GUIStyle style)
            {
                _style = style;
                return this;
            }

            public LabelBuilder Color(Color textColor)
            {
                _style = new GUIStyle(_style);
                _style.normal.textColor = textColor;
                return this;
            }
            
            /// <summary>设置 Tooltip 文本。</summary>
            public LabelBuilder Tooltip(string tooltip)
            {
                _tooltip = tooltip;
                return this;
            }

            /// <summary>绘制 Label。</summary>
            public Rect Draw()
            {
                var content = new GUIContent(_text, _tooltip);
                Rect rect = GUILayoutUtility.GetRect(content, _style);
                GUI.Label(rect, content, _style);
                return rect;
            }
        }
    }
}
