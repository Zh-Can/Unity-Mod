using System.Globalization;
using Il2Cpp;
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
    private WindowData _mainWindow;
    private string _status = "";
    private Vector2 _tableScrollPos;
    
    private void Awake()
    {
        HttpGet.TryHit(this);
        
        // 如果有表格， 5个column * 120 + 50
        _mainWindow = UI.NewWindow(
                new Rect(100, 100, 650, 700),
                Loc.Get("轮回商店"),
                DrawMainWindow)
            .Id(1)
            .Hide()
            .Footer(DrawStatusBar)
            .Build();
    }
    
    // 物品分类
    private static readonly string[] ItemTypeRadioOptions =
    {
        Loc.Get("全部"), Loc.Get("装备"), Loc.Get("丹药"), Loc.Get("饮食"), Loc.Get("秘籍"), 
        Loc.Get("珍宝"), Loc.Get("材料"), Loc.Get("马匹")
    };
    private int _itemTypeRadioIndex;
    
     
    private int _itemLevelRadioIndex;
    
        // $"<color=#df2c45>{Loc.Get("极品")}</color>",
        // $"<color=#e08d07>{Loc.Get("珍品")}</color>",
        // $"<color=#9c7fe0>{Loc.Get("上品")}</color>",
        // $"<color=#307ede>{Loc.Get("中品")}</color>",
        // $"<color=#7ec00b>{Loc.Get("下品")}</color>",
        // $"<color=#949494>{Loc.Get("残品")}</color>" 
   
    private int _itemQualityRadioIndex;
    
        // $"<color=#df2c45>{Loc.Get("完本")}</color>",
        // $"<color=#e08d07>{Loc.Get("珍本")}</color>",
        // $"<color=#9c7fe0>{Loc.Get("古本")}</color>",
        // $"<color=#307ede>{Loc.Get("善本")}</color>",
        // $"<color=#7ec00b>{Loc.Get("仿本")}</color>",
        // $"<color=#949494>{Loc.Get("残本")}</color>" 
    
    private int _bookQualityRadioIndex;
    // 装备类型
    private static readonly string[] EquipmentOptions =
    {
        Loc.Get("武器"), Loc.Get("护甲"), Loc.Get("头盔"), Loc.Get("足履"),Loc.Get("饰品")
    };
    private int _equipmentRadioIndex;
    // 秘籍类型
    private static readonly string[] BookOptions =
    {
        Loc.Get("内功"), Loc.Get("轻功"), Loc.Get("绝技"), Loc.Get("拳掌"),Loc.Get("剑法"),
        Loc.Get("刀法"),Loc.Get("长兵"),Loc.Get("奇门"),Loc.Get("射术")
    };
    private int _bookRadioIndex;
    
    // 表格选中项
    private int _selectedTableRow;
    public static List<ShopItem> ShopItems = null!;

    private void DrawMainWindow()
    {
        // 物品分类
        _itemTypeRadioIndex = UI.RadioButtonGroup
            .Selected(_itemTypeRadioIndex)
            .Options(ItemTypeRadioOptions)
            .ButtonStyle()
            .Horizontal()
            .Draw();
        UI.Space(5);
        
        // 装备类型
        if (_itemTypeRadioIndex == 1)
        {
            _equipmentRadioIndex = UI.RadioButtonGroup
                .Options(EquipmentOptions)
                .Selected(_equipmentRadioIndex)
                .ButtonStyle()
                .Horizontal()
                .Draw();
            UI.Space(5);
        }
        // 秘籍
        if (_itemTypeRadioIndex == 4)
        {
           
        }
        
        
        
        UI.Horizontal(() =>
        {
            UI.Label(_status).Text().Draw(GUILayout.ExpandWidth(true));
            UI.TextFiled("", placeholder: "搜索", options: GUILayout.Width(150));
            UI.Button("购买").Add().OnClick(Buy).Draw(GUILayout.Width(80));
        });
        
        UI.Divider();
        
        // 可选表格
        _selectedTableRow = UI.Table(
            ShopItems,
            item => new[] { item.Name, item.Type, item.Price.ToString(), item.Fame.ToString(CultureInfo.InvariantCulture) },
            _selectedTableRow,
            new[] { Loc.Get("名称"), Loc.Get("类型"), Loc.Get("需要银两"), Loc.Get("需要声望") },
            showIndex: true,
            onDrawFirstCell: (rect, data, text) => IconHelper.DrawCellWithIcon(rect, data.IconName, text),
            scrollPosition: ref _tableScrollPos,
            scrollHeight: 440);
    }

    private void DrawStatusBar()
    {
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
            _mainWindow.Visible = !_mainWindow.Visible;
        }
    }

    private void OnGUI()
    {
        UI.WindowControls.OnGUI();
    }

    /// <summary>
    /// 初始化商店数据
    /// </summary>
    private void InitShopData()
    {
        // ShopItems 由 GamePatches.GameDataController_Awake_Postfix 填充
        // 如果 Patch 还没执行，先放空列表
        if (ShopItems == null)
            ShopItems = new List<ShopItem>();
    }
    /// <summary>
    /// 购买
    /// 先检查玩家的钱和声望
    /// </summary>
    private void Buy()
    {
        if (_selectedTableRow < 0 || _selectedTableRow >= ShopItems.Count || 
            !LyHelper.TryReadPlayer(out var player))
            return;

        var selectedItem = ShopItems[_selectedTableRow];
        
        if (player.fame < selectedItem.Fame)
        {
            _status = "提示：<color=red>声望不够</color>";
            return;
        }

        if (player.itemListData.money < selectedItem.Price)
        {
            _status = "提示：<color=yellow>银钱不够</color>";
            return;
        }
        _status = "提示：<color=green>购买成功</color>";
        // Log.Info(selectedItem.Name);
        
    }
}