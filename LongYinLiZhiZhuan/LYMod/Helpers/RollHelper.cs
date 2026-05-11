﻿using HarmonyLib;
using Il2Cpp;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using System.Collections;
using MelonLoader;

namespace LYMod.Helpers;

public class RollHelper
{
   
    // 拍卖会Roll
    public static void TryAuctionRoll()
    {
        if (ModConfig.HaveAucRoll) return;
        var auc = AuctionController.Instance;
        var plot = PlotController.Instance;
        if (auc != null && plot != null && auc.auctionPanel.activeInHierarchy)
        {
            foreach (var gm in auc.auctionItemIconList)
                if (gm != null)
                    Object.Destroy(gm);
            auc.auctionItemIconList.Clear();

            foreach (var gm in auc.heroIconList)
                if (gm != null)
                    Object.Destroy(gm);
            auc.heroIconList.Clear();

            var itemListData = new ItemListData();
            plot.GenerateAuctionItem(itemListData, auc.auctionDifficulty);
            auc.RestartAuction(auc.heroList, itemListData, auc.playerSellItem,
                auc.endMatchCallPlot, auc.auctionDifficulty, auc.havePlayer, auc.auctionKeeper);
        }
    }

    // 突破roll
    public static void TryBreakThoughtRoll()
    {
        var btc = BreakThroughController.Instance;
        if (btc != null && btc.breakThroughPanel != null && btc.breakThroughPanel.activeInHierarchy
            && btc.breakThroughPos != null && btc.breakThroughPos.transform.childCount > 0)
        {
            var componentsInChildren = btc.breakThroughPos
                .GetComponentsInChildren<BreakThroughChoiceController>();
            foreach (var btcc in componentsInChildren)
                if (btcc != null && btcc.gameObject != null)
                    Object.Destroy(btcc.gameObject);

            btc.StartShowBreakChoice();
        }
    }

    // 制造roll
    public static void TryCraftRoll()
    {
        var cuc = CraftUIController.Instance;
        if (cuc == null || cuc.creaftUIPanel == null || !cuc.creaftUIPanel.activeInHierarchy ||
            cuc.craftResultList == null || cuc.craftResultList.Count == 0)
            return;

        var craftType = cuc.craftType;

        var oldList = cuc.craftResultList;
        var newList = new Il2CppSystem.Collections.Generic.List<ItemData>();
        var gc = GameController.Instance;
        var heroData = gc.worldData.Player();

        var baseValue = cuc.GetCraftFinalValue();

        foreach (var itemData in oldList)
        {
            ItemData newItem;

            if (craftType == CraftType.Equipment)
            {
                var subType = itemData.subType;
                var littleType = itemData.equipmentData?.littleType ?? -1;
                var targetWeaponType = cuc.targetWeaponType;

                if (subType == 0)
                    newItem = gc.GenerateRandomItemValue(baseValue, (int)itemData.type, 1f,
                        subType, -1, heroData, targetWeaponType);
                else
                    newItem = gc.GenerateRandomItemValue(baseValue, (int)itemData.type, 1f,
                        subType, littleType, heroData);
            }
            else
            {
                newItem = gc.GenerateRandomItemValue(baseValue, (int)itemData.type, 1f,
                    itemData.subType, -1, heroData);
            }

            newList.Add(newItem);
        }

        cuc.craftResultList = newList;
        cuc.ShowCraftResultChoosePanel();
    }


    #region 中元鬼市/商店 Roll 与 BookOwnMark 标记

    private static string _shopParam;
    
    
    // 野外商人+鬼市 + 官府兑换 + 商店
    public static void TryZhongyuanRoll()
    {
        var tuic = TradeUIController.Instance;
        var gc = GameController.Instance;
        var pc = PlotController.Instance;
        var flag = HeroHelper.TryReadPlayer(out var player);
        if (pc == null || !flag || tuic == null || !tuic.tradeUI.activeInHierarchy || gc == null) return;
        
        if (player.GetArea() == null)
        {
            var eventName = pc.nowEvent.eventName;
            var rightItemListData = tuic.rightList.targetItemList;
            var itemNum = rightItemListData.allItem.Count == 0 ? 20 : rightItemListData.allItem.Count;
            
            tuic.rightList.ClearAllItem();
            tuic.rightList.targetItemList.ClearAllItem();
            rightItemListData.allItem.Clear();
            var availableItemType = new Il2CppSystem.Collections.Generic.List<int>();
            availableItemType.Add(0);
            availableItemType.Add(1);
            availableItemType.Add(2);
            availableItemType.Add(3);
            availableItemType.Add(4);
            availableItemType.Add(5);
            availableItemType.Add(6);
            if (eventName == "中元鬼市")
            {
                gc.GenerateRandomItem(rightItemListData, itemNum, availableItemType, Plugin.Instance.ZhongyuanLv.Value, 0f, false);
            }
            else
            {
                gc.GenerateRandomItem(rightItemListData,itemNum,pc.nowEvent.GetEventRareLv()*2,0f);
            }
            tuic.rightList.RefreshItemList(false);
           
            // 如果存在 BookOwnMark Mod，为新生成的秘籍添加标记
            if (ModConfig.HaveBookOwnMark)
            {
                MelonCoroutines.Start(MarkOwnedBooksCoroutine(tuic));
            }
        }
        else if(tuic.tradeUIType == TradeUIType.GovernStorage)
        {
            
            gc.RefreshGovernStorage();
            tuic.rightList.RefreshItemList(true);
            
            // 如果存在 BookOwnMark Mod，为新生成的秘籍添加标记
            if (ModConfig.HaveBookOwnMark)
            {
                MelonCoroutines.Start(MarkOwnedBooksCoroutine(tuic));
            }
        }
        else
        {
            Plugin.LOG.Msg($"ModConfig.HaveBookOwnMark:{ModConfig.HaveBookOwnMark}");
            var buildUI = BuildingUIController.Instance;
            if (buildUI == null || buildUI.targetBuildingData == null) return;

            var buildingData = buildUI.targetBuildingData;
            if (buildingData == null) return;
            var shopItemList = buildingData.shopItemList;
            var shopData = buildingData.DataBase()?.areaBuildingShopData;
            if (shopItemList == null || shopData == null) return;
            
            var rightItemListData = tuic.rightList.targetItemList;
            
            var oldCount = rightItemListData.allItem?.Count ?? shopData.itemNum;
            oldCount = oldCount == 0 ? shopData.itemNum * 2  : oldCount;
            
            shopItemList.ClearAllItem();
            tuic.rightList.ClearAllItem();
            
            // 重新生成商店物品
            gc.GenerateRandomItem(rightItemListData, (int)oldCount, shopData.itemType, buildingData.lv, shopData.itemBossLv, false);
            tuic.rightList.RefreshItemList(true);
            // 如果存在 BookOwnMark Mod，为新生成的秘籍添加标记
            if (ModConfig.HaveBookOwnMark)
            {
                MelonCoroutines.Start(MarkOwnedBooksCoroutine(tuic));
            }
        }
    }
    
    /// <summary>
    /// 协程：延迟标记已拥有的秘籍（等待UI渲染完成）
    /// </summary>
    private static IEnumerator MarkOwnedBooksCoroutine(TradeUIController tradeUI)
    {
        if (tradeUI == null) yield break;
        if (tradeUI.tradeUIType == TradeUIType.Storage) yield break;

        var merchantList = tradeUI.rightList;
        if (merchantList == null || merchantList.itemGrid == null) yield break;

        Plugin.LOG.Msg($"[BookOwnMark] 开始标记协程，TradeUIType: {tradeUI.tradeUIType}");

        // 等待一帧，确保UI开始渲染
        yield return null;

        // 最多等待50帧，等待图标创建完成
        var maxWait = 50;
        var waited = 0;
        while (waited < maxWait)
        {
            var icons = merchantList.itemGrid.GetComponentsInChildren<ItemIconController>(true);
            if (icons is { Length: > 0 })
            {
                Plugin.LOG.Msg($"[BookOwnMark] 等待 {waited} 帧后找到 {icons.Length} 个图标");
                break;
            }
            waited++;
            yield return null;
        }

        // 获取所有图标并标记
        var finalIcons = merchantList.itemGrid.GetComponentsInChildren<ItemIconController>(true);
        if (finalIcons == null || finalIcons.Length == 0) 
        {
            Plugin.LOG.Msg($"[BookOwnMark] 未找到任何图标");
            yield break;
        }

        var ownedBookNames = GetOwnedBookNames();
        var speBookNames = GetSpeBookNames();
        Plugin.LOG.Msg($"[BookOwnMark] 扫描到 {ownedBookNames.Count} 本普通秘籍, {speBookNames.Count} 本特殊秘籍");
        
        const string OWNED_MARK = " <color=#33cc86>☑</color>";
        const string SPE_BOOK_MARK = " <color=#ff3333>☑</color>";

        var markedCount = 0;
        var bookCount = 0;
        foreach (var icon in finalIcons)
        {
            if (icon?.itemData == null) continue;

            var itemType = icon.itemData.type;
            if (itemType != ItemType.Book) continue;
            
            bookCount++;
            var bookName = icon.itemData.name;
            var newName = bookName;
            var hasMark = false;

            // 获取干净的名称（去除已有标记）用于比较
            var cleanName = RemoveBookMark(bookName);

            // 检查普通仓库（绿色标记）
            if (ownedBookNames.Contains(cleanName) && !bookName.Contains(OWNED_MARK))
            {
                newName += OWNED_MARK;
                hasMark = true;
                Plugin.LOG.Msg($"[BookOwnMark] 标记普通秘籍: {cleanName}");
            }
            // 检查特殊仓库（红色标记）
            if (speBookNames.Contains(cleanName) && !bookName.Contains(SPE_BOOK_MARK))
            {
                newName += SPE_BOOK_MARK;
                hasMark = true;
                Plugin.LOG.Msg($"[BookOwnMark] 标记特殊秘籍: {cleanName}");
            }

            if (hasMark)
            {
                icon.itemData.name = newName;
                // 直接修改UI上的Text组件
                var nameText = icon.transform.Find("Name")?.GetComponent<Text>();
                if (nameText != null)
                {
                    nameText.text = newName;
                }
                markedCount++;
            }
        }
        
        Plugin.LOG.Msg($"[BookOwnMark] 完成标记，共找到 {bookCount} 本秘籍，标记了 {markedCount} 本");
    }
    
    /// <summary>
    /// 为物品列表中的秘籍添加 BookOwnMark 标记
    /// </summary>
    private static void ApplyBookOwnMark(ItemListData itemListData)
    {
        if (itemListData?.allItem == null) return;
        
        var ownedBookNames = GetOwnedBookNames();
        var speBookNames = GetSpeBookNames();
        const string OWNED_MARK = " <color=#33cc86>☑</color>";
        const string SPE_BOOK_MARK = " <color=#ff3333>☑</color>";
        
        foreach (var item in itemListData.allItem)
        {
            if (item?.type != ItemType.Book) continue;
            
            var bookName = item.name;
            var newName = bookName;
            var hasMark = false;
            
            // 获取干净的名称（去除已有标记）用于比较
            var cleanName = RemoveBookMark(bookName);
            
            // 检查普通仓库（绿色标记）
            if (ownedBookNames.Contains(cleanName) && !bookName.Contains(OWNED_MARK))
            {
                newName += OWNED_MARK;
                hasMark = true;
            }
            // 检查特殊仓库（红色标记）
            if (speBookNames.Contains(cleanName) && !bookName.Contains(SPE_BOOK_MARK))
            {
                newName += SPE_BOOK_MARK;
                hasMark = true;
            }
            
            if (hasMark)
            {
                item.name = newName;
            }
        }
    }
    
    /// <summary>
    /// 获取玩家已拥有的秘籍名称集合
    /// </summary>
    private static HashSet<string> GetOwnedBookNames()
    {
        var ownedBookNames = new HashSet<string>();
        var gc = GameController.Instance;
        if (gc?.worldData == null) 
        {
            Plugin.LOG.Msg("[BookOwnMark] GetOwnedBookNames: worldData is null");
            return ownedBookNames;
        }
        
        var player = gc.worldData.Player();
        if (player == null) 
        {
            Plugin.LOG.Msg("[BookOwnMark] GetOwnedBookNames: player is null");
            return ownedBookNames;
        }

        // 扫描玩家个人仓库
        var storageList = player.selfStorage?.allItem;
        var selfCount = 0;
        if (storageList != null)
        {
            foreach (var item in storageList)
            {
                if (item is { type: ItemType.Book })
                {
                    ownedBookNames.Add(RemoveBookMark(item.name));
                    selfCount++;
                }
            }
        }

        // 扫描门派藏书阁
        var forceCount = 0;
        if (player.belongForceID != -1)
        {
            var forceData = gc.worldData.GetHeroForce(0);
            var bookStorage = forceData.bookStorage?.allItem;
            if (bookStorage != null)
            {
                foreach (var item in bookStorage)
                {
                    if (item is { type: ItemType.Book })
                    {
                        ownedBookNames.Add(RemoveBookMark(item.name));
                        forceCount++;
                    }
                }
            }
        }

        Plugin.LOG.Msg($"[BookOwnMark] 个人仓库: {selfCount} 本, 门派藏书阁: {forceCount} 本, 总计: {ownedBookNames.Count} 本");
        return ownedBookNames;
    }
    
    /// <summary>
    /// 获取特殊秘籍名称集合
    /// </summary>
    private static HashSet<string> GetSpeBookNames()
    {
        var speBookNames = new HashSet<string>();
        var gc = GameController.Instance;
        if (gc?.worldData?.speBookStorage?.allItem == null) return speBookNames;

        foreach (var item in gc.worldData.speBookStorage.allItem)
        {
            if (item is { type: ItemType.Book })
            {
                speBookNames.Add(RemoveBookMark(item.name));
            }
        }

        return speBookNames;
    }
    
    /// <summary>
    /// 移除秘籍名称中的标记
    /// </summary>
    private static string RemoveBookMark(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        return name.Replace(" <color=#33cc86>☑</color>", "").Replace(" <color=#ff3333>☑</color>", "");
    }
    
    #endregion

    
    
    private static float _recruitLv = 0;
    private static int _heroNum = 0;
    
    // roll招募 - 按R键触发，关闭并重新打开招募界面以刷新人物列表
    public static void TryRefreshRecruitList()
    {
        if (ModConfig.HaveRecruitReRoll) return;
        
        var ruic = RecruitUIController.Instance;
        if (ruic == null || ruic.recruitUIPanel == null || !ruic.recruitUIPanel.activeInHierarchy) return;

        // 使用游戏原生的方式：关闭并重新打开招募界面
        // 这样会触发游戏重新生成招募人物
        var recruitType = ruic.recruitUIType;
        
        ruic.HideRecruitUI();
        ruic.ShowRecruitUI(recruitType, _heroNum, _recruitLv);
        
        Plugin.LOG.Msg($"[Recruit] 正在刷新招募列表...");
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(RecruitUIController), nameof(RecruitUIController.ShowRecruitUI))]
    public static void RecruitUIController_ShowRecruitUI_Postfix(RecruitUIController __instance, RecruitUIType targetType, int heroNum, float recruitLv)
    {
        _recruitLv = recruitLv;
        _heroNum = heroNum;
        Plugin.LOG.Msg(_recruitLv);
    }

    // 刷新特殊事件 没啥意思
    // public static void TryRerollSpeMasterOrStele()
    // {
    //     var pc = PlotController.Instance;
    //     if (pc == null || pc.nowEvent == null) return;
    //
    //     var plotPanel = pc.plotPanel;
    //     if (plotPanel == null || !plotPanel.activeInHierarchy) return;
    //
    //     var eventName = pc.nowEvent.eventName;
    //     if (string.IsNullOrEmpty(eventName)) return;
    //
    //     if (eventName.Contains("世外高人"))
    //     {
    //         pc.FindSpeMasterEvent("");
    //     }
    //     else if (eventName.Contains("失传秘籍") || eventName.Contains("石碑"))
    //     {
    //         pc.FindSpeSteleFight();
    //     }
    // }

    // 特殊毒药制作roll
    public static void TrySpePoisonRoll()
    {
        var spc = SpePoisonController.Instance;
        if (spc == null || spc.spePoisonUI == null || !spc.spePoisonUI.activeInHierarchy) return;
        
        var poisonData = spc.targetSpePoisonData;
        if (poisonData is not { finished: true }) return;
        var res = poisonData.result;
        
        var newPoison = GameController.Instance.GenerateRandomItemValue(spc.GetTotalScore(), (int)res.type, 1f, res.subType);
        spc.targetSpePoisonData.result = newPoison;
        spc.HideSpePoisonUI();
        spc.ShowSpePoisonUI();
        
    }

    // 大比/大会奖励重Roll（门派大比、比武大会、辩才大会）
    public static void TryFightMatchRewardRoll()
    {
        var fmc = FightMatchController.Instance;
        if (fmc == null) return;

        var oldReward = fmc.rewardList;
        if (oldReward == null || oldReward.Count == 0) return;

        var plot = PlotController.Instance;
        if (plot == null) return;

        // 检查plotPanel或fightMatchPanel是否打开
        var plotPanel = plot.plotPanel;
        var fightPanel = fmc.fightMatchPanel;
        var isPlotActive = plotPanel != null && plotPanel.activeInHierarchy;
        var isFightActive = fightPanel != null && fightPanel.activeInHierarchy;
        
        if (!isPlotActive && !isFightActive) return;

        // 用当前比赛的参数重新Roll奖励
        var newReward = FightMatchController.GenerateFightMatchRewardItemList(
            fmc.fightMatchType, fmc.matchDifficulty, fmc.isForceMatch, fmc.isForceGroupMatch);
        if (newReward == null || newReward.Count == 0) return;

        // 替换奖励列表
        fmc.rewardList = newReward;

        // 查找并更新 RewardItem 容器
        if (fightPanel == null) return;

        var rewardItemTransform = fightPanel.transform.Find("RewardItem");
        if (rewardItemTransform == null)
        {
            // 尝试在 fightPanel 中查找所有包含 "Reward" 的子物体
            for (var i = 0; i < fightPanel.transform.childCount; i++)
            {
                var child = fightPanel.transform.GetChild(i);
                if (child != null && child.name.Contains("Reward"))
                {
                    rewardItemTransform = child;
                    break;
                }
            }
        }
        
        if (rewardItemTransform == null) return;

        // 根据UI结构分析：真正的奖励物体是名为 "0"、"1"、"2" 等，子物体包含 "ItemIcon(Clone)"
        var rewardItemChildren = new Il2CppSystem.Collections.Generic.List<Transform>();
        for (var i = 0; i < rewardItemTransform.childCount; i++)
        {
            var child = rewardItemTransform.GetChild(i);
            if (child != null && int.TryParse(child.name, out _))
            {
                rewardItemChildren.Add(child);
            }
        }
        
        // 处理奖励子物体
        for (var i = 0; i < Mathf.Min(newReward.Count, rewardItemChildren.Count); i++)
        {
            var child = rewardItemChildren[i];
            if (child == null) continue;
            
            // 查找 ItemIcon(Clone)
            ItemIconController? icon = null;
            
            // 直接查找 "ItemIcon(Clone)"
            var itemIconObj = child.Find("ItemIcon(Clone)");
            if (itemIconObj != null)
            {
                icon = itemIconObj.GetComponent<ItemIconController>();
            }
            
            // 如果找不到，尝试不带 (Clone)
            if (icon == null)
            {
                itemIconObj = child.Find("ItemIcon");
                if (itemIconObj != null)
                {
                    icon = itemIconObj.GetComponent<ItemIconController>();
                }
            }
            
            // 如果还是找不到，在整个子树中查找
            if (icon == null)
            {
                icon = child.GetComponentInChildren<ItemIconController>();
            }

            if (icon != null)
            {
                icon.needRefreshPriceIcon = true;
                icon.itemData = newReward[i];
                icon.inited = false;
                icon.AutoSetName();
                icon.Update();
                icon.Update();
                icon.Update();
                
                // 禁用/启用整个奖励子物体来强制刷新
                child.gameObject.SetActive(false);
                child.gameObject.SetActive(true);
            }
        }
    }
    
   
}
