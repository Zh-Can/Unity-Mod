using HarmonyLib;
using Il2Cpp;
using LunHuiShop.GuiFramework.Localization;
using LunHuiShop.GuiFramework.Logger;

namespace LunHuiShop;

public class GamePatches
{
    private static readonly List<string> ItemLevelStrings = new()
    {
        $"<color=#949494>{Loc.Get("劣质")}</color>",
        $"<color=#7ec00b>{Loc.Get("普通")}</color>",
        $"<color=#307ede>{Loc.Get("优质")}</color>",
        $"<color=#9c7fe0>{Loc.Get("精良")}</color>",
        $"<color=#e08d07>{Loc.Get("完美")}</color>",
        $"<color=#df2c45>{Loc.Get("绝世")}</color>"
    };

    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameController), nameof(GameController.Start))]
    public static void GameController_Start_Postfix(GameController __instance)
    {
        var list = MainView.ShopItems;
        ItemData item;
        // 饰品
        var decorations = GlobalData.DecorationTypeName;
        for (int i = 0; i < decorations.Count; i++)
        {
            for (int j = 0; j < ItemLevelStrings.Count; j++)
            {
                item = __instance.GenerateDecoration(j, i, 0);
                item.CountValueAndWeight();
                list.Add(new ShopItem
                {
                    Id = item.itemID,
                    Name = item.Name(true),
                    Type = GetName(item.type.ToString()),
                    SortType = "饰品",
                    Price = item.value,
                    Fame = (int)(item.value / 10),
                    IconName = item.GetItemIconName()
                });
            }
        }
        
        // 珍宝
        var treasureTypeName = GlobalData.TreasureTypeName;
        
        for (int j = 0; j < ItemLevelStrings.Count; j++)
        {
            for (int i = 0; i < treasureTypeName.Count; i++)
            {
                item = __instance.GenerateTreasure(i, j, 0);
                item.CountValueAndWeight();
                list.Add(new ShopItem
                {
                    Id = item.itemID,
                    Name = item.Name(true),
                    Type = GetName(item.type.ToString()),
                    SortType = "珍宝",
                    Price = item.value,
                    Fame = (int)(item.value / 10),
                    IconName = item.GetItemIconName()
                });
            }
        }
        // 材料
        var materialTypeName = GlobalData.MaterialTypeName;
        for (int j = 0; j < ItemLevelStrings.Count; j++)
        {
            for (int i = 0; i < materialTypeName.Count; i++)
            {
                item = __instance.GenerateMaterial(i, j, 0);
                item.CountValueAndWeight();
                list.Add(new ShopItem
                {
                    Id = item.itemID,
                    Name = item.Name(true),
                    Type = GetName(item.type.ToString()),
                    SortType = "材料",
                    Price = item.value,
                    Fame = (int)(item.value / 10),
                    IconName = item.GetItemIconName()
                });
            }
        }
        MainView.ShopItems = list;
        Log.Info("饰品，秘籍，珍宝，材料初始化数据完成");
    }

    /// <summary>
    /// GameDataController 初始化完成后填充 ShopItems
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameDataController), nameof(GameDataController.Start))]
    public static void GameDataController_Start_Postfix(GameDataController __instance)
    {
        var list = new List<ShopItem>();
        
        // 武器
        foreach (var weapon in __instance.weaponDataBase.Values)
        {
            for (int i = 0; i < 6; i++)
            {
                weapon.itemLv = i;
                weapon.CountValueAndWeight();
                list.Add(new ShopItem
                {
                    Id = weapon.itemID,
                    Name = ItemLevelStrings[weapon.itemLv]+weapon.Name(true),
                    Type = GetName(weapon.type.ToString()),
                    SortType = "武器",
                    Price = weapon.value,
                    Fame = (int)(weapon.value / 10),
                    IconName = weapon.GetItemIconName()
                });
            }
        }
        // 头盔
        foreach (var helmet in __instance.helmetDataBase.Values)
        {
            for (int i = 0; i < 6; i++)
            {
                helmet.itemLv = i;
                helmet.CountValueAndWeight();
                list.Add(new ShopItem
                {
                    Id = helmet.itemID,
                    Name = ItemLevelStrings[helmet.itemLv]+helmet.Name(true),
                    Type = GetName(helmet.type.ToString()),
                    SortType = "头盔",
                    Price = helmet.value,
                    Fame = (int)(helmet.value / 10),
                    IconName = helmet.GetItemIconName()
                });
            }
        }
        // 护甲
        foreach (var armor in __instance.armorDataBase.Values)
        {
            for (int i = 0; i < 6; i++)
            {
                armor.itemLv = i;
                armor.CountValueAndWeight();
                list.Add(new ShopItem
                {
                    Id = armor.itemID,
                    Name = ItemLevelStrings[armor.itemLv]+armor.Name(true),
                    Type = GetName(armor.type.ToString()),
                    SortType = "护甲",
                    Price = armor.value,
                    Fame = (int)(armor.value / 10),
                    IconName = armor.GetItemIconName()
                });
            }
        }
        // 鞋履
        foreach (var shoes in __instance.shoesDataBase.Values)
        {
            for (int i = 0; i < 6; i++)
            {
                shoes.itemLv = i;
                shoes.CountValueAndWeight();
                list.Add(new ShopItem
                {
                    Id = shoes.itemID,
                    Name = ItemLevelStrings[shoes.itemLv]+shoes.Name(true),
                    Type = GetName(shoes.type.ToString()),
                    SortType = "鞋履",
                    Price = shoes.value,
                    Fame = (int)(shoes.value / 10),
                    IconName = shoes.GetItemIconName()
                });
            }
        }
        // 丹药
        foreach (var med in __instance.medDataBase.Values)
        {
            med.CountValueAndWeight();
            list.Add(new ShopItem
            {
                Id = med.itemID,
                Name = med.Name(true),
                Type = GetName(med.type.ToString()),
                SortType = "丹药",
                Price = med.value,
                Fame = (int)(med.value / 10),
                IconName = med.GetItemIconName()
            });
        }
        // 饮食
        foreach (var food in __instance.foodDataBase.Values)
        {
            food.CountValueAndWeight();
            list.Add(new ShopItem
            {
                Id = food.itemID,
                Name = ItemLevelStrings[food.itemLv]+food.Name(true),
                Type = GetName(food.type.ToString()),
                SortType = "饮食",
                Price = food.value,
                Fame = (int)(food.value / 10),
                IconName = food.GetItemIconName()
            });
        }
        // 马匹
        foreach (var horse in __instance.horseDataBase.Values)
        {
            horse.CountValueAndWeight();
            list.Add(new ShopItem
            {
                Id = horse.itemID,
                Name = horse.Name(true),
                Type = GetName(horse.type.ToString()),
                SortType = "马匹",
                Price = horse.value,
                Fame = (int)(horse.value / 10),
                IconName = horse.GetItemIconName()
            });
        }
        // 秘籍
        ItemData book;
        foreach (var skill in __instance.kungfuSkillDataBase.Values)
        {
            book = new ItemData(ItemType.Book).SetBookData(skill.skillID, 5);
        }
        
        MainView.ShopItems = list;
        
        Log.Info("武器，头盔，护甲，鞋履，丹药，饮食，马匹初始化数据完成");
    }

    

    private static string GetName(string type)
    {
        return type switch
        {
            "Equip" => "装备",
            "Med" => "丹药",
            "Horse" => "马匹",
            _ => "装备"
        };
    }
}