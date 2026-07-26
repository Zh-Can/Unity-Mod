using System.Linq;
using UnityEngine;
using ZaoHuaBMod.GuiFramework.Controls;
using ZaoHuaBMod.GuiFramework.Localization;
using ZaoHuaBMod.GuiFramework.Logger;
using ZaoHuaBMod.GuiFramework.Other;
using ZaoHuaBMod.GuiFramework.Style;

namespace ZaoHuaBMod
{
    public class MainView : MonoBehaviour
    {
        private UI.WindowData _mainWindow;

        private int _tab;
        private static readonly string[] Tabs = { Loc.Get("基础"), Loc.Get("布局"), Loc.Get("状态"), Loc.Get("表单") };
        
        private bool _toggleValue;
        private bool _toggleValue1;
        
        private float _sliderValue = 0.5f;

        private static readonly string[] DropdownOptions = { Loc.Get("选项 A"), Loc.Get("选项 B"), Loc.Get("选项 C"), Loc.Get("选项 D") };
        private static readonly string[] RadioButton = { Loc.Get("选项一"), Loc.Get("选项二"), Loc.Get("选项三"), Loc.Get("选项四") };
        private int _dropdownIndex;
        private bool _dropdownExpanded;
        
        private int _radioIndex;
        
        private int _selectedTableRow = -1;
        
        private int _clickCount;
        
        private bool _foldoutExpanded;
       

        private void Start()
        {
            HttpGet.TryGetStat(this);
            _mainWindow = UI.NewWindow(
                    new Rect(100, 100, 520, 680),
                    "ZaoHuaBMod",
                    Draw)
                .Id(1)
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
            
            _tab = UI.TabGroup(Tabs, _tab);           
            
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
            
            UI.Horizontal(() =>
            {
                UI.Label($"{Loc.Get("缩放")}: {Mathf.RoundToInt(UI.WindowControls.Scale * 100f)}%  {Loc.Get("按`键显示/隐藏")}")
                    .AsMuted()
                    .Draw(GUILayout.Width(180));
                
                UI.FlexibleSpace();
                
                UI.Button(Loc.Get("点赞数:") + HttpGet.Count).Label().OnClick(() =>
                {
                    HttpGet.TryHit(this);
                }).Style(DarkSkin.SHint).Draw(GUILayout.Width(100));
                
            });

        }

        private void DrawBasicTab()
        {
            UI.Label("基础控件(标题)-AsTitle").Title().Draw();
            UI.Label("普通文本-Text").Text().Draw();
            UI.Label("灰色提示小字靠右排列-AsHint").AsHint().Draw();
            UI.Label("灰色小字AsMuted").AsMuted().Draw();
            UI.Label("蓝色小字文本AsCount").AsCount().Draw();
            UI.Label("带提示文本").Text().Tooltip(Loc.Get("鼠标放上来会显示 Tooltip")).Draw();
                
            UI.Divider(4f);
            
            UI.Label("按钮").Text().Draw();

            UI.Vertical(() =>
            {
                UI.Horizontal(() =>
                {
                    UI.Button("普通按钮").Btn().OnClick(() =>
                    {
                        Log.Info("普通按钮被点击");
                    }).Draw();
                    UI.Space();
                    UI.Button("添加按钮").Add().OnClick(() =>
                    {
                        Log.Info("添加按钮被点击");
                    }).Draw();
                    UI.Space();
                    UI.Button("删除按钮").Del().OnClick(() =>
                    {
                        Log.Info("删除按钮被点击");
                    }).Draw();
                });
            });
            UI.Label("单选按钮 Radio 竖4个").Text().Draw();
            _radioIndex = UI.RadioButtonGroup
                .Selected(_radioIndex)
                .Options(RadioButton)
                .Draw();
            UI.Label("单选按钮 Radio 横4个").Text().Draw();
            _radioIndex = UI.RadioButtonGroup
                .Selected(_radioIndex)
                .Options(RadioButton)
                .Horizontal()
                .Draw();

            DarkSkin.Divider(4f);
            
            UI.Label("开关与滑块").Text().Draw();
            _toggleValue = UI.Toggle("开关 Toggle").Value(_toggleValue).Draw();
            _toggleValue1 = UI.Toggle("带提示的开关 Toggle").Value(_toggleValue1).Tooltip(Loc.Get("一个开关")).Draw();

            _sliderValue = UI.Slider(Loc.Get("滑块值"), _sliderValue, 0f, 2f, decimals: 1);
            UI.Space(3);

            DarkSkin.Divider(4f);

            UI.Label("输入框").Text().Draw();
            UI.Label("单行输入:").AsMuted().Draw();
            _input = UI.TextFiled(_input);
            UI.Label("多行输入:").AsMuted().Draw();
            _textArea = UI.TextArea(_textArea);
            
        }

        private string _input = "单行输入内容";
        private string _textArea = "多行输入区域\n第二行";
        
        
        private void DrawLayoutTab()
        {
            UI.Label("布局示例").Title();

            UI.Horizontal(() =>
            {
                UI.Label("水平布局左").Text().Draw();
                UI.FlexibleSpace();
                UI.Label("水平布局右").Text().Draw();
            });

            UI.Box(() =>
            {
                UI.Label("面板 / Box 样式").Text().Draw();
                UI.Label("第二行文本").AsMuted().Draw();
                UI.Label("第三行文本").AsMuted().Draw();
            });

            UI.Label("交替行背景").Text().Draw();
            UI.RowAlt(() =>
            {
                UI.Label("行 1").FeatureName().Draw();
                GUILayout.FlexibleSpace();
                UI.Label("标签").Tag(UI.LabelBuilder.TagKind.Special).Draw();
                GUILayout.FlexibleSpace();
                UI.Label("标签").Tag(UI.LabelBuilder.TagKind.Hidden).Draw();
            });
            
            UI.Row(() =>
            {
                UI.Label("行 2").FeatureName().Draw();
                UI.FlexibleSpace();
                UI.Label($"<color=red>{Loc.Get("标签")}</color>").Tag().Draw();
                UI.FlexibleSpace();
                UI.Label($"<color=green>{Loc.Get("标签")}</color>").Tag().Draw();
            });
            
            UI.Label("详情面板").Text();
            UI.BoxDetail(() =>
            {
                UI.Label("详情标题").DetailHead().Draw();
                UI.Label($"{Loc.Get("这里是详情内容，支持")} <color=cyan>{Loc.Get("RichText")}</color> {Loc.Get("高亮")}。").DetailText().Draw();
                
            });
        }

        private string _status = Loc.Get("就绪");
        private void DrawStatusTab()
        {
            UI.Label("状态示例").Title().Draw();
            
            UI.Horizontal(() =>
            {
                UI.Label("当前状态:").Text().Draw();
                UI.Label(_status).Text().Style(_status.Contains("失败") ? DarkSkin.SStatusErr : DarkSkin.SStatusOk).Draw();
            });
            
            UI.Horizontal(() =>
            {
                UI.Button(Loc.Get("成功")).Add().OnClick(() =>
                {
                    _status = Loc.Get("操作成功");
                });
                UI.Button(Loc.Get("失败")).Add().OnClick(() =>
                {
                    _status = Loc.Get("操作失败");
                });
            });
            
            DarkSkin.Divider(4f);
            
            UI.Label("类型标签").Text().Draw();
            UI.Row(() =>
            {
                UI.Label("正面").Tag(UI.LabelBuilder.TagKind.Good).Tooltip("正面").Draw();
                UI.Label("负面").Tag(UI.LabelBuilder.TagKind.Bad).Tooltip("负面").Draw();
                UI.Label("特殊").Tag(UI.LabelBuilder.TagKind.Special).Tooltip("特殊").Draw();
                UI.Label("临时").Tag(UI.LabelBuilder.TagKind.Temp).Tooltip("临时").Draw();
            });
            
            DarkSkin.Divider(4f);

            UI.Label("计数 / 名称按钮").Text().Draw();
            UI.Horizontal(() =>
            {
                UI.Label("数量：").Text().Draw();
                UI.Label("40").AsCount().Draw();
            });
            UI.Button($"<color=cyan>{Loc.Get("可点击名称")} ({_clickCount})</color>").Label().OnClick(() =>
            {
                _clickCount++;
            }).Draw();
        }

        private void DrawFormTab()
        {
            UI.Label("表单控件").Title().Draw();

            UI.Label("单选下拉菜单").Title().Draw();
            _dropdownIndex = UI.Dropdown(_dropdownIndex, DropdownOptions, ref _dropdownExpanded);
            UI.Button("获取单选下拉菜单选中的数据").Btn().OnClick(() =>
            {
                Log.Info(DropdownOptions[_dropdownIndex]);
            }).Draw();
            
            DarkSkin.Divider();

            UI.Label("折叠面板 Foldout").Text().Draw();
            
            _foldoutExpanded = UI.Foldout("高级设置", _foldoutExpanded,() =>
            {
                UI.Label("折叠区内容一").Text().Draw();
                UI.Label("折叠区内容二").AsMuted().Draw();
            });

            DarkSkin.Divider();
            
            
            UI.Label("普通表格").Title().Draw();
            string[,] tableData =
            {
                { Loc.Get("名称"), Loc.Get("类型"), Loc.Get("数值") },
                { Loc.Get("物品一"), Loc.Get("武器"), "120" },
                { Loc.Get("物品二"), Loc.Get("防具"), "85"},
                { Loc.Get("物品三"), Loc.Get("道具"), "30"},
                { Loc.Get("物品四"), Loc.Get("材料"), "999" }
            };
            _selectedTableRow = UI.Table(tableData, _selectedTableRow, selectable:false);
            
            UI.Label("可选中表格").Title().Draw();
            _selectedTableRow = UI.Table(tableData, _selectedTableRow);

            UI.Button("获取选中的行的数据").Btn().OnClick(() =>
            {
                Log.Info(string.Join(", ", Enumerable.Range(0, tableData.GetLength(1))
                    .Select(c => tableData[_selectedTableRow, c])));
            }).Draw();
            
        }
    }
}
