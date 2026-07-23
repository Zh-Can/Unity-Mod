using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZaoHuaBMod.Core;

namespace ZaoHuaBMod.UI.Framework
{
    /// <summary>
    ///     GUI 窗口管理器。
    ///     窗口绘制采用 FeatureEditor.Frontend 风格：静态 DarkSkin + GUI.matrix 缩放。
    /// </summary>
    public class GUIManager
    {
        private const float ResizeHandleSize = 16f;
        private static GUIManager _instance;

        private readonly Dictionary<int, WindowData> _windows = new Dictionary<int, WindowData>();

        private WindowData _draggingWindow;
        private Vector2 _dragOffset;
        private int _nextWindowId = 1000;
        private Vector2 _resizeStartMouse;
        private Vector2 _resizeStartSize;
        private WindowData _resizingWindow;

        public static GUIManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GUIManager();
                return _instance;
            }
        }

        // 全局缩放（与 FeatureEditor.Frontend 一致范围）
        public float Scale { get; private set; } = 1f;

        public void SetScale(float scale)
        {
            Scale = Mathf.Clamp(scale, 0.8f, 2.5f);
            ModConfig.Scale = Scale;
            ModConfig.Save();
        }

        public WindowData CreateWindow(
            Rect rect,
            WindowData.TitleBarConfig titleBar,
            Action<WindowData> content)
        {
            var id = _nextWindowId++;
            var window = new WindowData(id, rect, titleBar, content);
            _windows[id] = window;
            return window;
        }

        public WindowData CreateWindow(
            Rect rect,
            string title,
            Action<WindowData> content)
        {
            return CreateWindow(rect, new WindowData.TitleBarConfig(title), content);
        }

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

        public WindowData GetWindow(int id)
        {
            _windows.TryGetValue(id, out var window);
            return window;
        }

        public void DestroyAllWindows()
        {
            foreach (var window in _windows.Values) window.OnClose?.Invoke();
            _windows.Clear();
            _draggingWindow = null;
            _resizingWindow = null;
        }

        /// <summary>
        ///     主绘制入口，需要在 MonoBehaviour.OnGUI 中调用。
        /// </summary>
        public void OnGUI()
        {
            try
            {
                DarkSkin.InitStyles();

                var previousSkin = GUI.skin;
                GUI.skin = DarkSkin.Skin;
                var matrix = GUI.matrix;

                var sortedWindows = _windows.Values
                    .Where(w => w.Visible)
                    .OrderBy(w => w.Layer)
                    .ToList();

                foreach (var window in sortedWindows)
                {
                    GUI.matrix = matrix;
                    GUIUtility.ScaleAroundPivot(new Vector2(Scale, Scale), new Vector2(window.Rect.x, window.Rect.y));
                    GUI.Window(window.Id, window.Rect, id => DrawWindow(id, window), GUIContent.none, DarkSkin.SWindow);
                }

                GUI.matrix = matrix;
                HandleDragAndResize();
                DarkSkin.DrawTooltip();

                GUI.skin = previousSkin;
            }
            catch (Exception ex)
            {
                Log.Error($"[GUIManager] OnGUI 异常: {ex}");
            }
        }

        /// <summary>
        ///     绘制单个窗体（由 GUI.Window 调用）。
        /// </summary>
        private void DrawWindow(int id, WindowData window)
        {
            var titleBar = window.TitleBar;
            var hasTitleBar = !string.IsNullOrEmpty(titleBar.Title);

            // 标题栏（FeatureEditor.Frontend 风格）
            if (hasTitleBar)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(titleBar.Title, DarkSkin.STitle);
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("－", DarkSkin.SBtn))
                    SetScale(Scale - 0.1f);

                if (GUILayout.Button("＋", DarkSkin.SBtn))
                    SetScale(Scale + 0.1f);

                GUILayout.Space(10f);

                // 语言切换按钮：始终显示当前语言
                {
                    var currentName = Localization.CurrentLanguage;
                    var current = Localization.AvailableLanguages
                        .FirstOrDefault(l => l.Code == currentName);
                    var btnLabel = current?.DisplayName ?? currentName;

                    if (GUILayout.Button(btnLabel, DarkSkin.SBtn) && Localization.AvailableLanguages.Count > 1)
                        Localization.CycleLanguage();

                    GUILayout.Space(10f);
                }

                if (titleBar.ShowCloseButton)
                    if (GUILayout.Button(titleBar.CloseText, DarkSkin.SBtnDel))
                        window.Hide();

                GUILayout.EndHorizontal();
                DarkSkin.Divider(8f);
            }

            // 内容区域
            window.ContentScrollPos = GUILayout.BeginScrollView(window.ContentScrollPos);
            window.Content?.Invoke(window);
            GUILayout.EndScrollView();
        }

        /// <summary>
        ///     处理拖拽和调整大小输入。
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
                            var resizeRect = new Rect(
                                scaledRect.x + scaledRect.width - ResizeHandleSize,
                                scaledRect.y + scaledRect.height - ResizeHandleSize,
                                ResizeHandleSize,
                                ResizeHandleSize);

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
                            var canDrag = false;

                            if (window.DragArea == WindowData.DragMode.WholeWindow)
                            {
                                if (window.Resizable)
                                {
                                    var resizeRect = new Rect(
                                        scaledRect.x + scaledRect.width - ResizeHandleSize,
                                        scaledRect.y + scaledRect.height - ResizeHandleSize,
                                        ResizeHandleSize,
                                        ResizeHandleSize);

                                    canDrag = !resizeRect.Contains(mousePos);
                                }
                                else
                                {
                                    canDrag = true;
                                }
                            }
                            else
                            {
                                // 仅标题栏可拖拽，标题栏高度约 40（缩放后）
                                var titleBarHeight = 40f * Scale;
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