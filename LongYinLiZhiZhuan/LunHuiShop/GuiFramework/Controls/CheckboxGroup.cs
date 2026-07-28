using System;
using UnityEngine;
using LunHuiShop.GuiFramework.Style;

namespace LunHuiShop.GuiFramework.Controls
{
    /// <summary>
    ///     多选按钮组控件，支持水平/垂直布局，每个选项为一个按钮（chip 样式）。
    /// </summary>
    public static partial class UI
    {
        /// <summary>多选按钮组链式构造器入口。</summary>
        public static CheckboxGroupBuilder CheckboxGroup => new CheckboxGroupBuilder();

        /// <summary>
        ///     多选按钮组构造器，支持 Options/Selected/Horizontal/Vertical 后接 Draw。
        ///     Draw 返回新的选中状态数组（bool[]）。
        /// </summary>
        public class CheckboxGroupBuilder
        {
            private bool[] _selected = Array.Empty<bool>();
            private string[] _options = Array.Empty<string>();
            private bool _horizontal = true;
            private Action _onChange;
            private Action<bool[]> _onChangeWithArray;

            /// <summary>设置各选项的选中状态。</summary>
            public CheckboxGroupBuilder Selected(params bool[] selected)
            {
                _selected = selected ?? Array.Empty<bool>();
                return this;
            }

            /// <summary>设置选项文本。</summary>
            public CheckboxGroupBuilder Options(params string[] options)
            {
                _options = options ?? Array.Empty<string>();
                return this;
            }

            /// <summary>使用水平布局（多个选项在同一行排列，默认）。</summary>
            public CheckboxGroupBuilder Horizontal()
            {
                _horizontal = true;
                return this;
            }

            /// <summary>使用垂直布局（每个选项独占一行）。</summary>
            public CheckboxGroupBuilder Vertical()
            {
                _horizontal = false;
                return this;
            }

            /// <summary>注册选中状态变化时的回调（无参）。</summary>
            public CheckboxGroupBuilder OnChange(Action onChange)
            {
                _onChange = onChange;
                return this;
            }

            /// <summary>注册选中状态变化时的回调（传递选中状态数组）。</summary>
            public CheckboxGroupBuilder OnChange(Action<bool[]> onChange)
            {
                _onChangeWithArray = onChange;
                return this;
            }

            /// <summary>绘制多选按钮组。</summary>
            public bool[] Draw()
            {
                // 确保 _selected 长度与 _options 一致
                if (_selected.Length != _options.Length)
                    _selected = new bool[_options.Length];

                var prev = new bool[_selected.Length];
                Array.Copy(_selected, prev, _selected.Length);

                if (_horizontal)
                {
                    GUILayout.BeginHorizontal();
                    for (var i = 0; i < _options.Length; i++)
                    {
                        var idx = i;
                        if (GUILayout.Button(_options[i], _selected[i] ? DarkSkin.SChipOn : DarkSkin.SChip))
                            _selected[idx] = !_selected[idx];
                    }
                    GUILayout.EndHorizontal();
                }
                else
                {
                    for (var i = 0; i < _options.Length; i++)
                    {
                        var idx = i;
                        if (GUILayout.Button(_options[i], _selected[i] ? DarkSkin.SChipOn : DarkSkin.SChip))
                            _selected[idx] = !_selected[idx];
                    }
                }

                // 检测是否有变化
                var changed = false;
                for (var i = 0; i < _selected.Length; i++)
                {
                    if (_selected[i] != prev[i])
                    {
                        changed = true;
                        break;
                    }
                }

                if (changed)
                {
                    _onChange?.Invoke();
                    _onChangeWithArray?.Invoke(_selected);
                }

                return _selected;
            }
        }
    }
}
