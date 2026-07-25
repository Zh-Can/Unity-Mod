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

            /// <summary>绘制单选按钮组。</summary>
            public int Draw()
            {
                return DarkSkin.RadioGroup(_selectedIndex, _options);
            }
        }
    }
}
