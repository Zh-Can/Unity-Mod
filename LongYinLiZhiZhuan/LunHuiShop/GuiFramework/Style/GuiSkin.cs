using System;
using UnityEngine;
using LunHuiShop.GuiFramework.Logger;

namespace LunHuiShop.GuiFramework.Style
{
    /// <summary>
    ///     复刻 FeatureEditor.Frontend 的暗色 IMGUI 皮肤。
    ///     样式为静态对象，缩放由 GUIManager 通过 GUI.matrix 统一处理。
    /// </summary>
    public static class DarkSkin
    {
        // FeatureEditor.Frontend 原始色板
        public static readonly Color TextBright = new Color(0.88f, 0.89f, 0.92f);
        public static readonly Color TextNormal = new Color(0.74f, 0.76f, 0.82f);
        public static readonly Color TextDim = new Color(0.52f, 0.54f, 0.6f);
        public static readonly Color WindowBg = new Color(0.09f, 0.094f, 0.11f, 0.985f);
        public static readonly Color PanelBg = new Color(0.135f, 0.14f, 0.165f);
        public static readonly Color InputBg = new Color(0.06f, 0.063f, 0.078f);
        public static readonly Color RowBg = new Color(0.118f, 0.122f, 0.145f);
        public static readonly Color RowBgAlt = new Color(0.15f, 0.155f, 0.182f);
        public static readonly Color BtnNormal = new Color(0.205f, 0.215f, 0.255f);
        public static readonly Color BtnHover = new Color(0.27f, 0.29f, 0.34f);
        public static readonly Color BtnActive = new Color(0.15f, 0.16f, 0.19f);
        public static readonly Color BtnAddNormal = new Color(0.18f, 0.36f, 0.27f);
        public static readonly Color BtnAddHover = new Color(0.22f, 0.45f, 0.33f);
        public static readonly Color BtnAddActive = new Color(0.13f, 0.27f, 0.2f);
        public static readonly Color BtnDelNormal = new Color(0.55f, 0.22f, 0.22f);
        public static readonly Color BtnDelHover = new Color(0.75f, 0.30f, 0.30f);
        public static readonly Color BtnDelActive = new Color(0.45f, 0.18f, 0.18f);
        public static readonly Color BorderDim = new Color(0.4f, 0.43f, 0.5f, 0.35f);
        public static readonly Color WindowBorder = new Color(0.32f, 0.35f, 0.42f, 0.55f);
        public static readonly Color Accent = new Color(0.26f, 0.59f, 0.98f);
        public static readonly Color AccentBright = new Color(0.45f, 0.70f, 1.00f);
        public static readonly Color AccentDim = new Color(0.18f, 0.40f, 0.70f);
        public static readonly Color ChipBg = new Color(0.165f, 0.172f, 0.205f);
        public static readonly Color ChipHover = new Color(0.225f, 0.235f, 0.275f);
        public static readonly Color ChipActive = new Color(0.135f, 0.14f, 0.17f);
        public static readonly Color ChipOnBg = new Color(0.27f, 0.4f, 0.62f);
        public static readonly Color ChipOnHover = new Color(0.31f, 0.45f, 0.68f);
        public static readonly Color ChipOnActive = new Color(0.23f, 0.35f, 0.56f);
        public static readonly Color DetailBg = new Color(0.075f, 0.078f, 0.095f);
        public static readonly Color ScrollThumb = new Color(0.42f, 0.45f, 0.53f, 0.85f);
        public static readonly Color ScrollThumbHover = new Color(0.55f, 0.58f, 0.66f, 0.95f);
        public static readonly Color StatusOk = new Color(0.5f, 0.84f, 0.62f);
        public static readonly Color StatusErr = new Color(0.92f, 0.48f, 0.5f);
        public static readonly Color TagColor = new Color(0.55f, 0.72f, 0.92f);
        public static readonly Color TagHiddenColor = new Color(0.84f, 0.68f, 0.4f);
        public static readonly Color CountColor = new Color(0.62f, 0.78f, 0.92f);
        public static readonly Color TypeGood = new Color(0.5f, 0.84f, 0.62f);
        public static readonly Color TypeBad = new Color(0.92f, 0.5f, 0.52f);
        public static readonly Color TypeSpecial = new Color(0.66f, 0.7f, 0.95f);
        public static readonly Color TypeTemp = new Color(0.8f, 0.74f, 0.5f);

        private static bool _initialized;
        private static Texture2D _lineTex;
        private static GUISkin _skin;

        // 自定义样式引用（与 FeatureEditor.Frontend 对应）
        public static GUIStyle SWindow { get; private set; }
        public static GUIStyle STitle { get; private set; }
        public static GUIStyle SHint { get; private set; }
        public static GUIStyle SLabel { get; private set; }
        public static GUIStyle SMuted { get; private set; }
        public static GUIStyle SCount { get; private set; }
        public static GUIStyle SField { get; private set; }
        public static GUIStyle SFieldPlaceholder { get; private set; }
        public static GUIStyle SPanel { get; private set; }
        public static GUIStyle SBtn { get; private set; }
        public static GUIStyle SBtnAdd { get; private set; }
        public static GUIStyle SBtnDel { get; private set; }
        public static GUIStyle SRow { get; private set; }
        public static GUIStyle SRowAlt { get; private set; }
        public static GUIStyle SRowSelected { get; private set; }
        public static GUIStyle SFeatureName { get; private set; }
        public static GUIStyle STag { get; private set; }
        public static GUIStyle STagHidden { get; private set; }
        public static GUIStyle SStatusOk { get; private set; }
        public static GUIStyle SStatusErr { get; private set; }
        public static GUIStyle SNameBtn { get; private set; }
        public static GUIStyle SDetail { get; private set; }
        public static GUIStyle SDetailHead { get; private set; }
        public static GUIStyle SBonus { get; private set; }
        public static GUIStyle SChip { get; private set; }
        public static GUIStyle SChipOn { get; private set; }
        public static GUIStyle SScroll { get; private set; }
        public static GUIStyle SScrollThumb { get; private set; }
        public static GUIStyle STypeGood { get; private set; }
        public static GUIStyle STypeBad { get; private set; }
        public static GUIStyle STypeSpecial { get; private set; }
        public static GUIStyle STypeTemp { get; private set; }
        public static GUIStyle SToggle { get; private set; }
        public static GUIStyle SPopup { get; private set; }
        public static GUIStyle SRadio { get; private set; }
        public static GUIStyle SRadioOn { get; private set; }
        public static GUIStyle SFoldout { get; private set; }

        // 兼容旧 GUIManager 的快捷属性
        public static GUISkin Skin
        {
            get
            {
                if (!_initialized) InitStyles();
                return _skin;
            }
        }
        
        public static GUIStyle Window => SWindow;
        public static GUIStyle Label => SLabel;
        public static GUIStyle Button => SBtn;
        public static GUIStyle TextField => SField;
        public static GUIStyle TextArea => SField;
        public static GUIStyle Box => SPanel;
        public static GUIStyle Toggle => SToggle;

        /// <summary>
        ///     初始化所有样式。首次访问 Skin 或任何样式时自动调用。
        /// </summary>
        public static void InitStyles()
        {
            if (_initialized) return;
            if (Event.current == null && GUI.skin == null) return;

            try
            {

                
                _lineTex = Tex(BorderDim.r, BorderDim.g, BorderDim.b, BorderDim.a);

                SWindow = new GUIStyle(GUI.skin.window);
                SkinR(SWindow, WindowBg, TextBright, 9, null, null, WindowBorder);
                SWindow.padding = new RectOffset(16, 16, 14, 14);

                STitle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 18,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = TextBright }
                };

                SHint = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    alignment = TextAnchor.MiddleRight,
                    normal = { textColor = TextDim }
                };

                SLabel = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    normal = { textColor = TextNormal },
                    hover = { textColor = TextBright },
                    padding = new RectOffset(4, 4, 3, 3),
                    wordWrap = true,
                    richText = true
                };

                SMuted = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    normal = { textColor = TextDim }
                };

                SCount = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = CountColor }
                };

                SField = new GUIStyle(GUI.skin.textField)
                {
                    fontSize = 13,
                    padding = new RectOffset(10, 10, 8, 8),
                    margin = new RectOffset(2, 2, 2, 2),
                    alignment = TextAnchor.UpperLeft,
                    wordWrap = false
                };
                SkinR(SField, InputBg, TextBright, 5, null, null, BorderDim);

                SFieldPlaceholder = new GUIStyle(SField)
                {
                    normal = { textColor = TextDim }
                };

                SPanel = new GUIStyle(GUI.skin.box);
                SkinR(SPanel, PanelBg, TextNormal, 7, null, null, BorderDim);
                SPanel.padding = new RectOffset(12, 12, 10, 10);
                SPanel.fontSize = 13;
                SPanel.alignment = TextAnchor.UpperLeft;
                SPanel.wordWrap = true;

                SBtn = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 13,
                    fixedHeight = 30f,
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = false,
                    padding = new RectOffset(14, 14, 4, 4),
                    margin = new RectOffset(3, 3, 2, 2)
                };
                SkinR(SBtn, BtnNormal, TextBright, 6, BtnHover, BtnActive);

                SBtnAdd = new GUIStyle(SBtn);
                SkinR(SBtnAdd, BtnAddNormal, new Color(0.9f, 0.97f, 0.92f), 6, BtnAddHover, BtnAddActive);

                SBtnDel = new GUIStyle(SBtn);
                SkinR(SBtnDel, BtnDelNormal, TextBright, 6, BtnDelHover, BtnDelActive);

                SRow = new GUIStyle
                {
                    fixedHeight = 30f,
                    padding = new RectOffset(10, 10, 5, 5),
                    margin = new RectOffset(0, 0, 1, 1),
                    normal = { background = Tex(RowBg.r, RowBg.g, RowBg.b) }
                };

                SRowAlt = new GUIStyle(SRow)
                {
                    normal = { background = Tex(RowBgAlt.r, RowBgAlt.g, RowBgAlt.b) }
                };

                SRowSelected = new GUIStyle(SRow)
                {
                    normal = { background = Tex(ChipOnBg.r, ChipOnBg.g, ChipOnBg.b) }
                };

                SFeatureName = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    normal = { textColor = TextBright }
                };

                STag = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11,
                    richText = true,
                    normal = { textColor = TagColor }
                };

                STagHidden = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11,
                    normal = { textColor = TagHiddenColor }
                };

                SStatusOk = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    normal = { textColor = StatusOk }
                };

                SStatusErr = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    normal = { textColor = StatusErr }
                };

                SNameBtn = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    alignment = TextAnchor.MiddleLeft,
                    richText = true,
                    normal = { textColor = TextBright },
                    hover = { textColor = new Color(0.66f, 0.82f, 0.98f) }
                };

                SDetail = new GUIStyle(GUI.skin.box);
                SkinR(SDetail, DetailBg, TextNormal, 7, null, null, BorderDim);
                SDetail.padding = new RectOffset(14, 14, 10, 10);
                SDetail.fontSize = 12;
                SDetail.alignment = TextAnchor.UpperLeft;
                SDetail.wordWrap = true;
                SDetail.richText = true;

                SDetailHead = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = new Color(0.78f, 0.8f, 0.86f) }
                };

                SBonus = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    richText = true,
                    normal = { textColor = TextNormal }
                };

                SChip = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 12,
                    fixedHeight = 30f,
                    padding = new RectOffset(14, 14, 5, 5),
                    margin = new RectOffset(3, 3, 2, 2)
                };
                SkinR(SChip, ChipBg, TextDim, 8, ChipHover, ChipActive);

                SChipOn = new GUIStyle(SChip);
                SkinR(SChipOn, ChipOnBg, Color.white, 8, ChipOnHover, ChipOnActive);
                SChipOn.fontStyle = FontStyle.Bold;

                SScroll = new GUIStyle
                {
                    fixedWidth = 8f,
                    border = new RectOffset(0, 0, 0, 0),
                    normal = { background = Tex(0f, 0f, 0f, 0f) }
                };

                SScrollThumb = new GUIStyle
                {
                    border = new RectOffset(4, 4, 4, 4),
                    fixedWidth = 8f
                };
                var thumbTex = RoundTex(ScrollThumb, 4);
                SScrollThumb.normal.background = thumbTex;
                SScrollThumb.hover.background = thumbTex;
                SScrollThumb.active.background = thumbTex;

                STypeGood = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = TypeGood }
                };
                STypeBad = new GUIStyle(STypeGood) { normal = { textColor = TypeBad } };
                STypeSpecial = new GUIStyle(STypeGood) { normal = { textColor = TypeSpecial } };
                STypeTemp = new GUIStyle(STypeGood) { normal = { textColor = TypeTemp } };

                // Toggle：窗口暗色底 + 灰色细边框，选中后与滑块 thumb 同色
                var toggleUnchecked = SquareTex(16, WindowBg, new Color(0.35f, 0.37f, 0.42f));
                var toggleChecked = SquareTex(16, Accent, AccentBright);
                SToggle = new GUIStyle
                {
                    fontSize = 12,
                    fixedWidth = 16,
                    fixedHeight = 16,
                    normal = { background = toggleUnchecked, textColor = TextNormal },
                    hover = { background = toggleUnchecked, textColor = TextBright },
                    active = { background = toggleUnchecked, textColor = TextNormal },
                    onNormal = { background = toggleChecked, textColor = TextNormal },
                    onHover = { background = toggleChecked, textColor = TextBright },
                    onActive = { background = toggleChecked, textColor = TextNormal },
                    padding = new RectOffset(22, 4, 3, 3),
                    margin = new RectOffset(4, 4, 4, 4),
                    overflow = new RectOffset(1, 1, 1, 1),
                    border = new RectOffset(0, 0, 0, 0),
                    alignment = TextAnchor.MiddleLeft
                };

                // Popup / Dropdown：与输入框同款暗底，留出右侧箭头位置
                SPopup = new GUIStyle(SField)
                {
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(10, 24, 8, 8)
                };
                var popupHover = new Color(InputBg.r + 0.03f, InputBg.g + 0.03f, InputBg.b + 0.03f);
                SkinR(SPopup, InputBg, TextBright, 5, popupHover, InputBg, BorderDim);

                // Radio：未选中暗底灰框，选中蓝色填充
                var radioUnchecked = SquareTex(16, WindowBg, new Color(0.35f, 0.37f, 0.42f));
                var radioChecked = SquareTex(16, Accent, AccentBright);
                var radioBase = new GUIStyle
                {
                    fixedWidth = 16,
                    fixedHeight = 16,
                    padding = new RectOffset(0, 0, 0, 0),
                    margin = new RectOffset(4, 4, 4, 4),
                    overflow = new RectOffset(1, 1, 1, 1),
                    border = new RectOffset(0, 0, 0, 0),
                    alignment = TextAnchor.MiddleCenter
                };
                SRadio = new GUIStyle(radioBase)
                {
                    normal = { background = radioUnchecked },
                    hover = { background = radioUnchecked },
                    active = { background = radioUnchecked }
                };
                SRadioOn = new GUIStyle(radioBase)
                {
                    normal = { background = radioChecked },
                    hover = { background = radioChecked },
                    active = { background = radioChecked }
                };

                // Foldout：可折叠标题，左侧箭头，整行可点
                SFoldout = new GUIStyle(SBtn)
                {
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(10, 10, 8, 8),
                    fontStyle = FontStyle.Bold
                };

                _skin = ScriptableObject.CreateInstance<GUISkin>();
                _skin.font = GUI.skin.font;
                _skin.window = SWindow;
                _skin.label = SLabel;
                _skin.button = SBtn;
                _skin.textField = SField;
                _skin.textArea = new GUIStyle(SField) { wordWrap = true };
                _skin.box = SPanel;
                _skin.toggle = SToggle;
                _skin.scrollView = new GUIStyle("scrollView")
                {
                    normal = new GUIStyleState { background = null, textColor = TextNormal },
                    padding = new RectOffset(2, 2, 2, 2)
                };
                _skin.verticalScrollbar = SScroll;
                _skin.verticalScrollbarThumb = SScrollThumb;
                _skin.verticalScrollbarUpButton = GUIStyle.none;
                _skin.verticalScrollbarDownButton = GUIStyle.none;
                _skin.horizontalScrollbar = GUIStyle.none;

                // 滑块样式：单条轨道 + 居中蓝色方块滑块
                var trackColor = new Color(0.18f, 0.19f, 0.22f);
                var sliderTrackTex = Tex(trackColor.r, trackColor.g, trackColor.b);
                _skin.horizontalSlider = new GUIStyle(GUI.skin.horizontalSlider)
                {
                    normal =
                    {
                        background = sliderTrackTex,
                        textColor = Color.clear
                    },

                    fixedHeight = 12f,
                    stretchWidth = true,

                    margin = new RectOffset(4, 4, 0, 0),

                    border = new RectOffset(0, 0, 0, 0)
                };
                var sliderThumbTex = SliderThumbTex(12, Accent);

                _skin.horizontalSliderThumb = new GUIStyle(GUI.skin.horizontalSliderThumb)
                {
                    normal =
                    {
                        background = sliderThumbTex,
                        textColor = Color.clear
                    },
                    hover =
                    {
                        background = sliderThumbTex,
                        textColor = Color.clear
                    },
                    active =
                    {
                        background = sliderThumbTex,
                        textColor = Color.clear
                    },

                    fixedWidth = 12f,
                    fixedHeight = 12f,
                    alignment = TextAnchor.MiddleCenter
                };

                _skin.customStyles = new[]
                {
                    STitle, SHint, SMuted, SCount, SBtnAdd, SBtnDel,
                    SRow, SRowAlt, SRowSelected, SFeatureName, STag, STagHidden,
                    SStatusOk, SStatusErr, SNameBtn, SDetail, SDetailHead,
                    SBonus, SChip, SChipOn, STypeGood, STypeBad,
                    STypeSpecial, STypeTemp, SPopup, SRadio, SRadioOn
                };

                _skin.settings.selectionColor = C.Selection;
                _skin.settings.cursorColor = C.Cursor;
                _skin.settings.cursorFlashSpeed = 2.0f;
                _skin.settings.doubleClickSelectsWord = true;
                _skin.settings.tripleClickSelectsLine = true;

            }
            catch (Exception ex)
            {
                Log.Error($"[LunHuiShop] 皮肤初始化失败:  {ex}");
            }
            _initialized = true;
        }

        // ==================== 辅助方法 ====================

        private static Texture2D Tex(float r, float g, float b, float a = 1f)
        {
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, new Color(r, g, b, a));
            tex.Apply(false, false);
            tex.hideFlags = HideFlags.HideAndDontSave;
            return tex;
        }

        private static Texture2D RoundTex(Color fill, int radius = 6, Color? border = null, float borderW = 1f)
        {
            var size = radius * 2 + 2;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            var borderColor = border.GetValueOrDefault(fill);

            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var px = x + 0.5f;
                var py = y + 0.5f;
                var cx = Mathf.Clamp(px, radius, size - radius);
                var cy = Mathf.Clamp(py, radius, size - radius);
                var dist = Mathf.Sqrt((px - cx) * (px - cx) + (py - cy) * (py - cy));
                var alpha = Mathf.Clamp01(radius - dist + 0.5f);

                var color = fill;
                if (border != null)
                {
                    var blend = Mathf.Clamp01(dist - (radius - borderW) + 0.5f);
                    color = Color.Lerp(fill, borderColor, blend);
                }

                pixels[y * size + x] = new Color(color.r, color.g, color.b, color.a * alpha);
            }

            tex.SetPixels(pixels);
            tex.Apply(false, false);
            tex.hideFlags = HideFlags.HideAndDontSave;
            tex.wrapMode = TextureWrapMode.Clamp;
            return tex;
        }

        /// <summary>生成实心带边框正方形纹理。</summary>
        private static Texture2D SquareTex(int size, Color fill, Color border, float borderW = 1f)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];

            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var isEdge = x < borderW || x >= size - borderW || y < borderW || y >= size - borderW;
                pixels[y * size + x] = isEdge ? border : fill;
            }

            tex.SetPixels(pixels);
            tex.Apply(false, false);
            tex.hideFlags = HideFlags.HideAndDontSave;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Point;
            return tex;
        }

        /// <summary>生成透明底、中心带实心方块的纹理，用于滑块 thumb。</summary>
        private static Texture2D SliderThumbTex(int size, Color color)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];

            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
                pixels[y * size + x] = color;

            tex.SetPixels(pixels);
            tex.Apply(false, false);

            tex.hideFlags = HideFlags.HideAndDontSave;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Point;

            return tex;
        }

        private static void SkinR(GUIStyle s, Color fill, Color text, int radius = 6,
            Color? hover = null, Color? active = null, Color? border = null)
        {
            var normalTex = RoundTex(fill, radius, border);
            s.normal.background = normalTex;
            s.onNormal.background = normalTex;
            s.focused.background = normalTex;

            var hoverTex = RoundTex(hover.GetValueOrDefault(fill), radius, border);
            s.hover.background = hoverTex;
            s.onHover.background = hoverTex;

            var activeTex = RoundTex(active.GetValueOrDefault(fill), radius, border);
            s.active.background = activeTex;
            s.onActive.background = activeTex;

            s.normal.textColor = text;
            s.onNormal.textColor = text;
            s.focused.textColor = text;
            s.hover.textColor = text;
            s.onHover.textColor = text;
            s.active.textColor = text;
            s.onActive.textColor = text;
            s.border = new RectOffset(radius, radius, radius, radius);
        }

        /// <summary>绘制标签页芯片按钮。</summary>
        public static bool TabChip(string label, bool active)
        {
            return GUILayout.Button(label, active ? SChipOn : SChip, GUILayout.MinWidth(64f));
        }

        /// <summary>绘制自定义单选下拉菜单。返回新选中的索引。</summary>
        public static int Dropdown(int selectedIndex, string[] options, ref bool expanded)
        {
            var content = new GUIContent(options[selectedIndex] + "  ▼");
            if (GUILayout.Button(content, SPopup))
                expanded = !expanded;

            if (expanded)
            {
                GUILayout.BeginVertical(SPanel);
                for (var i = 0; i < options.Length; i++)
                    if (GUILayout.Button(options[i], i == selectedIndex ? SChipOn : SBtn))
                    {
                        selectedIndex = i;
                        expanded = false;
                    }

                GUILayout.EndVertical();
            }

            return selectedIndex;
        }

        /// <summary>绘制折叠展开标题。返回新的展开状态。</summary>
        public static bool Foldout(bool expanded, string label)
        {
            var icon = expanded ? "▼ " : "▶ ";
            if (GUILayout.Button(icon + label, SFoldout))
                expanded = !expanded;
            return expanded;
        }

        /// <summary>绘制分隔线。</summary>
        public static void Divider(float pad = 6f)
        {
            GUILayout.Space(pad);
            var rect = GUILayoutUtility.GetRect(1f, 1f, GUILayout.ExpandWidth(true));
            if (_lineTex != null)
                GUI.DrawTexture(rect, _lineTex);
            GUILayout.Space(pad);
        }

        /// <summary>绘制 Tooltip 气泡，OnGUI 末尾调用。</summary>
        public static void DrawTooltip()
        {
            if (string.IsNullOrEmpty(GUI.tooltip)) return;

            var tooltip = new GUIStyle(GUI.skin.label)
            {
                normal = new GUIStyleState { background = Tex(0.12f, 0.12f, 0.14f, 0.92f), textColor = TextBright },
                fontSize = 11,
                padding = new RectOffset(8, 8, 5, 5),
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                richText = true
            };

            var content = new GUIContent(GUI.tooltip);
            const float maxW = 300f;
            var w = tooltip.CalcSize(content).x;
            var h = tooltip.CalcHeight(content, Mathf.Min(w, maxW));
            if (w > maxW) w = maxW;

            var x = Event.current.mousePosition.x + 12f;
            var y = Event.current.mousePosition.y + 20f;
            if (x + w + 16f > Screen.width)
                x = Event.current.mousePosition.x - w - 8f;
            if (y + h + 8f > Screen.height)
                y = Event.current.mousePosition.y - h - 8f;

            GUI.Label(new Rect(x, y, w + 16f, h + 8f), GUI.tooltip, tooltip);
        }

        // 旧色板兼容类（保留以兼容现有调用）
        public static class C
        {
            public static Color TextBright => DarkSkin.TextBright;
            public static Color TextNormal => DarkSkin.TextNormal;
            public static Color TextDim => DarkSkin.TextDim;
            public static Color BgDarkest => InputBg;
            public static Color BgDark => WindowBg;
            public static Color BgPanel => PanelBg;
            public static Color BgMid => ChipBg;
            public static Color BgHighlight => ChipHover;
            public static Color RowBg => DarkSkin.RowBg;
            public static Color RowBgAlt => DarkSkin.RowBgAlt;
            public static Color BtnNormal => DarkSkin.BtnNormal;
            public static Color BtnHover => DarkSkin.BtnHover;
            public static Color BtnActive => DarkSkin.BtnActive;
            public static Color Accent => DarkSkin.Accent;
            public static Color AccentDim => DarkSkin.AccentDim;
            public static Color AccentBright => DarkSkin.AccentBright;
            public static Color Success => DarkSkin.StatusOk;
            public static Color Error => BtnDelNormal;
            public static Color ErrorHover => BtnDelHover;
            public static Color BorderDim => DarkSkin.BorderDim;
            public static Color Border => new Color(0.25f, 0.25f, 0.27f);
            public static Color BorderLight => new Color(0.35f, 0.35f, 0.37f);
            public static Color WindowBorder => DarkSkin.WindowBorder;
            public static Color ScrollBg => Color.clear;
            public static Color ThumbNormal => ScrollThumb;
            public static Color ThumbHover => ScrollThumbHover;
            public static Color Selection => new Color(DarkSkin.Accent.r, DarkSkin.Accent.g, DarkSkin.Accent.b, 0.35f);
            public static Color Cursor => DarkSkin.TextBright;
            public static Color CountColor => DarkSkin.CountColor;
            public static Color TagColor => DarkSkin.TagColor;
            public static Color StatusOk => DarkSkin.StatusOk;
            public static Color StatusErr => DarkSkin.StatusErr;
        }
    }
}