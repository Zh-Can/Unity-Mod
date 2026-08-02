using System.Linq;
using HarmonyLib;
using Il2Cpp;
using LunHuiShop.GuiFramework.Localization;
using LunHuiShop.GuiFramework.Logger;
using LunHuiShop.GuiFramework.Other;

namespace LunHuiShop;

public class GamePatches
{
    public static readonly BiDictionary<int, string> ItemLevelStrings = new()
    {
        { 0, $"<color=#949494>{Loc.Get("劣质")}</color>" },
        { 1, $"<color=#7ec00b>{Loc.Get("普通")}</color>" },
        { 2, $"<color=#307ede>{Loc.Get("优质")}</color>" },
        { 3, $"<color=#9c7fe0>{Loc.Get("精良")}</color>" },
        { 4, $"<color=#e08d07>{Loc.Get("完美")}</color>" },
        { 5, $"<color=#df2c45>{Loc.Get("绝世")}</color>" }
    };
    private static readonly string[] BookTypes =
    {
        Loc.Get("内功"), Loc.Get("轻功"), Loc.Get("绝技"), Loc.Get("拳掌"),Loc.Get("剑法"),
        Loc.Get("刀法"),Loc.Get("长兵"),Loc.Get("奇门"),Loc.Get("射术")
    };

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameDataController), nameof(GameDataController.Start))]
    public static void GameDataController_Start_Postfix(GameDataController __instance)
    {
        Log.Info($"[ShopData] GameDataController_Start_Postfix 触发，cfg 存在: {ShopDataSaver.Exists}");
        if (ShopDataSaver.Exists)
        {
            MainView.ShopItems = ShopDataSaver.Load();
            Log.Info($"[ShopData] 已从配置目录加载 {MainView.ShopItems.Count} 条数据");
            IconHelper.PreloadAll(MainView.ShopItems.Select(item => item.IconName));
        }
    }

    /// <summary>
    ///     加载存档后 GameController 已完整初始化，此时生成全部数据并写入配置目录。
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameController), nameof(GameController.Start))]
    public static void GameController_Start_Postfix(GameController __instance)
    {
        Log.Info($"[ShopData] GameController_Start_Postfix 触发，cfg 存在: {ShopDataSaver.Exists}");
        if (ShopDataSaver.Exists)
            return;

        var dataController = GameDataController.Instance;
        Log.Info($"[ShopData] GameDataController.Instance: {dataController != null}");
        if (dataController == null)
        {
            Log.Warning("[ShopData] 未找到 GameDataController，跳过 ShopItems 数据生成");
            return;
        }

        try
        {
            var list = GenerateAllShopItems(__instance, dataController);
            Log.Info($"[ShopData] 生成完成，共 {list.Count} 条数据");

            MainView.ShopItems = list;
            ShopDataSaver.Save(list);
            Log.Info($"[ShopData] 已写入配置目录，cfg 存在: {ShopDataSaver.Exists}");

            IconHelper.PreloadAll(MainView.ShopItems.Select(item => item.IconName));
        }
        catch (System.Exception ex)
        {
            Log.Error($"[ShopData] GameController_Start_Postfix 生成数据异常: {ex}");
        }
    }

    private static List<ShopItem> GenerateAllShopItems(GameController gc, GameDataController dataController)
    {
        var list = new List<ShopItem>();
        if (gc == null) return list;

        ItemData item;
        var sw = System.Diagnostics.Stopwatch.StartNew();

        // 饰品
        var decorations = GlobalData.DecorationTypeName;
        for (var i = 0; i < decorations.Count; i++)
        for (var j = 0; j < ItemLevelStrings.Count; j++)
        {
            item = gc.GenerateDecoration(j, i, 0);
            item.CountValueAndWeight();
            list.Add(new ShopItem
            {
                Id = i,
                Name = item.Name(true),
                ItemLevel = ItemLevelStrings[item.itemLv],
                Type = "饰品",
                SortType = "饰品",
                Price = item.value,
                Fame = item.value / 10f,
                IconName = item.GetItemIconName()
            });
        }
        Log.Info($"[ShopData] 饰品: {decorations.Count * ItemLevelStrings.Count} 条");

        // 珍宝
        var treasureTypeName = GlobalData.TreasureTypeName;
        for (var j = 0; j < ItemLevelStrings.Count; j++)
        for (var i = 0; i < treasureTypeName.Count; i++)
        {
            item = gc.GenerateTreasure(i, j, 100);
            for (var k = 0; k < item.treasureData.treasureLv.Count; k++)
            {
                item.treasureData.treasureLv[k] = 5;
                item.treasureData.identified[k] = true;
            }
            item.treasureData.fullIdentified = true;
            item.rareLv = 5;
            item.CountValueAndWeight();
            list.Add(new ShopItem
            {
                Id = i,
                Name = item.Name(true),
                ItemLevel = ItemLevelStrings[item.itemLv],
                Type = "珍宝",
                SortType = "珍宝",
                Price = item.GetTreasureRealValue(),
                Fame = item.GetTreasureRealValue() / 10f,
                IconName = item.GetItemIconName()
            });
        }
        Log.Info($"[ShopData] 珍宝: {treasureTypeName.Count * ItemLevelStrings.Count} 条");

        // 材料
        var materialTypeName = GlobalData.MaterialTypeName;
        for (var j = 0; j < ItemLevelStrings.Count; j++)
        for (var i = 0; i < materialTypeName.Count; i++)
        {
            item = gc.GenerateMaterial(i, j, 100);
            item.CountValueAndWeight();
            list.Add(new ShopItem
            {
                Id = i,
                Name = item.Name(true),
                ItemLevel = ItemLevelStrings[item.itemLv],
                Type = "材料",
                SortType = "材料",
                Price = item.value,
                Fame = item.value / 10f,
                IconName = item.GetItemIconName()
            });
        }
        Log.Info($"[ShopData] 材料: {materialTypeName.Count * ItemLevelStrings.Count} 条");

        // 马鞍
        for (var j = 0; j < ItemLevelStrings.Count; j++)
        {
            item = gc.GenerateHorseArmorData(j, 55);
            item.CountValueAndWeight();
            list.Add(new ShopItem
            {
                Id = j,
                Name = item.Name(true),
                ItemLevel = ItemLevelStrings[item.itemLv],
                Type = "马匹",
                SortType = "马鞍",
                Price = item.value,
                Fame = item.value / 10f,
                IconName = item.GetItemIconName()
            });
        }
        Log.Info($"[ShopData] 马鞍: {ItemLevelStrings.Count} 条");

        // 武器
        var weaponCount = 0;
        foreach (var weapon in dataController.weaponDataBase.Values)
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
                weaponCount++;
            }
        Log.Info($"[ShopData] 武器: {weaponCount} 条");

        // 头盔
        var helmetCount = 0;
        foreach (var helmet in dataController.helmetDataBase.Values)
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
                helmetCount++;
            }
        Log.Info($"[ShopData] 头盔: {helmetCount} 条");

        // 护甲
        var armorCount = 0;
        foreach (var armor in dataController.armorDataBase.Values)
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
                armorCount++;
            }
        Log.Info($"[ShopData] 护甲: {armorCount} 条");

        // 鞋履
        var shoesCount = 0;
        foreach (var shoes in dataController.shoesDataBase.Values)
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
                shoesCount++;
            }
        Log.Info($"[ShopData] 鞋履: {shoesCount} 条");

        // 丹药
        var medCount = 0;
        foreach (var med in dataController.medDataBase.Values)
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
            medCount++;
        }
        Log.Info($"[ShopData] 丹药: {medCount} 条");

        // 饮食
        var foodCount = 0;
        foreach (var food in dataController.foodDataBase.Values)
        {
            food.CountValueAndWeight();
            list.Add(new ShopItem
            {
                Id = food.itemID,
                ItemLevel = ItemLevelStrings[food.itemLv],
                Name = food.Name(true),
                Type = "饮食",
                SortType = food.subType == 0 ? "佳肴" : "美酒",
                Price = food.value,
                Fame = food.value / 10f,
                IconName = food.GetItemIconName()
            });
            foodCount++;
        }
        Log.Info($"[ShopData] 饮食: {foodCount} 条");

        // 马匹
        var horseCount = 0;
        foreach (var horse in dataController.horseDataBase.Values)
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
            horseCount++;
        }
        Log.Info($"[ShopData] 马匹: {horseCount} 条");

        // 秘籍
        var bookCount = 0;
        foreach (var skill in dataController.kungfuSkillDataBase.Values)
        {
            var book = new ItemData(ItemType.Book).SetBookData(skill.skillID, 5);
            book.CountValueAndWeight();
            list.Add(new ShopItem
            {
                Id = skill.skillID,
                ItemLevel = ItemLevelStrings[book.itemLv],
                Name = book.Name(true),
                Type = "秘籍",
                SortType = BookTypes[book.bookData.DataBase().type],
                Price = book.value,
                Fame = book.value / 10f,
                IconName = book.GetItemIconName()
            });
            bookCount++;
        }
        Log.Info($"[ShopData] 秘籍: {bookCount} 条");

        sw.Stop();
        Log.Info($"[ShopData] 生成总耗时: {sw.ElapsedMilliseconds}ms，总计: {list.Count} 条");
        return list;
    }
}
