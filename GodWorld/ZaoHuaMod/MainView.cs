using UnityEngine;
using ZaoHuaMod.GuiFramework.Localization;
using ZaoHuaMod.GuiFramework.Other;
using ZaoHuaMod.GuiFramework.Style;
using UI = ZaoHuaMod.GuiFramework.Controls.UI;

namespace ZaoHuaMod
{
    public class MainView : MonoBehaviour
    {
        private static UI.WindowData SettingsWindow { get; set; }
        public static UI.WindowData RefreshButtonWindow { get; private set; }

        /// <summary>生长速度输入框文本（在绘制间保持，避免输入被回填打断）。</summary>
        private string _growSpeedInput;
        private string _productionInput;

        private void Start()
        {
            HttpGet.TryHit(this);
            // 创建设置窗口
            SettingsWindow = UI.NewWindow(
                new Rect(50, 50, 450, 400),
                "ZaoHuaMod",
                DrawSettingsContent
            )
            .Id(20260718)
            .Resizable()
            .MinSize(new Vector2(350, 350))
            .Hide()
            .Build();
            
            // 创建刷新按钮窗口（无标题）
            RefreshButtonWindow = UI.NewWindow(
                new Rect(Screen.width / 5f, Screen.height / 6.1f, 130, 80),
                "",
                DrawRefreshContent
            )
            .Id(20260719)
            .DragBy(UI.WindowData.DragMode.WholeWindow)
            .Hide()
            .Build();
            
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                if (SettingsWindow != null)
                {
                    if (SettingsWindow.Visible)
                        SettingsWindow.Hide();
                    else
                        SettingsWindow.Show();
                }
            }
        }

        private void OnGUI()
        {
            UI.WindowControls.OnGUI();
        }

        private void DrawSettingsContent(UI.WindowData window)
        {
            UI.Vertical(() =>
            {
                ZaoHuaMod.ChooseCountFlag.Value = UI.Toggle("开局选择点数修改99开关")
                    .Value(ZaoHuaMod.ChooseCountFlag.Value)
                    .OnChange(v =>
                    {
                        ZaoHuaMod.ChooseCountFlag.Value = v;
                        ZaoHuaMod.SaveConfig();
                    })
                    .Draw();

                UI.Space();

                ZaoHuaMod.ZhCountFlag.Value = UI.Toggle("轮回商店9999点数开关")
                    .Value(ZaoHuaMod.ZhCountFlag.Value)
                    .OnChange(v =>
                    {
                        ZaoHuaMod.ZhCountFlag.Value = v;
                        ZaoHuaMod.SaveConfig();
                    })
                    .Draw();

                UI.Space();
                
                ZaoHuaMod.AllSkillFlag.Value = UI.Toggle("炼丹解锁两列的技能开关")
                    .Value(ZaoHuaMod.AllSkillFlag.Value)
                    .OnChange(v =>
                    {
                        ZaoHuaMod.AllSkillFlag.Value = v;
                        ZaoHuaMod.SaveConfig();
                    })
                    .Draw();

                UI.Space();

                ZaoHuaMod.MaxPlotCountFlag.Value = UI.Toggle("神器鼎地块扩增至100开关")
                    .Value(ZaoHuaMod.MaxPlotCountFlag.Value)
                    .OnChange(v =>
                    {
                        ZaoHuaMod.MaxPlotCountFlag.Value = v;
                        ZaoHuaMod.SaveConfig();
                    })
                    .Draw();
                
                UI.Space();
                
                ZaoHuaMod.BuildStoFlag.Value = UI.Toggle("神器鼎地块建筑范围全覆盖开关")
                    .Value(ZaoHuaMod.BuildStoFlag.Value)
                    .OnChange(v =>
                    {
                        ZaoHuaMod.BuildStoFlag.Value = v;
                        ZaoHuaMod.SaveConfig();
                    })
                    .Draw();

                UI.Space();

                ZaoHuaMod.DrugProfitLabelFlag.Value = UI.Toggle("丹药详情展示丹药价格")
                    .Value(ZaoHuaMod.DrugProfitLabelFlag.Value)
                    .OnChange(v =>
                    {
                        ZaoHuaMod.DrugProfitLabelFlag.Value = v;
                        ZaoHuaMod.SaveConfig();
                    })
                    .Draw();
                
                UI.Space();
                
                //灵泉/火炼池 生长速度
                UI.Horizontal(() =>
                {
                    UI.Label("生长速度修改：").Tooltip("灵泉/火炼池影响：原倍率是速度+1倍。默认值：1").Draw(GUILayout.Width(100));
                    // 输入框状态在绘制间保持；首次绘制时以当前配置值为初值
                    if (_growSpeedInput == null) _growSpeedInput = ZaoHuaMod.GrowSpeedMultiplier.Value.ToString();
                    _growSpeedInput = UI.TextFiled(_growSpeedInput, options: GUILayout.Width(70));
                    UI.Space();
                    UI.Button("设置").Btn().OnClick(() =>
                    {
                        GUIUtility.keyboardControl = 0;
                        if (int.TryParse(_growSpeedInput, out var v) && v > 0)
                        {
                            ZaoHuaMod.GrowSpeedMultiplier.Value = v;
                            ZaoHuaMod.SaveConfig();
                            Singleton<PastureImpl>.Instance.RefreshPastureEffect();
                        }
                        else
                        {
                            _growSpeedInput = ZaoHuaMod.GrowSpeedMultiplier.Value.ToString();
                        }
                    }).Draw(GUILayout.Width(80));
                });

                UI.Space();

                UI.Horizontal(() =>
                {
                    UI.Label("产量修改：").Tooltip("灵枢台影响：原倍率是产量+1倍。默认值：1").Draw(GUILayout.Width(80));
                    // 输入框状态在绘制间保持；首次绘制时以当前配置值为初值
                    if (_productionInput == null) _productionInput = ZaoHuaMod.CountMultiplier.Value.ToString();
                    _growSpeedInput = UI.TextFiled(_growSpeedInput, options: GUILayout.Width(70));
                    UI.Space();
                    UI.Button("设置").Btn().OnClick(() =>
                    {
                        GUIUtility.keyboardControl = 0;
                        if (int.TryParse(_growSpeedInput, out var v) && v > 0)
                        {
                            ZaoHuaMod.CountMultiplier.Value = v;
                            ZaoHuaMod.SaveConfig();
                            Singleton<PastureImpl>.Instance.RefreshPastureEffect();
                        }
                        else
                        {
                            _growSpeedInput = ZaoHuaMod.CountMultiplier.Value.ToString();
                        }
                    }).Draw(GUILayout.Width(80));
                });
                

                UI.Space();

                float newJuLingMul = UI.Slider("聚灵台 增幅倍率", ZaoHuaMod.JuLingMultiplier.Value, 1f, 100f, 0);
                if (!Mathf.Approximately(newJuLingMul, ZaoHuaMod.JuLingMultiplier.Value))
                {
                    ZaoHuaMod.JuLingMultiplier.Value = newJuLingMul;
                    ZaoHuaMod.SaveConfig();
                    Singleton<PastureImpl>.Instance.RefreshPastureEffect();
                }

                UI.Space();

                float newLingChiMul = UI.Slider("灵池 灵鱼成长倍率", ZaoHuaMod.LingChiMultiplier.Value, 1f, 100f, 0);
                if (!Mathf.Approximately(newLingChiMul, ZaoHuaMod.LingChiMultiplier.Value))
                {
                    ZaoHuaMod.LingChiMultiplier.Value = newLingChiMul;
                    ZaoHuaMod.SaveConfig();
                }
            });
           
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

        private static void DrawRefreshContent(UI.WindowData window)
        {
            UI.Button("刷新").OnClick(() =>
            {
                if (ZaoHuaMod.RefreshType == "Trade")
                    ZaoHuaMod.Instance.RefreshTrades();
                else if (ZaoHuaMod.RefreshType == "DeaconTask")
                    ZaoHuaMod.Instance.RefreshDeaconTasks();
            }).Draw(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        }
    }
}
