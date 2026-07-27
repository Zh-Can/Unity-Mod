using UnityEngine;
using ZaoHuaMod.GuiFramework.Controls;
using UI = ZaoHuaMod.GuiFramework.Controls.UI;

namespace ZaoHuaMod
{
    public class MainView : MonoBehaviour
    {
        public static UI.WindowData SettingsWindow { get; private set; }
        public static UI.WindowData RefreshButtonWindow { get; private set; }

        private void Start()
        {
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

        private static void DrawSettingsContent(UI.WindowData window)
        {
            UI.Horizontal(() =>
            {
                ZaoHuaMod.ChooseCountFlag.Value = UI.Toggle("开局选择点数修改99开关")
                    .Value(ZaoHuaMod.ChooseCountFlag.Value)
                    .Draw();
                ZaoHuaMod.ZhCountFlag.Value = UI.Toggle("轮回商店9999点数")
                    .Value(ZaoHuaMod.ZhCountFlag.Value)
                    .Draw();
                
            });
            
            
            UI.Vertical(() =>
            {
               
                ZaoHuaMod.ChooseCountFlag.Value = UI.Toggle("开局选择点数修改99开关")
                    .Value(ZaoHuaMod.ChooseCountFlag.Value)
                    .Draw();
            
                UI.Space();
            
                ZaoHuaMod.ZhCountFlag.Value = UI.Toggle("轮回商店9999点数")
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
                    .Draw();
            
                UI.Space();
            
                ZaoHuaMod.MaxPlotCountFlag.Value = UI.Toggle("神器鼎地块扩增至100开关")
                    .Value(ZaoHuaMod.MaxPlotCountFlag.Value)
                    .Draw();
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
