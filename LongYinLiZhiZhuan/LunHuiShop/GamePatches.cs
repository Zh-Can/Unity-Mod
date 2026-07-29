using HarmonyLib;
using Il2Cpp;
using LunHuiShop.GuiFramework.Localization;
using LunHuiShop.GuiFramework.Logger;
using LunHuiShop.GuiFramework.Other;

namespace LunHuiShop;

public class GamePatches
{
    private static readonly BiDictionary<int, string> ItemLevelStrings = new()
    {
        { 0, $"<color=#949494>{Loc.Get("劣质")}</color>" },
        { 1, $"<color=#7ec00b>{Loc.Get("普通")}</color>" },
        { 2, $"<color=#307ede>{Loc.Get("优质")}</color>" },
        { 3, $"<color=#9c7fe0>{Loc.Get("精良")}</color>" },
        { 4, $"<color=#e08d07>{Loc.Get("完美")}</color>" },
        { 5, $"<color=#df2c45>{Loc.Get("绝世")}</color>" }
    };
    

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameController), nameof(GameController.Start))]
    public static void GameController_Start_Postfix(GameController __instance)
    {
        var list = MainView.ShopItems;
        ItemData item;
        // 饰品
        var decorations = GlobalData.DecorationTypeName;
        for (var i = 0; i < decorations.Count; i++)
        for (var j = 0; j < ItemLevelStrings.Count; j++)
        {
            item = __instance.GenerateDecoration(j, i, 0);
            item.CountValueAndWeight();
            list.Add(new ShopItem
            {
                Id = item.itemID,
                Name = item.Name(true),
                Type = "饰品",
                SortType = "饰品",
                Price = item.value,
                Fame = item.value / 10f,
                IconName = item.GetItemIconName()
            });
        }

        // 珍宝
        var treasureTypeName = GlobalData.TreasureTypeName;

        for (var j = 0; j < ItemLevelStrings.Count; j++)
        for (var i = 0; i < treasureTypeName.Count; i++)
        {
            item = __instance.GenerateTreasure(i, j, 0);
            item.CountValueAndWeight();
            list.Add(new ShopItem
            {
                Id = item.itemID,
                Name = item.Name(true),
                Type = "珍宝",
                SortType = "珍宝",
                Price = item.value,
                Fame = item.value / 10f,
                IconName = item.GetItemIconName()
            });
        }

        // 材料
        var materialTypeName = GlobalData.MaterialTypeName;
        for (var j = 0; j < ItemLevelStrings.Count; j++)
        for (var i = 0; i < materialTypeName.Count; i++)
        {
            item = __instance.GenerateMaterial(i, j, 0);
            item.CountValueAndWeight();
            list.Add(new ShopItem
            {
                Id = item.itemID,
                Name = item.Name(true),
                Type = "材料",
                SortType = "材料",
                Price = item.value,
                Fame = item.value / 10f,
                IconName = item.GetItemIconName()
            });
        }

        MainView.ShopItems = list;
        Log.Info("饰品，秘籍，珍宝，材料初始化数据完成");
    }

    /// <summary>
    ///     GameDataController 初始化完成后填充 ShopItems
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameDataController), nameof(GameDataController.Start))]
    public static void GameDataController_Start_Postfix(GameDataController __instance)
    {
        var list = new List<ShopItem>();

        // 武器
        foreach (var weapon in __instance.weaponDataBase.Values)
            for (var i = 0; i < ItemLevelStrings.Count; i++)
            {
                weapon.itemLv = i;
                weapon.CountValueAndWeight();
                list.Add(new ShopItem
                {
                    Id = weapon.itemID,
                    Name = ItemLevelStrings[weapon.itemLv] + weapon.Name(true),
                    ItemLevel = ItemLevelStrings[weapon.itemLv],
                    Type = "装备",
                    SortType = "武器",
                    Price = weapon.value,
                    Fame = weapon.value / 10f,
                    IconName = weapon.GetItemIconName()
                });
            }

        // 头盔
        foreach (var helmet in __instance.helmetDataBase.Values)
            for (var i = 0; i < ItemLevelStrings.Count; i++)
            {
                helmet.itemLv = i;
                helmet.CountValueAndWeight();
                list.Add(new ShopItem
                {
                    Id = helmet.itemID,
                    Name = ItemLevelStrings[helmet.itemLv] + helmet.Name(true),
                    ItemLevel = ItemLevelStrings[helmet.itemLv],
                    Type = "装备",
                    SortType = "头盔",
                    Price = helmet.value,
                    Fame = helmet.value / 10f,
                    IconName = helmet.GetItemIconName()
                });
            }

        // 护甲
        foreach (var armor in __instance.armorDataBase.Values)
            for (var i = 0; i < ItemLevelStrings.Count; i++)
            {
                armor.itemLv = i;
                armor.CountValueAndWeight();
                list.Add(new ShopItem
                {
                    Id = armor.itemID,
                    Name = ItemLevelStrings[armor.itemLv] + armor.Name(true),
                    ItemLevel = ItemLevelStrings[armor.itemLv],
                    Type = "装备",
                    SortType = "护甲",
                    Price = armor.value,
                    Fame = armor.value / 10f,
                    IconName = armor.GetItemIconName()
                });
            }

        // 鞋履
        foreach (var shoes in __instance.shoesDataBase.Values)
            for (var i = 0; i < ItemLevelStrings.Count; i++)
            {
                shoes.itemLv = i;
                shoes.CountValueAndWeight();
                list.Add(new ShopItem
                {
                    Id = shoes.itemID,
                    Name = ItemLevelStrings[shoes.itemLv] + shoes.Name(true),
                    ItemLevel = ItemLevelStrings[shoes.itemLv],
                    Type = "装备",
                    SortType = "鞋履",
                    Price = shoes.value,
                    Fame = shoes.value / 10f,
                    IconName = shoes.GetItemIconName()
                });
            }

        // 丹药
        foreach (var med in __instance.medDataBase.Values)
        {
            med.CountValueAndWeight();
            list.Add(new ShopItem
            {
                Id = med.itemID,
                ItemLevel = ItemLevelStrings[med.itemLv],
                Name = med.Name(true),
                Type = "丹药",
                SortType = "丹药",
                Price = med.value,
                Fame = med.value / 10f,
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
                ItemLevel = ItemLevelStrings[food.itemLv],
                Name = ItemLevelStrings[food.itemLv]+food.Name(true),
                Type = "饮食",
                SortType = food.subType == 0 ? "佳肴" : "美酒",
                Price = food.value,
                Fame = food.value / 10f,
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
                ItemLevel = ItemLevelStrings[horse.itemLv],
                Name = horse.Name(true),
                Type = "马匹",
                SortType = "马匹",
                Price = horse.value,
                Fame = horse.value / 10f,
                IconName = horse.GetItemIconName()
            });
        }

        // 秘籍
        ItemData book;
        foreach (var skill in __instance.kungfuSkillDataBase.Values)
            book = new ItemData(ItemType.Book).SetBookData(skill.skillID, 5);

        MainView.ShopItems = list;

        Log.Info("武器，头盔，护甲，鞋履，丹药，饮食，马匹初始化数据完成");
    }
}