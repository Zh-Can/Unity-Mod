using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LunHuiShop.GuiFramework.Config;
using LunHuiShop.GuiFramework.Localization;
using LunHuiShop.GuiFramework.Logger;
using LunHuiShop.GuiFramework.Style;

namespace LunHuiShop.GuiFramework.Controls
{
    /// <summary>
    ///     UI 窗体管理分部：创建、绘制、拖拽、缩放。
    /// </summary>
    public static partial class UI
    {
        public static class WindowControls
        {
            private const float ResizeHandleSize = 16f;

            private static readonly Dictionary<int, WindowData> _windows = new Dictionary<int, WindowData>();
            private static WindowData _draggingWindow;
            private static Vector2 _dragOffset;
            private static Vector2 _resizeStartMouse;
            private static Vector2 _resizeStartSize;
            private static WindowData _resizingWindow;

            // ---- 防点穿状态 ----
            private static bool _isCapturingPointer; // MouseDown 在窗口内按下后直到 MouseUp 都 true
            private static Vector2 _lastGuiMousePos; // 上一帧 OnGUI 的鼠标位置（用于游戏 Update/LateUpdate 提前读取时判断）
            private static bool _blockGameInputEnabled = true;

            /// <summary>
            ///     被 WindowData.Visible setter 调用：窗口变为可见时立即启用指针捕获。
            ///     解决第一帧时 _isCapturingPointer 尚未更新导致的穿透问题。
            /// </summary>
            internal static void NotifyWindowBecameVisible()
            {
                _isCapturingPointer = true;
            }

            /// <summary>
            ///     设置是否启用防点穿（默认 true）。关闭后鼠标事件会穿透到游戏。
            /// </summary>
            public static bool BlockGameInputEnabled
            {
                get => _blockGameInputEnabled;
                set => _blockGameInputEnabled = value;
            }

            /// <summary>
            ///     被 Harmony Patch 调用：判断当前是否应阻止游戏接收鼠标输入。
            ///     Update/LateUpdate 阶段可能早于 OnGUI，用 _lastGuiMousePos 弥补
            ///     状态滞后问题。
            /// </summary>
            public static bool ShouldBlockGamePointerInput()
            {
                if (!_blockGameInputEnabled)
                    return false;

                // 没有任何窗口可见时，立即清除捕获状态并返回 false，
                // 防止窗口隐藏后捕获状态残留导致游戏输入被持续阻塞。
                if (!_windows.Values.Any(w => w.Visible))
                {
                    _isCapturingPointer = false;
                    return false;
                }

                if (_isCapturingPointer)
                    return true;
                // 检查上一帧 OnGUI 的鼠标位置是否在窗口内
                // （游戏在 LateUpdate/Update 中读取输入时，_isCapturingPointer 可能还没更新）
                if (IsPointerOverAnyWindow(_lastGuiMousePos))
                {
                    _isCapturingPointer = true;
                    return true;
                }
                return false;
            }

            /// <summary>
            ///     全局缩放比例，范围 0.8 ~ 2.5。
            /// </summary>
            public static float Scale { get; private set; } = 1f;

            /// <summary>
            ///     设置全局缩放比例，修改后会自动保存。
            /// </summary>
            public static void SetScale(float scale)
            {
                Scale = Mathf.Clamp(scale, 0.8f, 2.5f);
                BaseConfig.Scale = Scale;
                BaseConfig.Save();
            }

            /// <summary>
            ///     创建一个新窗口。
            /// </summary>
            internal static WindowData CreateWindow(
                int id,
                Rect rect,
                WindowData.TitleBarConfig titleBar,
                Action content,
                bool resizable = false)
            {
                if (_windows.ContainsKey(id))
                    throw new InvalidOperationException($"窗口 Id {id} 已存在，请为每个窗口指定唯一 Id。");

                var window = new WindowData(id, rect, titleBar, content);

                // 读取并应用上次保存的位置/尺寸
                if (resizable)
                {
                    var savedRect = BaseConfig.LoadWindowRect(id);
                    if (savedRect.HasValue)
                        window.Rect = savedRect.Value;
                }
                else
                {
                    var savedPos = BaseConfig.LoadWindowPosition(id);
                    if (savedPos.HasValue)
                        window.Rect = new Rect(savedPos.Value.x, savedPos.Value.y, rect.width, rect.height);
                }

                // 窗口关闭时保存
                window.OnClose += () =>
                {
                    if (window.Resizable)
                        BaseConfig.SaveWindowRect(id, window.Rect);
                    else
                        BaseConfig.SaveWindowPosition(id, window.Rect.position);
                };

                _windows[id] = window;
                return window;
            }

            /// <summary>
            ///     销毁指定窗口。
            /// </summary>
            internal static bool DestroyWindow(int id)
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
            ///     获取指定窗口。
            /// </summary>
            public static WindowData GetWindow(int id)
            {
                _windows.TryGetValue(id, out var window);
                return window;
            }

            /// <summary>
            ///     销毁所有窗口。
            /// </summary>
            public static void DestroyAllWindows()
            {
                foreach (var window in _windows.Values) window.OnClose?.Invoke();
                _windows.Clear();
                _draggingWindow = null;
                _resizingWindow = null;
            }

            /// <summary>
            ///     主绘制入口，需要在 MonoBehaviour.OnGUI 中调用。
            /// </summary>
            public static void OnGUI()
            {
                
                    DarkSkin.InitStyles();
                    
                    var previousSkin = GUI.skin;
                    GUI.skin = DarkSkin.Skin;
                    var matrixOld = GUI.matrix;

                    // 缓存当前帧鼠标位置，供 ShouldBlockGamePointerInput() 在游戏 Update/LateUpdate 阶段判断
                    if (Event.current != null)
                        _lastGuiMousePos = Event.current.mousePosition;

                    var sortedWindows = _windows.Values
                        .Where(w => w.Visible)
                        .OrderBy(w => w.Layer)
                        .ToList();
                    
                    foreach (var window in sortedWindows)
                    {
                        GUI.matrix = matrixOld;
                        GUIUtility.ScaleAroundPivot(new Vector2(Scale, Scale), new Vector2(window.Rect.x, window.Rect.y));
                        GUI.Window(window.Id, window.Rect, (GUI.WindowFunction)(id => DrawWindow(id, window)), GUIContent.none, DarkSkin.SWindow);
                    }
                    
                    GUI.matrix = matrixOld;
                    HandleDragAndResize();

                    // ---- 防点穿：更新指针捕获状态（供 Harmony Patch 使用） ----
                    if (_blockGameInputEnabled)
                    {
                        var currentEvent = Event.current;
                        UpdatePointerCapture(currentEvent);
                    }

                    GUI.skin = previousSkin;
                
            }

            /// <summary>
            ///     判断鼠标是否在任意可见 IMGUI 窗口的区域内（已考虑缩放）。
            /// </summary>
            private static bool IsPointerOverAnyWindow(Vector2 guiMousePos)
            {
                foreach (var w in _windows.Values)
                {
                    if (!w.Visible) continue;
                    var scaledRect = new Rect(w.Rect.x, w.Rect.y, w.Rect.width * Scale, w.Rect.height * Scale);
                    if (scaledRect.Contains(guiMousePos)) return true;
                }
                return false;
            }

            /// <summary>
            ///     根据当前 IMGUI 事件更新指针捕获状态。
            /// </summary>
            private static void UpdatePointerCapture(Event currentEvent)
            {
                if (currentEvent == null)
                {
                    _isCapturingPointer = false;
                    return;
                }

                var isInsideAnyWindow = IsPointerOverAnyWindow(currentEvent.mousePosition);

                switch (currentEvent.type)
                {
                    case EventType.MouseDown:
                        if (isInsideAnyWindow)
                            _isCapturingPointer = true;
                        else
                            _isCapturingPointer = false;
                        break;
                    case EventType.MouseDrag:
                    case EventType.MouseMove:
                    case EventType.ScrollWheel:
                        // 已经捕获后持续保持，或者鼠标在窗口内则捕获
                        _isCapturingPointer = _isCapturingPointer || isInsideAnyWindow;
                        break;
                    case EventType.MouseUp:
                        _isCapturingPointer = false;
                        break;
                    case EventType.ContextClick:
                        _isCapturingPointer = _isCapturingPointer || isInsideAnyWindow;
                        break;
                }
            }

            /// <summary>
            ///     绘制单个窗体（由 GUI.Window 调用）。
            /// </summary>
            private static void DrawWindow(int id, WindowData window)
            {
                var titleBar = window.TitleBar;
                var hasTitleBar = !string.IsNullOrEmpty(titleBar.Title);

                if (hasTitleBar)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(titleBar.Title, DarkSkin.STitle);
                    FlexibleSpace();

                    if (GUILayout.Button("－", DarkSkin.SBtn, GUILayout.Width(50)))
                        SetScale(Scale - 0.1f);

                    if (GUILayout.Button("＋", DarkSkin.SBtn, GUILayout.Width(50)))
                        SetScale(Scale + 0.1f);

                    GUILayout.Space(10f);

                    // 语言切换按钮
                    {
                        var btnLabel = Loc.CurrentLanguage;

                        if (GUILayout.Button(btnLabel, DarkSkin.SBtn, GUILayout.Width(80)) && Loc.AvailableLanguages.Count > 1)
                            Loc.CycleLanguage();

                        GUILayout.Space(10f);
                    }

                    if (titleBar.ShowCloseButton)
                        if (GUILayout.Button(titleBar.CloseText, DarkSkin.SBtnDel, GUILayout.Width(50)))
                            window.Hide();

                    GUILayout.EndHorizontal();
                    DarkSkin.Divider(8f);
                }

                window.ContentScrollPos = GUILayout.BeginScrollView(window.ContentScrollPos);
                try
                {
                    window.Content?.Invoke();
                }
                catch (Exception e)
                {
                    Log.Error($"[LunHuiShop] 窗口 {window.TitleBar?.Title} 绘制异常: {e}");
                }
                finally
                {
                    GUILayout.EndScrollView();
                }

                window.Footer?.Invoke();
                
                DrawWindowTooltip();
            }

            /// <summary>
            ///     处理拖拽和调整大小输入。
            /// </summary>
            private static void HandleDragAndResize()
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
                        if (_draggingWindow != null)
                        {
                            if (_draggingWindow.Resizable)
                                BaseConfig.SaveWindowRect(_draggingWindow.Id, _draggingWindow.Rect);
                            else
                                BaseConfig.SaveWindowPosition(_draggingWindow.Id, _draggingWindow.Rect.position);
                        }
                        if (_resizingWindow != null)
                            BaseConfig.SaveWindowRect(_resizingWindow.Id, _resizingWindow.Rect);

                        _draggingWindow = null;
                        _resizingWindow = null;
                        break;
                }
            }

            /// <summary>
            ///     在窗口内绘制 GUI.tooltip，使用窗口本地坐标。
            /// </summary>
            private static void DrawWindowTooltip()
            {
                if (Event.current.type != EventType.Repaint)
                    return;

                var tooltip = GUI.tooltip;
                if (string.IsNullOrEmpty(tooltip))
                    return;

                var mouse = Event.current.mousePosition;
                var scale = Scale;
                var size = GUI.skin.box.CalcSize(new GUIContent(tooltip));

                GUI.Box(
                    new Rect(
                        mouse.x + 15f / scale,
                        mouse.y + 15f / scale,
                        size.x + 20,
                        size.y + 10
                    ),
                    tooltip
                );
            }
        }

    }

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
        public Action Content;

        // 底部栏（在滚动视图外，始终固定在窗口底部）
        public Action Footer;

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

        private bool _visible = true;
        public bool Visible
        {
            get => _visible;
            set
            {
                if (_visible == value) return;
                _visible = value;
                // 窗口从隐藏变为可见时，立即启用指针捕获。
                // 解决第一帧时 _isCapturingPointer / _lastGuiMousePos 尚未更新
                // 导致 ShouldBlockGamePointerInput() 返回 false 的穿透问题。
                if (value)
                    UI.WindowControls.NotifyWindowBecameVisible();
            }
        }

        public WindowData(
            int id,
            Rect rect,
            TitleBarConfig titleBar,
            Action content)
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
            UI.WindowControls.DestroyWindow(Id);
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