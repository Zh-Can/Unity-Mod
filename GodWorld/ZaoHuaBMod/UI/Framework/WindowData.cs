using System;
using UnityEngine;

namespace ZaoHuaBMod.UI.Framework
{
    /// <summary>
    /// 窗体数据
    /// </summary>
    public class WindowData
    {
        public int Id;

        public Rect Rect;

        public bool Visible = true;

        // 是否可拖拽
        public bool Draggable = true;

        // 是否可调整大小
        public bool Resizable = false;

        // 最小尺寸
        public Vector2 MinSize = new Vector2(200f, 100f);

        // 窗口层级，越大越在上层
        public int Layer = 0;

        // 标题栏配置
        public TitleBarConfig TitleBar;

        // 内容绘制
        public Action<WindowData> Content;

        // 关闭回调
        public Action OnClose;

        // 拖拽模式
        public DragMode DragArea = DragMode.TitleBarOnly;

        public enum DragMode
        {
            // 仅标题栏可拖拽
            TitleBarOnly,

            // 整个窗体都可拖拽
            WholeWindow
        }

        public WindowData(
            int id,
            Rect rect,
            TitleBarConfig titleBar,
            Action<WindowData> content)
        {
            Id = id;
            Rect = rect;
            TitleBar = titleBar;
            Content = content;
        }

        /// <summary>
        /// 显示窗体
        /// </summary>
        public void Show() => Visible = true;

        /// <summary>
        /// 隐藏窗体
        /// </summary>
        public void Hide() => Visible = false;

        /// <summary>
        /// 销毁窗体
        /// </summary>
        public void Destroy() => GUIManager.Instance.DestroyWindow(Id);

        /// <summary>
        /// 标题栏配置
        /// </summary>
        public class TitleBarConfig
        {
            public string Title;

            // 高度
            public float Height = 30f;

            // 字体大小
            public int FontSize = 18;

            // 显示关闭按钮
            public bool ShowCloseButton = true;

            public string CloseText = "X";

            public TitleBarConfig(string title)
            {
                Title = title;
            }
        }
    }
}
