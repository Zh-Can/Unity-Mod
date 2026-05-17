using System.Collections;
using Il2Cpp;
using UnityEngine;

namespace LYMod.Helpers;

public static class OtherHelper
{
    /// <summary>
    ///     重置 _mainCategory 下的所有配置项为默认值
    /// </summary>
    public static void ResetAllMainConfig()
    {
        foreach (var entry in Plugin.Instance.MainCategory.Entries) entry.ResetToDefault();
        Plugin.Instance.MainCategory.SaveToFile();
    }

    // 输入框文本转字典
    public static Dictionary<int, float>? ParseInputBox(string inputText)
    {
        if (string.IsNullOrWhiteSpace(inputText))
            return null;

        return inputText
            // 先替换所有空格
            .Replace(" ", "")
            // 按分号分割键值对
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            // 按等号分割key/value
            .Select(pair => pair.Split('='))
            // 过滤无效格式（必须是key=value）
            .Where(kv => kv.Length == 2)
            // 安全转换类型（避免输错数字导致崩溃）
            .Where(kv => int.TryParse(kv[0], out _) && float.TryParse(kv[1], out _))
            // 转字典
            .ToDictionary(
                kv => int.Parse(kv[0]),
                kv => float.Parse(kv[1])
            );
    }

    public static Il2CppSystem.Collections.Generic.Dictionary<int, float>? ToIl2CppDictionary(
        Dictionary<int, float>? systemDict)
    {
        // 初始化 IL2CPP 字典
        var il2CPPDict = new Il2CppSystem.Collections.Generic.Dictionary<int, float>();

        // 空值判断，避免崩溃
        if (systemDict == null || systemDict.Count == 0) return null;

        // 遍历原生字典，逐个添加到 IL2CPP 字典
        foreach (var kvp in systemDict)
            // 避免重复key（IL2CPP字典添加重复key会抛异常）
            if (!il2CPPDict.ContainsKey(kvp.Key))
                il2CPPDict.Add(kvp.Key, kvp.Value);

        return il2CPPDict;
    }


    /// <summary>
    ///     添加游戏内提示信息
    /// </summary>
    /// <param name="infoText"></param>
    /// <param name="atlasName"></param>
    /// <param name="infoPic"></param>
    /// <param name="soundName"></param>
    /// <param name="volumn"></param>
    /// <param name="lastTime"></param>
    /// <param name="picColor"></param>
    public static void AddInfoTab(string infoText, string atlasName = "UIAtlas", string infoPic = null,
        string soundName = "Woosh", float volumn = 1f, float lastTime = 5f, Color picColor = default)
    {
        var infoController = InfoController.Instance;
        if (infoController == null) return;
        infoController.AddInfoTab(infoText, atlasName, infoPic, soundName, volumn, lastTime, picColor);
    }

    // /// <summary>
    // ///     修改武学修炼数量限制倍数
    // /// </summary>
    // public static void ChaneMaxNum()
    // {
    //     if (ModConfig.HaveNpcMod) return;
    //
    //     List<float> skillBaseNum = new() { 12, 10, 8, 6, 4, 2 };
    //
    //     var maxSkillNum = GlobalData.MaxSkillNum;
    //     if (maxSkillNum.Count == 6)
    //         for (var i = 0; i < 6; i++)
    //             maxSkillNum[i] = skillBaseNum[i] * Plugin.Instance.KungFuMaxLimitTimes.Value;
    //
    //     GlobalData.MaxSkillNum = maxSkillNum;
    // }

    /// <summary>
    ///     添加所有书到星辰阁
    /// </summary>
    public static void GenAllBookToSpeBookStorage()
    {
        var gc = GameController.Instance;
        if (gc?.worldData?.speBookStorage?.allItem == null) return;

        var speBookStorage = gc.worldData.speBookStorage;
        var gameData = GameDataController.Instance;
        if (gameData?.kungfuSkillDataBase == null) return;

        // 获取星辰阁中已有的秘籍skillID集合
        var existingSkillIds = new HashSet<int>();
        foreach (var item in speBookStorage.allItem)
            if (item is { type: ItemType.Book, bookData: not null })
                existingSkillIds.Add(item.bookData.skillID);

        // 遍历所有武功秘籍数据，添加没有的秘籍
        foreach (var skillData in gameData.kungfuSkillDataBase)
        {
            if (skillData == null) continue;

            // 如果星辰阁中还没有此秘籍，则添加
            if (!existingSkillIds.Contains(skillData.skillID) && skillData.belongForceID != -1)
            {
                // 创建秘籍物品，使用秘籍的原始稀有度
                var book = new ItemData(ItemType.Book).SetBookData(skillData.skillID, skillData.rareLv);
                speBookStorage.allItem.Add(book);
            }
        }

        SpeBookStorageController.Instance.RefreshBookStorageSpeAdd();
    }

    /// <summary>
    ///     解锁图鉴
    /// </summary>
    public static void UnlockHandBook()
    {
        Plugin.LOG.Msg("正在解锁全图鉴...");

        var gdc = GameDataController.Instance;
        if (gdc == null)
        {
            Plugin.LOG.Msg("GameDataController 未初始化！请在游戏完全加载后再按 F8");
            return;
        }

        // 获取 PlayerPrefData
        var playerPrefData = GameDataController.playerPrefData;
        if (playerPrefData == null)
        {
            Plugin.LOG.Msg("PlayerPrefData 未初始化！");
            return;
        }

        var prefDict = playerPrefData.playerPrefData;
        if (prefDict == null)
        {
            Plugin.LOG.Msg("PlayerPrefDictionary 未初始化！");
            return;
        }

        var unlockCount = 0;

        // ========== 1. 解锁所有武学图鉴 ==========
        var allSkills = gdc.kungfuSkillDataBase;
        if (allSkills != null)
        {
            Plugin.LOG.Msg($"发现 {allSkills.Count} 个武功");

            foreach (var skill in allSkills)
                if (skill != null)
                {
                    var key = $"HandBookSkill_{skill.skillID}";

                    // 检查是否已解锁
                    if (!prefDict.ContainsKey(key))
                    {
                        // 解锁该武功
                        prefDict.SetKey(key, 1);
                        unlockCount++;
                    }
                }

            Plugin.LOG.Msg($"已解锁 {unlockCount} 个武学图鉴");
        }

        // ========== 2. 解锁所有人物图鉴 ==========
        var allHeros = gdc.SpeHeroDataBase;
        if (allHeros != null)
        {
            Plugin.LOG.Msg($"发现 {allHeros.Count} 个特殊英雄");

            var heroUnlockCount = 0;
            foreach (var hero in allHeros)
                if (hero != null)
                {
                    var key = $"HandBookHero_{hero.heroID}";

                    // 检查是否已解锁
                    if (!prefDict.ContainsKey(key))
                    {
                        // 解锁该英雄
                        prefDict.SetKey(key, 1);
                        heroUnlockCount++;
                    }
                }

            Plugin.LOG.Msg($"已解锁 {heroUnlockCount} 个人物图鉴");
            unlockCount += heroUnlockCount;
        }

        // ========== 3. 保存数据 ==========
        if (unlockCount > 0)
        {
            // 保存到文件
            gdc.SavePlayerprefData();

            Plugin.LOG.Msg("========================================");
            Plugin.LOG.Msg($"全图鉴解锁完成！共解锁 {unlockCount} 项");
            Plugin.LOG.Msg("请重新打开图鉴界面查看效果");
            Plugin.LOG.Msg("========================================");
        }
        else
        {
            Plugin.LOG.Msg("所有图鉴已经解锁，无需重复操作");
        }
    }

    public static IEnumerator OverrideExtraAddData()
    {
        yield return null;

        if (!HeroHelper.TryReadPlayer(out var player))
            yield break;

        bool hasForceBonus = player.HaveForceFunction(14);
        var skills = player.kungfuSkills;

        foreach (var skill in skills)
        {
            var ids = skill.GetBreakThroughAvailableChoice();
            var dict = new Il2CppSystem.Collections.Generic.Dictionary<int, float>();

            int skillRareLv = Mathf.Clamp(skill.DataBase().rareLv, 0, 5);
            int skillLv = skill.lv;

            foreach (var id in ids)
            {
                const int rareLv = 5;
                float multiplier = Mathf.Max(0.5f, (hasForceBonus ? 1 : 0) + rareLv);

                var speAddBase = GameDataController.Instance.speAddDataBase[id];
                float baseValue = multiplier * speAddBase.speValue;
                float finalValue = 0f;

                switch (skillRareLv)
                {
                    case 0 or 1:
                        if (skillLv == 10)
                            finalValue = baseValue;
                        break;

                    case 2 or 3:
                        if (skillLv is > 4 and < 10)
                            finalValue = baseValue;
                        else if (skillLv == 10)
                            finalValue = baseValue * 2;
                        break;

                    case 4 or 5:
                        if (skillLv is > 3 and < 7)
                            finalValue = baseValue;
                        else if (skillLv is > 6 and < 10)
                            finalValue = baseValue * 2;
                        else if (skillLv == 10)
                            finalValue = baseValue * 3;
                        break;
                }

                if (finalValue != 0f)
                    dict[id] = finalValue;
            }

            skill.extraAddData.heroSpeAddData = dict;
        }
        AddInfoTab("【LYMod】<color=#00ff00>覆盖玩家突破属性完成！</color>");
    }

}