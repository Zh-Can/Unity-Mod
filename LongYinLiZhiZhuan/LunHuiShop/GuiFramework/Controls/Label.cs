using System.Collections.Generic;
using UnityEngine;
using LunHuiShop.GuiFramework.Localization;
using LunHuiShop.GuiFramework.Style;

namespace LunHuiShop.GuiFramework.Controls
{
    /// <summary>
    ///     Label 相关控件，提供链式调用支持。
    /// </summary>
    public static partial class UI
    {
        /// <summary>Label 链式构造器入口，text 会自动繁化。</summary>
        public static LabelBuilder Label(string text)
        {
            return new LabelBuilder(Loc.Get(text));
        }
        
        /// <summary>
        ///     Label 构造器，支持 Title/Text 后接 Tooltip 或 Draw。
        /// </summary>
        public class LabelBuilder
        {
            private string _text;
            private string _tooltip;
            private GUIStyle _style;

            internal LabelBuilder(string text)
            {
                _text = text;
                _style = DarkSkin.SLabel;
            }

            /// <summary>标题样式</summary>
            public LabelBuilder Title()
            {
                _tooltip = null;
                _style = DarkSkin.STitle;
                return this;
            }
            /// <summary>详情标题 样式。</summary>
            public LabelBuilder DetailHead()
            {
                _tooltip = null;
                _style = DarkSkin.SDetailHead;
                return this;
            }
            /// <summary>详情内容 样式。</summary>
            public LabelBuilder DetailText()
            {
                _tooltip = null;
                _style = DarkSkin.SBonus;
                return this;
            }
            
            /// <summary>靠右侧提示灰色小字样式</summary>
            public LabelBuilder AsHint()
            {
                _tooltip = null;
                _style = DarkSkin.SHint;
                return this;
            }
            /// <summary>灰色小字样式</summary>
            public LabelBuilder AsMuted()
            {
                _tooltip = null;
                _style = DarkSkin.SMuted;
                return this;
            }
            /// <summary>数字样式</summary>
            public LabelBuilder AsCount()
            {
                _tooltip = null;
                _style = DarkSkin.SCount;
                return this;
            }

            /// <summary>普通文本 样式。</summary>
            public LabelBuilder Text()
            {
                _tooltip = null;
                _style = DarkSkin.SLabel;
                return this;
            }
            /// <summary>功能名称 样式。</summary>
            public LabelBuilder FeatureName()
            {
                _tooltip = null;
                _style = DarkSkin.SFeatureName;
                return this;
            }
            public enum TagKind
            {
                Tag,
                Good,
                Bad,
                Special,
                Temp,
                Hidden
            }
            GUIStyle GetTagStyle(TagKind kind)
            {
                switch (kind)
                {
                    case TagKind.Tag: return DarkSkin.STag;
                    case TagKind.Good: return DarkSkin.STypeGood;
                    case TagKind.Bad: return DarkSkin.STypeBad;
                    case TagKind.Special: return DarkSkin.STypeSpecial;
                    case TagKind.Temp: return DarkSkin.STypeTemp;
                    case TagKind.Hidden: return DarkSkin.STagHidden;
                    default: return DarkSkin.STag;
                }
            }
            /// <summary>标签 样式。</summary>
            public LabelBuilder Tag(TagKind kind = default)
            {
                _tooltip = null;
                _style = GetTagStyle(kind);
                return this;
            }
            
            /// <summary>指定自定义样式。</summary>
            public LabelBuilder Style(GUIStyle style)
            {
                _style = style;
                return this;
            }

            public LabelBuilder Color(Color color)
            {
                _style.normal.textColor = color;
                return this;
            }
            
            /// <summary>设置 Tooltip 文本。</summary>
            public LabelBuilder Tooltip(string tooltip)
            {
                _tooltip = Loc.Get(tooltip);
                return this;
            }

            /// <summary>绘制 Label。</summary>
            public Rect Draw(params GUILayoutOption[] options)
            {
                var content = new GUIContent(_text, _tooltip);
                Rect rect = GUILayoutUtility.GetRect(content, _style, options);
                GUI.Label(rect, content, _style);
                return rect;
            }
        }
    }
}
