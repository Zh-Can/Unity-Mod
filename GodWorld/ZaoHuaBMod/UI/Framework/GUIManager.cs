using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZaoHuaBMod.Core;

namespace ZaoHuaBMod.UI.Framework
{
    /// <summary>
    /// GUI 工具管理器
    /// </summary>
    public class GUIManager
    {
        private static GUIManager _instance;
        public static GUIManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GUIManager();
                return _instance;
            }
        }

        private readonly Dictionary<int, WindowData> _windows = new Dictionary<int, WindowData>();

        // 窗体 ID 生成器
        private int _nextWindowId = 1000;

        // 当前拖拽的窗体
        private WindowData _draggingWindow;

        // 拖拽开始时鼠标与窗体左上角的偏移
        private Vector2 _dragOffset;

        // 当前调整大小的窗体
        private WindowData _resizingWindow;

        // 调整大小时鼠标按下位置
        private Vector2 _resizeStartMouse;

        // 调整大小时窗体原始尺寸
        private Vector2 _resizeStartSize;

        // 调整大小区域的尺寸
        private const float ResizeHandleSize = 16f;

        // 全局缩放比例（只影响尺寸，不影响位置）
        public float Scale { get; private set; } = 1f;

        /// <summary>
        /// 设置全局缩放
        /// </summary>
        public void SetScale(float scale)
        {
            Scale = Mathf.Max(scale, 1f);
        }

        /// <summary>
        /// 创建窗体
        /// </summary>
        public WindowData CreateWindow(
            Rect rect,
            WindowData.TitleBarConfig titleBar,
            System.Action<WindowData> content)
        {
            var id = _nextWindowId++;
            var window = new WindowData(id, rect, titleBar, content);
            _windows[id] = window;
            return window;
        }

        /// <summary>
        /// 创建窗体（简化版，只传标题）
        /// </summary>
        public WindowData CreateWindow(
            Rect rect,
            string title,
            System.Action<WindowData> content)
        {
            return CreateWindow(rect, new WindowData.TitleBarConfig(title), content);
        }

        /// <summary>
        /// 销毁窗体
        /// </summary>
        public bool DestroyWindow(int id)
        {
            if (!_windows.TryGetValue(id, out var window))
                return false;

            window.OnClose?.Invoke();
            _windows.Remove(id);

            if (_draggingWindow == window)
                _draggingWindow = null;
            if (_resizingWindow == window)
                _resizingWindow = null;

            return true;
        }

        /// <summary>
        /// 获取窗体
        /// </summary>
        public WindowData GetWindow(int id)
        {
            _windows.TryGetValue(id, out var window);
            return window;
        }

        /// <summary>
        /// 销毁所有窗体
        /// </summary>
        public void DestroyAllWindows()
        {
            foreach (var window in _windows.Values)
            {
                window.OnClose?.Invoke();
            }
            _windows.Clear();
            _draggingWindow = null;
            _resizingWindow = null;
        }

        /// <summary>
        /// 主绘制入口，需要在 MonoBehaviour.OnGUI 中调用
        /// </summary>
        public void OnGUI()
        {
            try
            {
                var previousSkin = GUI.skin;
                GUI.skin = DarkSkin.GetSkin(Scale);

                var sortedWindows = _windows.Values
                    .Where(w => w.Visible)
                    .OrderBy(w => w.Layer)
                    .ToList();

                foreach (var window in sortedWindows)
                {
                    DrawWindow(window);
                }

                HandleDragAndResize();
                DarkSkin.DrawTooltip();

                GUI.skin = previousSkin;
            }
            catch (System.Exception ex)
            {
                Log.Error($"[GUIManager] OnGUI 异常: {ex}");
            }
        }

        /// <summary>
        /// 绘制单个窗体
        /// </summary>
        private void DrawWindow(WindowData window)
        {
            var rect = window.Rect;
            var scaledRect = new Rect(rect.x, rect.y, rect.width * Scale, rect.height * Scale);
            var titleBar = window.TitleBar;
            var hasTitleBar = !string.IsNullOrEmpty(titleBar.Title);
            var titleBarHeight = hasTitleBar ? titleBar.Height * Scale : 0f;
            var titleBarRect = new Rect(0f, 0f, scaledRect.width, titleBarHeight);

            // 窗体背景
            GUI.BeginGroup(scaledRect);
            GUI.color = DarkSkin.C.BgDark;
            GUI.DrawTexture(new Rect(0f, 0f, scaledRect.width, scaledRect.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            if (hasTitleBar)
            {
                // 标题栏背景
                GUI.color = DarkSkin.C.BgMid;
                GUI.DrawTexture(titleBarRect, Texture2D.whiteTexture);
                GUI.color = Color.white;

                // 标题
                var titleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = Mathf.RoundToInt(titleBar.FontSize * Scale),
                    alignment = TextAnchor.MiddleLeft,
                    normal = { textColor = DarkSkin.C.TextBright }
                };
                GUI.Label(new Rect(10f * Scale, 0f, scaledRect.width - 60f * Scale, titleBarHeight), titleBar.Title, titleStyle);

                // 标题栏右侧按钮区
                var btnSize = titleBarHeight - 4f * Scale;
                var btnY = 2f * Scale;
                var rightX = scaledRect.width - 4f * Scale;

                // 缩放按钮：- [100%] + 在关闭按钮左侧
                var minusBtnRect = new Rect(rightX - btnSize * 4.8f - 10f * Scale, btnY, btnSize, btnSize);
                var scaleLabelRect = new Rect(rightX - btnSize * 3.8f - 8f * Scale, btnY, btnSize * 1.8f, btnSize);
                var plusBtnRect = new Rect(rightX - btnSize * 2f - 4f * Scale, btnY, btnSize, btnSize);

                var titleBtnStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = Mathf.RoundToInt(titleBar.FontSize * Scale),
                    alignment = TextAnchor.MiddleCenter,
                    padding = new RectOffset(0, 0, 0, 0)
                };
                var scaleLabelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = Mathf.RoundToInt(titleBar.FontSize * Scale * 0.8f),
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = DarkSkin.C.TextBright }
                };

                if (GUI.Button(minusBtnRect, "-", titleBtnStyle))
                    SetScale(Scale - 0.1f);

                GUI.Label(scaleLabelRect, $"{Mathf.RoundToInt(Scale * 100)}%", scaleLabelStyle);
                if (Event.current.type == EventType.MouseDown && scaleLabelRect.Contains(Event.current.mousePosition))
                {
                    SetScale(1f);
                    Event.current.Use();
                }

                if (GUI.Button(plusBtnRect, "+", titleBtnStyle))
                    SetScale(Scale + 0.1f);

                // 关闭按钮
                if (titleBar.ShowCloseButton)
                {
                    var closeBtnRect = new Rect(rightX - btnSize, btnY, btnSize, btnSize);
                    var isHover = closeBtnRect.Contains(Event.current.mousePosition);

                    GUI.color = isHover ? DarkSkin.C.ErrorHover : DarkSkin.C.Error;
                    GUI.DrawTexture(closeBtnRect, Texture2D.whiteTexture);
                    GUI.color = Color.white;

                    var closeStyle = new GUIStyle(GUI.skin.label)
                    {
                        fontSize = Mathf.RoundToInt(titleBar.FontSize * Scale),
                        alignment = TextAnchor.MiddleCenter,
                        normal = { textColor = Color.white }
                    };
                    GUI.Label(closeBtnRect, titleBar.CloseText, closeStyle);

                    if (Event.current.type == EventType.MouseDown && closeBtnRect.Contains(Event.current.mousePosition))
                    {
                        window.Hide();
                        Event.current.Use();
                        GUI.EndGroup();
                        return;
                    }
                }
            }

            // 内容区域
            var contentRect = new Rect(0f, titleBarHeight, scaledRect.width, scaledRect.height - titleBarHeight);
            GUILayout.BeginArea(contentRect);
            window.Content?.Invoke(window);
            GUILayout.EndArea();

            // 调整大小手柄
            if (window.Resizable)
            {
                var scaledHandleSize = ResizeHandleSize * Scale;
                var resizeRect = new Rect(scaledRect.width - scaledHandleSize, scaledRect.height - scaledHandleSize, scaledHandleSize, scaledHandleSize);
                GUI.color = new Color(1f, 1f, 1f, 0.3f);
                GUI.DrawTexture(resizeRect, Texture2D.whiteTexture);
                GUI.color = Color.white;
            }

            GUI.EndGroup();
        }

        /// <summary>
        /// 处理拖拽和调整大小输入
        /// </summary>
        private void HandleDragAndResize()
        {
            var evt = Event.current;
            var mousePos = evt.mousePosition;

            switch (evt.type)
            {
                case EventType.MouseDown:
                    foreach (var window in _windows.Values
                        .Where(w => w.Visible)
                        .OrderByDescending(w => w.Layer))
                    {
                        var scaledRect = new Rect(
                            window.Rect.x,
                            window.Rect.y,
                            window.Rect.width * Scale,
                            window.Rect.height * Scale);

                        if (!scaledRect.Contains(mousePos))
                            continue;

                        // 优先检查调整大小
                        if (window.Resizable)
                        {
                            var scaledHandleSize = ResizeHandleSize * Scale;
                            var resizeRect = new Rect(
                                scaledRect.x + scaledRect.width - scaledHandleSize,
                                scaledRect.y + scaledRect.height - scaledHandleSize,
                                scaledHandleSize,
                                scaledHandleSize);

                            if (resizeRect.Contains(mousePos))
                            {
                                _resizingWindow = window;
                                _resizeStartMouse = mousePos;
                                _resizeStartSize = new Vector2(window.Rect.width, window.Rect.height);
                                evt.Use();
                                break;
                            }
                        }

                        // 检查拖拽
                        if (window.Draggable)
                        {
                            bool canDrag = false;

                            if (window.DragArea == WindowData.DragMode.WholeWindow)
                            {
                                // 全窗口拖拽：排除调整大小手柄区域
                                if (window.Resizable)
                                {
                                    var scaledHandleSize = ResizeHandleSize * Scale;
                                    var resizeRect = new Rect(
                                        scaledRect.x + scaledRect.width - scaledHandleSize,
                                        scaledRect.y + scaledRect.height - scaledHandleSize,
                                        scaledHandleSize,
                                        scaledHandleSize);

                                    canDrag = !resizeRect.Contains(mousePos);
                                }
                                else
                                {
                                    canDrag = true;
                                }
                            }
                            else
                            {
                                // 仅标题栏拖拽
                                var titleBarHeight = window.TitleBar.Height * Scale;
                                var titleBarRect = new Rect(
                                    scaledRect.x,
                                    scaledRect.y,
                                    scaledRect.width,
                                    titleBarHeight);

                                canDrag = titleBarRect.Contains(mousePos);
                            }

                            if (canDrag)
                            {
                                _draggingWindow = window;
                                _dragOffset = mousePos - new Vector2(window.Rect.x, window.Rect.y);
                                evt.Use();
                            }
                        }

                        break;
                    }
                    break;

                case EventType.MouseDrag:
                    if (_draggingWindow != null)
                    {
                        _draggingWindow.Rect = new Rect(
                            mousePos.x - _dragOffset.x,
                            mousePos.y - _dragOffset.y,
                            _draggingWindow.Rect.width,
                            _draggingWindow.Rect.height);
                        evt.Use();
                    }

                    if (_resizingWindow != null)
                    {
                        // 鼠标移动是屏幕像素，需要除以 Scale 转成逻辑尺寸变化
                        var delta = (mousePos - _resizeStartMouse) / Scale;
                        var newWidth = Mathf.Max(_resizeStartSize.x + delta.x, _resizingWindow.MinSize.x);
                        var newHeight = Mathf.Max(_resizeStartSize.y + delta.y, _resizingWindow.MinSize.y);
                        _resizingWindow.Rect = new Rect(
                            _resizingWindow.Rect.x,
                            _resizingWindow.Rect.y,
                            newWidth,
                            newHeight);
                        evt.Use();
                    }
                    break;

                case EventType.MouseUp:
                    _draggingWindow = null;
                    _resizingWindow = null;
                    break;
            }
        }
    }
}
