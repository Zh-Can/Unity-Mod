using System;
using UnityEngine;
using ZaoHuaMod.GuiFramework.Localization;
using ZaoHuaMod.GuiFramework.Style;

namespace ZaoHuaMod.GuiFramework.Controls
{
    /// <summary>
    ///     UI 入口，提供窗体创建与布局。
    /// </summary>
    public static partial class UI
    {
        /// <summary>
        ///     创建一个新窗体。
        /// </summary>
        /// <param name="rect">窗口位置与大小。</param>
        /// <param name="title">窗口标题。</param>
        /// <param name="content">窗口内容绘制回调。</param>
        /// <returns>窗体构造器，可继续链式配置。</returns>
        public static WindowBuilder NewWindow(Rect rect, string title, Action<WindowData> content)
        {
            return new WindowBuilder(rect, new WindowData.TitleBarConfig(title), content);
        }

        /// <summary>
        ///     创建一个新窗体（自定义标题栏配置）。
        /// </summary>
        /// <param name="rect">窗口位置与大小。</param>
        /// <param name="titleBar">标题栏配置。</param>
        /// <param name="content">窗口内容绘制回调。</param>
        /// <returns>窗体构造器，可继续链式配置。</returns>
        public static WindowBuilder NewWindow(Rect rect, WindowData.TitleBarConfig titleBar, Action<WindowData> content)
        {
            return new WindowBuilder(rect, titleBar, content);
        }

        /// <summary>
        ///     Box样式Vertical布局
        /// </summary>
        /// <param name="content"></param>
        /// <param name="options"></param>
        public static void Box(Action content, params GUILayoutOption[] options)
        {
            GUILayout.BeginVertical(DarkSkin.SPanel, options);
            content?.Invoke();
            GUILayout.EndVertical();
        }
        /// <summary>
        ///     Box样式Vertical布局
        /// </summary>
        /// <param name="content"></param>
        /// <param name="options"></param>
        public static void BoxDetail(Action content, params GUILayoutOption[] options)
        {
            GUILayout.BeginVertical(DarkSkin.SDetail, options);
            content?.Invoke();
            GUILayout.EndVertical();
        }

        /// <summary>
        /// 行，交替行 样式1
        /// </summary>
        /// <param name="content"></param>
        /// <param name="height"></param>高度最好去GuiSkin去修改，自定义会new
        public static void Row(Action content, float height = 30f)
        {
            var style = !Mathf.Approximately(height, 30f)
                ? new GUIStyle(DarkSkin.SRow) { fixedHeight = height }
                : DarkSkin.SRow;
            GUILayout.BeginHorizontal(style);
            content?.Invoke();
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 行，交替行 样式2
        /// </summary>
        /// <param name="content"></param>
        /// <param name="height"></param> 高度最好去GuiSkin去修改，自定义会new
        public static void RowAlt(Action content, float height = 30f)
        {
           var style = !Mathf.Approximately(height, 30f)
                ? new GUIStyle(DarkSkin.SRowAlt) { fixedHeight = height }
                : DarkSkin.SRowAlt;
            GUILayout.BeginHorizontal(style);
            content?.Invoke();
            GUILayout.EndHorizontal();
        }
        
        /// <summary>
        ///     水平布局容器（回调式）。
        ///     例如：
        ///     UI.Horizontal(() =>
        ///     {
        ///     UI.Label().Text("普通文本").Draw();
        ///     });
        /// </summary>
        public static void Horizontal(Action content, GUIStyle style = null, params GUILayoutOption[] options)
        {
            if (style != null)
                GUILayout.BeginHorizontal(style, options);
            else
                GUILayout.BeginHorizontal(options);

            content?.Invoke();
            GUILayout.EndHorizontal();
        }

        /// <summary>
        ///     垂直布局容器（回调式）。
        ///     例如：
        ///     UI.Vertical(() =>
        ///     {
        ///     UI.Label().Text("普通文本").Draw();
        ///     });
        /// </summary>
        public static void Vertical(Action content, GUIStyle style = null, params GUILayoutOption[] options)
        {
            if (style != null)
                GUILayout.BeginVertical(style, options);
            else
                GUILayout.BeginVertical(options);

            content?.Invoke();
            GUILayout.EndVertical();
        }

        /// <summary>
        ///     开始一个水平布局容器，配合 using 自动结束。
        ///     例如：
        ///     using (UI.HorizontalScope())
        ///     {
        ///     UI.Label().Text("普通文本").Draw();
        ///     }
        /// </summary>
        public static LayoutScope HorizontalScope(GUIStyle style = null, params GUILayoutOption[] options)
        {
            if (style != null)
                GUILayout.BeginHorizontal(style, options);
            else
                GUILayout.BeginHorizontal(options);

            return new LayoutScope(LayoutScope.LayoutType.Horizontal);
        }

        /// <summary>
        ///     开始一个垂直布局容器，配合 using 自动结束。
        ///     例如：
        ///     using (UI.VerticalScope())
        ///     {
        ///     UI.Label().Text("普通文本").Draw();
        ///     }
        /// </summary>
        public static LayoutScope VerticalScope(GUIStyle style = null, params GUILayoutOption[] options)
        {
            if (style != null)
                GUILayout.BeginVertical(style, options);
            else
                GUILayout.BeginVertical(options);

            return new LayoutScope(LayoutScope.LayoutType.Vertical);
        }

        /// <summary>
        ///     固定像素间隔。
        /// </summary>
        public static void Space(float pixels = 10f)
        {
            GUILayout.Space(pixels);
        }

        /// <summary>
        ///     弹性间隔，把两边元素推到两端。
        /// </summary>
        public static void FlexibleSpace()
        {
            GUILayout.FlexibleSpace();
        }

        /// <summary>
        ///     绘制一条水平分隔线。
        /// </summary>
        public static void Divider(float pad = 6f)
        {
            DarkSkin.Divider(pad);
        }

        /// <summary>
        ///     布局容器作用域，支持 using 自动结束布局。
        /// </summary>
        public readonly struct LayoutScope : IDisposable
        {
            public enum LayoutType
            {
                Horizontal,
                Vertical
            }

            private readonly LayoutType _type;

            public LayoutScope(LayoutType type)
            {
                _type = type;
            }

            public void Dispose()
            {
                switch (_type)
                {
                    case LayoutType.Horizontal:
                        GUILayout.EndHorizontal();
                        break;
                    case LayoutType.Vertical:
                        GUILayout.EndVertical();
                        break;
                }
            }
        }

        /// <summary>折叠面板（包含标题和展开内容区）</summary>
        /// <param name="title">标题文本</param>
        /// <param name="expanded">是否展开</param>
        /// <param name="content">展开后绘制的内容</param>
        /// <returns>新的展开状态</returns>
        public static bool Foldout(string title, bool expanded, Action content = null)
        {
            bool newExpanded = DarkSkin.Foldout(expanded, Loc.Get(title));
            if (newExpanded && content != null)
                Box(content);
            return newExpanded;
        }

        /// <summary>
        ///     表格（首行为表头，支持点击选中行）
        /// </summary>
        /// <param name="data">二维数组，第 0 行为表头</param>
        /// <param name="selectedRow">当前选中行索引</param>
        /// <param name="colWidth">列宽（所有列统一）</param>
        /// <param name="selectable">是否可以选中</param>
        /// <returns>新的选中行索引</returns>
        public static int Table(string[,] data, int selectedRow, float colWidth = 120, bool selectable = true)
        {
            var cols = data.GetLength(1);

            // 表头
            Horizontal(() =>
            {
                for (int c = 0; c < cols; c++)
                    GUILayout.Label(data[0, c], DarkSkin.SDetailHead, GUILayout.Width(colWidth));
            });

            // 数据行
            for (int r = 1; r < data.GetLength(0); r++)
            {
                var row = r;
                var rowStyle = selectable && row == selectedRow
                    ? DarkSkin.SRowSelected
                    : (row % 2 == 0 ? DarkSkin.SRow : DarkSkin.SRowAlt);

                Horizontal(() =>
                {
                    for (int c = 0; c < cols; c++)
                        UI.Label(data[row, c]).Text().Draw(GUILayout.Width(colWidth));
                }, rowStyle);

                if (selectable)
                {
                    var rowRect = GUILayoutUtility.GetLastRect();
                    if (Event.current.type == EventType.MouseDown && rowRect.Contains(Event.current.mousePosition))
                    {
                        selectedRow = row;
                        Event.current.Use();
                    }
                }
            }
            return selectedRow;
        }

        /// <summary>标签页组，水平排列芯片按钮，返回新的选中索引。</summary>
        public static int TabGroup(string[] labels, int active)
        {
            Horizontal(() =>
            {
                for (int i = 0; i < labels.Length; i++)
                {
                    if (DarkSkin.TabChip(labels[i], active == i))
                        active = i;
                }
            });
            return active;
        }

        /// <summary>
        /// 滑动条
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="sliderValue">值</param>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <param name="decimals">保留几位小数</param>
        /// <param name="topPadding"></param>
        /// <param name="options">滑动条布局选项，默认占满可用宽度</param>
        /// <returns></returns>
        public static float Slider(string text, float sliderValue, float min = 0f, float max = 1f, int decimals = 2,
            float topPadding = 8f, params GUILayoutOption[] options)
        {
            decimals = Mathf.Max(0, decimals);
            GUILayout.BeginHorizontal();
            var format = $"F{decimals}";
            var content = new GUIContent($"{Loc.Get(text)}: {sliderValue.ToString(format)}");
            var size = DarkSkin.SLabel.CalcSize(content);
            GUILayout.Label(content, DarkSkin.SLabel, GUILayout.Width(size.x));
            GUILayout.BeginVertical();
            GUILayout.Space(topPadding);
            if (options == null || options.Length == 0)
                options = new[] { GUILayout.ExpandWidth(true) };
            sliderValue = GUILayout.HorizontalSlider(sliderValue, min, max, options);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            var multiplier = Mathf.Pow(10f, decimals);
            return Mathf.Round(sliderValue * multiplier) / multiplier;
        }

        /// <summary>
        ///     输入框（支持 placeholder）
        /// </summary>
        /// <param name="text">当前文本</param>
        /// <param name="style">自定义样式</param>
        /// <param name="placeholder">为空时显示的提示文本</param>
        /// <param name="options">布局选项</param>
        /// <returns>文本</returns>
        public static string TextFiled(string text, GUIStyle style = null, string placeholder = null, params GUILayoutOption[] options)
        {
            text = text ?? string.Empty;
            if (style == null) style = DarkSkin.SField;

            // 空文本时用 placeholder 计算布局矩形，避免输入框过窄
            var content = new GUIContent(string.IsNullOrEmpty(text) ? (placeholder ?? string.Empty) : text);
            var rect = GUILayoutUtility.GetRect(content, style, options);
            text = GUI.TextField(rect, text, style);

            if (string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(placeholder))
            {
                GUI.Label(rect, Loc.Get(placeholder), DarkSkin.SFieldPlaceholder);
            }

            return text;
        }

        /// <summary>
        ///     文本域
        /// </summary>
        /// <param name="text"></param>
        /// <param name="style"></param>
        /// <param name="options"></param>
        /// 文本
        /// <returns>文本</returns>
        public static string TextArea(string text, GUIStyle style = null, params GUILayoutOption[] options)
        {
            text = text ?? string.Empty;
            if (style == null) style = DarkSkin.SField;
            if (options == null || options.Length == 0)
                options = new[] { GUILayout.Height(80) };
            return GUILayout.TextArea(text, style, options);
        }
    }

    /// <summary>
    ///     窗体链式构造器。
    /// </summary>
    public class WindowBuilder
    {
        private readonly Rect _rect;
        private readonly UI.WindowData.TitleBarConfig _titleBar;
        private readonly Action<UI.WindowData> _content;
        private int _id = 1000;
        private bool _visible = true;
        private bool _draggable = true;
        private UI.WindowData.DragMode _dragMode = UI.WindowData.DragMode.TitleBarOnly;
        private bool _resizable;
        private int _layer;
        private Vector2 _minSize = new Vector2(200f, 100f);
        private Action _onClose;

        public WindowBuilder(Rect rect, UI.WindowData.TitleBarConfig titleBar, Action<UI.WindowData> content)
        {
            _rect = rect;
            _titleBar = titleBar ?? throw new ArgumentNullException(nameof(titleBar));
            _content = content;
        }

        /// <summary>窗口唯一标识，同时用于 GUI 绘制和位置持久化。</summary>
        public WindowBuilder Id(int id)
        {
            _id = id;
            return this;
        }

        /// <summary>显示窗体。</summary>
        public WindowBuilder Show()
        {
            _visible = true;
            return this;
        }

        /// <summary>隐藏窗体。</summary>
        public WindowBuilder Hide()
        {
            _visible = false;
            return this;
        }

        /// <summary>设置是否可拖拽。</summary>
        public WindowBuilder Draggable(bool value = true)
        {
            _draggable = value;
            return this;
        }

        /// <summary>设置拖拽模式。</summary>
        public WindowBuilder DragBy(UI.WindowData.DragMode mode)
        {
            _dragMode = mode;
            return this;
        }

        /// <summary>设置是否可调整大小。</summary>
        public WindowBuilder Resizable(bool value = true)
        {
            _resizable = value;
            return this;
        }

        /// <summary>设置窗口层级，越大越在上层。</summary>
        public WindowBuilder Layer(int layer)
        {
            _layer = layer;
            return this;
        }

        /// <summary>设置窗口最小尺寸。</summary>
        public WindowBuilder MinSize(Vector2 size)
        {
            _minSize = size;
            return this;
        }

        /// <summary>设置关闭回调。</summary>
        public WindowBuilder OnClose(Action callback)
        {
            _onClose = callback;
            return this;
        }

        /// <summary>获取最终窗体数据。</summary>
        public UI.WindowData Build()
        {
            var window = UI.WindowControls.CreateWindow(_id, _rect, _titleBar, _content);
            window.Visible = _visible;
            window.Draggable = _draggable;
            window.DragArea = _dragMode;
            window.Resizable = _resizable;
            window.Layer = _layer;
            window.MinSize = _minSize;
            if (_onClose != null) window.OnClose += _onClose;
            return window;
        }
    }
}
