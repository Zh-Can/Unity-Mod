using UnityEngine;
using ZaoHuaBMod.GuiFramework.Controls;
using ZaoHuaBMod.GuiFramework.Localization;
using ZaoHuaBMod.GuiFramework.Logger;
using ZaoHuaBMod.GuiFramework.Style;
using UI = ZaoHuaBMod.GuiFramework.Controls.UI;

namespace ZaoHuaBMod
{
    public class MainView : MonoBehaviour
    {
        private UI.WindowData _mainWindow;

        private int _tab;
        private bool _toggleValue;
        private bool _toggleValue1;
        private float _sliderValue = 0.5f;

        private int _dropdownIndex;
        private bool _dropdownExpanded;
        private int _radioIndex;
        private int _selectedTableRow = -1;
        private int _clickCount;
        private bool _foldoutExpanded;
       

        private void Start()
        {
            _mainWindow = UI.NewWindow(
                    new Rect(100, 100, 520, 680),
                    "ZaoHuaBMod",
                    Draw)
                .Resizable()
                .Show()
                .Build();
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
            UI.WindowControls.OnGUI();
        }

        private void Draw(UI.WindowData window)
        {
            UI.Horizontal(() =>
            {
                // 标签页芯片
                string[] tabs = { Loc.Get("基础"), Loc.Get("布局"), Loc.Get("状态"), Loc.Get("表单") };
                for (int i = 0; i < tabs.Length; i++)
                {
                    if (DarkSkin.TabChip(tabs[i], _tab == i))
                        _tab = i;
                }
            });
            UI.Divider();
            
            

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

            UI.FlexibleSpace();
            UI.Divider();
            GUILayout.Label($"{Loc.Get("缩放")}: {Mathf.RoundToInt(UI.WindowControls.Scale * 100f)}%  {Loc.Get("按 ` 键显示/隐藏")}", DarkSkin.SMuted);
        }

        private void DrawBasicTab()
        {
            UI.Label().AsTitle(Loc.Get("基础控件(标题)-AsTitle")).Draw();
            UI.Label().Text(Loc.Get("普通文本-Text")).Draw();
            UI.Label().AsHint(Loc.Get("灰色提示小字靠右排列-AsHint")).Draw();
            UI.Label().AsMuted(Loc.Get("灰色小字AsMuted")).Draw();
            UI.Label().AsCount(Loc.Get("蓝色小字文本AsCount")).Draw();
            UI.Label().Text(Loc.Get("带提示文本")).Tooltip(Loc.Get("鼠标放上来会显示 Tooltip")).Draw();
                
            UI.Divider(4f);
            
            UI.Label().Text(Loc.Get("按钮")).Draw();

            UI.Vertical(() =>
            {
                UI.Horizontal(() =>
                {
                    UI.Btn().Text("普通按钮").OnClick(() =>
                    {
                        Log.Info("普通按钮被点击");
                    }).Draw();
                    UI.Space();
                    UI.Btn().Add("添加按钮").OnClick(() =>
                    {
                        Log.Info("添加按钮被点击");
                    }).Draw();
                    UI.Space();
                    UI.Btn().Del("删除按钮").OnClick(() =>
                    {
                        Log.Info("删除按钮被点击");
                    }).Draw();
                });
            });
            UI.Label().Text(Loc.Get("单选按钮 Radio 竖3个")).Draw();
            _radioIndex = UI.RadioButtonGroup
                .Selected(_radioIndex)
                .Options(Loc.Get("方案一"), Loc.Get("方案二"), Loc.Get("方案三"))
                .Draw();
            UI.Label().Text(Loc.Get("单选按钮 Radio 横3个")).Draw();
            _radioIndex = UI.RadioButtonGroup
                .Selected(_radioIndex)
                .Options(Loc.Get("方案一"), Loc.Get("方案二"), Loc.Get("方案三"))
                .Horizontal()
                .Draw();

            DarkSkin.Divider(4f);
            
            UI.Label().Text(Loc.Get("开关与滑块")).Draw();
            _toggleValue = UI.Toggle.Text(Loc.Get("开关 Toggle"), _toggleValue).Draw();
            _toggleValue1 = UI.Toggle.Text(Loc.Get("带提示的开关 Toggle"), _toggleValue).Tooltip(Loc.Get("一个开关")).Draw();

            _sliderValue = UI.Slider(Loc.Get("滑块值"), _sliderValue, 0f, 1f, decimals: 1);
            UI.Space(3);

            DarkSkin.Divider(4f);

            UI.Label().Text(Loc.Get("输入框")).Draw();
            UI.Label().AsMuted(Loc.Get("单行输入:")).Draw();
            _input = UI.TextFiled(_input);
            UI.Label().AsMuted(Loc.Get("多行输入:")).Draw();
            _textArea = UI.TextArea(_textArea);
            
        }

        private string _input = "单行输入内容";
        private string _textArea = "多行输入区域\n第二行";
        
        private void DrawLayoutTab()
        {
            GUILayout.Label(Loc.Get("布局示例"), DarkSkin.STitle);

            GUILayout.BeginHorizontal();
            GUILayout.Label(Loc.Get("水平布局左"), DarkSkin.SLabel);
            GUILayout.FlexibleSpace();
            GUILayout.Label(Loc.Get("水平布局右"), DarkSkin.SLabel);
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical(DarkSkin.SPanel);
            GUILayout.Label(Loc.Get("面板 / Box 样式"), DarkSkin.SLabel);
            GUILayout.Label(Loc.Get("第二行文本"), DarkSkin.SMuted);
            GUILayout.EndVertical();

            GUILayout.Label(Loc.Get("交替行背景"), DarkSkin.SLabel);
            GUILayout.BeginHorizontal(DarkSkin.SRow);
            GUILayout.Label(Loc.Get("行 1"), DarkSkin.SFeatureName);
            GUILayout.FlexibleSpace();
            GUILayout.Label(Loc.Get("标签"), DarkSkin.STag);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(DarkSkin.SRowAlt);
            GUILayout.Label(Loc.Get("行 2"), DarkSkin.SFeatureName);
            GUILayout.FlexibleSpace();
            GUILayout.Label(Loc.Get("隐藏"), DarkSkin.STagHidden);
            GUILayout.EndHorizontal();

            GUILayout.Label(Loc.Get("详情面板"), DarkSkin.SLabel);
            GUILayout.BeginVertical(DarkSkin.SDetail);
            GUILayout.Label(Loc.Get("详情标题"), DarkSkin.SDetailHead);
            GUILayout.Label(Loc.Get("这里是详情内容，支持 <color=cyan>RichText</color> 高亮。"), DarkSkin.SBonus);
            GUILayout.EndVertical();
        }

        private string _status = Loc.Get(Loc.Get("就绪"));
        private void DrawStatusTab()
        {
            GUILayout.Label(Loc.Get("状态示例"), DarkSkin.STitle);

            GUILayout.BeginHorizontal();
            GUILayout.Label(Loc.Get("当前状态:"), DarkSkin.SLabel);
            GUILayout.Label(_status, _status.Contains("失败") ? DarkSkin.SStatusErr : DarkSkin.SStatusOk);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(Loc.Get("成功"), DarkSkin.SBtnAdd))
                _status = "操作成功";
            if (GUILayout.Button(Loc.Get("失败"), DarkSkin.SBtnDel))
                _status = "操作失败";
            GUILayout.EndHorizontal();

            DarkSkin.Divider(4f);
            GUILayout.Label(Loc.Get("类型标签"), DarkSkin.SLabel);

            GUILayout.BeginHorizontal();
            GUILayout.Label(Loc.Get("正面"), DarkSkin.STypeGood);
            GUILayout.Label(Loc.Get("负面"), DarkSkin.STypeBad);
            GUILayout.Label(Loc.Get("特殊"), DarkSkin.STypeSpecial);
            GUILayout.Label(Loc.Get("临时"), DarkSkin.STypeTemp);
            GUILayout.EndHorizontal();

            DarkSkin.Divider(4f);
            GUILayout.Label(Loc.Get("计数 / 名称按钮"), DarkSkin.SLabel);
            GUILayout.BeginHorizontal();
            GUILayout.Label(Loc.Get("数量:"), DarkSkin.SLabel);
            GUILayout.Label("42", DarkSkin.SCount);
            GUILayout.EndHorizontal();

            if (GUILayout.Button($"<color=cyan>{Loc.Get("可点击名称")} ({_clickCount})</color>", DarkSkin.SNameBtn))
            {
                _clickCount++;
            }
        }

        private void DrawFormTab()
        {
            GUILayout.Label(Loc.Get("表单控件"), DarkSkin.STitle);

            GUILayout.Label(Loc.Get("单选下拉菜单"), DarkSkin.SLabel);
            string[] dropdownOptions = { Loc.Get("选项 A"), Loc.Get("选项 B"), Loc.Get("选项 C"), Loc.Get("选项 D") };
            _dropdownIndex = DarkSkin.Dropdown(_dropdownIndex, dropdownOptions, ref _dropdownExpanded);

            DarkSkin.Divider(4f);
            GUILayout.Label(Loc.Get("折叠面板 Foldout"), DarkSkin.SLabel);
            _foldoutExpanded = DarkSkin.Foldout(_foldoutExpanded, Loc.Get("高级设置"));
            if (_foldoutExpanded)
            {
                GUILayout.BeginVertical(DarkSkin.SPanel);
                GUILayout.Label(Loc.Get("折叠区内容一"), DarkSkin.SLabel);
                GUILayout.Label(Loc.Get("折叠区内容二"), DarkSkin.SMuted);
                GUILayout.EndVertical();
            }

            DarkSkin.Divider(4f);
            GUILayout.Label(Loc.Get("表格 Table"), DarkSkin.SLabel);

            // 表头
            string[,] tableData =
            {
                { Loc.Get("名称"), Loc.Get("类型"), Loc.Get("数值") },
                { Loc.Get("物品一"), Loc.Get("武器"), "120" },
                { Loc.Get("物品二"), Loc.Get("防具"), "85"},
                { Loc.Get("物品三"), Loc.Get("道具"), "30"},
                { Loc.Get("物品四"), Loc.Get("材料"), "999" }
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
