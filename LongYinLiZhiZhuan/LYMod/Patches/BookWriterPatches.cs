using HarmonyLib;
using Il2Cpp;
using LYMod.Helpers;
using UnityEngine;

namespace LYMod.Patches;

/// <summary>
/// 私宅抄书 功能
/// 1.私宅有4个抄书位
/// 2.选人可以选和自己有关系的，但不是同门派的人物（原逻辑包含自己），按id排序
/// </summary>
public class BookWriterPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ChooseController), nameof(ChooseController.ShowChoosePanel),
        typeof(ChooseType), typeof(Il2CppSystem.Collections.Generic.List<HeroData>), typeof(GameObject), typeof(string),
        typeof(string), typeof(ChooseFilterType), typeof(string))]
    public static void ShowChoosePanelPrefix(ChooseController __instance, ChooseType _chooseType, 
        Il2CppSystem.Collections.Generic.List<HeroData> param, GameObject _sendResultFucTarget, 
        string _sendResultFuc, string _sendResultParam, ChooseFilterType _filterType, string _cancelFuc)
    {
        var gc = GameController.Instance;
        if (Plugin.Instance.BookWriterSelfFlag.Value && _sendResultFuc == "BookWriterTargetHeroChoosen" 
            && _chooseType == ChooseType.Hero && gc != null && HeroHelper.TryReadPlayer(out var player) 
            && player.belongForceID != -1 && player.GetForce().MainArea().areaID != player.GetArea().areaID)
        {
            var forceId = player.belongForceID;
            var relationHeroIds = new HashSet<int>(); // 用 HashSet 自动去重
    
            // 收集所有有关系的人物ID
            void AddRelation(int heroId)
            {
                if (heroId != -1)
                    relationHeroIds.Add(heroId);
            }

            void AddRelations(Il2CppSystem.Collections.Generic.List<int> heroIds)
            {
                foreach (var id in heroIds)
                {
                    AddRelation(id);
                }
            }

            AddRelation(player.Teacher);
            AddRelation(player.Lover);
            
            AddRelations(player.Students);
            AddRelations(player.PreLovers);
            AddRelations(player.Relatives) ;
            AddRelations(player.Brothers);
            AddRelations(player.Friends);
            AddRelations(player.teamMates); 
            var sortedHeroIds = relationHeroIds.ToList();
            sortedHeroIds.Sort();
            // 添加到 param
            foreach (var heroId in sortedHeroIds)
            {
                var hero = gc.worldData.GetHero(heroId);
                if (forceId != -1)
                {
                    // 有门派，同门派不添加
                    if (hero != null && hero.belongForceID != forceId)
                        param.Add(hero);
                }
                else
                {
                    // 无门派，全部添加
                    param.Add(hero);
                }
            }
        }
    }
}
