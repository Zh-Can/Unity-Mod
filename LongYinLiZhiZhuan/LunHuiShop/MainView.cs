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
    
    private void Awake()
    {
        HttpGet.TryHit(this);
        InitShopData();
        // 如果有表格， x个column * 120 + ScrollView 左右内边距 2+2=4 + 垂直滚动条宽度8
        _mainWindow = UI.NewWindow(
                new Rect(100, 100, 888, 680),
                Loc.Get("轮回商店"),
                DrawMainWindow)
            .Id(1)
            .Hide()
            .Build();
    }
    
    // 物品分类
    private static readonly string[] ItemTypeRadioOptions =
    {
        Loc.Get("全部"), Loc.Get("装备"), Loc.Get("丹药"), Loc.Get("饮食"),
        Loc.Get("秘籍"),Loc.Get("珍宝"), Loc.Get("材料"), Loc.Get("马匹")
    };
    private int _itemTypeRadioIndex;
    // 物品等级
    private static readonly string[] ItemLevelOptions =
    {
        $"<color=#df2c45>{Loc.Get("绝世")}</color>",
        $"<color=#e08d07>{Loc.Get("完美")}</color>",
        $"<color=#9c7fe0>{Loc.Get("精良")}</color>",
        $"<color=#307ede>{Loc.Get("优质")}</color>",
        $"<color=#7ec00b>{Loc.Get("普通")}</color>",
        $"<color=#949494>{Loc.Get("劣质")}</color>" 
    };
    private int _itemLevelRadioIndex;
    // 物品品质
    private static readonly string[] ItemQualityOptions =
    {
        $"<color=#df2c45>{Loc.Get("极品")}</color>",
        $"<color=#e08d07>{Loc.Get("珍品")}</color>",
        $"<color=#9c7fe0>{Loc.Get("上品")}</color>",
        $"<color=#307ede>{Loc.Get("中品")}</color>",
        $"<color=#7ec00b>{Loc.Get("下品")}</color>",
        $"<color=#949494>{Loc.Get("残品")}</color>" 
    };
    private int _itemQualityRadioIndex;
    // 秘籍品质
    private static readonly string[] BookQualityOptions =
    {
        $"<color=#df2c45>{Loc.Get("完本")}</color>",
        $"<color=#e08d07>{Loc.Get("珍本")}</color>",
        $"<color=#9c7fe0>{Loc.Get("古本")}</color>",
        $"<color=#307ede>{Loc.Get("善本")}</color>",
        $"<color=#7ec00b>{Loc.Get("仿本")}</color>",
        $"<color=#949494>{Loc.Get("残本")}</color>" 
    };
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
        Loc.Get("内功"), Loc.Get("轻功"), Loc.Get("绝技"), Loc.Get("拳掌"),Loc.Get("剑法")
        ,Loc.Get("刀法"),Loc.Get("长兵"),Loc.Get("奇门"),Loc.Get("射术")
    };
    private int _bookRadioIndex;
    
    // 表格选中项
    private int _selectedTableRow;
    private List<ShopItem> _shopItems;

    private void DrawMainWindow()
    {
        // 物品分类单选
        _itemTypeRadioIndex = UI.RadioButtonGroup
            .Selected(_itemTypeRadioIndex)
            .Options(ItemTypeRadioOptions)
            .ButtonStyle()
            .Horizontal()
            .Draw();
        UI.Space(5);
        // 物品等级多选
        _itemLevelRadioIndex = UI.RadioButtonGroup
            .Options(ItemLevelOptions)
            .Selected(_itemLevelRadioIndex)
            .ButtonStyle()
            .Horizontal()
            .Draw();
        UI.Space(5);
        // 物品品质多选
        if (_itemTypeRadioIndex != 4)
        {
            _itemQualityRadioIndex = UI.RadioButtonGroup
                .Options(ItemQualityOptions)
                .Selected(_itemQualityRadioIndex)
                .ButtonStyle()
                .Horizontal()
                .Draw();
            UI.Space(5);
        }
        // 装备类型多选
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
        // 秘籍多选
        if (_itemTypeRadioIndex == 4)
        {
            _bookQualityRadioIndex = UI.RadioButtonGroup
                .Options(BookQualityOptions)
                .Selected(_bookQualityRadioIndex)
                .ButtonStyle()
                .Horizontal()
                .Draw();
            UI.Space(5);
            _bookRadioIndex = UI.RadioButtonGroup
                .Options(BookOptions)
                .Selected(_bookRadioIndex)
                .ButtonStyle()
                .Horizontal()
                .Draw();
            UI.Space(5);
        }
        
        
        UI.Horizontal(() =>
        {
            UI.Label(_status).Text().Draw(GUILayout.ExpandWidth(true));
            UI.Button("购买").Add().OnClick(Buy).Draw(GUILayout.Width(80));
        });
        
        UI.Divider();
        
        // 可选表格
        _selectedTableRow = UI.Table(
            _shopItems,
            item => new[] { item.Name, item.Type, item.Level, item.Quality, item.Price.ToString(), item.Reputation.ToString() },
            _selectedTableRow,
            new[] { Loc.Get("名称"), Loc.Get("类型"), Loc.Get("物品等级"), Loc.Get("物品品质"), Loc.Get("需要价格"), Loc.Get("需要声望") },
            showIndex: true);
            
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

    /// <summary>
    /// 初始化商店数据
    /// </summary>
    private void InitShopData()
    {
        _shopItems = new List<ShopItem>();
        // 示例数据：
        _shopItems.Add(new ShopItem { Id = 3, Name = "示例物品1", Type = "装备", Level = "精良", Quality = "上品", Price = 1000, Reputation = 500 });
        _shopItems.Add(new ShopItem { Id = 2, Name = "示例物品2", Type = "装备", Level = "精良", Quality = "上品", Price = 1000, Reputation = 500 });
    }
    /// <summary>
    /// 购买
    /// 1.先检查玩家的钱和声望
    /// 2.检查玩家重量
    /// </summary>
    private void Buy()
    {
        if (_selectedTableRow < 0 || _selectedTableRow >= _shopItems.Count)
            return;

        var selectedItem = _shopItems[_selectedTableRow];
        // _status = "提示：<color=green>购买成功</color>";
        // _status = "提示：<color=red>银钱或声望不够</color>";
        _status = "提示：<color=yellow>负重不够了</color>";
        Log.Info(selectedItem.Name);
    }
}