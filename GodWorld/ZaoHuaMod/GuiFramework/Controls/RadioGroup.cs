using UnityEngine;
using ZaoHuaBMod.GuiFramework.Style;

namespace ZaoHuaBMod.GuiFramework.Controls
{
    /// <summary>
    ///     单选按钮组控件，提供链式调用支持。
    /// </summary>
    public static partial class UI
    {
        /// <summary>单选按钮组链式构造器入口。</summary>
        public static RadioGroupBuilder RadioButtonGroup => new RadioGroupBuilder();

        /// <summary>
        ///     单选按钮组构造器，支持 Selected/Options 后接 Draw。
        ///     Draw 返回新的选中索引。
        /// </summary>
        public class RadioGroupBuilder
        {
            private int _selectedIndex;
            private string[] _options = System.Array.Empty<string>();
            private bool _horizontal;

            /// <summary>设置当前选中索引。</summary>
            public RadioGroupBuilder Selected(int index)
            {
                _selectedIndex = index;
                return this;
            }

            /// <summary>设置选项文本。</summary>
            public RadioGroupBuilder Options(params string[] options)
            {
                _options = options;
                return this;
            }

            /// <summary>使用水平布局（多个选项在同一行排列）。</summary>
            public RadioGroupBuilder Horizontal()
            {
                _horizontal = true;
                return this;
            }

            /// <summary>使用垂直布局（每个选项独占一行，默认）。</summary>
            public RadioGroupBuilder Vertical()
            {
                _horizontal = false;
                return this;
            }

            /// <summary>绘制单选按钮组。</summary>
            public int Draw()
            {
                if (_horizontal)
                {
                    GUILayout.BeginHorizontal();
                    for (var i = 0; i < _options.Length; i++)
                        if (RadioButton(_options[i], i == _selectedIndex))
                            _selectedIndex = i;
                    GUILayout.EndHorizontal();
                }
                else
                {
                    for (var i = 0; i < _options.Length; i++)
                        if (RadioButton(_options[i], i == _selectedIndex))
                            _selectedIndex = i;
                }

                return _selectedIndex;
            }

            private static bool RadioButton(string label, bool selected)
            {
                GUILayout.BeginHorizontal();
                var clicked = GUILayout.Button(string.Empty, selected ? DarkSkin.SRadioOn : DarkSkin.SRadio,
                    GUILayout.Width(16), GUILayout.Height(16));
                GUILayout.Label(label, DarkSkin.SLabel);
                GUILayout.EndHorizontal();
                return clicked;
            }
        }
    }
}
