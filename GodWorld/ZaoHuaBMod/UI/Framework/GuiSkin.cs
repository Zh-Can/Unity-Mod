using System.Collections.Generic;
using UnityEngine;

namespace ZaoHuaBMod.UI.Framework
{
    public static class DarkSkin
    {
        private static GUISkin _skin;
        private static bool _initialized;
        private static readonly Dictionary<float, GUISkin> _skinCache = new Dictionary<float, GUISkin>();

        public static GUISkin Skin
        {
            get
            {
                if (!_initialized)
                {
                    _initialized = true;
                    _skin = Create();
                }

                return _skin;
            }
        }

        /// <summary>
        ///     获取指定缩放比例的 skin
        /// </summary>
        public static GUISkin GetSkin(float scale)
        {
            var key = Mathf.Round(scale * 10f) / 10f;
            if (!_skinCache.TryGetValue(key, out var skin))
            {
                skin = Create();
                if (Mathf.Abs(key - 1f) > 0.001f)
                    ScaleSkin(skin, key);
                _skinCache[key] = skin;
            }

            return skin;
        }

        // ==================== 快捷属性 ====================
        public static GUIStyle Window => Skin.window;
        public static GUIStyle Label => Skin.label;
        public static GUIStyle Button => Skin.button;
        public static GUIStyle TextField => Skin.textField;
        public static GUIStyle TextArea => Skin.textArea;
        public static GUIStyle Box => Skin.box;
        public static GUIStyle Toggle => Skin.toggle;
        public static GUIStyle ScrollView => Skin.scrollView;
        public static GUIStyle Header => Skin.GetStyle("Header");
        public static GUIStyle SubLabel => Skin.GetStyle("SubLabel");
        public static GUIStyle PrimaryBtn => Skin.GetStyle("PrimaryBtn");
        public static GUIStyle DangerBtn => Skin.GetStyle("DangerBtn");
        public static GUIStyle MiniButton => Skin.GetStyle("MiniButton");
        public static GUIStyle ToolbarBtn => Skin.GetStyle("ToolbarBtn");
        public static GUIStyle Toolbar => Skin.GetStyle("Toolbar");
        public static GUIStyle TooltipStyle => Skin.GetStyle("Tooltip");
        public static GUIStyle LinkLabel => Skin.GetStyle("LinkLabel");
        public static GUIStyle Separator => Skin.GetStyle("Separator");
        public static GUIStyle HelpBox => Skin.GetStyle("HelpBox");

        // ==================== 色板（适配 FeatureEditor 暗色风格） ====================
        public static class C
        {
            // 背景色
            public static readonly Color BgDarkest = new Color(0.06f, 0.063f, 0.078f);    // 输入框背景
            public static readonly Color BgDark = new Color(0.09f, 0.094f, 0.11f);         // 窗体背景
            public static readonly Color BgPanel = new Color(0.135f, 0.14f, 0.165f);       // 面板/Box 背景
            public static readonly Color BgMid = new Color(0.165f, 0.172f, 0.205f);       // 标签按钮
            public static readonly Color BgHighlight = new Color(0.225f, 0.235f, 0.275f); // 悬停标签按钮

            // 行背景
            public static readonly Color RowBg = new Color(0.118f, 0.122f, 0.145f);
            public static readonly Color RowBgAlt = new Color(0.15f, 0.155f, 0.182f);

            // 按钮
            public static readonly Color BtnNormal = new Color(0.205f, 0.215f, 0.255f);
            public static readonly Color BtnHover = new Color(0.27f, 0.29f, 0.34f);
            public static readonly Color BtnActive = new Color(0.15f, 0.16f, 0.19f);

            // 文本色
            public static readonly Color TextBright = new Color(0.88f, 0.89f, 0.92f);     // 主标题/高亮
            public static readonly Color TextNormal = new Color(0.74f, 0.76f, 0.82f);     // 普通文本
            public static readonly Color TextDim = new Color(0.52f, 0.54f, 0.6f);         // 辅助文本

            // 功能色
            public static readonly Color Accent = new Color(0.26f, 0.59f, 0.98f);
            public static readonly Color AccentDim = new Color(0.18f, 0.40f, 0.70f);
            public static readonly Color AccentBright = new Color(0.45f, 0.70f, 1.00f);
            public static readonly Color Success = new Color(0.18f, 0.36f, 0.27f);       // 添加按钮绿
            public static readonly Color Error = new Color(0.42f, 0.2f, 0.225f);         // 删除按钮红
            public static readonly Color ErrorHover = new Color(0.52f, 0.25f, 0.28f);

            // 边框
            public static readonly Color BorderDim = new Color(0.4f, 0.43f, 0.5f, 0.35f); // 通用边框
            public static readonly Color Border = new Color(0.25f, 0.25f, 0.27f);
            public static readonly Color BorderLight = new Color(0.35f, 0.35f, 0.37f);
            public static readonly Color WindowBorder = new Color(0.32f, 0.35f, 0.42f, 0.55f);

            // 滚动条/滑块
            public static readonly Color ScrollBg = new Color(0f, 0f, 0f, 0f);
            public static readonly Color ThumbNormal = new Color(0.42f, 0.45f, 0.53f, 0.85f); // 圆角 4
            public static readonly Color ThumbHover = new Color(0.55f, 0.58f, 0.66f, 0.95f);
            public static readonly Color SliderTrack = new Color(0.135f, 0.14f, 0.165f);
            public static readonly Color SliderFill = new Color(0.26f, 0.59f, 0.98f);

            // 其他
            public static readonly Color Selection = new Color(0.26f, 0.59f, 0.98f, 0.35f);
            public static readonly Color Cursor = new Color(0.88f, 0.89f, 0.92f);

            // 特殊文本色
            public static readonly Color CountColor = new Color(0.62f, 0.78f, 0.92f);
            public static readonly Color TagColor = new Color(0.55f, 0.72f, 0.92f);
            public static readonly Color StatusOk = new Color(0.5f, 0.84f, 0.62f);
            public static readonly Color StatusErr = new Color(0.92f, 0.48f, 0.5f);
        }

        // ==================== 辅助方法 ====================

        /// <summary>快速创建 GUIStyleState</summary>
        public static GUIStyleState MakeState(TexCache cache, Color bg, Color txt)
        {
            return new GUIStyleState
            {
                background = cache.Get(bg),
                textColor = txt
            };
        }

        /// <summary>快速创建 RectOffset</summary>
        public static RectOffset Pad(int l, int r, int t, int b)
        {
            return new RectOffset(l, r, t, b);
        }

        /// <summary>将自定义样式数组赋值给 skin</summary>
        public static void SetCustomStyles(GUISkin skin, params GUIStyle[] styles)
        {
            skin.customStyles = styles;
        }

        /// <summary>绘制 Tooltip 气泡，OnGUI 末尾调用</summary>
        public static void DrawTooltip()
        {
            if (string.IsNullOrEmpty(GUI.tooltip)) return;

            var style = GUI.skin.GetStyle("Tooltip");
            var content = new GUIContent(GUI.tooltip);
            var maxW = 300f;
            var w = style.CalcSize(content).x;
            var h = style.CalcHeight(content, Mathf.Min(w, maxW));

            if (w > maxW) w = maxW;

            var x = Event.current.mousePosition.x + 12f;
            var y = Event.current.mousePosition.y + 20f;

            if (x + w + 16f > Screen.width)
                x = Event.current.mousePosition.x - w - 8f;
            if (y + h + 8f > Screen.height)
                y = Event.current.mousePosition.y - h - 8f;

            GUI.Label(new Rect(x, y, w + 16f, h + 8f), GUI.tooltip, style);
        }

        // ==================== 构建 ====================

        private static GUISkin Create()
        {
            var skin = ScriptableObject.CreateInstance<GUISkin>();
            var cache = new TexCache();

            BuildWindow(skin, cache);
            BuildLabel(skin, cache);
            BuildButton(skin, cache);
            BuildInputField(skin, cache, false);
            BuildInputField(skin, cache, true);
            BuildBox(skin, cache);
            BuildToggle(skin, cache);
            BuildScrollView(skin, cache);
            BuildScrollbar(skin, cache);
            BuildSlider(skin, cache);
            BuildCustomStyles(skin, cache);

            // 全局设置
            skin.settings.selectionColor = C.Selection;
            skin.settings.cursorColor = C.Cursor;
            skin.settings.cursorFlashSpeed = 2.0f;
            skin.settings.doubleClickSelectsWord = true;
            skin.settings.tripleClickSelectsLine = true;

            return skin;
        }

        /// <summary>
        ///     按比例缩放 skin 中所有样式的尺寸
        /// </summary>
        private static void ScaleSkin(GUISkin skin, float scale)
        {
            ScaleStyle(skin.window, scale);
            ScaleStyle(skin.label, scale);
            ScaleStyle(skin.button, scale);
            ScaleStyle(skin.textField, scale);
            ScaleStyle(skin.textArea, scale);
            ScaleStyle(skin.box, scale);
            ScaleStyle(skin.toggle, scale);
            ScaleStyle(skin.scrollView, scale);
            ScaleStyle(skin.verticalScrollbar, scale);
            ScaleStyle(skin.verticalScrollbarThumb, scale);
            ScaleStyle(skin.verticalScrollbarUpButton, scale);
            ScaleStyle(skin.verticalScrollbarDownButton, scale);
            ScaleStyle(skin.horizontalScrollbar, scale);
            ScaleStyle(skin.horizontalScrollbarThumb, scale);
            ScaleStyle(skin.horizontalScrollbarLeftButton, scale);
            ScaleStyle(skin.horizontalScrollbarRightButton, scale);
            ScaleStyle(skin.horizontalSlider, scale);
            ScaleStyle(skin.horizontalSliderThumb, scale);
            ScaleStyle(skin.verticalSlider, scale);
            ScaleStyle(skin.verticalSliderThumb, scale);

            if (skin.customStyles != null)
                foreach (var style in skin.customStyles)
                    ScaleStyle(style, scale);
        }

        private static void ScaleStyle(GUIStyle style, float scale)
        {
            if (style == null) return;

            if (style.fontSize > 0)
                style.fontSize = Mathf.RoundToInt(style.fontSize * scale);

            if (style.fixedHeight > 0)
                style.fixedHeight *= scale;

            if (style.fixedWidth > 0)
                style.fixedWidth *= scale;

            style.padding = ScaleRectOffset(style.padding, scale);
            style.margin = ScaleRectOffset(style.margin, scale);
            style.border = ScaleRectOffset(style.border, scale);
            style.overflow = ScaleRectOffset(style.overflow, scale);
        }

        private static RectOffset ScaleRectOffset(RectOffset offset, float scale)
        {
            if (offset == null) return new RectOffset(0, 0, 0, 0);
            return new RectOffset(
                Mathf.RoundToInt(offset.left * scale),
                Mathf.RoundToInt(offset.right * scale),
                Mathf.RoundToInt(offset.top * scale),
                Mathf.RoundToInt(offset.bottom * scale));
        }

        private static void BuildWindow(GUISkin skin, TexCache cache)
        {
            skin.window = new GUIStyle("window")
            {
                normal = MakeState(cache, C.BgPanel, C.TextNormal),
                padding = Pad(12, 12, 24, 12),
                margin = Pad(6, 6, 6, 6),
                alignment = TextAnchor.UpperLeft,
                fontSize = 13,
                fontStyle = FontStyle.Bold
            };
        }

        private static void BuildLabel(GUISkin skin, TexCache cache)
        {
            skin.label = new GUIStyle("label")
            {
                normal = MakeState(cache, Color.clear, C.TextNormal),
                hover = MakeState(cache, Color.clear, C.TextBright),
                fontSize = 12,
                padding = Pad(4, 4, 3, 3),
                wordWrap = true,
                richText = true
            };
        }

        private static GUIStyle MakeButtonStyle(TexCache cache)
        {
            return new GUIStyle("button")
            {
                normal = MakeState(cache, C.BtnNormal, C.TextNormal),
                hover = MakeState(cache, C.BtnHover, C.TextBright),
                active = MakeState(cache, C.BtnActive, C.TextNormal),
                focused = MakeState(cache, C.BtnHover, C.TextBright),
                onNormal = MakeState(cache, C.AccentDim, Color.white),
                onHover = MakeState(cache, C.Accent, Color.white),
                onActive = MakeState(cache, C.AccentDim, Color.white),
                onFocused = MakeState(cache, C.Accent, Color.white),
                fontSize = 12,
                padding = Pad(10, 10, 6, 6),
                margin = Pad(4, 4, 4, 4),
                alignment = TextAnchor.MiddleCenter,
                imagePosition = ImagePosition.ImageLeft,
                wordWrap = true
            };
        }

        private static void BuildButton(GUISkin skin, TexCache cache)
        {
            skin.button = MakeButtonStyle(cache);
        }

        private static void BuildInputField(GUISkin skin, TexCache cache, bool isTextArea)
        {
            var style = new GUIStyle(isTextArea ? "textArea" : "textField")
            {
                normal = MakeState(cache, C.BgDark, C.TextNormal),
                hover = MakeState(cache, C.BgMid, C.TextNormal),
                focused = MakeState(cache, C.BgMid, C.TextBright),
                active = MakeState(cache, C.BgMid, C.TextNormal),
                fontSize = 12,
                padding = Pad(8, 8, 5, 5),
                border = Pad(3, 3, 3, 3),
                margin = Pad(4, 4, 4, 4),
                alignment = TextAnchor.UpperLeft,
                wordWrap = isTextArea
            };

            if (isTextArea)
                skin.textArea = style;
            else
                skin.textField = style;
        }

        private static void BuildBox(GUISkin skin, TexCache cache)
        {
            skin.box = new GUIStyle("box")
            {
                normal = MakeState(cache, C.BgPanel, C.TextNormal),
                hover = MakeState(cache, C.BgHighlight, C.TextNormal),
                fontSize = 11,
                padding = Pad(10, 10, 8, 8),
                margin = Pad(6, 6, 6, 6)
            };
        }

        private static void BuildToggle(GUISkin skin, TexCache cache)
        {
            const int texSize = 32;
            const int border = 3;

            var uncheckedTex = TexGen.BorderedBox(texSize, C.BgDark, C.Border);
            var checkedTex = TexGen.BorderedBox(texSize, C.Accent, C.AccentBright);

            skin.toggle = new GUIStyle("toggle")
            {
                fixedHeight = 19,
                fixedWidth = 16,
                normal = new GUIStyleState { background = uncheckedTex, textColor = C.TextNormal },
                hover = new GUIStyleState { background = uncheckedTex, textColor = C.TextBright },
                active = new GUIStyleState { background = uncheckedTex, textColor = C.TextNormal },
                onNormal = new GUIStyleState { background = checkedTex, textColor = C.TextNormal },
                onHover = new GUIStyleState { background = checkedTex, textColor = C.TextBright },
                onActive = new GUIStyleState { background = checkedTex, textColor = C.TextNormal },
                fontSize = 12,
                border = Pad(border, border, border, border),
                padding = Pad(22, 4, 3, 3),
                margin = Pad(4, 4, 4, 4),
                alignment = TextAnchor.MiddleLeft
            };
        }

        private static void BuildScrollView(GUISkin skin, TexCache cache)
        {
            skin.scrollView = new GUIStyle("scrollView")
            {
                normal = MakeState(cache, Color.clear, C.TextNormal),
                padding = Pad(2, 2, 2, 2)
            };
        }

        private static void BuildScrollbar(GUISkin skin, TexCache cache)
        {
            skin.verticalScrollbar = new GUIStyle("verticalScrollbar")
            {
                normal = MakeState(cache, Color.clear, Color.clear),
                fixedWidth = 12,
                stretchHeight = true,
                margin = Pad(2, 0, 2, 2)
            };
            skin.verticalScrollbarThumb = new GUIStyle("verticalScrollbarThumb")
            {
                normal = MakeState(cache, C.ThumbNormal, Color.clear),
                hover = MakeState(cache, C.ThumbHover, Color.clear),
                active = MakeState(cache, C.ThumbHover, Color.clear),
                fixedWidth = 12,
                stretchHeight = true
            };
            skin.horizontalScrollbar = new GUIStyle("horizontalScrollbar")
            {
                normal = MakeState(cache, Color.clear, Color.clear),
                fixedHeight = 12,
                stretchWidth = true,
                margin = Pad(2, 2, 0, 2)
            };
            skin.horizontalScrollbarThumb = new GUIStyle("horizontalScrollbarThumb")
            {
                normal = MakeState(cache, C.ThumbNormal, Color.clear),
                hover = MakeState(cache, C.ThumbHover, Color.clear),
                active = MakeState(cache, C.ThumbHover, Color.clear),
                fixedHeight = 12,
                stretchWidth = true
            };

            // 隐藏箭头按钮
            skin.verticalScrollbarUpButton = new GUIStyle { fixedHeight = 0 };
            skin.verticalScrollbarDownButton = skin.verticalScrollbarUpButton;
            skin.horizontalScrollbarLeftButton = new GUIStyle { fixedWidth = 0 };
            skin.horizontalScrollbarRightButton = skin.horizontalScrollbarLeftButton;
        }

        private static void BuildSlider(GUISkin skin, TexCache cache)
        {
            var trackTex = TexGen.BorderedRect(12, 10, C.SliderTrack, C.Border);
            var thumbTex = TexGen.BorderedRect(16, 22, C.SliderFill, C.AccentBright);
            var thumbHoverTex = TexGen.BorderedRect(16, 22, C.AccentBright, C.AccentBright);
            var thumbActiveTex = TexGen.BorderedRect(16, 22, C.AccentDim, C.AccentBright);

            skin.horizontalSlider = new GUIStyle("horizontalSlider")
            {
                normal = new GUIStyleState { background = trackTex, textColor = Color.clear },
                fixedHeight = 10,
                margin = Pad(8, 8, 12, 12)
            };
            skin.horizontalSliderThumb = new GUIStyle("horizontalSliderThumb")
            {
                normal = new GUIStyleState { background = thumbTex, textColor = Color.clear },
                hover = new GUIStyleState { background = thumbHoverTex, textColor = Color.clear },
                active = new GUIStyleState { background = thumbActiveTex, textColor = Color.clear },
                fixedWidth = 16,
                fixedHeight = 22,
                overflow = new RectOffset(0, 0, -6, -6),
                margin = Pad(0, 0, 0, 0),
                alignment = TextAnchor.MiddleCenter
            };

            skin.verticalSlider = new GUIStyle("verticalSlider")
            {
                normal = new GUIStyleState { background = trackTex, textColor = Color.clear },
                fixedWidth = 10,
                stretchHeight = true,
                margin = Pad(12, 12, 8, 8)
            };
            skin.verticalSliderThumb = new GUIStyle("verticalSliderThumb")
            {
                normal = new GUIStyleState { background = thumbTex, textColor = Color.clear },
                hover = new GUIStyleState { background = thumbHoverTex, textColor = Color.clear },
                active = new GUIStyleState { background = thumbActiveTex, textColor = Color.clear },
                fixedWidth = 22,
                fixedHeight = 16,
                overflow = new RectOffset(-6, -6, 0, 0),
                margin = Pad(0, 0, 0, 0)
            };
        }

        private static void BuildCustomStyles(GUISkin skin, TexCache cache)
        {
            var header = new GUIStyle(skin.label)
            {
                name = "Header",
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = MakeState(cache, Color.clear, C.TextBright),
                padding = Pad(4, 4, 6, 6)
            };

            var sub = new GUIStyle(skin.label)
            {
                name = "SubLabel",
                fontSize = 10,
                normal = MakeState(cache, Color.clear, C.TextDim),
                padding = Pad(2, 2, 1, 1)
            };

            var primary = MakeButtonStyle(cache);
            primary.name = "PrimaryBtn";
            primary.normal = MakeState(cache, C.Accent, Color.white);
            primary.hover = MakeState(cache, C.AccentBright, Color.white);
            primary.active = MakeState(cache, C.AccentDim, Color.white);
            primary.fontSize = 13;
            primary.fontStyle = FontStyle.Bold;
            primary.padding = Pad(14, 14, 8, 8);

            var danger = MakeButtonStyle(cache);
            danger.name = "DangerBtn";
            danger.normal = MakeState(cache, C.Error, Color.white);
            danger.hover = MakeState(cache, C.ErrorHover, Color.white);
            danger.active = MakeState(cache, new Color(0.65f, 0.15f, 0.15f), Color.white);

            var mini = MakeButtonStyle(cache);
            mini.name = "MiniButton";
            mini.fontSize = 10;
            mini.padding = Pad(6, 6, 3, 3);
            mini.margin = Pad(2, 2, 2, 2);

            var toolbar = new GUIStyle(skin.box)
            {
                name = "Toolbar",
                normal = MakeState(cache, C.BgDarkest, C.TextNormal),
                padding = Pad(8, 8, 4, 4),
                margin = Pad(0, 0, 0, 0),
                stretchWidth = true,
                fixedHeight = 30
            };

            var tbBtn = MakeButtonStyle(cache);
            tbBtn.name = "ToolbarBtn";
            tbBtn.normal = MakeState(cache, Color.clear, C.TextNormal);
            tbBtn.hover = MakeState(cache, C.BgHighlight, C.TextBright);
            tbBtn.active = MakeState(cache, C.BgMid, C.TextNormal);
            tbBtn.padding = Pad(8, 8, 4, 4);
            tbBtn.margin = Pad(2, 2, 0, 0);
            tbBtn.fixedHeight = 26;

            var link = new GUIStyle(skin.label)
            {
                name = "LinkLabel",
                normal = MakeState(cache, Color.clear, C.Accent),
                hover = MakeState(cache, Color.clear, C.AccentBright),
                fontSize = 12
            };

            var sep = new GUIStyle(skin.box)
            {
                name = "Separator",
                normal = MakeState(cache, C.Border, Color.clear),
                stretchWidth = true,
                fixedHeight = 1,
                padding = Pad(0, 0, 0, 0),
                margin = Pad(8, 8, 4, 4)
            };

            var tooltipBg = TexGen.BorderedRect(200, 40,
                new Color(0.12f, 0.12f, 0.14f, 0.92f), C.Border);
            var tooltip = new GUIStyle(skin.label)
            {
                name = "Tooltip",
                normal = new GUIStyleState { background = tooltipBg, textColor = C.TextBright },
                fontSize = 11,
                padding = Pad(8, 8, 5, 5),
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                richText = true
            };

            var help = new GUIStyle(skin.box)
            {
                name = "HelpBox",
                normal = MakeState(cache, new Color(0.18f, 0.22f, 0.12f), C.Success),
                padding = Pad(10, 10, 8, 8),
                fontSize = 11,
                wordWrap = true,
                richText = true
            };

            SetCustomStyles(skin, header, sub, primary, danger, mini,
                toolbar, tbBtn, link, sep, tooltip, help);
        }
    }

    // ==================== 纹理缓存 ====================

    public sealed class TexCache
    {
        private readonly Dictionary<string, Texture2D> _cache = new Dictionary<string, Texture2D>();

        public Texture2D Get(Color color)
        {
            var key = $"{color.r:F3}_{color.g:F3}_{color.b:F3}_{color.a:F3}";
            if (!_cache.TryGetValue(key, out var tex))
            {
                tex = TexGen.Solid(1, 1, color);
                _cache[key] = tex;
            }

            return tex;
        }

        public Texture2D GetRounded(Color fill, int radius, Color? border = null)
        {
            var key = $"round_{fill.r:F3}_{fill.g:F3}_{fill.b:F3}_{fill.a:F3}_{radius}";
            if (border != null)
            {
                var b = border.Value;
                key += $"_{b.r:F3}_{b.g:F3}_{b.b:F3}_{b.a:F3}";
            }

            if (!_cache.TryGetValue(key, out var tex))
            {
                tex = TexGen.RoundTex(fill, radius, border);
                _cache[key] = tex;
            }

            return tex;
        }
    }

    // ==================== 纹理生成 ====================

    public static class TexGen
    {
        public static Texture2D Solid(int w, int h, Color color)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.hideFlags = HideFlags.HideAndDontSave;
            var pixels = new Color[w * h];
            for (var i = 0; i < pixels.Length; i++)
                pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply(false, false);
            return tex;
        }

        public static Texture2D BorderedBox(int size, Color bg, Color border)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.hideFlags = HideFlags.HideAndDontSave;
            var pixels = new Color[size * size];
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var edge = x == 0 || x == size - 1 || y == 0 || y == size - 1;
                pixels[y * size + x] = edge ? border : bg;
            }

            tex.SetPixels(pixels);
            tex.Apply(false, false);
            return tex;
        }

        public static Texture2D BorderedRect(int w, int h, Color bg, Color border)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.hideFlags = HideFlags.HideAndDontSave;
            var pixels = new Color[w * h];
            for (var y = 0; y < h; y++)
            for (var x = 0; x < w; x++)
            {
                var edge = x == 0 || x == w - 1 || y == 0 || y == h - 1;
                pixels[y * w + x] = edge ? border : bg;
            }

            tex.SetPixels(pixels);
            tex.Apply(false, false);
            return tex;
        }

        /// <summary>
        ///     生成圆角矩形纹理（方形），适合 GUIStyle 9-slice 缩放。
        ///     纹理尺寸 = radius * 2 + 2，border 设为 radius 即可保持圆角。
        /// </summary>
        public static Texture2D RoundTex(Color fill, int radius, Color? border = null, float borderW = 1f)
        {
            var size = radius * 2 + 2;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.hideFlags = HideFlags.HideAndDontSave;
            var pixels = new Color[size * size];
            var borderColor = border.GetValueOrDefault(fill);

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    float px = x + 0.5f;
                    float py = y + 0.5f;

                    // 到最近边角的距离
                    float cx = Mathf.Clamp(px, radius, size - radius);
                    float cy = Mathf.Clamp(py, radius, size - radius);
                    float dist = Mathf.Sqrt((px - cx) * (px - cx) + (py - cy) * (py - cy));

                    // 圆角抗锯齿
                    float alpha = Mathf.Clamp01(radius - dist + 0.5f);
                    var color = fill;

                    if (border != null)
                    {
                        float borderAlpha = Mathf.Clamp01(dist - (radius - borderW) + 0.5f);
                        color = Color.Lerp(fill, borderColor, Mathf.Clamp01(borderAlpha));
                    }

                    pixels[y * size + x] = new Color(color.r, color.g, color.b, color.a * alpha);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(false, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            return tex;
        }
    }
}