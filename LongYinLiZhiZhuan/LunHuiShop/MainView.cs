using LunHuiShop.GuiFramework.Controls;
using LunHuiShop.GuiFramework.Localization;
using LunHuiShop.GuiFramework.Logger;
using LunHuiShop.GuiFramework.Other;
using LunHuiShop.GuiFramework.Style;
using MelonLoader;
using UnityEngine;

namespace LunHuiShop;

[RegisterTypeInIl2Cpp]
public class MainView : MonoBehaviour
{
    private UI.WindowData _mainWindow;
    
    private void Start()
    {
        HttpGet.TryHit(this);
        
        _mainWindow = UI.NewWindow(
                new Rect(100, 100, 800, 680),
                Loc.Get("轮回商店"),
                DrawMainWindow)
            .Id(1)
            .Resizable()
            .Hide()
            .Build();
    }
    
    // 单选按钮
    private static readonly string[] ButtonRadioOptions =
    {
        Loc.Get("全部"), Loc.Get("装备"), Loc.Get("丹药"), Loc.Get("饮食"),
        Loc.Get("秘籍"),Loc.Get("珍宝"), Loc.Get("材料"), Loc.Get("马匹")
    };
    private int _buttonRadioIndex = 0;
    // 多选按钮
    private static readonly string[] EquipmentOptions =
    {
        Loc.Get("武器"), Loc.Get("护甲"), Loc.Get("头盔"), Loc.Get("足履"),Loc.Get("饰品")
    };
    private bool[] _checkboxValues = { true, false, false, false, false};
    
    
    private void DrawMainWindow(UI.WindowData window)
    {
        _buttonRadioIndex = UI.RadioButtonGroup
            .Selected(_buttonRadioIndex)
            .Options(ButtonRadioOptions)
            .ButtonStyle()
            .Horizontal()
            .Draw();

        if (_buttonRadioIndex == 1)
        {
            _checkboxValues = UI.CheckboxGroup
                .Options(EquipmentOptions)
                .Selected(_checkboxValues)
                .Horizontal()
                .Draw();
        }
        
        
        
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

   
}