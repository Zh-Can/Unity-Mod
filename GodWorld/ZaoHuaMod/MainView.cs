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

        private void Start()
        {
            HttpGet.TryHit(this);
            // 创建设置窗口
            SettingsWindow = UI.NewWindow(
                new Rect(50, 50, 450, 200),
                "ZaoHuaMod",
                DrawSettingsContent
            )
            .Id(20260718)
            .Resizable()
            .MinSize(new Vector2(350, 150))
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

                ZaoHuaMod.DrugResistLabelFlag.Value = UI.Toggle("没吃满的丹药显示吃了多少丹药")
                    .Value(ZaoHuaMod.DrugResistLabelFlag.Value)
                    .OnChange(v =>
                    {
                        ZaoHuaMod.DrugResistLabelFlag.Value = v;
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
