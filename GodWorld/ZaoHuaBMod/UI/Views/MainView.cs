using UnityEngine;
using ZaoHuaBMod.Core;
using ZaoHuaBMod.UI.Framework;

namespace ZaoHuaBMod.UI.Views
{
    public class MainView : MonoBehaviour
    {
        private WindowData _mainWindow;

        private bool _toggleValue;
        private float _sliderValue = 0.5f;
        private string _textFieldValue = "输入文字";
        private string _textAreaValue = "多行输入区域\n第二行";
        private int _selectedToolbar;
        private int _selectedGrid;
        private int _popupIndex;
        private bool _showPopup;
        private Vector2 _scrollPos;
        private string _password = "";

        private readonly string[] _toolbarItems = { "标签1", "标签2", "标签3" };
        private readonly string[] _gridItems = { "选项A", "选项B", "选项C", "选项D" };
        private readonly string[] _popupItems = { "项目1", "项目2", "项目3", "项目4" };

        private void Start()
        {
            Log.Info("MainView Start called");
            _mainWindow = GUIManager.Instance.CreateWindow(
                new Rect(100, 100, 500, 500),
                "主窗口",
                Draw);
            Log.Info($"MainView window created, Visible={_mainWindow.Visible}");
            _mainWindow.Show();
            Log.Info($"MainView window shown, Visible={_mainWindow.Visible}");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                Log.Info("BackQuote pressed!");
                if (_mainWindow != null)
                {
                    if (_mainWindow.Visible)
                    {
                        Log.Info("Hiding window");
                        _mainWindow.Hide();
                    }
                    else
                    {
                        Log.Info("Showing window");
                        _mainWindow.Show();
                    }
                }
            }
        }

        private int _onGuiCount;

        private void OnGUI()
        {
            _onGuiCount++;
            if (_onGuiCount == 1)
                Log.Info("MainView OnGUI first call");
            
            GUIManager.Instance.OnGUI();
        }

        private void Draw(WindowData window)
        {
            _scrollPos = GUILayout.BeginScrollView(_scrollPos);

            GUILayout.Label("=== 基础控件测试 ===");

            GUILayout.Label("普通文本 Label");
            GUILayout.Label(new GUIContent("带提示文本", "鼠标放上来会显示 Tooltip"));

            GUILayout.Space(10);
            GUILayout.Label("=== 按钮 ===");

            if (GUILayout.Button("普通按钮"))
            {
                Log.Info("普通按钮被点击");
            }

            var btnContent = new GUIContent("带提示按钮", "我是悬浮提示文字");
            if (GUILayout.Button(btnContent))
            {
                Log.Info("带提示按钮被点击");
            }

            if (GUILayout.RepeatButton("按住重复按钮"))
            {
                Log.Info("重复按钮按住中");
            }

            GUILayout.Space(10);
            GUILayout.Label("=== 开关与滑块 ===");

            GUILayout.BeginHorizontal();
            _toggleValue = GUILayout.Toggle(_toggleValue, GUIContent.none);
            GUILayout.Label("开关 Toggle");
            GUILayout.EndHorizontal();

            GUILayout.Label($"滑块值: {_sliderValue:F2}");
            _sliderValue = GUILayout.HorizontalSlider(_sliderValue, 0f, 1f);

            GUILayout.Space(10);
            GUILayout.Label("=== 输入框 ===");

            GUILayout.Label("单行输入:");
            _textFieldValue = GUILayout.TextField(_textFieldValue);

            GUILayout.Label("密码输入:");
            _password = GUILayout.PasswordField(_password, '*');

            GUILayout.Label("多行输入:");
            _textAreaValue = GUILayout.TextArea(_textAreaValue, GUILayout.Height(60));

            GUILayout.Space(10);
            GUILayout.Label("=== 选择控件 ===");

            _selectedToolbar = GUILayout.Toolbar(_selectedToolbar, _toolbarItems);

            GUILayout.Label($"当前 Toolbar: {_toolbarItems[_selectedToolbar]}");

            _selectedGrid = GUILayout.SelectionGrid(_selectedGrid, _gridItems, 2);

            GUILayout.Label($"当前 Grid: {_gridItems[_selectedGrid]}");

            GUILayout.Label("下拉选择:");
            if (GUILayout.Button($"▼ {_popupItems[_popupIndex]}"))
            {
                _showPopup = !_showPopup;
            }

            if (_showPopup)
            {
                for (int i = 0; i < _popupItems.Length; i++)
                {
                    if (GUILayout.Button($"    {_popupItems[i]}"))
                    {
                        _popupIndex = i;
                        _showPopup = false;
                    }
                }
            }

            GUILayout.Space(10);
            GUILayout.Label("=== 布局 ===");

            GUILayout.BeginHorizontal("box");
            GUILayout.Label("水平布局左");
            GUILayout.FlexibleSpace();
            GUILayout.Label("水平布局右");
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical("box");
            GUILayout.Label("垂直布局 上");
            GUILayout.Label("垂直布局 中");
            GUILayout.Label("垂直布局 下");
            GUILayout.EndVertical();

            GUILayout.Space(20);
            if (GUILayout.Button("隐藏窗口"))
            {
                window.Hide();
            }

            GUILayout.EndScrollView();
        }
    }
}
