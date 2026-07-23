using UnityEngine;
using ZaoHuaBMod.Core;
using ZaoHuaBMod.UI.Core;
using ZaoHuaBMod.UI.Style;

namespace ZaoHuaBMod
{
    public class MainView : MonoBehaviour
    {
        private WindowData _mainWindow;

        private int _tab;
        private bool _toggleValue;
        private float _sliderValue = 0.5f;
        private string _textFieldValue = "输入文字";
        private string _textAreaValue = "多行输入区域\n第二行";
        private string _status = "就绪";

        private readonly string[] _tabs = { "基础", "布局", "状态", "表单" };
        private readonly string[] _dropdownOptions = { "选项 A", "选项 B", "选项 C", "选项 D" };
        private int _dropdownIndex;
        private bool _dropdownExpanded;
        private int _radioIndex;
        private int _selectedTableRow = -1;
        private int _clickCount;

        private readonly string[,] _tableData =
        {
            { "名称", "类型", "数值" },
            { "物品一", "武器", "120" },
            { "物品二", "防具", "85" },
            { "物品三", "道具", "30" },
            { "物品四", "材料", "999" }
        };

        private void Start()
        {
            _mainWindow = GUIManager.Instance.CreateWindow(
                new Rect(100, 100, 520, 680),
                "ZaoHuaBMod",
                Draw);
            _mainWindow.Resizable = true;
            _mainWindow.Show();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                if (_mainWindow != null)
                    _mainWindow.Visible = !_mainWindow.Visible;
            }
        }

        private void OnGUI()
        {
            GUIManager.Instance.OnGUI();
        }

        private void Draw(WindowData window)
        {
            // 标签页芯片
            GUILayout.BeginHorizontal();
            for (int i = 0; i < _tabs.Length; i++)
            {
                if (DarkSkin.TabChip(_tabs[i], _tab == i))
                    _tab = i;
            }
            GUILayout.EndHorizontal();
            DarkSkin.Divider(6f);

            switch (_tab)
            {
                case 0:
                    DrawBasicTab();
                    break;
                case 1:
                    DrawLayoutTab();
                    break;
                case 2:
                    DrawStatusTab();
                    break;
                case 3:
                    DrawFormTab();
                    break;
            }

            GUILayout.FlexibleSpace();
            DarkSkin.Divider(4f);
            GUILayout.Label($"缩放: {Mathf.RoundToInt(GUIManager.Instance.Scale * 100f)}%  按 ` 键显示/隐藏", DarkSkin.SMuted);
        }

        private void DrawBasicTab()
        {
            GUILayout.Label("基础控件", DarkSkin.STitle);

            GUILayout.Label("普通文本 Label", DarkSkin.SLabel);
            GUILayout.Label(new GUIContent("带提示文本", "鼠标放上来会显示 Tooltip"), DarkSkin.SLabel);

            DarkSkin.Divider(4f);
            GUILayout.Label("按钮", DarkSkin.SLabel);

            if (GUILayout.Button("普通按钮", DarkSkin.SBtn))
                Log.Info("普通按钮被点击");

            if (GUILayout.Button("添加按钮", DarkSkin.SBtnAdd))
                Log.Info("添加按钮被点击");

            if (GUILayout.Button("删除按钮", DarkSkin.SBtnDel))
                Log.Info("删除按钮被点击");

            DarkSkin.Divider(4f);
            GUILayout.Label("开关与滑块", DarkSkin.SLabel);

            _toggleValue = GUILayout.Toggle(_toggleValue, " 开关 Toggle", DarkSkin.SToggle);

            GUILayout.Label($"滑块值: {_sliderValue:F2}", DarkSkin.SLabel);
            _sliderValue = GUILayout.HorizontalSlider(_sliderValue, 0f, 1f);

            DarkSkin.Divider(4f);
            GUILayout.Label("输入框", DarkSkin.SLabel);

            GUILayout.Label("单行输入:", DarkSkin.SMuted);
            _textFieldValue = GUILayout.TextField(_textFieldValue, DarkSkin.SField);

            GUILayout.Label("多行输入:", DarkSkin.SMuted);
            _textAreaValue = GUILayout.TextArea(_textAreaValue, DarkSkin.SField, GUILayout.Height(80));
        }

        private void DrawLayoutTab()
        {
            GUILayout.Label("布局示例", DarkSkin.STitle);

            GUILayout.BeginHorizontal();
            GUILayout.Label("水平布局左", DarkSkin.SLabel);
            GUILayout.FlexibleSpace();
            GUILayout.Label("水平布局右", DarkSkin.SLabel);
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical(DarkSkin.SPanel);
            GUILayout.Label("面板 / Box 样式", DarkSkin.SLabel);
            GUILayout.Label("第二行文本", DarkSkin.SMuted);
            GUILayout.EndVertical();

            GUILayout.Label("交替行背景", DarkSkin.SLabel);
            GUILayout.BeginHorizontal(DarkSkin.SRow);
            GUILayout.Label("行 1", DarkSkin.SFeatureName);
            GUILayout.FlexibleSpace();
            GUILayout.Label("标签", DarkSkin.STag);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(DarkSkin.SRowAlt);
            GUILayout.Label("行 2", DarkSkin.SFeatureName);
            GUILayout.FlexibleSpace();
            GUILayout.Label("隐藏", DarkSkin.STagHidden);
            GUILayout.EndHorizontal();

            GUILayout.Label("详情面板", DarkSkin.SLabel);
            GUILayout.BeginVertical(DarkSkin.SDetail);
            GUILayout.Label("详情标题", DarkSkin.SDetailHead);
            GUILayout.Label("这里是详情内容，支持 <color=cyan>RichText</color> 高亮。", DarkSkin.SBonus);
            GUILayout.EndVertical();
        }

        private void DrawStatusTab()
        {
            GUILayout.Label("状态示例", DarkSkin.STitle);

            GUILayout.BeginHorizontal();
            GUILayout.Label("当前状态:", DarkSkin.SLabel);
            GUILayout.Label(_status, _status.Contains("失败") ? DarkSkin.SStatusErr : DarkSkin.SStatusOk);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("成功", DarkSkin.SBtnAdd))
                _status = "操作成功";
            if (GUILayout.Button("失败", DarkSkin.SBtnDel))
                _status = "操作失败";
            GUILayout.EndHorizontal();

            DarkSkin.Divider(4f);
            GUILayout.Label("类型标签", DarkSkin.SLabel);

            GUILayout.BeginHorizontal();
            GUILayout.Label("正面", DarkSkin.STypeGood);
            GUILayout.Label("负面", DarkSkin.STypeBad);
            GUILayout.Label("特殊", DarkSkin.STypeSpecial);
            GUILayout.Label("临时", DarkSkin.STypeTemp);
            GUILayout.EndHorizontal();

            DarkSkin.Divider(4f);
            GUILayout.Label("计数 / 名称按钮", DarkSkin.SLabel);
            GUILayout.BeginHorizontal();
            GUILayout.Label("数量:", DarkSkin.SLabel);
            GUILayout.Label("42", DarkSkin.SCount);
            GUILayout.EndHorizontal();

            if (GUILayout.Button($"<color=cyan>可点击名称 ({_clickCount})</color>", DarkSkin.SNameBtn))
            {
                _clickCount++;
                Log.Info($"可点击名称被点击，当前计数：{_clickCount}");
            }
        }

        private void DrawFormTab()
        {
            GUILayout.Label("表单控件", DarkSkin.STitle);

            GUILayout.Label("单选下拉菜单", DarkSkin.SLabel);
            _dropdownIndex = DarkSkin.Dropdown(_dropdownIndex, _dropdownOptions, ref _dropdownExpanded);

            DarkSkin.Divider(4f);
            GUILayout.Label("单项单选 Radio", DarkSkin.SLabel);
            _radioIndex = DarkSkin.RadioGroup(_radioIndex, "方案一", "方案二", "方案三");

            DarkSkin.Divider(4f);
            GUILayout.Label("表格 Table", DarkSkin.SLabel);

            // 表头
            GUILayout.BeginHorizontal(DarkSkin.SRow);
            for (int col = 0; col < _tableData.GetLength(1); col++)
            {
                GUILayout.Label(_tableData[0, col], DarkSkin.SDetailHead, GUILayout.Width(120));
            }
            GUILayout.EndHorizontal();

            // 数据行（点击可选中）
            for (int row = 1; row < _tableData.GetLength(0); row++)
            {
                var rowStyle = row == _selectedTableRow
                    ? DarkSkin.SRowSelected
                    : (row % 2 == 0 ? DarkSkin.SRow : DarkSkin.SRowAlt);

                GUILayout.BeginHorizontal(rowStyle);
                for (int col = 0; col < _tableData.GetLength(1); col++)
                {
                    GUILayout.Label(_tableData[row, col], DarkSkin.SLabel, GUILayout.Width(120));
                }
                GUILayout.EndHorizontal();

                var rowRect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.MouseDown && rowRect.Contains(Event.current.mousePosition))
                {
                    _selectedTableRow = row;
                    Event.current.Use();
                }
            }
        }
    }
}
