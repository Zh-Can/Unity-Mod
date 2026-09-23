using SMR2Mod.GuiFramework.Config;
using SMR2Mod.GuiFramework.Controls;
using SMR2Mod.GuiFramework.Localization;
using SMR2Mod.GuiFramework.Other;
using SMR2Mod.GuiFramework.Style;
using UnityEngine;

namespace SMR2Mod
{
    public class MainView : MonoBehaviour
    {
        private WindowData _mainWindow = null;
        private WindowData _hotkeyWindow = null;
        private HotkeyConfig _editingHotkey = new HotkeyConfig();
        private bool _capturingHotkey;
        
        
        private void Awake()
        {
            HttpGet.TryHit(this);
        
            _mainWindow = UI.NewWindow(
                    new Rect(100, 100, 780, 780),
                    "SMR2Mod",
                    DrawMainWindow)
                .Id(1)
                .Hide()
                .Footer(DrawStatusBar)
                .Build();

            _hotkeyWindow = UI.NewWindow(
                    new Rect(300, 300, 320, 220),
                    "",
                    DrawHotkeyWindow)
                .Id(2)
                .Hide()
                .Build();
        }
        private void Start()
        {
        }
        private void Update()
        {
            if (BaseConfig.Hotkey.IsPressed())
            {
                _mainWindow.Visible = !_mainWindow.Visible;
            }
        }
        
        private void OnGUI()
        {
            if (_capturingHotkey)
            {
                var e = Event.current;
                if (e.type == EventType.KeyDown)
                {
                    // 忽略纯修饰键
                    KeyCode key = e.keyCode;
                    if (key != KeyCode.LeftControl &&
                        key != KeyCode.RightControl &&
                        key != KeyCode.LeftAlt &&
                        key != KeyCode.RightAlt &&
                        key != KeyCode.LeftShift &&
                        key != KeyCode.RightShift &&
                        key != KeyCode.None)
                    {
                        _editingHotkey.Key = e.keyCode;
                        _editingHotkey.Ctrl = e.control;
                        _editingHotkey.Alt = e.alt;
                        _editingHotkey.Shift = e.shift;
                        _capturingHotkey = false;
                    }
                    e.Use();
                }
            }
            UI.WindowControls.OnGUI();
        }
        /// <summary>
        /// 主窗体绘制
        /// </summary>
        private void DrawMainWindow()
        {
            
        }
        
        /// <summary>
        /// 底部
        /// </summary>
        private void DrawStatusBar()
        {
            UI.Divider(5);
            UI.Horizontal(() =>
            {
                UI.Button($"{Loc.Get("缩放")}: {Mathf.RoundToInt(UI.WindowControls.Scale * 100f)}%  {Loc.Get("按")}{BaseConfig.Hotkey.GetDisplayName()}{Loc.Get("键显示/隐藏")}")
                    .Label().OnClick(() =>
                    {
                        _editingHotkey = BaseConfig.Hotkey.Clone();
                        _hotkeyWindow.Show();
                    }).Style(DarkSkin.SMuted).Draw(GUILayout.Width(220));
                UI.FlexibleSpace();
                
                UI.Button(Loc.Get("点赞数:") + HttpGet.Count).Label().OnClick(() =>
                {
                    HttpGet.TryHit(this);
                }).Style(DarkSkin.SHint).Draw(GUILayout.Width(100));
            
            });
        }
        /// <summary>
        /// 自定义快捷键窗体
        /// </summary>
        private void DrawHotkeyWindow()
        {
            UI.Vertical(() =>
            {
                UI.Space(8);

                UI.Label($"{Loc.Get("当前热键")}: {(_capturingHotkey ? Loc.Get("请按下按键...") : _editingHotkey.GetDisplayName())}")
                    .AsMuted()
                    .Draw();

                UI.Space(8);

                if (UI.Button(_capturingHotkey ? Loc.Get("取消捕获") : Loc.Get("按下新按键")).Draw())
                {
                    _capturingHotkey = !_capturingHotkey;
                }

                UI.Space(12);
                UI.Label(Loc.Get("修饰键")).AsMuted().Draw();

                UI.Horizontal(() =>
                {
                    _editingHotkey.Ctrl = UI.Toggle("Ctrl").Value(_editingHotkey.Ctrl).Draw();
                    _editingHotkey.Alt = UI.Toggle("Alt").Value(_editingHotkey.Alt).Draw();
                    _editingHotkey.Shift = UI.Toggle("Shift").Value(_editingHotkey.Shift).Draw();
                });

                UI.Space(16);
                UI.Horizontal(() =>
                {
                    UI.FlexibleSpace();
                    if (UI.Button(Loc.Get("确定")).Add().Draw())
                    {
                        BaseConfig.Hotkey = _editingHotkey.Clone();
                        BaseConfig.Save();
                        _hotkeyWindow.Hide();
                    }
                    UI.Space(10);
                    if (UI.Button(Loc.Get("取消")).Draw())
                    {
                        _capturingHotkey = false;
                        _hotkeyWindow.Hide();
                    }
                    UI.FlexibleSpace();
                });
            });
        }
    }
}