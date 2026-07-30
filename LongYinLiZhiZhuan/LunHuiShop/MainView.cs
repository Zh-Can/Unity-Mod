using System.Globalization;
using System.Text;
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
    private WindowData _mainWindow = null!;
    private string _status = "";
    private Vector2 _tableScrollPos;
    
    private void Awake()
    {
        HttpGet.TryHit(this);
        
        // 如果有表格， 7个column * 120 + 50
        _mainWindow = UI.NewWindow(
                new Rect(100, 100, 780, 780),
                Loc.Get("轮回商店"),
                DrawMainWindow)
            .Id(1)
            .Hide()
            .Footer(DrawStatusBar)
            .Build();
    }

    private void Start()
    {
        RefreshShopData();
    }
    
    // 物品分类
    private static readonly string[] ItemTypeRadioOptions =
    {
        Loc.Get("装备"), Loc.Get("丹药"), Loc.Get("饮食"), Loc.Get("秘籍"), Loc.Get("珍宝"), 
        Loc.Get("材料"), Loc.Get("马匹")
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
    
    // 装备类型
    private static readonly string[] EquipmentOptions =
    {
        Loc.Get("武器"), Loc.Get("护甲"), Loc.Get("头盔"), Loc.Get("鞋履"),Loc.Get("饰品")
    };
    private int _equipmentRadioIndex;
    // 秘籍类型
    private static readonly string[] BookTypeOptions =
    {
        Loc.Get("内功"), Loc.Get("轻功"), Loc.Get("绝技"), Loc.Get("拳掌"),Loc.Get("剑法"),
        Loc.Get("刀法"),Loc.Get("长兵"),Loc.Get("奇门"),Loc.Get("射术")
    };
    private int _bookTypeIndex;
    
    // 表格选中项
    private int _selectedTableRow;
    public static List<ShopItem> ShopItems = null!;
    private List<ShopItem> _displayItems = new();
    private string _searchText = "";

    private string[] _tableTitles = new[] { Loc.Get("名称"), Loc.Get("类型"), Loc.Get("物品等级"), Loc.Get("需要银两"), Loc.Get("需要声望") };
    
    private void DrawMainWindow()
    {
        // 物品分类
        _itemTypeRadioIndex = UI.RadioButtonGroup
            .Selected(_itemTypeRadioIndex)
            .Options(ItemTypeRadioOptions)
            .ButtonStyle()
            .Horizontal()
            .OnChange(index =>
            {
                _itemTypeRadioIndex = index;
                RefreshShopData();
            })
            .Draw();
        UI.Space(5);

        // 装备类型
        if (_itemTypeRadioIndex == 0)
        {
            _equipmentRadioIndex = UI.RadioButtonGroup
                .Options(EquipmentOptions)
                .Selected(_equipmentRadioIndex)
                .ButtonStyle()
                .Horizontal()
                .OnChange(index =>
                {
                    _equipmentRadioIndex = index;
                    RefreshShopData();
                })
                .Draw();
            UI.Space(5);
        }
        // 秘籍
        if (_itemTypeRadioIndex == 3)
        {
            _bookTypeIndex = UI.RadioButtonGroup
                .Options(BookTypeOptions)
                .Selected(_bookTypeIndex)
                .ButtonStyle()
                .Horizontal()
                .OnChange(index =>
                {
                    _bookTypeIndex = index;
                    RefreshShopData();
                })
                .Draw();
            UI.Space(5);
        }
        // 物品等级
        _itemLevelRadioIndex = UI.RadioButtonGroup
            .Options(ItemLevelOptions)
            .Selected(_itemLevelRadioIndex)
            .ButtonStyle()
            .Horizontal()
            .OnChange(index =>
            {
                _itemLevelRadioIndex = index;
                RefreshShopData();
            })
            .Draw();
        UI.Space(5);

        UI.Horizontal(() =>
        {
            UI.Horizontal(() =>
            {
                UI.Label(_status).Text().Draw(GUILayout.ExpandWidth(true));
            });
            var prevSearch = _searchText;
            _searchText = UI.TextFiled(_searchText, placeholder: Loc.Get("搜索名称"), options: GUILayout.Width(150));
            if (_searchText != prevSearch)
                RefreshShopData();

            UI.Button("购买").Add().OnClick(Buy).Draw(GUILayout.Width(80));
        });
        
        UI.Divider();
        
        // 可选表格
        _selectedTableRow = UI.Table(
            _displayItems,
            item => new[] { item.Name, item.SortType, item.ItemLevel, item.Price.ToString(), item.Fame.ToString(CultureInfo.InvariantCulture) },
            _selectedTableRow,
            _tableTitles,
            showIndex: true,
            onDrawFirstCell: (rect, data, text) => IconHelper.DrawCellWithIcon(rect, data.IconName, text),
            scrollPosition: ref _tableScrollPos,
            scrollHeight: 440);
    }

    private void DrawStatusBar()
    {
        UI.Divider(5);
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
    /// 根据当前筛选条件刷新表格显示数据。
    /// </summary>
    private void RefreshShopData()
    {
        _selectedTableRow = -1;

        if (ShopItems == null)
        {
            _displayItems = new List<ShopItem>();
            return;
        }

        var selectedType = ItemTypeRadioOptions[_itemTypeRadioIndex];
        var selectedLevel = ItemLevelOptions[_itemLevelRadioIndex];

        _displayItems = new List<ShopItem>(ShopItems.Count);
        foreach (var item in ShopItems)
        {
            if (item == null) continue;

            // 物品分类
            if (selectedType == "装备" && item.SortType == "饰品")
            {
                // 饰品在数据中独立为 Type="饰品"，但在装备子类型里提供入口
            }
            else if (item.Type != selectedType)
            {
                continue;
            }

            // 子类型筛选
            if (selectedType == "装备")
            {
                var subType = EquipmentOptions[_equipmentRadioIndex];
                if (subType == "饰品")
                {
                    if (item.Type != "饰品")
                        continue;
                }
                else
                {
                    if (item.Type != "装备" || item.SortType != subType)
                        continue;
                }
            }
            else if (selectedType == "秘籍")
            {
                if (item.SortType != BookTypeOptions[_bookTypeIndex])
                    continue;
            }

            // 物品等级
            if (item.ItemLevel != selectedLevel)
                continue;

            // 名称搜索
            if (!string.IsNullOrEmpty(_searchText) &&
                !StripColorTags(item.Name).Contains(_searchText, StringComparison.OrdinalIgnoreCase))
                continue;

            _displayItems.Add(item);
        }
    }

    private static string StripColorTags(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var sb = new StringBuilder(input.Length);
        var i = 0;
        while (i < input.Length)
        {
            if (input[i] == '<')
            {
                var close = input.IndexOf('>', i);
                if (close > i)
                {
                    i = close + 1;
                    continue;
                }
            }

            sb.Append(input[i]);
            i++;
        }

        return sb.ToString();
    }

    /// <summary>
    /// 购买
    /// 先检查玩家的钱和声望
    /// </summary>
    private void Buy()
    {
        var gc = GameController.Instance;
        if (_selectedTableRow < 0 || _selectedTableRow >= _displayItems.Count || 
            !LyHelper.TryReadPlayer(out var player) || gc == null)
            return;
        
        
        var selectedItem = _displayItems[_selectedTableRow];
        
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
        
        
        ItemData item = null!;
        GamePatches.ItemLevelStrings.TryGetKey(selectedItem.ItemLevel, out var lv);
        
        switch (selectedItem.SortType)
        {
            case "武器":
                item = gc.GenerateWeapon(lv, selectedItem.Id, 100);
                break;
            case "护甲":
                item = gc.GenerateArmor(lv, selectedItem.Id, 100);
                break;
            case "头盔":
                item = gc.GenerateHelmet(lv, selectedItem.Id, 100);
                break;
            case "鞋履":
                item = gc.GenerateShoes(lv, selectedItem.Id, 100);
                break;
            case "饰品":
                item = gc.GenerateDecoration(lv, selectedItem.Id, 100);
                break;
            case "丹药":
                item = gc.GenerateMedData(selectedItem.Id, 100);
                break;
            case "饮食":
            case "佳肴":
            case "美酒":
                item = gc.GenerateFoodData(selectedItem.Id, 100);
                item?.CountValueAndWeight();
                break;
            case "秘籍":
            case "内功":
            case "轻功":
            case "绝技":
            case "拳掌":
            case "剑法":
            case "刀法":
            case "长兵":
            case "奇门":
            case "射术":
                item = new ItemData(ItemType.Book);
                item.SetBookData(selectedItem.Id, 5);
                item.CountValueAndWeight();
                break;
            case "珍宝":
                item = gc.GenerateTreasure(selectedItem.Id, lv, 100);
                for (var k = 0; k < item.treasureData.treasureLv.Count; k++)
                {
                    item.treasureData.treasureLv[k] = 5;
                    item.treasureData.identified[k] = true;
                }
                item.treasureData.fullIdentified = true;
                item.value = item.GetTreasureRealValue();
                item.CountValueAndWeight();
                break;
            case "材料":
                item = gc.GenerateMaterial(selectedItem.Id, lv, 100);
                break;
            case "马匹":
                item = gc.GenerateHorseData(selectedItem.Id, 100);
                break;
            case "马鞍":
                item = gc.GenerateHorseArmorData(lv, 100);
                break;
        }

        if (item == null)
        {
            _status = $"提示：<color=red>生成物品失败：{selectedItem.SortType} ID={selectedItem.Id}</color>";
            Log.Warning($"Buy: 生成物品失败，SortType={selectedItem.SortType}, Id={selectedItem.Id}, Name={selectedItem.Name}");
            return;
        }

        try
        {
            player.fame -= selectedItem.Fame;
            player.itemListData.money -= selectedItem.Price;
            _status = "提示：<color=green>购买成功</color>";
            LyHelper.AddInfoTab($"<color=#red>轮回商店</color> -> 消耗 <color=yellow>{selectedItem.Price} 银两</color>，<color=yellow>{selectedItem.Fame} 声望</color> 购买成功, 成功获得：{item.Name(true)}");
            player.GetItem(item, false);
            // player.itemListData.GetItem(item, true);
        }
        catch (Exception ex)
        {
            _status = $"提示：<color=red>购买失败：{ex.Message}</color>";
            Log.Warning($"Buy: GetItem 异常，SortType={selectedItem.SortType}, Id={selectedItem.Id}, Name={selectedItem.Name}, 异常：{ex.GetType().Name}: {ex.Message}");
        }
    }
}