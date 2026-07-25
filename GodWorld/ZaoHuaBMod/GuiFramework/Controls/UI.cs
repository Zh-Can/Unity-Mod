using System;
using UnityEngine;
using ZaoHuaBMod.GuiFramework.Style;

namespace ZaoHuaBMod.GuiFramework.Controls
{
    /// <summary>
    ///     UI 入口，提供窗体创建与基础控件。
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
        public UI.WindowData Build() => _window;
    }
}
