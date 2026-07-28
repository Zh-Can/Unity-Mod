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
    
    // 物品分类
    private static readonly string[] ButtonRadioOptions =
    {
        Loc.Get("全部"), Loc.Get("装备"), Loc.Get("丹药"), Loc.Get("饮食"),
        Loc.Get("秘籍"),Loc.Get("珍宝"), Loc.Get("材料"), Loc.Get("马匹")
    };
    private int _buttonRadioIndex = 0;
    // 物品等级多选
    private static readonly string[] ItemLevelOptions =
    {
        $"<color=#df2c45>{Loc.Get("绝世")}</color>",
        $"<color=#e08d07>{Loc.Get("完美")}</color>",
        $"<color=#9c7fe0>{Loc.Get("精良")}</color>",
        $"<color=#307ede>{Loc.Get("优质")}</color>",
        $"<color=#7ec00b>{Loc.Get("普通")}</color>",
        $"<color=#949494>{Loc.Get("劣质")}</color>" 
    };
    private bool[] _itemLevelChecks = { true, false, false, false, false, false };
    // 装备类型多选
    private static readonly string[] EquipmentOptions =
    {
        Loc.Get("武器"), Loc.Get("护甲"), Loc.Get("头盔"), Loc.Get("足履"),Loc.Get("饰品")
    };
    private bool[] _equipmentChecks = { true, false, false, false, false};
    // 秘籍类型多选
    private static readonly string[] BookOptions =
    {
        Loc.Get("内功"), Loc.Get("轻功"), Loc.Get("绝技"), Loc.Get("拳掌"),Loc.Get("剑法")
        ,Loc.Get("刀法"),Loc.Get("长兵"),Loc.Get("奇门"),Loc.Get("射术")
    };
    private bool[] _bookChecks = { true, false, false, false, false, false, false, false, false};
    
    private void DrawMainWindow(UI.WindowData window)
    {
        // 物品分类单选
        _buttonRadioIndex = UI.RadioButtonGroup
            .Selected(_buttonRadioIndex)
            .Options(ButtonRadioOptions)
            .ButtonStyle()
            .Horizontal()
            .Draw();
        UI.Space(5);
        // 物品等级多选
        _itemLevelChecks = UI.CheckboxGroup
            .Options(ItemLevelOptions)
            .Selected(_itemLevelChecks)
            .Horizontal()
            .Draw();
        UI.Space(5);
        // 装备类型多选
        if (_buttonRadioIndex == 1)
        {
            _equipmentChecks = UI.CheckboxGroup
                .Options(EquipmentOptions)
                .Selected(_equipmentChecks)
                .Horizontal()
                .Draw();
        }
        UI.Space(5);
        // 秘籍多选
        if (_buttonRadioIndex == 4)
        {
            _bookChecks = UI.CheckboxGroup
                .Options(BookOptions)
                .Selected(_bookChecks)
                .Horizontal()
                .Draw();
        }
        UI.Space(5);
        
        
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