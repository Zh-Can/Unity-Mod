using HarmonyLib;
using Il2Cpp;
using MelonLoader;

namespace ForceHeroGrowthMod;

public class Patches
{
    
    /// <summary>
    /// 过日时处理非本门派掌门和副掌门身上武学秘籍如果学会了会放入藏经阁
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameController), nameof(GameController.ChangeDay), new Type[0])]
    public static void ChangeDay_Postfix()
    {
        // 检查是否启用了秘籍入库功能
        if (!Plugin.EnableMoveBookToStorage.Value)
            return;
        
        // 使用协程延迟执行，确保游戏数据已更新
        MelonCoroutines.Start(ProcessHeroGrowthOnDayChange());
    }

    private static System.Collections.IEnumerator ProcessHeroGrowthOnDayChange()
    {
        yield return null;
        var gc = GameController.Instance; 
        if (gc?.worldData == null) yield break; 
        var forces = gc.worldData.Forces; 
        var playerForce = gc.worldData.Player().GetForce(); 
        var heroesCopy = new List<HeroData>(); 

        // 收集需要处理的英雄
        foreach (var force in forces) 
        { 
            if (playerForce != null && playerForce.forceID != force.forceID) 
            { 
                foreach (var hero in force.GetOwnHeros()) 
                { 
                    if (hero.heroForceLv >= 5) 
                    { 
                        heroesCopy.Add(hero); 
                    } 
                } 
            } 
        } 

        // 处理每个英雄
        foreach (var hero in heroesCopy) 
        { 
            var itemsToMove = new List<ItemData>(); 
            var bookStorage = hero.GetForce().bookStorage.allItem; 
    
            // 获取藏书阁中已有的物品名称集合
            var existingItemNames = new HashSet<string>();
            foreach (var existingItem in bookStorage)
            {
                existingItemNames.Add(existingItem.name);
            }
    
            foreach (var item in hero.itemListData.allItem)
            {
                if (item.type != ItemType.Book) continue;
                var skill = hero.FindSkill(item.bookData.skillID);
                if (skill == null || skill.lv < Plugin.MoveBookToStorageMinLevel.Value) continue;
                // 使用名称比较，避免重复添加
                if (existingItemNames.Contains(item.name)) continue;
                itemsToMove.Add(item); 
                existingItemNames.Add(item.name);  // 添加到已存在集合，防止同一英雄有多个同名物品
            } 
    
            foreach (var item in itemsToMove) 
            { 
                var heroForce = hero.GetForce();
                if (Plugin.EnableDetailedLog.Value) 
                    Plugin.LOG.Msg($"{heroForce.forceName}的{hero.heroName}向藏书阁添加了秘籍{item.name}");
                
                hero.AddLog($"[过月成长] {hero.Name(true)}将已学会的秘籍{item.Name(true)}放入了藏经阁");
                hero.itemListData.LoseItem(item); 
                heroForce.bookStorage.allItem.Add(item); 
            } 
        }
    }

    /// <summary>
    /// 过月时处理门派弟子成长
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameController), nameof(GameController.ChangeMonth))]
    public static void ChangeMonth_Postfix()
    {
        // 使用协程延迟执行，确保游戏数据已更新
        MelonCoroutines.Start(ProcessHeroGrowthOnMonthChange());
    }
    
    private static System.Collections.IEnumerator ProcessHeroGrowthOnMonthChange()
    {
        // 等待一帧确保数据已更新
        yield return null;
        
        var gc = GameController.Instance;
        if (gc == null || gc.worldData == null) yield break;
        
        if (Plugin.EnableDetailedLog.Value)
        {
            Plugin.LOG.Msg("[门派弟子成长Mod] 过月处理开始...");
        }
        
        // 获取玩家ID用于判断
        var player = gc.worldData.Player();
        int playerId = player?.heroID ?? -1;
        
        // 根据配置决定处理范围
        if (Plugin.HeroProcessRange.Value == 1)
        {
            // 处理所有人物
            if (Plugin.EnableDetailedLog.Value)
            {
                Plugin.LOG.Msg("[门派弟子成长Mod] 处理所有人物");
            }
            
            var allHeroes = gc.worldData.Heros;
            if (allHeroes != null)
            {
                // 复制列表以避免遍历时集合被修改的异常
                var heroesCopy = new List<HeroData>();
                foreach (var hero in allHeroes)
                {
                    if (hero != null)
                        heroesCopy.Add(hero);
                }
                
                foreach (var hero in heroesCopy)
                {
                    bool isPlayer = (hero.heroID == playerId);
                    Plugin.ProcessHero(hero, isPlayer);
                    yield return null;
                }
            }
        }
        else if (Plugin.HeroProcessRange.Value == 2)
        {
            // 处理星标人物
            if (Plugin.EnableDetailedLog.Value)
            {
                Plugin.LOG.Msg("[门派弟子成长Mod] 处理星标人物");
            }
            
            var allHeroes = gc.worldData.Heros;
            if (allHeroes != null)
            {
                // 复制列表以避免遍历时集合被修改的异常
                var heroesCopy = new List<HeroData>();
                foreach (var hero in allHeroes)
                {
                    if (hero != null && hero.interestingStar)
                        heroesCopy.Add(hero);
                }
                
                foreach (var hero in heroesCopy)
                {
                    bool isPlayer = (hero.heroID == playerId);
                    Plugin.ProcessHero(hero, isPlayer);
                    yield return null;
                }
            }
        }
        else
        {
            // 处理特殊人物(ID 0-170，但展示给玩家看是1-170)
            if (Plugin.EnableDetailedLog.Value)
            {
                Plugin.LOG.Msg("[门派弟子成长Mod] 处理特殊人物(ID 1-170)");
            }
            
            for (int heroId = 0; heroId <= 170; heroId++)
            {
                var hero = gc.worldData.GetHero(heroId);
                if (hero == null) continue;
                
                bool isPlayer = (hero.heroID == playerId);
                Plugin.ProcessHero(hero, isPlayer);
                yield return null;
            }
        }
        
        if (Plugin.EnableDetailedLog.Value)
        {
            Plugin.LOG.Msg("[门派弟子成长Mod] 过月处理完成");
        }
    }
}