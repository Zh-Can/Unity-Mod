using System;
using UnityEngine;
using ZaoHuaBMod.GuiFramework.Style;

namespace ZaoHuaBMod.GuiFramework.Controls
{
    /// <summary>
    ///     UI 布局分部：水平/垂直容器、间隔、分隔线。
    /// </summary>
    public static partial class UI
    {
        public static class LayoutControls
        {
            /// <summary>
            ///     水平布局容器（回调式）。
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
        }
    }
}