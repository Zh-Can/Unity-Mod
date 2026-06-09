using MelonLoader;
using MelonLoader.Utils;
using Il2Cpp;
using UnityEngine;

[assembly: MelonInfo(typeof(ForceHeroGrowthMod.Plugin), "ForceHeroGrowthMod", "5.0.1", "Can")]
[assembly: MelonGame("TppStudio", "LongYinLiZhiZhuan")]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]

namespace ForceHeroGrowthMod;

public class Plugin : MelonMod
{
    public static readonly MelonLogger.Instance LOG = Melon<Plugin>.Logger;

    private static MelonPreferences_Category _configCategory = null!;
    
    // 是否启用功能
    private static MelonPreferences_Entry<bool> _enableSkillUpgrade = null!;
    private static MelonPreferences_Entry<bool> _enableLibrarySkillLearn = null!;
    private static MelonPreferences_Entry<bool> _enableJianghuSkillLearn = null!;  // 学完藏经阁后学习江湖武学
    public static MelonPreferences_Entry<bool> EnableMoveBookToStorage = null!;  // 将已学会秘籍放入藏经阁
    public static MelonPreferences_Entry<int> MoveBookToStorageMinLevel = null!;  // 秘籍入库最低等级要求（默认4级）
    private static MelonPreferences_Entry<bool> _enableSkillLearnLimit = null!;  // 学习限制：有未满级武学时不学新武学
    
    // 详细日志开关
    public static MelonPreferences_Entry<bool> EnableDetailedLog = null!;
    
    // 处理范围：0=特殊人物(1-170)，1=全人物, 2=星标人物
    public static MelonPreferences_Entry<int> HeroProcessRange = null!;
    
    // 自动升级是否包含玩家
    private static MelonPreferences_Entry<bool> _includePlayerInUpgrade = null!;
    
    // 按天赋等级配置升级，格式: "0=5,1=4,2=3,3=2,4=1,5=0" (稀有度=升级数)
    // 稀有度: 0=基础(白), 1=进阶(绿), 2=上乘(蓝), 3=秘传(紫), 4=顶级(橙), 5=绝世(红)
    private static MelonPreferences_Entry<string> _talent0UpgradeConfig = null!;  // 愚钝
    private static MelonPreferences_Entry<string> _talent1UpgradeConfig = null!;  // 平平
    private static MelonPreferences_Entry<string> _talent2UpgradeConfig = null!;  // 聪慧
    private static MelonPreferences_Entry<string> _talent3UpgradeConfig = null!;  // 超群
    private static MelonPreferences_Entry<string> _talent4UpgradeConfig = null!;  // 究极
    
    // 玩家单独配置（玩家天赋固定为0，但需要单独配置升级规则）
    private static MelonPreferences_Entry<string> _playerUpgradeConfig = null!;
    
    // 缓存解析后的配置
    private static readonly Dictionary<int, int>[] TalentUpgradeCache = new Dictionary<int, int>[5];
    private static Dictionary<int, int> _playerUpgradeCache = null!;

    public override void OnInitializeMelon()
    {
        InitializeConfig();
        
        var harmony = new HarmonyLib.Harmony("ForceHeroGrowthMod");
        harmony.PatchAll(typeof(Patches));
        
        LOG.Msg("[门派弟子成长Mod] 已加载");
    }
    
    private void InitializeConfig()
    {
        _configCategory = MelonPreferences.CreateCategory("ForceHeroGrowth", "门派弟子成长");
        _configCategory.SetFilePath(MelonEnvironment.UserDataDirectory + "\\ForceHeroGrowth.cfg");
        
        // 功能开关
        _enableSkillUpgrade = _configCategory.CreateEntry("EnableSkillUpgrade", true, description: "启用技能自动升级，是否启用根据天赋等级自动升级技能功能");
        _enableLibrarySkillLearn = _configCategory.CreateEntry("EnableLibrarySkillLearn", true, description: "启用藏经阁技能学习，是否启用自动学习藏经阁技能功能，会检查功绩是否足够，如果足够会学习超过身份等级的武学技能");
        _enableJianghuSkillLearn = _configCategory.CreateEntry("EnableJianghuSkillLearn", false, description: "启用江湖武学学习，藏经阁武学全部学完后，是否自动学习江湖武学（无门派武学）");
        EnableMoveBookToStorage = _configCategory.CreateEntry("EnableMoveBookToStorage", false, description: "启用秘籍入库，过日时是否将非玩家门派掌门/副掌门已学会且掌握等级>=配置等级的武学秘籍放入藏经阁");
        MoveBookToStorageMinLevel = _configCategory.CreateEntry("MoveBookToStorageMinLevel", 4, description: "秘籍入库掌握等级门槛，已学会秘籍的掌握等级>=此值才会放入藏经阁，默认4级");
        _enableSkillLearnLimit = _configCategory.CreateEntry("EnableSkillLearnLimit", true, description: "启用自动学习限制，开启后如果人物有未满级的武学，则不会从藏经阁或江湖奇遇学会新的武学");

        // 详细日志开关
        EnableDetailedLog = _configCategory.CreateEntry("EnableDetailedLog", false, description: "启用详细日志，开启后每月会输出详细处理日志");

        // 处理范围
        HeroProcessRange = _configCategory.CreateEntry("HeroProcessRange", 0, description: "处理人物范围，0=只处理特殊人物(ID 1-170)，1=处理所有人物，2=只处理星标人物");

        // 按天赋等级配置升级，格式: "0=5,1=4,2=3,3=2,4=1,5=0" (稀有度=升级数)
        _talent0UpgradeConfig = _configCategory.CreateEntry("Talent0UpgradeConfig", "0=1,1=1,2=0,3=0,4=0,5=0", description: "天赋0(愚钝)升级配置，格式: 稀有度=升级数, 如 0=5,1=4,2=3,3=2,4=1,5=0");
        _talent1UpgradeConfig = _configCategory.CreateEntry("Talent1UpgradeConfig", "0=1,1=1,2=1,3=0,4=0,5=0", description: "天赋1(平平)升级配置，格式: 稀有度=升级数, 如 0=5,1=4,2=3,3=2,4=1,5=0");
        _talent2UpgradeConfig = _configCategory.CreateEntry("Talent2UpgradeConfig", "0=2,1=2,2=1,3=1,4=0,5=0", description: "天赋2(聪慧)升级配置，格式: 稀有度=升级数, 如 0=5,1=4,2=3,3=2,4=1,5=0");
        _talent3UpgradeConfig = _configCategory.CreateEntry("Talent3UpgradeConfig", "0=3,1=3,2=2,3=1,4=1,5=0", description: "天赋3(超群)升级配置，格式: 稀有度=升级数, 如 0=5,1=4,2=3,3=2,4=1,5=0");
        _talent4UpgradeConfig = _configCategory.CreateEntry("Talent4UpgradeConfig", "0=4,1=4,2=3,3=2,4=1,5=1", description: "天赋4(究极)升级配置，格式: 稀有度=升级数, 如 0=5,1=4,2=3,3=2,4=1,5=0");

        // 自动升级是否包含玩家
        _includePlayerInUpgrade = _configCategory.CreateEntry("IncludePlayerInUpgrade", false, description: "启用自动升级包含玩家，自动升级技能时是否包含玩家角色，如果到达瓶颈不会升级");
        // 玩家单独配置
        _playerUpgradeConfig = _configCategory.CreateEntry("PlayerUpgradeConfig", "0=1,1=1,2=1,3=1,4=1,5=1", description: "玩家升级配置，玩家单独配置，格式: 稀有度=升级数");
        
        _configCategory.SaveToFile();
        
        // 解析配置
        ParseUpgradeConfigs();
    }
    
    /// <summary>
    /// 解析升级配置字符串
    /// </summary>
    private void ParseUpgradeConfigs()
    {
        TalentUpgradeCache[0] = ParseUpgradeConfig(_talent0UpgradeConfig.Value);
        TalentUpgradeCache[1] = ParseUpgradeConfig(_talent1UpgradeConfig.Value);
        TalentUpgradeCache[2] = ParseUpgradeConfig(_talent2UpgradeConfig.Value);
        TalentUpgradeCache[3] = ParseUpgradeConfig(_talent3UpgradeConfig.Value);
        TalentUpgradeCache[4] = ParseUpgradeConfig(_talent4UpgradeConfig.Value);
        _playerUpgradeCache = ParseUpgradeConfig(_playerUpgradeConfig.Value);
    }
    
    /// <summary>
    /// 解析单个配置字符串
    /// 格式: "0=5,1=4,2=3,3=2,4=1,5=0"
    /// </summary>
    private Dictionary<int, int> ParseUpgradeConfig(string config)
    {
        var result = new Dictionary<int, int>();
        
        // 默认所有稀有度为0（不升级）
        for (int i = 0; i <= 5; i++)
        {
            result[i] = 0;
        }
        
        if (string.IsNullOrWhiteSpace(config))
            return result;
        
        // 解析配置
        var pairs = config.Split(',');
        foreach (var pair in pairs)
        {
            var parts = pair.Trim().Split('=');
            if (parts.Length == 2 && 
                int.TryParse(parts[0].Trim(), out int rareLv) && 
                int.TryParse(parts[1].Trim(), out int upgradeCount))
            {
                if (rareLv is >= 0 and <= 5)
                {
                    result[rareLv] = Mathf.Clamp(upgradeCount, 0, 10);
                }
            }
        }
        
        return result;
    }

    
    
    /// <summary>
    /// 处理单个英雄的成长
    /// </summary>
    public static void ProcessHero(HeroData hero, bool isPlayer)
    {
        
        // 功能1: 按天赋等级升级技能
        if (_enableSkillUpgrade.Value)
        {
            // 如果是玩家且不包含玩家升级，则跳过
            if (isPlayer && !_includePlayerInUpgrade.Value)
            {
                return;
            }
            ProcessSkillUpgrade(hero, isPlayer);
        }
        
        // 功能2&3: 学习藏经阁技能，如果没学且启用江湖武学，则学习江湖武学
        if (_enableLibrarySkillLearn.Value && !isPlayer)
        {
            bool learnedLibrarySkill = ProcessLibrarySkillLearn(hero);
            
            // 如果藏经阁没学技能，且启用江湖武学学习，则学习江湖武学
            if (!learnedLibrarySkill && _enableJianghuSkillLearn.Value)
            {
                ProcessJianghuSkillLearn(hero);
            }
        }
        else if (_enableJianghuSkillLearn.Value && !isPlayer)
        {
            // 如果禁用藏经阁学习但启用江湖武学，直接学习江湖武学
            ProcessJianghuSkillLearn(hero);
        }
    }
    
    /// <summary>
    /// 处理技能升级 - 根据天赋等级和武功稀有度每月升级指定等级数
    /// 优先升级装备的武学（不考虑天赋限制），然后升级非装备的武学（考虑天赋）
    /// 玩家角色会检查突破障碍
    /// </summary>
    private static void ProcessSkillUpgrade(HeroData hero, bool isPlayer)
    {
        // 获取天赋等级 (0-4)
        var talentLv = Mathf.Clamp(hero.talent, 0, 4);
        
        if (hero.kungfuSkills == null) return;
        
        // 获取装备的武学列表（优先升级）
        var equippedSkillIds = GetEquippedSkillIds(hero);
        
        // 将技能分为装备的和非装备的
        var equippedSkills = new List<KungfuSkillLvData>();
        var unequippedSkills = new List<KungfuSkillLvData>();
        
        foreach (var skill in hero.kungfuSkills)
        {
            if (skill == null) continue;
            if (equippedSkillIds.Contains(skill.skillID))
                equippedSkills.Add(skill);
            else
                unequippedSkills.Add(skill);
        }
        
        // 先尝试升级装备的武学（不考虑天赋限制，直接升1级）
        if (TryUpgradeEquippedSkill(hero, isPlayer, equippedSkills))
            return;
        
        // 如果没有装备的武学可升级，尝试升级非装备的武学（考虑天赋）
        TryUpgradeSkill(hero, isPlayer, talentLv, unequippedSkills);
    }
    
    /// <summary>
    /// 尝试升级装备的武学
    /// 如果天赋允许升级，按天赋配置升级；如果不允许，仍升1级
    /// 返回是否成功升级
    /// </summary>
    private static bool TryUpgradeEquippedSkill(HeroData hero, bool isPlayer, List<KungfuSkillLvData> skills)
    {
        // 获取天赋等级
        var talentLv = Mathf.Clamp(hero.talent, 0, 4);
        
        foreach (var skill in skills)
        {
            var oldLv = skill.lv;
            
            // 如果已满级(10级)，跳过
            if (oldLv >= 10) continue;
            
            // 玩家角色需要检查突破障碍
            if (isPlayer && skill.SkillMeetObstacleLv())
            {
                if (EnableDetailedLog.Value)
                {
                    LOG.Msg($"[门派弟子成长Mod] 玩家 {hero.heroName} 装备技能 {skill.Name(true)} 遇到突破障碍，跳过升级");
                }
                continue;
            }
            
            // 获取武功稀有度
            var skillRareLv = GetSkillRareLv(skill.skillID);
            
            // 根据天赋等级和武功稀有度获取升级等级数
            int upgradeCount;
            if (isPlayer)
            {
                // 玩家使用单独配置
                upgradeCount = _playerUpgradeCache.GetValueOrDefault(skillRareLv, 0);
            }
            else
            {
                // NPC使用天赋配置
                var config = TalentUpgradeCache[talentLv];
                upgradeCount = config.GetValueOrDefault(skillRareLv, 0);
            }
            
            // 如果天赋不允许升级，仍升1级（装备技能保底）
            if (upgradeCount <= 0)
            {
                upgradeCount = 1;
            }
            
            // 限制升级数在0-10之间
            upgradeCount = Mathf.Clamp(upgradeCount, 0, 10);
            
            // 计算实际可升级的等级数（不超过10级）
            var actualUpgrade = Mathf.Min(upgradeCount, 10 - oldLv);
            
            // 使用HeroData.UpgradeSkill升级技能（每次升1级，循环调用）
            for (var i = 0; i < actualUpgrade; i++)
            {
                hero.UpgradeSkill(skill);
            }
            
            var upgradeMsg = $"[过月成长] {hero.Name(true)} 精进装备技能 {skill.Name(true)} 从 {oldLv} 级提升到 {skill.lv} 级";
            if (EnableDetailedLog.Value)
            {
                LOG.Msg($"[门派弟子成长Mod] 人物 {hero.heroName} (天赋{talentLv}){upgradeMsg}");
            }
            // 只有NPC才记录日志（玩家没有经历日志显示）
            if (!isPlayer)
            {
                hero.AddLog(upgradeMsg);
            }
            
            // 成功升级一个技能
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 获取角色装备的武学ID列表
    /// </summary>
    private static HashSet<int> GetEquippedSkillIds(HeroData hero)
    {
        var equippedIds = new HashSet<int>();

        // 内功
        if (hero.internalSkill != null)
            equippedIds.Add(hero.internalSkill.skillID);
        
        // 轻功
        if (hero.dodgeSkill != null)
            equippedIds.Add(hero.dodgeSkill.skillID);
        
        // 绝技
        if (hero.uniqueSkill != null)
            equippedIds.Add(hero.uniqueSkill.skillID);
        
        // 攻击技能（可能有多个）
        if (hero.attackSkills == null) return equippedIds;
        foreach (var skill in hero.attackSkills)
        {
            if (skill != null)
                equippedIds.Add(skill.skillID);
        }

        return equippedIds;
    }
    
    /// <summary>
    /// 尝试从技能列表中升级一个技能
    /// 返回是否成功升级
    /// </summary>
    private static void TryUpgradeSkill(HeroData hero, bool isPlayer, int talentLv, List<KungfuSkillLvData> skills)
    {
        foreach (var skill in skills)
        {
            var oldLv = skill.lv;
            
            // 如果已满级(10级)，跳过
            if (oldLv >= 10) continue;
            
            // 获取武功稀有度
            var skillRareLv = GetSkillRareLv(skill.skillID);
            
            // 根据天赋等级和武功稀有度获取升级等级数
            int upgradeCount;
            if (isPlayer)
            {
                // 玩家使用单独配置
                upgradeCount = _playerUpgradeCache.GetValueOrDefault(skillRareLv, 0);
            }
            else
            {
                // NPC使用天赋配置
                var config = TalentUpgradeCache[talentLv];
                upgradeCount = config.GetValueOrDefault(skillRareLv, 0);
            }
            
            // 限制升级数在0-10之间
            upgradeCount = Mathf.Clamp(upgradeCount, 0, 10);
            
            if (upgradeCount <= 0) continue;
            
            // 玩家角色需要检查突破障碍
            if (isPlayer && skill.SkillMeetObstacleLv())
            {
                if (EnableDetailedLog.Value)
                {
                    LOG.Msg($"[门派弟子成长Mod] 玩家 {hero.heroName} 技能 {skill.Name(true)} 遇到突破障碍，跳过升级");
                }
                continue;
            }
            
            // 计算实际可升级的等级数（不超过10级）
            var actualUpgrade = Mathf.Min(upgradeCount, 10 - oldLv);
            
            // 使用HeroData.UpgradeSkill升级技能（每次升1级，循环调用）
            for (var i = 0; i < actualUpgrade; i++)
            {
                hero.UpgradeSkill(skill);
            }
            
            var upgradeMsg = $"[过月成长] {hero.Name(true)} 奋发图强，将技能 {skill.Name(true)} 从 {oldLv} 级提升到 {skill.lv} 级";
            if (EnableDetailedLog.Value)
            {
                LOG.Msg($"[门派弟子成长Mod] 人物 {hero.heroName} (天赋{talentLv}){upgradeMsg}");
            }
            // 只有NPC才记录日志（玩家没有经历日志显示）
            if (!isPlayer)
            {
                hero.AddLog(upgradeMsg);
            }
        }
    }
    
    /// <summary>
    /// 处理藏经阁技能学习
    /// 使用hero.GetForce()获取人物所属门派
    /// 返回是否成功学习了技能
    /// </summary>
    private static bool ProcessLibrarySkillLearn(HeroData hero)
    {
        // 检查学习限制：如果开启限制且人物有未满级武学，则不学习新武学
        if (_enableSkillLearnLimit.Value && HasUnmaxedSkill(hero))
        {
            if (EnableDetailedLog.Value)
            {
                LOG.Msg($"[门派弟子成长Mod] 人物 {hero.heroName} 有未满级武学，跳过藏经阁学习");
            }
            return false;
        }
        
        // 获取人物所属门派
        var force = hero.GetForce();
        if (hero.belongForceID == -1) return false;
        
        // 获取藏经阁中的秘籍
        var bookStorage = force.bookStorage;
        if (bookStorage?.allItem == null) return false;
        
        // 遍历藏经阁中的书籍，寻找可以学习的技能
        foreach (var item in bookStorage.allItem)
        {
            if (item == null) continue;
            if ((int)item.type != 3) continue; // 只查找书籍 (ItemType.Book = 3)
            if (item.bookData == null) continue;
            
            var skillId = item.bookData.skillID;
            if (skillId <= 0) continue;
            
            // 检查是否已学习该技能
            if (HasSkill(hero, skillId)) continue;
            
            // 根据武功稀有度等级计算消耗功绩（从技能数据获取稀有度）
            var skillRareLv = GetSkillRareLv(skillId);
            var cost = GetSkillLearnCost(skillRareLv);
            
            // 检查功绩是否足够
            // 游戏会自动按功绩排序（低功绩在前），所以如果功绩不足直接跳出循环
            if (hero.forceContribution < cost)
            {
                if (EnableDetailedLog.Value)
                {
                    LOG.Msg($"[门派弟子成长Mod] 人物 {hero.heroName} 功绩不足({hero.forceContribution}/{cost})，无法学习藏经阁技能");
                }
                break;
            }
            
            // 扣除功绩并学习技能
            hero.forceContribution -= cost;
            
            // 学习技能
            LearnSkill(hero, skillId);
            
            var skillName = GetSkillName(skillId);
            var rareName = GetRareName(skillRareLv);
            
            var learnMsg = $"[过月成长] {hero.Name(true)}消耗{cost}功绩学会了{rareName}技能 {skillName}";
            if (EnableDetailedLog.Value)
            {
                LOG.Msg($"[过月成长] 人物 {hero.heroName} {learnMsg}");
            }
            hero.AddLog(learnMsg);
            
            // 每月只学习一个技能，返回成功
            return true;
        }
        // 没有学习任何技能
        return false;
    }
    
    /// <summary>
    /// 根据武功稀有度等级获取学习消耗功绩
    /// 基础(0)=20, 进阶(1)=40, 上乘(2)=80, 秘传(3)=160, 顶级(4)=320, 绝世(5)=640
    /// </summary>
    private static int GetSkillLearnCost(int rareLv)
    {
        return rareLv switch
        {
            0 => 20,   // 基础武功
            1 => 40,   // 进阶武功
            2 => 80,   // 上乘武功
            3 => 160,  // 秘传武功
            4 => 320,  // 顶级武功
            5 => 640,  // 绝世武功
            _ => 20    // 默认基础武功
        };
    }
    
    /// <summary>
    /// 根据稀有度等级获取名称
    /// 使用GlobalData.SkillLvName获取
    /// </summary>
    private static string GetRareName(int rareLv)
    {
        var skillLvNames = GlobalData.SkillLvName;
        if (skillLvNames != null && rareLv >= 0 && rareLv < skillLvNames.Count)
        {
            return skillLvNames[rareLv];
        }
        
        // 备用方案
        return rareLv switch
        {
            0 => "基础",
            1 => "进阶",
            2 => "上乘",
            3 => "秘传",
            4 => "顶级",
            5 => "绝世",
            _ => "基础"
        };
    }
    
    /// <summary>
    /// 获取技能名称
    /// 使用gdc.GetSkillDataBase(skillId).name
    /// </summary>
    private static string GetSkillName(int skillId)
    {
        var gdc = GameDataController.Instance;
        if (gdc == null) return $"技能{skillId}";
        
        var skillData = gdc.GetSkillDataBase(skillId);
        if (skillData == null) return $"技能{skillId}";
        
        return skillData.Name(true);
    }
    
    /// <summary>
    /// 获取技能稀有度等级
    /// 使用gdc.GetSkillDataBase(skillId).rareLv
    /// </summary>
    private static int GetSkillRareLv(int skillId)
    {
        var gdc = GameDataController.Instance;
        if (gdc == null) return 0;
        
        var skillData = gdc.GetSkillDataBase(skillId);
        if (skillData == null) return 0;
        
        return skillData.rareLv;
    }
    
    /// <summary>
    /// 检查人物是否已学习指定技能
    /// 使用HeroData.FindSkill判断
    /// </summary>
    private static bool HasSkill(HeroData hero, int skillId)
    {
        // 使用FindSkill查找技能，如果返回null表示未学习
        var skill = hero.FindSkill(skillId);
        return skill != null;
    }

    /// <summary>
    /// 检查人物是否有未满级(10级满级)的武学
    /// </summary>
    private static bool HasUnmaxedSkill(HeroData hero)
    {
        if (hero.kungfuSkills == null) return false;
        
        foreach (var skill in hero.kungfuSkills)
        {
            if (skill == null) continue;
            // 技能满级为10级，小于10级视为未满级
            if (skill.lv < 10)
            {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// 让人物学习指定技能
    /// 使用HeroData.GetSkill学习
    /// 注意：调用前需确保技能未学习
    /// </summary>
    private static void LearnSkill(HeroData hero, int skillId)
    {
        // 创建新的技能数据
        var newSkill = new KungfuSkillLvData(skillId);
        
        // 使用GetSkill学习技能
        hero.GetSkill(newSkill);
        
        // 标记数据需要更新
        hero.heroDetailDirty = true;
    }
    
    /// <summary>
    /// 处理江湖武学学习
    /// 按身份等级从高到低学习江湖武学（调用前需确保藏经阁已学完）
    /// 江湖武学：belongForceID == -1 的武学
    /// </summary>
    private static void ProcessJianghuSkillLearn(HeroData hero)
    {
        // 检查学习限制：如果开启限制且人物有未满级武学，则不学习新武学
        if (_enableSkillLearnLimit.Value && HasUnmaxedSkill(hero))
        {
            if (EnableDetailedLog.Value)
            {
                LOG.Msg($"[门派弟子成长Mod] 人物 {hero.heroName} 有未满级武学，跳过江湖武学学习");
            }
            return;
        }
        
        // 获取人物所属门派
        var force = hero.GetForce();
        if (force == null) return;
        
        // 按身份等级学习江湖武学
        // 身份等级对应稀有度：身份等级 == 武学稀有度（最大为5）
        var identityLevel = Mathf.Min(hero.heroForceLv, 5);
        
        // 从当前身份等级开始，逐级向下学习
        for (var targetRareLv = identityLevel; targetRareLv >= 0; targetRareLv--)
        {
            // 查找该稀有度的江湖武学
            var skillToLearn = FindJianghuSkillByRareLv(hero, targetRareLv);

            if (skillToLearn <= 0) continue;
            // 学习该技能
            LearnSkill(hero, skillToLearn);
                
            var skillName = GetSkillName(skillToLearn);
            var rareName = GetRareName(targetRareLv);
                
            var learnMsg = $"[过月成长] {hero.Name(true)}触发江湖奇遇学到了{rareName}江湖武学{skillName}";
            if (EnableDetailedLog.Value)
            {
                LOG.Msg($"[过月成长] 人物 {hero.heroName} {learnMsg}");
            }
            hero.AddLog(learnMsg);
                
            // 每月只学习一个技能
            break;
        }
    }
    
    /// <summary>
    /// 查找指定稀有度的未学习江湖武学
    /// 江湖武学：belongForceID == -1
    /// </summary>
    private static int FindJianghuSkillByRareLv(HeroData hero, int rareLv)
    {
        var gdc = GameDataController.Instance;
        if (gdc == null) return 0;
        
        var skillDataBase = gdc.kungfuSkillDataBase;
        if (skillDataBase == null) return 0;
        
        // 遍历所有武学数据
        foreach (var skillData in skillDataBase.Values)
        {
            if (skillData == null) continue;
            
            // 检查是否为江湖武学（belongForceID == -1）
            if (skillData.belongForceID != -1) continue;
            
            // 检查是否为隐藏武学（hide == true 的不学习）
            if (skillData.hide) continue;
            
            // 检查稀有度是否匹配
            if (skillData.rareLv != rareLv) continue;
            
            // 检查是否已学习
            if (HasSkill(hero, skillData.skillID)) continue;
            
            // 找到未学习的江湖武学
            return skillData.skillID;
        }
        
        return 0;
    }
}
