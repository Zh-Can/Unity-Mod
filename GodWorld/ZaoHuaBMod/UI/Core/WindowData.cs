using System;
using UnityEngine;

namespace ZaoHuaBMod.UI.Core
{
    /// <summary>
    ///     窗体数据
    /// </summary>
    public class WindowData
    {
        public enum DragMode
        {
            // 仅标题栏可拖拽
            TitleBarOnly,

            // 整个窗体都可拖拽
            WholeWindow
        }

        // 内容绘制
        public Action<WindowData> Content;

        // 内容区滚动位置（GUILayout.BeginScrollView 需要）
        internal Vector2 ContentScrollPos;

        // 拖拽模式
        public DragMode DragArea = DragMode.TitleBarOnly;

        // 是否可拖拽
        public bool Draggable = true;
        public int Id;

        // 窗口层级，越大越在上层
        public int Layer = 0;

        // 最小尺寸
        public Vector2 MinSize = new Vector2(200f, 100f);

        // 关闭回调
        public Action OnClose;

        public Rect Rect;

        // 是否可调整大小
        public bool Resizable = false;

        // 标题栏配置
        public TitleBarConfig TitleBar;

        public bool Visible = true;

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
        ///     显示窗体
        /// </summary>
        public void Show()
        {
            Visible = true;
        }

        /// <summary>
        ///     隐藏窗体
        /// </summary>
        public void Hide()
        {
            Visible = false;
        }

        /// <summary>
        ///     销毁窗体
        /// </summary>
        public void Destroy()
        {
            GUIManager.Instance.DestroyWindow(Id);
        }

        /// <summary>
        ///     标题栏配置
        /// </summary>
        public class TitleBarConfig
        {
            public string CloseText = "✕";

            // 字体大小
            public int FontSize = 18;

            // 高度
            public float Height = 36f;

            // 显示关闭按钮
            public bool ShowCloseButton = true;
            public string Title;

            public TitleBarConfig(string title)
            {
                Title = title;
            }
        }
    }
}