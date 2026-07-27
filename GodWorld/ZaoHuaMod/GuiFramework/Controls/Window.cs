using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZaoHuaMod.GuiFramework.Config;
using ZaoHuaMod.GuiFramework.Localization;
using ZaoHuaMod.GuiFramework.Logger;
using ZaoHuaMod.GuiFramework.Style;

namespace ZaoHuaMod.GuiFramework.Controls
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
                Action<WindowData> content)
            {
                if (_windows.ContainsKey(id))
                    throw new InvalidOperationException($"窗口 Id {id} 已存在，请为每个窗口指定唯一 Id。");

                var window = new WindowData(id, rect, titleBar, content);

                // 读取并应用上次保存的位置/尺寸
                var savedRect = BaseConfig.LoadWindowRect(id);
                if (savedRect.HasValue)
                    window.Rect = savedRect.Value;

                // 窗口关闭时保存位置/尺寸
                window.OnClose += () => BaseConfig.SaveWindowRect(id, window.Rect);

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
                    
                    var sortedWindows = _windows.Values
                        .Where(w => w.Visible)
                        .OrderBy(w => w.Layer)
                        .ToList();
                    
                    foreach (var window in sortedWindows)
                    {
                        GUI.matrix = matrixOld;
                        GUIUtility.ScaleAroundPivot(new Vector2(Scale, Scale), new Vector2(window.Rect.x, window.Rect.y));
                        GUI.Window(window.Id, window.Rect, id => DrawWindow(id, window), GUIContent.none, DarkSkin.SWindow);
                    }
                    
                    GUI.matrix = matrixOld;
                    HandleDragAndResize();

                    GUI.skin = previousSkin;
                
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
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("－", DarkSkin.SBtn))
                        SetScale(Scale - 0.1f);

                    if (GUILayout.Button("＋", DarkSkin.SBtn))
                        SetScale(Scale + 0.1f);

                    GUILayout.Space(10f);

                    // 语言切换按钮
                    {
                        var btnLabel = Loc.CurrentLanguage;

                        if (GUILayout.Button(btnLabel, DarkSkin.SBtn) && Loc.AvailableLanguages.Count > 1)
                            Loc.CycleLanguage();

                        GUILayout.Space(10f);
                    }

                    if (titleBar.ShowCloseButton)
                        if (GUILayout.Button(titleBar.CloseText, DarkSkin.SBtnDel))
                            window.Hide();

                    GUILayout.EndHorizontal();
                    DarkSkin.Divider(8f);
                }

                window.ContentScrollPos = GUILayout.BeginScrollView(window.ContentScrollPos);
                try
                {
                    window.Content?.Invoke(window);
                }
                catch (Exception e)
                {
                    Log.Error($"[ZaoHuaBMod] 窗口 {window.TitleBar?.Title} 绘制异常: {e}");
                }
                finally
                {
                    GUILayout.EndScrollView();
                }
                
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
                            BaseConfig.SaveWindowRect(_draggingWindow.Id, _draggingWindow.Rect);
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
                global::ZaoHuaMod.GuiFramework.Controls.UI.WindowControls.DestroyWindow(Id);
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
}