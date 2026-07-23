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

        private int _dropdownIndex;
        private bool _dropdownExpanded;
        private int _radioIndex;
        private int _selectedTableRow = -1;
        private int _clickCount;

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
            
            string[] tabs = { Localization.Get("基础"), Localization.Get("布局"), Localization.Get("状态"), Localization.Get("表单") };
            for (int i = 0; i < tabs.Length; i++)
            {
                if (DarkSkin.TabChip(tabs[i], _tab == i))
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
            GUILayout.Label($"{Localization.Get("缩放")}: {Mathf.RoundToInt(GUIManager.Instance.Scale * 100f)}%  {Localization.Get("按 ` 键显示/隐藏")}", DarkSkin.SMuted);
        }

        private void DrawBasicTab()
        {
            GUILayout.Label(Localization.Get("基础控件"), DarkSkin.STitle);

            GUILayout.Label(Localization.Get("普通文本 Label"), DarkSkin.SLabel);
            GUILayout.Label(new GUIContent(Localization.Get("带提示文本"), Localization.Get("鼠标放上来会显示 Tooltip")), DarkSkin.SLabel);

            DarkSkin.Divider(4f);
            GUILayout.Label(Localization.Get("按钮"), DarkSkin.SLabel);

            if (GUILayout.Button(Localization.Get("普通按钮"), DarkSkin.SBtn))
                Log.Info("普通按钮被点击");

            if (GUILayout.Button(Localization.Get("添加按钮"), DarkSkin.SBtnAdd))
                Log.Info("添加按钮被点击");

            if (GUILayout.Button(Localization.Get("删除按钮"), DarkSkin.SBtnDel))
                Log.Info("删除按钮被点击");

            DarkSkin.Divider(4f);
            GUILayout.Label(Localization.Get("开关与滑块"), DarkSkin.SLabel);

            _toggleValue = GUILayout.Toggle(_toggleValue, Localization.Get(" 开关 Toggle"), DarkSkin.SToggle);

            GUILayout.Label($"{Localization.Get("滑块值")}: {_sliderValue:F2}", DarkSkin.SLabel);
            _sliderValue = GUILayout.HorizontalSlider(_sliderValue, 0f, 1f);

            DarkSkin.Divider(4f);
            GUILayout.Label(Localization.Get("输入框"), DarkSkin.SLabel);

            GUILayout.Label(Localization.Get("单行输入:"), DarkSkin.SMuted);
            GUILayout.TextField(Localization.Get("输入文字"), DarkSkin.SField);

            GUILayout.Label(Localization.Get("多行输入:"), DarkSkin.SMuted);
            GUILayout.TextArea(Localization.Get("多行输入区域") + "\n" + Localization.Get("第二行"), DarkSkin.SField, GUILayout.Height(80));
        }

        private void DrawLayoutTab()
        {
            GUILayout.Label(Localization.Get("布局示例"), DarkSkin.STitle);

            GUILayout.BeginHorizontal();
            GUILayout.Label(Localization.Get("水平布局左"), DarkSkin.SLabel);
            GUILayout.FlexibleSpace();
            GUILayout.Label(Localization.Get("水平布局右"), DarkSkin.SLabel);
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical(DarkSkin.SPanel);
            GUILayout.Label(Localization.Get("面板 / Box 样式"), DarkSkin.SLabel);
            GUILayout.Label(Localization.Get("第二行文本"), DarkSkin.SMuted);
            GUILayout.EndVertical();

            GUILayout.Label(Localization.Get("交替行背景"), DarkSkin.SLabel);
            GUILayout.BeginHorizontal(DarkSkin.SRow);
            GUILayout.Label(Localization.Get("行 1"), DarkSkin.SFeatureName);
            GUILayout.FlexibleSpace();
            GUILayout.Label(Localization.Get("标签"), DarkSkin.STag);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(DarkSkin.SRowAlt);
            GUILayout.Label(Localization.Get("行 2"), DarkSkin.SFeatureName);
            GUILayout.FlexibleSpace();
            GUILayout.Label(Localization.Get("隐藏"), DarkSkin.STagHidden);
            GUILayout.EndHorizontal();

            GUILayout.Label(Localization.Get("详情面板"), DarkSkin.SLabel);
            GUILayout.BeginVertical(DarkSkin.SDetail);
            GUILayout.Label(Localization.Get("详情标题"), DarkSkin.SDetailHead);
            GUILayout.Label(Localization.Get("这里是详情内容，支持 <color=cyan>RichText</color> 高亮。"), DarkSkin.SBonus);
            GUILayout.EndVertical();
        }

        private void DrawStatusTab()
        {
            GUILayout.Label(Localization.Get("状态示例"), DarkSkin.STitle);

            GUILayout.BeginHorizontal();
            GUILayout.Label(Localization.Get("当前状态:"), DarkSkin.SLabel);
            string status = Localization.Get(Localization.Get("就绪"));
            GUILayout.Label(status, status.Contains("失败") ? DarkSkin.SStatusErr : DarkSkin.SStatusOk);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(Localization.Get("成功"), DarkSkin.SBtnAdd))
                status = "操作成功";
            if (GUILayout.Button(Localization.Get("失败"), DarkSkin.SBtnDel))
                status = "操作失败";
            GUILayout.EndHorizontal();

            DarkSkin.Divider(4f);
            GUILayout.Label(Localization.Get("类型标签"), DarkSkin.SLabel);

            GUILayout.BeginHorizontal();
            GUILayout.Label(Localization.Get("正面"), DarkSkin.STypeGood);
            GUILayout.Label(Localization.Get("负面"), DarkSkin.STypeBad);
            GUILayout.Label(Localization.Get("特殊"), DarkSkin.STypeSpecial);
            GUILayout.Label(Localization.Get("临时"), DarkSkin.STypeTemp);
            GUILayout.EndHorizontal();

            DarkSkin.Divider(4f);
            GUILayout.Label(Localization.Get("计数 / 名称按钮"), DarkSkin.SLabel);
            GUILayout.BeginHorizontal();
            GUILayout.Label(Localization.Get("数量:"), DarkSkin.SLabel);
            GUILayout.Label("42", DarkSkin.SCount);
            GUILayout.EndHorizontal();

            if (GUILayout.Button($"<color=cyan>{Localization.Get("可点击名称")} ({_clickCount})</color>", DarkSkin.SNameBtn))
            {
                _clickCount++;
            }
        }

        private void DrawFormTab()
        {
            GUILayout.Label(Localization.Get("表单控件"), DarkSkin.STitle);

            GUILayout.Label(Localization.Get("单选下拉菜单"), DarkSkin.SLabel);
            string[] dropdownOptions = { Localization.Get("选项 A"), Localization.Get("选项 B"), Localization.Get("选项 C"), Localization.Get("选项 D") };
            _dropdownIndex = DarkSkin.Dropdown(_dropdownIndex, dropdownOptions, ref _dropdownExpanded);

            DarkSkin.Divider(4f);
            GUILayout.Label(Localization.Get("单项单选 Radio"), DarkSkin.SLabel);
            _radioIndex = DarkSkin.RadioGroup(_radioIndex, Localization.Get("方案一"), Localization.Get("方案二"), Localization.Get("方案三"));

            DarkSkin.Divider(4f);
            GUILayout.Label(Localization.Get("表格 Table"), DarkSkin.SLabel);

            // 表头
            string[,] tableData =
            {
                { Localization.Get("名称"), Localization.Get("类型"), Localization.Get("数值") },
                { Localization.Get("物品一"), Localization.Get("武器"), "120" },
                { Localization.Get("物品二"), Localization.Get("防具"), "85"},
                { Localization.Get("物品三"), Localization.Get("道具"), "30"},
                { Localization.Get("物品四"), Localization.Get("材料"), "999" }
            };
            GUILayout.BeginHorizontal(DarkSkin.SRow);
            for (int col = 0; col < tableData.GetLength(1); col++)
            {
                GUILayout.Label(tableData[0, col], DarkSkin.SDetailHead, GUILayout.Width(120));
            }
            GUILayout.EndHorizontal();

            // 数据行（点击可选中）
            for (int row = 1; row < tableData.GetLength(0); row++)
            {
                var rowStyle = row == _selectedTableRow
                    ? DarkSkin.SRowSelected
                    : (row % 2 == 0 ? DarkSkin.SRow : DarkSkin.SRowAlt);

                GUILayout.BeginHorizontal(rowStyle);
                for (int col = 0; col < tableData.GetLength(1); col++)
                {
                    GUILayout.Label(tableData[row, col], DarkSkin.SLabel, GUILayout.Width(120));
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
