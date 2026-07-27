using System;
using UnityEngine;
using LunHuiShop.GuiFramework.Style;

namespace LunHuiShop.GuiFramework.Controls
{
    public static partial class UI
    {
        /// <summary>单选下拉框链式构造器入口。</summary>
        public static DropdownBuilder Dropdown => new DropdownBuilder();

        /// <summary>
        ///     单选下拉框构造器，支持 Options/Selected/OnChange 后接 Draw。
        ///     Draw 返回新的选中索引。
        /// </summary>
        public class DropdownBuilder
        {
            private int _selectedIndex;
            private string[] _options = Array.Empty<string>();
            private Action<int> _onChange;

            /// <summary>设置当前选中索引。</summary>
            public DropdownBuilder Selected(int index)
            {
                _selectedIndex = index;
                return this;
            }

            /// <summary>设置选项文本。</summary>
            public DropdownBuilder Options(params string[] options)
            {
                _options = options;
                return this;
            }

            /// <summary>注册选中项变化时的回调。</summary>
            public DropdownBuilder OnChange(Action<int> onChange)
            {
                _onChange = onChange;
                return this;
            }

            /// <summary>绘制下拉框。</summary>
            /// <param name="expanded">展开状态（需外部持久化）</param>
            public int Draw(ref bool expanded)
            {
                var prev = _selectedIndex;

                var content = new GUIContent(_options[_selectedIndex] + "  ▼");
                if (GUILayout.Button(content, DarkSkin.SPopup))
                    expanded = !expanded;

                if (expanded)
                {
                    GUILayout.BeginVertical(DarkSkin.SPanel);
                    for (var i = 0; i < _options.Length; i++)
                        if (GUILayout.Button(_options[i], i == _selectedIndex ? DarkSkin.SChipOn : DarkSkin.SBtn))
                        {
                            _selectedIndex = i;
                            expanded = false;
                        }

                    GUILayout.EndVertical();
                }

                if (_selectedIndex != prev)
                    _onChange?.Invoke(_selectedIndex);

                return _selectedIndex;
            }
        }
    }
}
