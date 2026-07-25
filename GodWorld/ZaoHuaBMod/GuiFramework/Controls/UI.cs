using System;
using UnityEngine;
using ZaoHuaBMod.GuiFramework.Style;

namespace ZaoHuaBMod.GuiFramework.Controls
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
            var window = WindowControls.CreateWindow(rect, title, content);
            return new WindowBuilder(window);
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
            var window = WindowControls.CreateWindow(rect, titleBar, content);
            return new WindowBuilder(window);
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

        /// <summary>
        ///     滑动条
        /// </summary>
        /// <param name="text"></param>
        /// 文本
        /// <param name="sliderValue"></param>
        /// 值
        /// <param name="min"></param>
        /// 最小值
        /// <param name="max"></param>
        /// 最大值
        /// <param name="decimals"></param>
        /// 保留几位小数
        /// <param name="topPadding"></param>
        /// <returns></returns>
        public static float Slider(string text, float sliderValue, float min = 0f, float max = 1f, int decimals = 2,
            float topPadding = 8f)
        {
            decimals = Mathf.Max(0, decimals);
            GUILayout.BeginHorizontal();
            var format = $"F{decimals}";
            var content = new GUIContent($"{text}: {sliderValue.ToString(format)}");
            var size = DarkSkin.SLabel.CalcSize(content);
            GUILayout.Label(content, DarkSkin.SLabel, GUILayout.Width(size.x));
            GUILayout.BeginVertical();
            GUILayout.Space(topPadding);
            sliderValue = GUILayout.HorizontalSlider(sliderValue, min, max, GUILayout.ExpandWidth(true));
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            var multiplier = Mathf.Pow(10f, decimals);
            return Mathf.Round(sliderValue * multiplier) / multiplier;
        }

        /// <summary>
        ///     输入框
        /// </summary>
        /// <param name="text"></param>
        /// <param name="style"></param>
        /// <param name="options"></param>
        /// 文本
        /// <returns>文本</returns>
        public static string TextFiled(string text, GUIStyle style = null, params GUILayoutOption[] options)
        {
            text = text ?? string.Empty;
            if (style == null) style = DarkSkin.SField;
            return GUILayout.TextField(text, style, options);
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
        private readonly UI.WindowData _window;

        public WindowBuilder(UI.WindowData window)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
        }

        /// <summary>显示窗体。</summary>
        public WindowBuilder Show()
        {
            _window.Show();
            return this;
        }

        /// <summary>隐藏窗体。</summary>
        public WindowBuilder Hide()
        {
            _window.Hide();
            return this;
        }

        /// <summary>设置是否可拖拽。</summary>
        public WindowBuilder Draggable(bool value = true)
        {
            _window.Draggable = value;
            return this;
        }

        /// <summary>设置拖拽模式。</summary>
        public WindowBuilder DragBy(UI.WindowData.DragMode mode)
        {
            _window.DragArea = mode;
            return this;
        }

        /// <summary>设置是否可调整大小。</summary>
        public WindowBuilder Resizable(bool value = true)
        {
            _window.Resizable = value;
            return this;
        }

        /// <summary>设置窗口层级，越大越在上层。</summary>
        public WindowBuilder Layer(int layer)
        {
            _window.Layer = layer;
            return this;
        }

        /// <summary>设置窗口最小尺寸。</summary>
        public WindowBuilder MinSize(Vector2 size)
        {
            _window.MinSize = size;
            return this;
        }

        /// <summary>设置关闭回调。</summary>
        public WindowBuilder OnClose(Action callback)
        {
            _window.OnClose = callback;
            return this;
        }

        /// <summary>获取最终窗体数据。</summary>
        public UI.WindowData Build()
        {
            return _window;
        }
    }
}