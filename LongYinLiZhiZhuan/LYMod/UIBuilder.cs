using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Il2Cpp;
using LYMod.Helpers;
using MelonLoader;
using UnityEngine;

namespace LYMod;

/// <summary>
///     UI构建器 - 提供链式API简化IMGUI界面构建
/// </summary>
public class UIBuilder
{
    #region 布局参数
    public float Scale => _scale;
    private readonly float _scale;
    private readonly float _labelWidth;
    private readonly float _inputWidth;
    private readonly float _rowSpacing;
    private readonly float _itemSpacing;
    private readonly float _leftPadding;
    private readonly float _toggleHeight;
    private readonly float _inputHeight;

    #endregion

    #region 内部状态

    private readonly List<Action> _pendingActions = new();
    private readonly Dictionary<object, string> _inputBuffers = new();
    private FoldoutPanel? _currentPanel;
    private bool _panelExpanded;
    private static readonly Dictionary<string, FoldoutPanel> Panels = new();
    private readonly Dictionary<string, Vector2> _scrollPositions = new();
    
    #endregion

    #region 构造函数

    public UIBuilder(float scale = 1f, float labelWidth = 100f, float inputWidth = 60f)
    {
        _scale = scale;
        _labelWidth = labelWidth * scale;
        _inputWidth = inputWidth * scale;
        _rowSpacing = 5f * scale;
        _itemSpacing = 15f * scale;
        _leftPadding = 5f * scale;
        _toggleHeight = 24f * scale;
        _inputHeight = 22f * scale;
    }

    #endregion

    #region 核心内部方法

    public void ExecuteOrDefer(Action action)
    {
        if (_currentPanel != null) _pendingActions.Add(action);
        else action.Invoke();
    }

    private float GetLabelW(float customWidth)
    {
        return customWidth > 0 ? customWidth * _scale : _labelWidth;
    }

    private float GetControlW(float customWidth)
    {
        return customWidth > 0 ? customWidth * _scale : _inputWidth;
    }

    private void SaveConfig(MelonPreferences_Category? customCategory)
    {
        (customCategory ?? Plugin.Instance.MainCategory).SaveToFile();
    }

    /// <summary>
    /// 自适应宽度：测量当前文本在 IMGUI 里渲染的实际像素宽度
    /// </summary>
    private float GetAdaptiveLabelW(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0f;
        var style = GUI.skin.label;
        if (style == null) return GetLabelW(0); 
        
        // 计算这段文字画出来到底占多少像素宽
        var size = style.CalcSize(new GUIContent(text));
        
        // size.x 就是真实宽度，额外加上 5 个像素的右边距，让文字和后面的输入框别贴得太死
        return size.x + 5f * _scale; 
    }

    #endregion

    #region 折叠面板方法

    public UIBuilder BeginFoldout(string title, bool defaultExpanded = false)
    {
        _currentPanel = GetOrCreatePanel(title, defaultExpanded);
        _panelExpanded = _currentPanel.DrawHeader(_scale);
        _currentPanel.BeginContent();
        _pendingActions.Clear();
        return this;
    }

    public UIBuilder EndFoldout()
    {
        if (_currentPanel == null) return this;
        if (_panelExpanded && _pendingActions.Count > 0)
            foreach (var action in _pendingActions.ToList())
                action();
        else
            GUILayout.Space(1);

        _currentPanel.EndContent();
        _currentPanel = null;
        _pendingActions.Clear();
        return this;
    }

    private static FoldoutPanel GetOrCreatePanel(string title, bool defaultExpanded)
    {
        if (!Panels.TryGetValue(title, out var panel))
        {
            panel = new FoldoutPanel(title, defaultExpanded);
            Panels[title] = panel;
        }

        return panel;
    }

    #endregion

    #region 布局容器

    /// <summary>
    /// 水平布局开始
    /// </summary>
    public UIBuilder BeginHorizontal()
    {
        ExecuteOrDefer(() => GUILayout.BeginHorizontal());
        return this;
    }

    /// <summary>
    /// 水平布局结束
    /// </summary>
    public UIBuilder EndHorizontal()
    {
        ExecuteOrDefer(GUILayout.EndHorizontal);
        return this;
    }
    /// <summary>
    /// 垂直布局开始
    /// </summary>
    public UIBuilder BeginVertical(string style = "Box")
    {
        ExecuteOrDefer(() => GUILayout.BeginVertical(style)); 
        return this;
    }
    /// <summary>
    /// 垂直布局结束
    /// </summary>
    public UIBuilder EndVertical()
    {
        ExecuteOrDefer(GUILayout.EndVertical); 
        return this;
    }
    /// <summary>
    /// 空格
    /// </summary>
    public UIBuilder Space(float pixels)
    {
        var p = pixels * _scale;
        ExecuteOrDefer(() => GUILayout.Space(p));
        return this;
    }
    /// <summary>
    /// 开始滚动视图（无需外部传 ref 变量，内部通过 ID 自动管理状态）
    /// </summary>
    /// <param name="id">当前滚动条的唯一名字（随便取，比如 "attr_list"）</param>
    /// <param name="height">可视区域高度，0则自动撑满</param>
    public UIBuilder BeginScrollView(string id, float height = 0f)
    {
        var h = height > 0 ? height * _scale : 0f;
        ExecuteOrDefer(() =>
        {
            // 如果字典里没记录，默认给个顶部起点
            if (!_scrollPositions.TryGetValue(id, out var pos)) pos = Vector2.zero;

            // 画滚动条，并把返回的新坐标更新回字典！
            if (h > 0)
                _scrollPositions[id] = GUILayout.BeginScrollView(pos, GUILayout.Height(h));
            else
                _scrollPositions[id] = GUILayout.BeginScrollView(pos);
        });
        return this;
    }

    /// <summary>
    /// 结束滚动视图
    /// </summary>
    public UIBuilder EndScrollView()
    {
        ExecuteOrDefer(GUILayout.EndScrollView);
        return this;
    }
    #endregion

    #region AddLinkedInt/Float/String/bool
    
    /// <summary>
    /// bool开关：直接绑定 bool 数据源
    /// </summary>
    public UIBuilder AddLinkedBool(string label, Func<bool> getter, Action<bool> setter, float labelWidth = 0f, float controlWidth = 0f)
    {
        var lw = GetLabelW(labelWidth);
        var cw = controlWidth > 0 ? controlWidth * _scale : _toggleHeight;

        ExecuteOrDefer(() =>
        {
            GUILayout.Label(label, GUILayout.Width(lw));
            var newVal = GUILayout.Toggle(getter.Invoke(), "", GUILayout.Width(cw), GUILayout.Height(_toggleHeight));
            if (newVal != getter.Invoke())
            {
                setter.Invoke(newVal);
            }
        });
        return this;
    }
    /// <summary>
    /// int输入框：直接绑定 int 数据源
    /// </summary>
    public UIBuilder AddLinkedInt(string label, Func<int> getter, Action<int> setter, string bufferId, float labelWidth = 0f, float inputWidth = 0f)
    {
        var lw = GetLabelW(labelWidth);
        var iw = GetControlW(inputWidth);

        ExecuteOrDefer(() =>
        {
            var realVal = getter.Invoke();

            // 智能同步缓存
            if (!_inputBuffers.TryGetValue(bufferId, out var buffer) || 
                (int.TryParse(buffer, out var bufVal) && bufVal != realVal))
            {
                buffer = realVal.ToString();
            }

            // 画 UI
            GUILayout.Label(label, GUILayout.Width(lw));
            var newBuffer = GUILayout.TextField(buffer, GUILayout.Width(iw), GUILayout.Height(_inputHeight));

            // 写回数据
            if (newBuffer != buffer)
            {
                if (int.TryParse(newBuffer, out var parsed))
                {
                    setter.Invoke(parsed); 
                    _inputBuffers[bufferId] = parsed.ToString();
                }
                else
                {
                    _inputBuffers[bufferId] = newBuffer; 
                }
            }
        });
        return this;
    }

    /// <summary>
    /// string输入框：直接绑定 string 数据源
    /// </summary>
    public UIBuilder AddLinkedString(string label, Func<string> getter, Action<string> setter, string bufferId, float labelWidth = 0f, float inputWidth = 0f)
    {
        var lw = GetLabelW(labelWidth);
        var iw = GetControlW(inputWidth);

        ExecuteOrDefer(() =>
        {
            var realVal = getter.Invoke() ?? "";

            // 智能同步缓存
            if (!_inputBuffers.TryGetValue(bufferId, out var buffer) || buffer != realVal)
            {
                buffer = realVal;
            }

            // 画 UI
            GUILayout.Label(label, GUILayout.Width(lw));
            var newBuffer = GUILayout.TextField(buffer, GUILayout.Width(iw), GUILayout.Height(_inputHeight));

            // 写回数据
            if (newBuffer != buffer)
            {
                setter.Invoke(newBuffer); 
                _inputBuffers[bufferId] = newBuffer;
            }
        });
        return this;
    }
    
    /// <summary>
    /// float输入框：直接绑定 float 数据源
    /// </summary>
    public UIBuilder AddLinkedFloat(string label, Func<float> getter, Action<float> setter, string bufferId, float labelWidth = 0f, float inputWidth = 0f)
    {
        var lw = GetLabelW(labelWidth);
        var iw = GetControlW(inputWidth);

        ExecuteOrDefer(() =>
        {
            var realVal = getter.Invoke();

            if (!_inputBuffers.TryGetValue(bufferId, out var buffer))
            {
                buffer = realVal.ToString("F1", CultureInfo.InvariantCulture);
            }
            else if (float.TryParse(buffer, NumberStyles.Any, CultureInfo.InvariantCulture, out var bufVal) && !Mathf.Approximately(bufVal, realVal))
            {
                buffer = realVal.ToString("F1", CultureInfo.InvariantCulture);
            }
            
            GUILayout.Label(label, GUILayout.Width(lw));
            var newBuffer = GUILayout.TextField(buffer, GUILayout.Width(iw), GUILayout.Height(_inputHeight));

            if (newBuffer != buffer)
            {
                if (float.TryParse(newBuffer, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
                {
                    setter.Invoke(parsed); 
                    _inputBuffers[bufferId] = parsed.ToString("F1", CultureInfo.InvariantCulture);
                }
                else
                {
                    _inputBuffers[bufferId] = newBuffer; 
                }
            }
        });
        return this;
    }

    #endregion

    #region 

    /// <summary>
    /// 一行单控件：自动保存智能绘制
    /// </summary>
    public UIBuilder AddAutoSave<T>(string label, MelonPreferences_Entry<T> entry, MelonPreferences_Category category = null, float labelWidth = 0f, float controlWidth = 0f)
    {
        var cw = controlWidth > 0 ? controlWidth * _scale : _toggleHeight; // 对于 bool，这个宽度就是 Toggle 的宽度
        var iw = GetControlW(controlWidth); // 对于数值，这个宽度就是输入框的宽度
        var type = typeof(T);
        var capturedLabelWidth = labelWidth;
        ExecuteOrDefer(() =>
        {
            // 传了具体数字就用具体数字，没传(0)就自动测距
            float lw = capturedLabelWidth > 0 ? GetLabelW(capturedLabelWidth) : GetAdaptiveLabelW(label);
            GUILayout.BeginHorizontal();
            GUILayout.Space(_leftPadding);
            GUILayout.Label(label, GUILayout.Width(lw));

            if (type == typeof(bool))
            {
                var newVal = GUILayout.Toggle((bool)(object)entry.Value, "", GUILayout.Width(cw), GUILayout.Height(_toggleHeight));
                if (newVal != (bool)(object)entry.Value) { entry.Value = (T)(object)newVal; SaveConfig(category); }
            }
            else if (type == typeof(string))
            {
                var newVal = GUILayout.TextField((string)(object)entry.Value, GUILayout.Width(iw), GUILayout.Height(_inputHeight));
                if (newVal != (string)(object)entry.Value) { entry.Value = (T)(object)newVal; SaveConfig(category); }
            }
            else if (type == typeof(int))
            {
                if (!_inputBuffers.TryGetValue(entry, out var buffer)) buffer = entry.Value.ToString();
                var newBuffer = GUILayout.TextField(buffer, GUILayout.Width(iw), GUILayout.Height(_inputHeight));
                if (newBuffer != buffer)
                {
                    if (int.TryParse(newBuffer, out var p))
                    {
                        _inputBuffers[entry] = p.ToString(); // 更新缓存

                        if (Convert.ToInt32(entry.Value) != p) 
                        {
                            entry.Value = (T)(object)Convert.ToInt32(p); // 这里转换目标 int
                            SaveConfig(category);
                        }
                    }
                    else
                    {
                        _inputBuffers[entry] = newBuffer;
                    }
                }
            }
            else if (type == typeof(float))
            {
                var realVal = (float)(object)entry.Value;
                if (!_inputBuffers.TryGetValue(entry, out var buffer))
                {
                    buffer = realVal.ToString("F1", CultureInfo.InvariantCulture);
                }
                else if (float.TryParse(buffer, NumberStyles.Any, CultureInfo.InvariantCulture, out var bufVal) && !Mathf.Approximately(bufVal, realVal))
                {
                    buffer = realVal.ToString("F1", CultureInfo.InvariantCulture);
                }
                var newBuffer = GUILayout.TextField(buffer, GUILayout.Width(iw), GUILayout.Height(_inputHeight));

                if (newBuffer != buffer)
                {
                    if (float.TryParse(newBuffer, NumberStyles.Any, CultureInfo.InvariantCulture, out var p))
                    {
                        if (!Mathf.Approximately(p, realVal))
                        {
                            _inputBuffers[entry] = p.ToString("F1", CultureInfo.InvariantCulture);
                            entry.Value = (T)(object)p;
                            SaveConfig(category);
                        }
                        else
                        {
                            _inputBuffers[entry] = newBuffer;
                        }
                    }
                    else
                    {
                        _inputBuffers[entry] = newBuffer;
                    }
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(_rowSpacing);
        });
        return this;
    }
    /// <summary>
    /// 智能添加一行两个【不同类型】控件并自动保存
    /// </summary>
    public UIBuilder AddAutoSaveRow<T1, T2>(string label1, MelonPreferences_Entry<T1> entry1, string label2, MelonPreferences_Entry<T2> entry2, 
        MelonPreferences_Category? category = null, float labelWidth = 0f, float controlWidth = 0f)
    {
        var cw = controlWidth > 0 ? controlWidth * _scale : _toggleHeight;
        var iw = GetControlW(controlWidth);
        var type1 = typeof(T1);
        var type2 = typeof(T2);
        var capturedLabelWidth = labelWidth;
        ExecuteOrDefer(() =>
        {
            // 传了具体数字就用具体数字，没传就自动测距
            float lw1 = capturedLabelWidth > 0 ? GetLabelW(capturedLabelWidth) : GetAdaptiveLabelW(label1);
            float lw2 = capturedLabelWidth > 0 ? GetLabelW(capturedLabelWidth) : GetAdaptiveLabelW(label2);

            GUILayout.BeginHorizontal();
            GUILayout.Space(_leftPadding);

            // --- 绘制第一个控件 ---
            GUILayout.Label(label1, GUILayout.Width(lw1)); 
            if (type1 == typeof(bool)) { var n = GUILayout.Toggle((bool)(object)entry1.Value, "", GUILayout.Width(cw), GUILayout.Height(_toggleHeight)); if (n != (bool)(object)entry1.Value) { entry1.Value = (T1)(object)n; SaveConfig(category); } }
            else if (type1 == typeof(string)) { var n = GUILayout.TextField((string)(object)entry1.Value, GUILayout.Width(iw), GUILayout.Height(_inputHeight)); if (n != (string)(object)entry1.Value) { entry1.Value = (T1)(object)n; SaveConfig(category); } }
            else if (type1 == typeof(int)) { if (!_inputBuffers.TryGetValue(entry1, out var b)) b = entry1.Value!.ToString(); var nb = GUILayout.TextField(b, GUILayout.Width(iw), GUILayout.Height(_inputHeight)); if (nb != b) { if (int.TryParse(nb, out var p) && p != (int)(object)entry1.Value) { entry1.Value = (T1)(object)p; SaveConfig(category); _inputBuffers[entry1] = p.ToString(); } else _inputBuffers[entry1] = nb; } }
            else if (type1 == typeof(float)) { var realVal1 = (float)(object)entry1.Value; string b; if (!_inputBuffers.TryGetValue(entry1, out b)) b = realVal1.ToString("F1", CultureInfo.InvariantCulture); else if (float.TryParse(b, NumberStyles.Any, CultureInfo.InvariantCulture, out var bv1) && !Mathf.Approximately(bv1, realVal1)) b = realVal1.ToString("F1", CultureInfo.InvariantCulture); var nb = GUILayout.TextField(b, GUILayout.Width(iw), GUILayout.Height(_inputHeight)); if (nb != b) { if (float.TryParse(nb, NumberStyles.Any, CultureInfo.InvariantCulture, out var p)) { if (!Mathf.Approximately(p, realVal1)) { _inputBuffers[entry1] = p.ToString("F1", CultureInfo.InvariantCulture); entry1.Value = (T1)(object)p; SaveConfig(category); } else _inputBuffers[entry1] = nb; } else _inputBuffers[entry1] = nb; } }

            GUILayout.Space(_itemSpacing);

            // --- 绘制第二个控件 ---
            GUILayout.Label(label2, GUILayout.Width(lw2));
            if (type2 == typeof(bool)) { var n = GUILayout.Toggle((bool)(object)entry2.Value, "", GUILayout.Width(cw), GUILayout.Height(_toggleHeight)); if (n != (bool)(object)entry2.Value) { entry2.Value = (T2)(object)n; SaveConfig(category); } }
            else if (type2 == typeof(string)) { var n = GUILayout.TextField((string)(object)entry2.Value, GUILayout.Width(iw), GUILayout.Height(_inputHeight)); if (n != (string)(object)entry2.Value) { entry2.Value = (T2)(object)n; SaveConfig(category); } }
            else if (type2 == typeof(int)) { if (!_inputBuffers.TryGetValue(entry2, out var b)) b = entry2.Value!.ToString(); var nb = GUILayout.TextField(b, GUILayout.Width(iw), GUILayout.Height(_inputHeight)); if (nb != b) { if (int.TryParse(nb, out var p) && p != (int)(object)entry2.Value) { entry2.Value = (T2)(object)p; SaveConfig(category); _inputBuffers[entry2] = p.ToString(); } else _inputBuffers[entry2] = nb; } }
            else if (type2 == typeof(float)) { var realVal2 = (float)(object)entry2.Value; string b; if (!_inputBuffers.TryGetValue(entry2, out b)) b = realVal2.ToString("F1", CultureInfo.InvariantCulture); else if (float.TryParse(b, NumberStyles.Any, CultureInfo.InvariantCulture, out var bv2) && !Mathf.Approximately(bv2, realVal2)) b = realVal2.ToString("F1", CultureInfo.InvariantCulture); var nb = GUILayout.TextField(b, GUILayout.Width(iw), GUILayout.Height(_inputHeight)); if (nb != b) { if (float.TryParse(nb, NumberStyles.Any, CultureInfo.InvariantCulture, out var p)) { if (!Mathf.Approximately(p, realVal2)) { _inputBuffers[entry2] = p.ToString("F1", CultureInfo.InvariantCulture); entry2.Value = (T2)(object)p; SaveConfig(category); } else _inputBuffers[entry2] = nb; } else _inputBuffers[entry2] = nb; } }

            GUILayout.EndHorizontal();
            GUILayout.Space(_rowSpacing);
        });
        return this;
    }

    #endregion

    #region 只读信息展示控件
    /// <summary>
    /// 一行展示任意个数的只读信息（强制指定 label 宽度以保证对齐）
    /// </summary>
    public UIBuilder AddInfoRow(float labelWidth, params InfoItem[]? items)
    {
        if (items == null || items.Length == 0) return this;
        var lw = GetLabelW(labelWidth);

        ExecuteOrDefer(() =>
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(_leftPadding);

            for (int i = 0; i < items.Length; i++)
            {
                GUILayout.Label(items[i].Label, GUILayout.Width(lw));
                GUILayout.Label(items[i].Value);
                // 如果不是最后一个，中间加点间距
                if (i < items.Length - 1)
                {
                    GUILayout.Space(_itemSpacing);
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(_rowSpacing);
        });
        return this;
    }
    #endregion
    
    #region 其他方法

    public UIBuilder AddLabel(string text, float width = 60f)
    {
        var w = width > 0 ? width * _scale : 0f;
        ExecuteOrDefer(() =>
        {
            GUILayout.Label(text, w > 0 ? GUILayout.Width(w) : GUILayout.Width(GetAdaptiveLabelW(text)));
        });
        return this;
    }

    public UIBuilder AddLabelRow(string text, float width = 0f)
    {
        var w = width > 0 ? width * _scale : 0f;
        ExecuteOrDefer(() =>
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(text, w > 0 ? GUILayout.Width(w) : GUILayout.Width(GetAdaptiveLabelW(text)));
            GUILayout.EndHorizontal();
            GUILayout.Space(_rowSpacing);
        });
        return this;
    }
    

    public UIBuilder AddButton(string text, Action onClick, float width = 0f, float height = 0f)
    {
        var w = width > 0 ? width * _scale : 0f;
        var h = height > 0 ? height * _scale : _toggleHeight;
        
        ExecuteOrDefer(() =>
        {
            if (w > 0)
            {
                if (GUILayout.Button(text, GUILayout.Width(w), GUILayout.Height(h))) onClick?.Invoke();
            }
            else
            {
                if (GUILayout.Button(text, GUILayout.Height(h))) onClick?.Invoke();
            }
        });
        return this;
    }

    public UIBuilder AddButtonRow(string text, Action? onClick, float width = 0f, float height = 0f)
    {
        var w = width > 0 ? width * _scale : 0f;
        var h = height > 0 ? height * _scale : _toggleHeight;
        ExecuteOrDefer(() =>
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(_leftPadding);
            if (w > 0)
            {
                if (GUILayout.Button(text, GUILayout.Width(w), GUILayout.Height(h))) onClick?.Invoke();
            }
            else
            {
                if (GUILayout.Button(text, GUILayout.Height(h))) onClick?.Invoke();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(_rowSpacing);
        });
        return this;
    }
    
    /// <summary>
    /// 添加滑块(绑定配置项，自动保存)
    /// </summary>
    public UIBuilder AddSlider(string label, MelonPreferences_Entry<float> entry, float min, float max,
        MelonPreferences_Category? category = null, float labelWidth = 0f, float sliderWidth = 0f, bool useFixedLayout = false)
    {
        float lw;
        float sw;

        if (useFixedLayout)
        {
            if (labelWidth > 0)
            {
                lw = labelWidth + 15f; 
            }
            else
            {
                var style = GUI.skin.label;
                if (style != null)
                {
                    var size = style.CalcSize(new GUIContent($"{label}: {entry.Value:F2}"));
                    lw = size.x + 5f; 
                }
                else
                {
                    lw = 150f;
                }
            }
            sw = sliderWidth > 0 ? sliderWidth : 150f; 
        }
        else
        {
            // 正常缩放模式保持不变
            lw = GetLabelW(labelWidth) + 50 * _scale;
            sw = sliderWidth > 0 ? sliderWidth * _scale : 0f;
        }

        ExecuteOrDefer(() =>
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(useFixedLayout ? 5f : _leftPadding);
            
            GUILayout.Label($"{label}: {entry.Value:F2}", GUILayout.Width(lw));
            
            float newVal;
            if (sw > 0)
            {
                var sliderOptions = GUILayout.Width(sw);
                newVal = GUILayout.HorizontalSlider(entry.Value, min, max, sliderOptions, GUILayout.ExpandWidth(false));
            }
            else
            {
                newVal = GUILayout.HorizontalSlider(entry.Value, min, max);
            }

            if (!Mathf.Approximately(newVal, entry.Value))
            {
                entry.Value = newVal;
                SaveConfig(category);
            }
            
            GUILayout.EndHorizontal();
            GUILayout.Space(useFixedLayout ? 3f : _rowSpacing);
        });
        return this;
    }
    
    
    #endregion
}

/// <summary>
///     折叠面板 - 可展开/收起的UI容器
/// </summary>
public class FoldoutPanel
{
    private readonly string _title;
    private static GUIStyle? _headerStyle;
    private static GUIStyle? _contentStyle;
    private static GUIStyle? _toggleStyle;

    public FoldoutPanel(string title, bool defaultExpanded = false)
    {
        _title = title;
        IsExpanded = defaultExpanded;
    }

    private static void InitStyles()
    {
        if (_headerStyle != null && GUI.skin != null) return;
        if (GUI.skin == null) return;

        _headerStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { textColor = new Color(0.9f, 0.9f, 0.9f, 1f) },
            fontSize = 16, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(5, 5, 5, 5), margin = new RectOffset(0, 0, 2, 0)
        };
        _contentStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = null },
            padding = new RectOffset(10, 10, 5, 5), margin = new RectOffset(0, 0, 0, 2)
        };
        _toggleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft,
            normal = { textColor = new Color(0.9f, 0.9f, 0.9f, 1f) }
        };
    }

    public bool DrawHeader(float scale = 1f)
    {
        InitStyles();
        GUILayout.BeginHorizontal(_headerStyle, GUILayout.Height(30 * scale));
        var arrow = IsExpanded ? "▼ " : "▶ ";
        if (GUILayout.Button($"{arrow}{_title}", _toggleStyle, GUILayout.Height(26 * scale))) IsExpanded = !IsExpanded;
        GUILayout.EndHorizontal();
        return IsExpanded;
    }

    public void BeginContent() => GUILayout.BeginVertical(_contentStyle);
    public void EndContent() => GUILayout.EndVertical();
    private bool IsExpanded { get; set; }
    public void SetExpanded(bool expanded) => IsExpanded = expanded;
}

/// <summary>
///     UI辅助工具类
/// </summary>
public static class UIHelper
{
    private static readonly Dictionary<string, UIBuilder> Builders = new();

    public static UIBuilder CreateBuilder(float scale = 1f, float labelWidth = 100f, float inputWidth = 60f)
    {
        return new UIBuilder(scale, labelWidth, inputWidth);
    }

    public static UIBuilder GetBuilder(string key, float scale = 1f)
    {
        if (!Builders.TryGetValue(key, out var builder))
        {
            builder = new UIBuilder(scale);
            Builders[key] = builder;
        }
        return builder;
    }
}
/// <summary>
/// 给 AddInfo 喂数据
/// </summary>
public readonly struct InfoItem
{
    public string Label { get; }
    public string Value { get; }
    
    // 构造函数自动把任何类型转成字符串，并处理 null
    public InfoItem(string label, object? value)
    {
        Label = label;
        Value = value?.ToString() ?? "";
    }
}

// ==========================================
// 扩展方法 
// ==========================================
public static class UIBuilderExtensions
{
    #region 门派特性

    public static HashSet<int> EnabledForceIDs = new HashSet<int>();
    private static readonly List<ForceData> AllForces = new List<ForceData>();
    private static string _searchText = "";
    private static int _selectedForceID = -1;

    /// <summary>
    /// 在 UIBuilder 绘制门派特性选择器
    /// </summary>
    public static UIBuilder DrawForceSpeFunction(this UIBuilder builder)
    {
        builder.ExecuteOrDefer(() =>
        {
            // --- 顶部按遍历 ---
            GUILayout.Space(5);
            GUILayout.BeginVertical("Box");
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("刷新", GUILayout.Width(60))) RefreshForceList();
            if (GUILayout.Button("全选", GUILayout.Width(60)))
            {
                foreach (var force in AllForces) EnabledForceIDs.Add(force.forceID);
            }
            if (GUILayout.Button("保存", GUILayout.Width(60))) SaveForceFunctions();
            if (GUILayout.Button("清空", GUILayout.Width(60))) EnabledForceIDs.Clear();
            
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            // --- 搜索栏 ---
            GUILayout.BeginHorizontal();
            GUILayout.Label("搜索:", GUILayout.Width(45));
            var newSearch = GUILayout.TextField(_searchText, GUILayout.Width(150));
            if (newSearch != _searchText) _searchText = newSearch;
            GUILayout.Label($"已选: {EnabledForceIDs.Count}", GUILayout.Width(80));
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.Space(10);
            // --- 门派列表循环 ---
            foreach (var force in AllForces)
            {
                if (!string.IsNullOrEmpty(_searchText) && !force.forceName.Contains(_searchText, StringComparison.OrdinalIgnoreCase))
                {
                    continue; 
                }

                bool enabled = EnabledForceIDs.Contains(force.forceID);
                
                // 列表项
                GUILayout.BeginVertical("Box");
                GUILayout.BeginHorizontal();
                
                bool newEnabled = GUILayout.Toggle(enabled, "", GUILayout.Width(20));
                if (newEnabled != enabled)
                {
                    if (newEnabled) EnabledForceIDs.Add(force.forceID);
                    else EnabledForceIDs.Remove(force.forceID);
                }

                if (GUILayout.Button($"{force.forceName} (ID:{force.forceID})", GUI.skin.label, GUILayout.Width(340)))
                {
                    _selectedForceID = _selectedForceID == force.forceID ? -1 : force.forceID; 
                }

                GUILayout.EndHorizontal();

                // 详情展示
                if (_selectedForceID == force.forceID && !string.IsNullOrEmpty(force.speFunctionDescribe))
                {
                    GUILayout.BeginVertical(GUI.skin.box);
                    string desc = force.speFunctionDescribe
                        .Replace("<b>", "")
                        .Replace("</b>", "")
                        .Replace("\\n", "\n");
                    GUILayout.Label(desc, GUI.skin.label);
                    GUILayout.EndVertical();
                }
                GUILayout.EndVertical();
            }
        });
            

        return builder;
    }
   
    /// <summary>
    /// 保存拥有哪些门派特性
    /// </summary>
    private static void SaveForceFunctions()
    {
        Plugin.Instance.ForceSpeFunctions.Value = string.Join(",", EnabledForceIDs.Select(n => n.ToString()));
        Plugin.Instance.MainCategory.SaveToFile();
    }
    /// <summary>
    ///   读取门派数据
    /// </summary>
    public static void RefreshForceList()
    {
        EnabledForceIDs.Clear();
        AllForces.Clear();
        // 读取门派数据
        var gdc = GameDataController.Instance;
        if (gdc?.forceDataBase == null) return;
        
        foreach (var force in gdc.forceDataBase)
        {
            if (force != null)
            {
                AllForces.Add(force);
            }
        }
        
        // 读取已经选中的数据
        var gc = GameController.Instance;
        if (gc == null) return;
        if  (Plugin.Instance.ForceSpeFunctions.Value == "")
        {
            var heroData = gc?.worldData?.Player();
            if (heroData?.belongForceID != null)
            {
                EnabledForceIDs.Add(heroData.belongForceID);
            } 
        }

        if (Plugin.Instance.ForceSpeFunctions.Value != "")
        {
            var heroData = gc?.worldData?.Player();
            EnabledForceIDs = new HashSet<int>(
                Plugin.Instance.ForceSpeFunctions.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
            );
            if (heroData?.belongForceID != null)
            {
                EnabledForceIDs.Add(heroData.belongForceID);
            }
        }
    }
    #endregion

    #region 功能：建筑效果翻倍
    
    public static Dictionary<int, int> BuildingTimesMap = new(); // 建筑索引 -> 倍率值
    private static readonly List<AreaBuildingDataBase> AllBuildings = new ();
    private static int _selectedBuildingIDs = -1;
    private static Dictionary<int, string> _buildingTimesInputs = new(); // 建筑索引 -> 倍率输入文本

    /// <summary>
    /// 在 UIBuilder 绘制建筑属性翻倍选择器
    /// </summary>
    public static UIBuilder DrawSelectBuildings(this UIBuilder builder)
    {
        builder.ExecuteOrDefer(() =>
        {
            // --- 顶部按钮栏 ---
            GUILayout.Space(5);
            GUILayout.BeginVertical("Box");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("刷新", GUILayout.Width(60))) RefreshBuildingList();
            if (GUILayout.Button("全选", GUILayout.Width(60)))
            {
                for (int i = 0; i < AllBuildings.Count; i++) BuildingTimesMap.TryAdd(i, 1);
            }
            if (GUILayout.Button("保存", GUILayout.Width(60))) SaveSelectedBuildings();
            if (GUILayout.Button("清空", GUILayout.Width(60))) BuildingTimesMap.Clear();
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            // --- 建筑列表遍历 ---
            for (var m = 0; m < AllBuildings.Count; m++)
            {
                var building = AllBuildings[m];
                if (!string.IsNullOrEmpty(_searchText) && !building.name.Contains(_searchText, StringComparison.OrdinalIgnoreCase))
                {
                    continue; 
                }
                bool enabled = BuildingTimesMap.ContainsKey(m);
                // 列表项
                GUILayout.BeginVertical("Box");
                GUILayout.BeginHorizontal();
                
                bool newEnabled = GUILayout.Toggle(enabled, "", GUILayout.Width(20));
                if (newEnabled != enabled)
                {
                    if (newEnabled) BuildingTimesMap[m] = 1; // 默认倍率1
                    else BuildingTimesMap.Remove(m);
                    enabled = newEnabled; // 及时更新，防止同帧内倍率输入框写回
                }

                if (GUILayout.Button($"{building.name}", GUI.skin.label, GUILayout.Width(260)))
                {
                    _selectedBuildingIDs = _selectedBuildingIDs == m ? -1 : m; 
                }

                // 每个建筑独立的倍率输入
                GUILayout.Label("倍率:", GUILayout.Width(50));
                if (!_buildingTimesInputs.ContainsKey(m))
                {
                    _buildingTimesInputs[m] = enabled ? BuildingTimesMap[m].ToString() : "1";
                }
                _buildingTimesInputs[m] = GUILayout.TextField(_buildingTimesInputs[m], GUILayout.Width(40));
                if (int.TryParse(_buildingTimesInputs[m], out int timesVal) && timesVal >= 1)
                {
                    if (enabled) BuildingTimesMap[m] = timesVal;
                }

                GUILayout.EndHorizontal();

                // 详情展示
                if (_selectedBuildingIDs == m)
                {
                    string desc = "<b>每月产出</b>：";
                    for (int i = 0; i <= 10; i++)
                    {
                        var list = building.GetTotalChangeResource(i,1);
                        bool hasResource = false;
                        string levelText = $"{i}级：";

                        for (int j = 0; j < list.Count; j++)
                        {
                            float value = list[j];
                            if (Mathf.Approximately(value, 0))  continue;
                            if (j >= GlobalData.ResourceName.Count) continue;
                            
                            string sign = value > 0 ? "+" : "";
                            levelText += $"{GlobalData.ResourceName[j]}{sign}{value}，";
                            hasResource = true;
                        }
                        // 这一级有资源才加到描述里
                        if (hasResource)
                        {
                            // 去掉最后多余的 ，
                            levelText = levelText.TrimEnd('，');
                            desc += levelText + "；";
                        }
                    }
                    // 最后去掉末尾多余的 ；
                    if (desc.EndsWith("；")) desc = desc.TrimEnd('；');
                    
                    desc += "\n<b>加成</b>：";
                    for (int i = 0; i <= 10; i++)
                    {
                        bool hasResource = false;
                        string levelText = $"{i}级：";
                        var dict =  building.GetBuildingSpeAddData(i).forceSpeAddData;
                        if (dict == null || dict.Count == 0) break;
                        
                        foreach (var d in dict)
                        {
                            var value = d.Value < 1
                                ? ((int)(d.Value * 100)).ToString(CultureInfo.InvariantCulture) + "%"
                                : d.Value.ToString(CultureInfo.InvariantCulture);
                            
                            levelText += $"{GameDataController.Instance.forceSpeAddDataBase[d.Key].name}+{value}，";
                            hasResource = true;
                        }
                        // 这一级有资源才加到描述里
                        if (hasResource)
                        {
                            // 去掉最后多余的 ，
                            levelText = levelText.TrimEnd('，');
                            desc += levelText + "；";
                        }
                    }
                    // 最后去掉末尾多余的 ；
                    if (desc.EndsWith("；")) desc = desc.TrimEnd('；');
                    
                    // desc += "\n<b>增加效率</b>：";
                    GUILayout.Label(desc, GUI.skin.label);
                }
                GUILayout.EndVertical();
            }

            GUILayout.EndVertical();
        });
        return builder;
    }
    
    /// <summary>
    /// 保存选择翻倍效果的建筑及其独立倍率
    /// </summary>
    private static void SaveSelectedBuildings()
    {
        // 序列化格式: "索引:倍率,索引:倍率"
        Plugin.Instance.BuildingTimesMapStr.Value = string.Join(",",
            BuildingTimesMap.Select(kv => $"{kv.Key}:{kv.Value}"));
        Plugin.Instance.MainCategory.SaveToFile();
        Plugin.LOG.Msg($"建筑倍率映射：{Plugin.Instance.BuildingTimesMapStr.Value}");
        HeroHelper.TryReadPlayer(out var player);
        var force = player.GetForce();
        force.forceDetailDirty = true;
        player.GetArea().areaDetailDirty = true;
    }

 
    /// <summary>
    /// 刷新建筑数据
    /// </summary>
    public static void RefreshBuildingList()
    {
        BuildingTimesMap.Clear();
        AllBuildings.Clear();
        _buildingTimesInputs.Clear();
        // 读取所有建筑数据
        var gdc = GameDataController.Instance;
        if (gdc == null) return;
        foreach (var bdb in gdc.buildingDataBase)
        {
            AllBuildings.Add(bdb);
        }
        // 优先读取新格式配置 "索引:倍率,索引:倍率"
        if (!string.IsNullOrEmpty(Plugin.Instance.BuildingTimesMapStr.Value))
        {
            foreach (var pair in Plugin.Instance.BuildingTimesMapStr.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = pair.Split(':');
                if (parts.Length == 2 && int.TryParse(parts[0], out int id) && int.TryParse(parts[1], out int times) && times >= 1)
                {
                    BuildingTimesMap[id] = times;
                }
            }
        }
    }
    #endregion
}