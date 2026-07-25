using System;
using System.Collections.Generic;
using System.Globalization;
using Il2Cpp;
using LYMod;
using LYMod.Helpers;
using LYMod.Patches;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;

[assembly: MelonInfo(typeof(Plugin), ModConfig.ModName, ModConfig.ModVersion, ModConfig.ModAuthor)]
[assembly:MelonGame("TppStudio", "LongYinLiZhiZhuan")]
[assembly:MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]
namespace LYMod;

public class Plugin : MelonMod
{
    public static Plugin Instance = null!;
    public static readonly MelonLogger.Instance LOG = Melon<Plugin>.Logger;
    
    
    #region 配置项
    public MelonPreferences_Category MainCategory= null!;
    private MelonPreferences_Category _otherCategory= null!;
    public MelonPreferences_Entry<bool> TeachNewSkillToNpc = null!; // 传授满级
    public MelonPreferences_Entry<bool> TeachNpc= null!; // 指点满级
    public MelonPreferences_Entry<bool> Interaction= null!; // 无限交互
    public MelonPreferences_Entry<float> ReadBook = null!; // 读书经验倍率
    public MelonPreferences_Entry<bool> ReadBookChangePatient1Flag = null!; // 读书耐心1
    public MelonPreferences_Entry<bool> Explore= null!; // 探险耐力锁
    public MelonPreferences_Entry<bool> Cost0= null!; // 建筑升级资源消耗0
    public MelonPreferences_Entry<bool> BuildingDestroyFlag= null!; // 建筑可拆除开关
    public MelonPreferences_Entry<bool> RedBook= null!; // 必获得完本
    public MelonPreferences_Entry<float> RedBreak= null!; // 突破倍率
    public MelonPreferences_Entry<bool> RedTreasure= null!; // 必红色珍宝
    public MelonPreferences_Entry<float> Pzqh= null!; // 烹饪铸造炼药强化
    public MelonPreferences_Entry<bool> StealRate= null!; // 偷窃/偷学成功
    public MelonPreferences_Entry<bool> Hgbj= null!; // 好感不减
    public MelonPreferences_Entry<int> HorseMaxSeeRangeTimes= null!; // 马和马鞍视野加成倍数
    public MelonPreferences_Entry<int> HorseMaxWeightTimes= null!; // 马和马鞍负重倍数
    public MelonPreferences_Entry<int> HorseStepAddRateTimes= null!; // 马和马鞍探险耐力加成倍数
    public MelonPreferences_Entry<float> EquipmentWeight= null!; // 装备重量倍率
    public MelonPreferences_Entry<float> StudyFightRate= null!; // 练功房学习战斗经验倍率
    public MelonPreferences_Entry<float> StudyUniqeRate= null!; // 闭关室学习理论经验倍率
    public MelonPreferences_Entry<float> ShopLvRate= null!; // 拍卖会品质倍率
    public MelonPreferences_Entry<int> ItemNum= null!; // 拍卖会物品数量
    public MelonPreferences_Entry<bool> CopyBookFlag= null!; //默写/抄书1天
    public MelonPreferences_Entry<bool> ReasearchFlag= null!; //门派研究一天
    public MelonPreferences_Entry<int> FavorTimes= null!; //好感倍数
    public MelonPreferences_Entry<int> MoneyTimes= null!; //金钱倍数
    public MelonPreferences_Entry<bool> UpgradeDay1= null!; //门派升级一天
    public MelonPreferences_Entry<bool> JianBaoFlag= null!; //一眼鉴宝
    public MelonPreferences_Entry<float> LivingSkillExpRate= null!; //生活经验倍率
    public MelonPreferences_Entry<int> MaxLivingSkillExpTimes= null!; //生活潜力倍数
    public MelonPreferences_Entry<float> FavorMax= null!; //最大好感度
    public MelonPreferences_Entry<int> MaxSpeBuildingNum= null!; //最大特殊建筑数
    public MelonPreferences_Entry<bool> AutoJianBaoFlag= null!; //自动鉴宝
    public MelonPreferences_Entry<bool> TeachAnyNewSkill= null!; //传授任意技能
    public MelonPreferences_Entry<bool> RemoveAnySkill= null!; //遗忘任意技能
    private MelonPreferences_Entry<bool> _breakRollFlag= null!; //Roll开关
    public MelonPreferences_Entry<float> BattleChangeSkillFightRate= null!; //实战经验倍率
    public MelonPreferences_Entry<float> ZhongyuanLv= null!; //鬼市商店等级
    public MelonPreferences_Entry<float> ChanDaoRate= null!; //禅宗道法修行倍率
    public MelonPreferences_Entry<string> ForceSpeFunctions= null!; // 门派特性
    public MelonPreferences_Entry<string> BuildingTimesMapStr= null!; // 建筑倍率映射 "索引:倍率,索引:倍率"
    public MelonPreferences_Entry<bool> PoisonNumReduceFlag= null!; // 淬毒值消耗开关
    public MelonPreferences_Entry<bool> TimeFreezeFlag= null!; // 时间停止
    public MelonPreferences_Entry<bool> DrinkOneWinFlag= null!; // 斗酒一轮必胜
    public MelonPreferences_Entry<float> ExpRateMultiplier= null!; // 游戏难度经验倍率
    public MelonPreferences_Entry<bool> ExpRateMultiplierSelfForceFlag= null!; // 游戏难度经验倍率是否对自己门派生效
    public MelonPreferences_Entry<bool> GoodTreasure= null!; // 珍宝品质修改当前等级全红
    public MelonPreferences_Entry<float> ForceContributionRate= null!; // 非本门派功绩倍率
    public MelonPreferences_Entry<float> GovernContributionRate= null!; // 官府功绩倍率
    public MelonPreferences_Entry<bool> BattleSkipFlag= null!; // 跳过战斗
    public MelonPreferences_Entry<bool> BattleSkipAddExpFlag= null!; // 跳过战斗加经验
    public MelonPreferences_Entry<bool> BreakMaxLimitFlag= null!; // 突破潜力限制
    public MelonPreferences_Entry<bool> RedQuality= null!; // 获得所有物品都是红品质
    public MelonPreferences_Entry<bool> NewGameTagNumFlag = null!; // 获得所有物品都是红品质
    public MelonPreferences_Entry<bool> AnyTagFlag = null!; // 天赋无视要求和前置
    public MelonPreferences_Entry<bool> NewGameAnyTagFlag = null!; // 新档天赋无视要求和前置
    public MelonPreferences_Entry<bool> ExternalStorageFlag = null!; // 藏宝阁价值容量锁定1亿开关
    public MelonPreferences_Entry<bool> DodgeHitFlag = null!; // 轻功训练不受击
    public MelonPreferences_Entry<bool> DrinkUiAutoFillFlag = null!; // 喝酒自动倒满
    public MelonPreferences_Entry<bool> ExploreSeeAllFlag = null!; // 探险去除迷雾
    public MelonPreferences_Entry<bool> ExploreFreeMoveFlag = null!; // 探险随意移动
    public MelonPreferences_Entry<bool> PoisonTime1Flag = null!; // 毒相关消耗1天
    public MelonPreferences_Entry<int> TeammateLeaveDay = null!; // 队友离队时间
    public MelonPreferences_Entry<int> PlayerMaxTagNum = null!; // 玩家天赋数量上限
    public MelonPreferences_Entry<int> NpcMaxTagNum = null!; // NPC天赋数量上限
    // public MelonPreferences_Entry<int> KungFuMaxLimitTimes = null!; // 武学修炼数量限制倍数
    public MelonPreferences_Entry<bool> AddSpeBuildingsFlag = null!; // 添加特殊建筑开关
    public MelonPreferences_Entry<bool> BattleMaxTime999Flag = null!; // 战斗最大回合数999开关
    public MelonPreferences_Entry<bool> AutoReadBookFlag = null!; // 一键阅读开关
    public MelonPreferences_Entry<bool> SwordPoolEasyFlag = null!; // 剑池天工 耗时1天 只用1块

    public MelonPreferences_Entry<int> SpecifiedSkinId = null!; // 指定的服装ID
    public MelonPreferences_Entry<bool> Dlc0Flag = null!; // DLC0解锁开关
    public MelonPreferences_Entry<int> NewSaveSliderMin = null!; // 新档自定义难度滑块最小值
    public MelonPreferences_Entry<int> NewSaveSliderMax = null!; // 新档自定义难度滑块最大值
    public MelonPreferences_Entry<bool> AnyDifficultUnlockAchFlag = null!; // 任意难度都可以解锁成就
    public MelonPreferences_Entry<bool> BreakMaxLimitNotForPlayerFlag = null!; // 突破潜力限制不对玩家生效
    public MelonPreferences_Entry<bool> WeatherLockSunnyFlag = null!; // 天气锁定晴天
    public MelonPreferences_Entry<bool> FastRemoveTag = null!; // 快速遗忘天赋
    public MelonPreferences_Entry<bool> Relation999Flag = null!; // 友人/结义/情侣上限99
    public MelonPreferences_Entry<bool> LoyalLockFlag = null!; // 自门派忠诚不减
    public MelonPreferences_Entry<bool> EnableChangeForceJobCdZero = null!; // 门派职位变更cd0开关
    public MelonPreferences_Entry<bool> EnableNpcEquipAndSkill = null!; // Npc管理装备和技能
    public MelonPreferences_Entry<bool> EnablePrivateHouseNpcTag = null!; // 私宅管理npc天赋
    public MelonPreferences_Entry<bool> BzemFlag = null!; // 不增恶名
    public MelonPreferences_Entry<bool> PlayerLoverUnHappyEventFlag = null!; // 玩家是否让自己的伴侣不开心了
    public MelonPreferences_Entry<bool> PlayerAllBreakThroughFlag = null!; // 玩家突破全词条开关
    public MelonPreferences_Entry<bool> BookWriterSelfFlag = null!; // 私宅抄书/默写开关
    public MelonPreferences_Entry<bool> AskHeroJoinForceFlag = null!; // 请人物加入玩家门派是否收为徒弟
    public MelonPreferences_Entry<bool> AuctionAutoOfferFlag = null!; // 拍卖会自动出价
    
    
    private MelonPreferences_Entry<bool> _useModifier = null!; // 使用组合键
    private MelonPreferences_Entry<KeyCode> _key1 = null!; // 第一个键
    private MelonPreferences_Entry<KeyCode> _key2 = null!; // 第二个键
    public MelonPreferences_Entry<float> WindowScaling = null!; // 窗体缩放百分比
    
    #endregion
    
    // 其他数据
    private HeroData? _readedHeroData;
    public string BreakChoiceListStr = "";// 随机选择列表
    public bool BreakChoiceFlag;// 突破选择类型和数值修改
    public bool BreakFlag;// 突破指定类型和数值修改
    public string BreakType = "0";// 属性类别
    public string BreakValue = "5";// 属性数值
    public bool RedMaterial;//必定获得红材料
    public string MaterialAttr = "6=20;70=0.2;131=0.2;132=0.2";//材料属性
    public bool MaxAreaFlag; //是否仙霞初建存档地块最大化
    public bool MaxAreaFlag1; //是否需要城墙
    
    public static int ExpRate;
    public static int FameRate;
    public static int MaxweightRate;
    public static int SelfforceExpRate;
    public static int OtherforceExpRate;
    public static int RandomEnemyStrength;
    public static int RandomEnemyNum;
    public static int BadfameRate;
    public static int MaxSkillNum;
    public static int TeammateLimit;
    public static int AiForceDevelopSpeed;
    public static float TimeDifficulty;
    
    
    
    // GUI状态
    private Vector2 _mainScrollPos;
    private const float Hight = 1000;
    private const int Width = 590;
    private Rect _mainWindowRect = new(50, 50, Width, Hight);
    private bool _showMainWindow;
    private readonly string[] _tabNames = { "功能开关", "属性ID", "门派特性和建筑效果" };
    private int _selectedTab;
    private bool _isCapturingMainWindowPointer;
    private static GUIStyle _windowStyle = null!;
    private static GUIStyle _titleBarStyle = null!;
    private static GUIStyle _closeButtonStyle = null!;
    private static bool _windowStyleInitialized;
        
        
    public override void OnInitializeMelon()
    {
        Instance = this;
        _otherCategory = MelonPreferences.CreateCategory("UIConfig", "UI配置");
        _otherCategory.SetFilePath(MelonEnvironment.UserDataDirectory + "\\LYModConfig.cfg");
        MainCategory = MelonPreferences.CreateCategory("LYModConfig", "功能配置");
        MainCategory.SetFilePath(MelonEnvironment.UserDataDirectory + "\\LYModConfig.cfg");
        
        #region 配置项
        _useModifier = _otherCategory.CreateEntry("_useModifier", true,  description: "使用组合键");
        _key1 = _otherCategory.CreateEntry("_key1", KeyCode.LeftAlt,  description: "键1");
        _key2 = _otherCategory.CreateEntry("_key2", KeyCode.E,  description: "键2");
        WindowScaling = _otherCategory.CreateEntry("WindowScaling", 1.0f,  description: "窗体缩放百分比");
        StudyFightRate = MainCategory.CreateEntry("studyFightRate", 1.0f,  description: "练功房学习战斗经验倍率");
        StudyUniqeRate = MainCategory.CreateEntry("studyUniqeRate", 1.0f,  description: "闭关室学习理论经验倍率");
        ReadBook = MainCategory.CreateEntry("readBookRate", 1.0f,  description: "读书倍率");
        RedBreak = MainCategory.CreateEntry("redBreakRate",1.0f,  description: "突破倍率");
        Pzqh = MainCategory.CreateEntry("pzlRate", 1.0f,  description: "烹饪铸造炼药倍率");
        HorseMaxWeightTimes = MainCategory.CreateEntry("HorseMaxWeightTimes", 1, description:"马和马鞍负重的倍数");
        HorseMaxSeeRangeTimes = MainCategory.CreateEntry("HorseMaxSeeRangeTimes", 1, description:"马和马鞍视野范围的倍数");
        HorseStepAddRateTimes = MainCategory.CreateEntry("HorseStepAddRateTimes", 1, description:"马和马鞍探险耐力加成倍数");
        EquipmentWeight = MainCategory.CreateEntry("equipmentWeight", 1.0f, description: "装备负重清零");
        ShopLvRate = MainCategory.CreateEntry("shopLvRate", 1.0f,  description: "拍卖会品质倍率");
        ItemNum = MainCategory.CreateEntry("itemNum", -1,  description: "拍卖会物品数量");
        FavorTimes = MainCategory.CreateEntry("favorTimes", 1,  description: "好感倍数");
        MoneyTimes = MainCategory.CreateEntry("MoneyTimes", 1,  description: "金钱倍数");
        LivingSkillExpRate = MainCategory.CreateEntry("LivingSkillExpRate", 1.0f, description:"生活经验倍率");
        MaxLivingSkillExpTimes = MainCategory.CreateEntry("MaxLivingSkillExpRate", 1, description:"生活潜力倍数");
        FavorMax = MainCategory.CreateEntry("FavorMax", 100.0f, description:"最大好感度");
        MaxSpeBuildingNum = MainCategory.CreateEntry("MaxSpeBuildingNum", 5, description:"特殊建筑限制数");
        BattleChangeSkillFightRate = MainCategory.CreateEntry("BattleChangeSkillFightRate", 1.0f, description:"实战经验倍率");
        ZhongyuanLv = MainCategory.CreateEntry("ZhongyuanLv", -1.0f, description:"鬼市商店等级");
        ChanDaoRate = MainCategory.CreateEntry("ChanDaoRate", 1.0f, description:"禅宗道法修行倍率");
        ForceSpeFunctions = MainCategory.CreateEntry("ForceSpeFunctions", "", description:"选择的门派特性");
        BuildingTimesMapStr = MainCategory.CreateEntry("BuildingTimesMapStr", "",description:"建筑倍率映射 格式:索引:倍率,索引:倍率");
        ExpRateMultiplier = MainCategory.CreateEntry("ExpRateMultiplier", 1.0f, description:"游戏难度经验倍率,最高难度非本门经验倍率1.6（+60%）,这里默认2（+100%）");
        ForceContributionRate = MainCategory.CreateEntry("ForceContributionRate", 1.0f,description:"非本门功绩倍率");
        GovernContributionRate = MainCategory.CreateEntry("GovernContributionRate", 1.0f,description:"官府功绩倍率");
        TeammateLeaveDay = MainCategory.CreateEntry("TeammateLeaveDay", 30,description:"队友自动离队天数");
        PlayerMaxTagNum = MainCategory.CreateEntry("PlayerMaxTagNum", 9,description:"玩家天赋数量上限");
        NpcMaxTagNum = MainCategory.CreateEntry("NpcMaxTagNum", 9,description:"Npc天赋数量上限");
        // KungFuMaxLimitTimes = MainCategory.CreateEntry("KungFuMaxLimitTimes", 1,description:"武学修炼限制倍数");
        SpecifiedSkinId = MainCategory.CreateEntry("SpecifiedSkinId", 99999,description:"指定的服装ID（99999为默认不修改）");
        NewSaveSliderMin = MainCategory.CreateEntry("NewSaveSliderMin", -5, description:"新档自定义难度滑块最小值");
        NewSaveSliderMax = MainCategory.CreateEntry("NewSaveSliderMax", 5, description:"新档自定义难度滑块最大值");
        
        PoisonNumReduceFlag = MainCategory.CreateEntry("PoisonNumReduceFlag", false, description:"淬毒消耗开关");
        UpgradeDay1 = MainCategory.CreateEntry("upgrade1", false, description:"升级一天");
        CopyBookFlag = MainCategory.CreateEntry("copyBookFlag", false, description:"抄书一天");
        ReasearchFlag = MainCategory.CreateEntry("reaserchFlag", false, description:"研究一天");
        TeachNewSkillToNpc = MainCategory.CreateEntry("teachNewSkillToNPCFull",false,  description: "传授满级");
        TeachNpc = MainCategory.CreateEntry("teachNPCToFull",false,  description: "指点满级");
        Explore = MainCategory.CreateEntry("explore", false,  description: "探险耐力锁定");
        Interaction = MainCategory.CreateEntry("interaction", false,  description: "无限指点传授");
        RedBook = MainCategory.CreateEntry("redBook", false,  description: "必定获得完本");
        StealRate = MainCategory.CreateEntry("stealRate", false,  description: "偷窃偷师必成功");
        Hgbj = MainCategory.CreateEntry("hfbj", false,  description: "好感度不会减少");
        Cost0 = MainCategory.CreateEntry("cost0", false,  description: "建筑升级资源零消耗");
        BuildingDestroyFlag = MainCategory.CreateEntry("buildingDestroyFlag", true,  description: "建筑可拆除开关");
        RedTreasure = MainCategory.CreateEntry("redTreasure", false,  description: "必定是红色珍宝");
        JianBaoFlag = MainCategory.CreateEntry("JianBaoFlag", false,  description: "一眼看穿宝物品质");
        AutoJianBaoFlag = MainCategory.CreateEntry("AutoJianBaoFlag", false,  description: "自动鉴宝");
        _breakRollFlag = MainCategory.CreateEntry("BreakRollFlag", false,  description: "Roll开关");
        TeachAnyNewSkill = MainCategory.CreateEntry("TeachAnyNewSkill", false,  description: "传授任意等级技能");
        RemoveAnySkill = MainCategory.CreateEntry("RemoveAnySkill", false,  description: "遗忘任意等级技能");
        TimeFreezeFlag = MainCategory.CreateEntry("TimeFreezeFlag", false,  description: "时间停止");
        DrinkOneWinFlag = MainCategory.CreateEntry("DrinkOneWinFlag", false,  description: "斗酒一轮必胜");
        DrinkUiAutoFillFlag = MainCategory.CreateEntry("DrinkUiAutoFillFlag", false,  description: "喝酒自动倒满");
        GoodTreasure = MainCategory.CreateEntry("GoodTreasure", false,  description: "珍宝等级不变品质变红");
        BattleSkipFlag = MainCategory.CreateEntry("BattleSkipFlag", false,  description: "跳过战斗");
        BattleSkipAddExpFlag = MainCategory.CreateEntry("BattleSkipAddExpFlag", true,  description: "跳过战斗加经验开关");
        BreakMaxLimitFlag = MainCategory.CreateEntry("BreakMaxLimitFlag", false,  description: "突破潜力限制");
        RedQuality = MainCategory.CreateEntry("RedQuality", false,  description: "获得所有物品品质都是红");
        NewGameTagNumFlag = MainCategory.CreateEntry("NewGameTagNumFlag", false,  description: "新档天赋点数999");
        AnyTagFlag = MainCategory.CreateEntry("AnyTagFlag", false,  description: "天赋无视前置要求");
        NewGameAnyTagFlag = MainCategory.CreateEntry("NewGameAnyTagFlag", false,  description: "新档天赋无视前置要求");
        ExternalStorageFlag = MainCategory.CreateEntry("ExternalStorageFlag", false,  description: "藏宝阁价值容量锁定1亿开关");
        DodgeHitFlag = MainCategory.CreateEntry("DodgeHitFlag", false,  description: "轻功训练不受击");
        ExploreSeeAllFlag = MainCategory.CreateEntry("ExploreSeeAllFlag", false,  description: "探险去除迷雾");
        ExploreFreeMoveFlag = MainCategory.CreateEntry("ExploreFreeMoveFlag", false,  description: "探险随意移动");
        ReadBookChangePatient1Flag = MainCategory.CreateEntry("ReadBookChangePatient1Flag", false,  description: "读书耐心减1");
        PoisonTime1Flag = MainCategory.CreateEntry("PoisonTime1Flag", false,  description: "毒相关消耗1天");
        AddSpeBuildingsFlag = MainCategory.CreateEntry("AddSpeBuildingsFlag", false,  description: "添加可建造的特殊建筑开关");
        BattleMaxTime999Flag = MainCategory.CreateEntry("BattleMaxTime999Flag", false,  description: "战斗最大回合数999");
        AutoReadBookFlag = MainCategory.CreateEntry("AutoReadBookFlag", false,  description: "一键阅读开关");
        ExpRateMultiplierSelfForceFlag = MainCategory.CreateEntry("ExpRateMultiplierSelfForceFlag", false,  description: "游戏难度经验倍率是否对自己门派生效");
        SwordPoolEasyFlag = MainCategory.CreateEntry("SwordPoolEasyFlag", false,  description: "剑池天工 耗时1天 只用1块");
        Dlc0Flag = MainCategory.CreateEntry("Dlc0Flag", true,  description: "Dlc0解锁开关");
        AnyDifficultUnlockAchFlag = MainCategory.CreateEntry("AnyDifficultUnlockACHFlag", false,  description: "任意难度都可解锁成功");
        BreakMaxLimitNotForPlayerFlag = MainCategory.CreateEntry("BreakMaxLimitNotForPlayerFlag", false,  description: "突破潜力限制不对玩家生效");
        WeatherLockSunnyFlag = MainCategory.CreateEntry("WeatherLockSunnyFlag", false,  description: "天气锁定晴天开关");
        FastRemoveTag = MainCategory.CreateEntry("FastRemoveTag", false,  description: "快速遗忘天赋开关");
        Relation999Flag = MainCategory.CreateEntry("Relation999Flag", false,  description: "友人/结义/情侣上限99");
        LoyalLockFlag = MainCategory.CreateEntry("LoyalLockFlag", false,  description: "自门派忠诚度不减");
        EnableNpcEquipAndSkill = MainCategory.CreateEntry("EnableNpcEquipAndSkill", false,  description: "Npc管理装备和技能开关");
        EnablePrivateHouseNpcTag = MainCategory.CreateEntry("EnablePrivateHouseNpcTag", false,  description: "私宅管理npc天赋");
        EnableChangeForceJobCdZero = MainCategory.CreateEntry("EnableChangeForceJobCDZero", false,  description: "门派职位变更cd0开关");
        BzemFlag = MainCategory.CreateEntry("bzemFlag", false,  description: "不增恶名");
        PlayerLoverUnHappyEventFlag = MainCategory.CreateEntry("PlayerLoverUnHappyEventFlag", false,  description: "玩家是否让自己的伴侣不开心了");
        PlayerAllBreakThroughFlag = MainCategory.CreateEntry("PlayerAllBreakThroughFlag", false,  description: "玩家突破全词条开关");
        BookWriterSelfFlag = MainCategory.CreateEntry("BookWriterSelfFlag", false,  description: "私宅抄书默写开关");
        AskHeroJoinForceFlag = MainCategory.CreateEntry("AskHeroJoinForceFlag", false,  description: "请人物加入玩家门派是否收为徒弟");
        AuctionAutoOfferFlag = MainCategory.CreateEntry("AuctionAutoOfferFlag", false,  description: "拍卖会自动出价");
        #endregion
      
        var harmony = new HarmonyLib.Harmony("LYMod");
        harmony.PatchAll(typeof(ReadBookControllerPatches));
        harmony.PatchAll(typeof(ReadBookAutoReadPatches));
        harmony.PatchAll(typeof(ItemListDataPatches));
        harmony.PatchAll(typeof(BreakThroughControllerPatches));
        harmony.PatchAll(typeof(ForceDataPatches));
        harmony.PatchAll(typeof(ExploreControllerPatches));
        harmony.PatchAll(typeof(PlotControllerPatches));
        harmony.PatchAll(typeof(CraftingPatches));
        harmony.PatchAll(typeof(HeroDataPatch));
        harmony.PatchAll(typeof(StudySkillPatches));
        harmony.PatchAll(typeof(BookWriterUIControllerPatches));
        harmony.PatchAll(typeof(BreakThroughChoiceControllerPatch));
        harmony.PatchAll(typeof(AreaBuildingDataPatches));
        harmony.PatchAll(typeof(ManageTagControllerPatches));
        harmony.PatchAll(typeof(LivingSkillPatches));
        harmony.PatchAll(typeof(IdentifyMatchControllerPatches));
        harmony.PatchAll(typeof(ChooseControllerPatches));
        harmony.PatchAll(typeof(MeditationDataPatches));
        harmony.PatchAll(typeof(PoisonPatches));
        harmony.PatchAll(typeof(GameControllerPatches));
        harmony.PatchAll(typeof(TestPatches));
        harmony.PatchAll(typeof(UIPatches));
        harmony.PatchAll(typeof(BattleSkip));
        harmony.PatchAll(typeof(GameDataControllerPatches));
        harmony.PatchAll(typeof(StudyDodgePlayerPatches));
        harmony.PatchAll(typeof(DrinkUIControllerPatches));
        harmony.PatchAll(typeof(RollHelper));
        harmony.PatchAll(typeof(SpeEnhanceEquipControllerPatches));
        harmony.PatchAll(typeof(GameCustomDifficultyPatches));
        harmony.PatchAll(typeof(AutoChangeSkinPatches));
        harmony.PatchAll(typeof(LoadSavePostPatches));
        harmony.PatchAll(typeof(WeatherControllerPatches));
        harmony.PatchAll(typeof(TagRemoveConfirmPatches));
        harmony.PatchAll(typeof(BookWriterPatches));
        harmony.PatchAll(typeof(AskHeroJoinForcePatches));
        
        LOG.Msg("===================================================");
        LOG.Msg("【LYMod】LYMod is loaded! 默认打开窗体：左alt + e !");
        LOG.Msg("===================================================");
        
    }

    private bool IsOpenWindowTriggered()
    {
        if (!_useModifier.Value)
        {
            return Input.GetKeyDown(_key1.Value);
        }
        return Input.GetKey(_key1.Value) && Input.GetKeyDown(_key2.Value);
    }

    public override void OnUpdate()
    {
        ReadBookAutoReadPatches.HandleAutoReadButton();

        // Alt+E 切换主面板
        if (IsOpenWindowTriggered())
        {
            _showMainWindow = !_showMainWindow;
            _isCapturingMainWindowPointer = false;

            UIBuilderExtensions.RefreshForceList();
            UIBuilderExtensions.RefreshBuildingList();
            if (_showMainWindow)
            {
                HeroHelper.TryReadNowHero(out _readedHeroData);
                GameCustomDifficultyPatches.GetCustomDifficultyData();
            }

            // OtherHelper.ChaneMaxNum();
            if (Relation999Flag.Value)
            {
                GlobalData.MaxLoverNum = 99;
                GlobalData.MaxFriendNum = 99;
                GlobalData.MaxBrotherNum = 99;
            }
            else
            {
                GlobalData.MaxLoverNum = 4;
                GlobalData.MaxBrotherNum = 4;
                GlobalData.MaxFriendNum = 16;
            }
        }

        // 按 R 重刷几个可复用的 Roll 场景
        if (Input.GetKeyDown(KeyCode.R) && _breakRollFlag.Value)
        {
            RollHelper.TryBreakThoughtRoll();
            RollHelper.TryCraftRoll();
            RollHelper.TryAuctionRoll();
            RollHelper.TryZhongyuanRoll();
            RollHelper.TryRefreshRecruitList();
            RollHelper.TrySpePoisonRoll();
            RollHelper.TryFightMatchRewardRoll();
        }
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
           
        }
    }
 
    
    public override void OnGUI()
    {
        var scale = WindowScaling.Value;
        var baseFontSize = 18;
        var scaledFontSize = (int)(baseFontSize * scale);
        
        GUI.skin.label.fontSize = scaledFontSize;
        GUI.skin.button.fontSize = scaledFontSize;
        GUI.skin.toggle.fontSize = scaledFontSize;
        GUI.skin.textField.fontSize = scaledFontSize;

        if (!_showMainWindow)
        {
            _isCapturingMainWindowPointer = false;
            return;
        }
        
        var currentEvent = Event.current;
        // IMGUI 先决定这次鼠标事件是否由 MOD 窗口接管
        var shouldConsumePointerEvent = UpdateMainWindowPointerCapture(currentEvent);
        var scaledWidth = Width * scale;
        var scaledHeight = Hight * scale;
        _mainWindowRect = new Rect(_mainWindowRect.x, _mainWindowRect.y, scaledWidth, scaledHeight);
        _mainWindowRect = GUI.ModalWindow(0, _mainWindowRect, (GUI.WindowFunction)DrawMainWindow, "");

        if (shouldConsumePointerEvent && currentEvent != null)
        {
            // 只消费当前已由 IMGUI 面板接管的鼠标事件，键盘不在这里处理
            currentEvent.Use();
        }
    }

    #region  防止点穿窗体的方法
    
    public bool ShouldBlockGamePointerInput()
    {
        return _showMainWindow && (_isCapturingMainWindowPointer || IsPointerInsideMainWindow(ToGuiMousePosition(Input.mousePosition)));
    }

    private static Vector2 ToGuiMousePosition(Vector3 mousePosition)
    {
        return new Vector2(mousePosition.x, Screen.height - mousePosition.y);
    }

    private bool IsPointerInsideMainWindow(Vector2 guiMousePosition)
    {
        return _showMainWindow && _mainWindowRect.Contains(guiMousePosition);
    }

    private bool UpdateMainWindowPointerCapture(Event? currentEvent)
    {
        if (!_showMainWindow)
        {
            _isCapturingMainWindowPointer = false;
            return false;
        }

        if (currentEvent == null)
        {
            return false;
        }

        var isInsideWindow = IsPointerInsideMainWindow(currentEvent.mousePosition);
        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                // 在窗内按下后，直到 MouseUp 前都持续认为由窗口接管
                if (isInsideWindow)
                {
                    _isCapturingMainWindowPointer = true;
                    return true;
                }
                return false;
            case EventType.MouseDrag:
                if (_isCapturingMainWindowPointer || isInsideWindow)
                {
                    _isCapturingMainWindowPointer = true;
                    return true;
                }
                return false;
            case EventType.MouseMove:
            case EventType.ContextClick:
            case EventType.ScrollWheel:
                return _isCapturingMainWindowPointer || isInsideWindow;
            case EventType.MouseUp:
            {
                var shouldBlock = _isCapturingMainWindowPointer || isInsideWindow;
                _isCapturingMainWindowPointer = false;
                return shouldBlock;
            }
            default:
                return false;
        }
    }
    #endregion
    
    private void InitWindowStyle()
    {
        if (_windowStyleInitialized && _windowStyle.normal?.background != null) return;
        
        var bgTex = new Texture2D(2, 2);
        var pixels = new Color[4];
        for (int i = 0; i < 4; i++)
            pixels[i] = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        bgTex.SetPixels(pixels);
        bgTex.Apply();
        
        _windowStyle = new GUIStyle(GUI.skin.window)
        {
            normal = { background = bgTex, textColor = Color.white }
        };
        
        _titleBarStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = null, textColor = Color.white },
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(5, 5, 5, 5),
            margin = new RectOffset(0, 0, 0, 0)
        };
        
        _closeButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            padding = new RectOffset(5, 5, 2, 2),
            margin = new RectOffset(0, 0, 0, 0)
        };
        
        _windowStyleInitialized = true;
    }
   
    private void DrawMainWindow(int windowId)
    {
        var scale = WindowScaling.Value;
        var scaledWidth = (Width - 30) * scale;
        var scaledHeight = (Hight * scale) - (70 * scale);
       
        InitWindowStyle();
        GUI.Box(new Rect(0, 0, _mainWindowRect.width, _mainWindowRect.height), "", _windowStyle);
        
        var titleBarHeight = 30 * scale;
        var titleBarRect = new Rect(0, 0, _mainWindowRect.width, titleBarHeight);
        
        GUI.Box(titleBarRect, "", _titleBarStyle);
        
        var titleText = $"LYMod {ModConfig.ModVersion} (支持游戏版本{ModConfig.GamveVersion})";
        var titleSize = _titleBarStyle.CalcSize(new GUIContent(titleText));
        var titlePos = new Vector2((_mainWindowRect.width - titleSize.x) / 2, (titleBarHeight - titleSize.y) / 2);
        GUI.Label(new Rect(titlePos.x, titlePos.y, titleSize.x, titleSize.y), titleText, _titleBarStyle);
        
        var closeButtonSize = new Vector2(50 * scale, 20 * scale);
        var closeButtonRect = new Rect(_mainWindowRect.width - closeButtonSize.x - 5 * scale, 
                                       (titleBarHeight - closeButtonSize.y) / 2, 
                                       closeButtonSize.x, 
                                       closeButtonSize.y);
        if (GUI.Button(closeButtonRect, "✕", _closeButtonStyle))
        {
            _showMainWindow = false;
        }
        
        GUI.DragWindow(titleBarRect);
        
        GUILayout.Space(10 * scale);
        // 标签页
        GUILayout.BeginHorizontal();
        for (var i = 0; i < _tabNames.Length; i++)
        {
            if (GUILayout.Toggle(_selectedTab == i, _tabNames[i], "Button")) _selectedTab = i;
            GUILayout.Space(10 * scale);
        }

        GUILayout.EndHorizontal();
        // 主滚动区域
        _mainScrollPos = GUILayout.BeginScrollView(_mainScrollPos, GUILayout.Width(scaledWidth), GUILayout.Height(scaledHeight));
        
        // 根据标签页绘制内容
        switch (_selectedTab)
        {
            case 0: // 功能开关
                DrawMainTab();
                break;
            case 1: // 属性ID
                OtherElement.Label();
                break;
            case 2: // 门派特性
                OtherElement.ForceSpeFunction();
                break;
        }

        GUILayout.EndScrollView();
    }

    #region 主界面
    
    private void DrawMainTab()
    {
        var scale = WindowScaling.Value;
        var builder = UIHelper.CreateBuilder(scale);

        builder.AddButtonRow("重置所有", OtherHelper.ResetAllMainConfig, 100);
        
        builder.BeginFoldout("人物相关").Space(10)
            .BeginHorizontal()
            .AddButton("读取人物", () =>
            {
                HeroHelper.TryReadNowHero(out _readedHeroData);
            }, width:100)
            .AddButton("刷新玩家月限制", () =>
            {
                if (HeroHelper.TryReadPlayer(out var player))
                    player.playerInteractionTimeData.ResetTime();
            }, 175)
            .AddButton("解锁所有服装", HeroHelper.UnlockSkins,150)
            .EndHorizontal().Space(5)
            .BeginHorizontal()
            .AddAutoSave("友人/结义/情侣上限99", Relation999Flag)
            .EndHorizontal()
            .BeginHorizontal()
            .AddInfoRow(60,
                new InfoItem("ID：", _readedHeroData?.heroID),
                new InfoItem("姓名：", _readedHeroData?.heroName),
                new InfoItem("年龄：", _readedHeroData?.age)
            )
            .AddInfoRow(60, new InfoItem("天赋：", _readedHeroData?.talent > 4 ? _readedHeroData?.talent : GlobalData.TalentText[_readedHeroData?.talent ?? 0]))
            .AddButton("+", () =>
            {
                if (_readedHeroData == null) return;
                if (_readedHeroData.talent < 4) _readedHeroData.talent += 1;
                if (_readedHeroData.talent > 4)  _readedHeroData.talent = 4;
            })
            .EndHorizontal()
            .BeginHorizontal()
            .AddButton("添加喜好佳肴/丹药", () =>
            {
                if (_readedHeroData == null) return;
                
                _readedHeroData.hobby ??= new Il2CppSystem.Collections.Generic.List<int>();
                if (!_readedHeroData.hobby.Contains(10))
                {
                    _readedHeroData.hobby.Add(10);
                }
                if (!_readedHeroData.hobby.Contains(20))
                {
                    _readedHeroData.hobby.Add(20);
                }
            }, width:180)
            .EndHorizontal()
            .Space(5)
            .BeginHorizontal()
            .AddLabel("天赋点数：",100).AddLabel(_readedHeroData?.heroTagPoint.ToString(CultureInfo.InvariantCulture) ?? "")
            .AddButton("+100", () =>
            {
                _readedHeroData?.ChangeTagPoint(100,  true);
            }, 60)
            .EndHorizontal()
            .Space(5)
            // .AddAutoSaveRow("无前置天赋要求", AnyTagFlag, "武学修炼限制倍数", KungFuMaxLimitTimes, labelWidth:150)
            .AddAutoSave("无前置天赋要求", AnyTagFlag)
            .AddAutoSaveRow("在闭关/私宅快速移除天赋", FastRemoveTag, "有关系的Npc技能和装备管理", EnableNpcEquipAndSkill)
            .AddAutoSaveRow("私宅管理有关系的NPC天赋", EnablePrivateHouseNpcTag, "私宅抄书默写", BookWriterSelfFlag)
            .AddAutoSave("请人物加入玩家门派时需确认是否收徒", AskHeroJoinForceFlag)
           
            .BeginHorizontal()
            
            .AddButton("私宅设置4个槽位", () =>
            {
                var list = GameController.Instance.worldData.playerBookWriter;
                var bwd = new BookWriterData();
                var bwd1 = new BookWriterData();
                var bwd2 = new BookWriterData();
                bwd1.Reset();
                list.Add(bwd);
                list.Add(bwd1);
                list.Add(bwd2);
            }, width:200)
            .AddButton("还原私宅1个槽位", () =>
            {
                var list = GameController.Instance.worldData.playerBookWriter;
                list.RemoveAt(3);
                list.RemoveAt(2);
                list.RemoveAt(1);
            }, width:200)
            .EndHorizontal()
           
            .AddAutoSaveRow("玩家天赋数量上限", PlayerMaxTagNum, "Npc天赋数量上限", NpcMaxTagNum,  labelWidth:150)
            .AddAutoSaveRow("属性未达限制时不受潜力限制",BreakMaxLimitFlag, "该项不对玩家生效", BreakMaxLimitNotForPlayerFlag)
            .AddLabelRow("修改读取到人物的潜力：", 200)
            .Space(5)
            .BeginHorizontal()
            .AddButton("基本属性潜力120", () =>
            {
                if (_readedHeroData == null) return;
                var list = _readedHeroData.maxAttri;
                for (var i = 0; i < list.Count; i++)
                {
                    list[i] = 120;
                }
            }, 170)
            .Space(3)
            .AddButton("战斗技能潜力120", () =>
            {
                if (_readedHeroData == null) return;
                var list = _readedHeroData.maxFightSkill;
                for (var i = 0; i < list.Count; i++)
                {
                    list[i] = 120;
                }
            }, 170)
            .Space(5)
            .AddButton("生活技能潜力100", () =>
            {
                if (_readedHeroData == null) return;
                var list = _readedHeroData.maxLivingSkill;
                for (var i = 0; i < list.Count; i++)
                {
                    list[i] = 100;
                }
            }, 170)
            .EndHorizontal()
            .AddLabelRow("装备马的数据:", 125)
            .BeginHorizontal()
            .AddLinkedFloat("速度：", () => _readedHeroData?.horse?.horseData.speed ?? 0f, val =>
            {
                if (_readedHeroData?.horse != null)_readedHeroData.horse.horseData.speed = val;
            }, "hs_speed",60,50)
            .AddLinkedFloat("冲刺：", () => _readedHeroData?.horse?.horseData.sprint ?? 0f, val =>
            {
                if (_readedHeroData?.horse != null)_readedHeroData.horse.horseData.sprint = val;
            }, "hs_sprint",60,50)
            .AddLinkedFloat("耐力：", () => _readedHeroData?.horse?.horseData.power ?? 0f, val =>
            {
                if (_readedHeroData?.horse != null)_readedHeroData.horse.horseData.power = val;
            }, "hs_power",60,50)
            .AddLinkedFloat("坚韧：", () => _readedHeroData?.horse?.horseData.resist ?? 0f, val =>
            {
                if (_readedHeroData?.horse != null)_readedHeroData.horse.horseData.resist = val;
            }, "hs_resist",60,50)
            .EndHorizontal()
            .AddLabelRow("装备马鞍数据:", 125)
            .BeginHorizontal()
            .AddLinkedFloat("速度：", () => _readedHeroData?.horseArmor?.horseData.speed ?? 0f, val =>
            {
                if (_readedHeroData?.horseArmor != null)_readedHeroData.horseArmor.horseData.speed = val;
            }, "ha_speed",60,50)
            .AddLinkedFloat("冲刺：", () => _readedHeroData?.horseArmor?.horseData.sprint ?? 0f, val =>
            {
                if (_readedHeroData?.horseArmor != null)_readedHeroData.horseArmor.horseData.sprint = val;
            }, "ha_sprint",60,50)
            .AddLinkedFloat("耐力：", () => _readedHeroData?.horseArmor?.horseData.power ?? 0f, val =>
            {
                if (_readedHeroData?.horseArmor != null)_readedHeroData.horseArmor.horseData.power = val;
            }, "ha_power",60,50)
            .AddLinkedFloat("坚韧：", () => _readedHeroData?.horseArmor?.horseData.resist ?? 0f, val =>
            {
                if (_readedHeroData?.horseArmor != null)_readedHeroData.horseArmor.horseData.resist = val;
            }, "ha_resist",60,50)
            .EndHorizontal()
            .EndFoldout();

        builder.BeginFoldout("个人相关").Space(10)
            .AddAutoSaveRow("练功倍率:", StudyFightRate, "闭关倍率:",  StudyUniqeRate)
            .AddAutoSaveRow("实战倍率:", BattleChangeSkillFightRate,  "读书倍率:", ReadBook)
            .AddAutoSaveRow("读书耐心减1", ReadBookChangePatient1Flag, "毒相关耗时1天", PoisonTime1Flag)
            .AddAutoSave("一键阅读", AutoReadBookFlag)
            .AddAutoSaveRow("突破倍率:", RedBreak, "抄书一天", CopyBookFlag)
            .AddAutoSaveRow("获得金钱倍数:",MoneyTimes, "莫高窟遗忘任意技能", RemoveAnySkill)
            .AddAutoSaveRow("生活经验倍率:", LivingSkillExpRate, "生活潜力倍数:", MaxLivingSkillExpTimes)
            .AddAutoSave("轻功训练不受击", DodgeHitFlag)
            .Space(10)
            .AddLabelRow("突破属性修改方案1：")
            .BeginHorizontal()
            .AddButton("获取当前武学突破随机值", () =>
            {
                var btc = BreakThroughController._instance;
                if (btc != null)
                {
                    var kfsld = btc.targetSkill;
                    var list = kfsld.GetBreakThroughAvailableChoice();
                    List<int> distinctList = new List<int>();
                    foreach (int id in list)
                    {
                        if (!distinctList.Contains(id))
                        {
                            distinctList.Add(id);
                        }
                    }
                    distinctList.Sort();
                    BreakChoiceListStr = string.Join(",", distinctList.ToArray());
                }
            }, 275)
            .EndHorizontal()
            .BeginHorizontal().AddLinkedString("随机值：", ()=> BreakChoiceListStr,val => BreakChoiceListStr = val, "bcls", labelWidth: 75, inputWidth: 225).EndHorizontal()
            .BeginHorizontal().AddLinkedBool("指定随机值", ()=>BreakChoiceFlag, val => BreakChoiceFlag = val, labelWidth:110).EndHorizontal()
            .AddLabelRow("突破属性修改方案2：")
            .BeginHorizontal()
            .AddLinkedString("指定属性类别：", ()=>BreakType, val => BreakType = val, "bt",labelWidth:130, inputWidth:40)
            .Space(10)
            .AddLinkedString("指定属性的值：", ()=>BreakValue, val => BreakValue = val, "bv",labelWidth:130, inputWidth:40)
            .EndHorizontal()
            .BeginHorizontal().AddLinkedBool("突破指定类型和值",()=>BreakFlag, val => BreakFlag = val, labelWidth:170).EndHorizontal()
            .AddLabelRow("突破属性修改方案3：")
            .AddAutoSave("玩家全词条突破", PlayerAllBreakThroughFlag)
            .AddButtonRow("覆盖玩家突破词条", () =>
            {
                MelonCoroutines.Start(OtherHelper.OverrideExtraAddData());
            }, width:180)
            .EndFoldout();
        
        builder.BeginFoldout("门派相关").Space(10)
            .AddButtonRow("刷新门派月限制", () =>
            {
                if (HeroHelper.TryReadPlayer(out var player) && player.belongForceID != -1)
                {
                    var force = player.GetForce();
                    force.forceInteractionTimeData.ResetTime();
                    force.thisMonthAttack = false;
                    force.thisMonthAttackArea = 0;
                }
                
            }, width:175)
            .AddAutoSaveRow("研究一天",ReasearchFlag, "禅道修行倍率:", ChanDaoRate)
            .AddAutoSaveRow("建筑资源零消耗",Cost0, "建造升级移动拆除1天", UpgradeDay1)
            .AddAutoSaveRow("非本门功绩倍率:", ForceContributionRate,"特殊建筑上限", MaxSpeBuildingNum)
            .AddAutoSaveRow("建造添加城镇特殊建筑", AddSpeBuildingsFlag, "剑池天工简单模式", SwordPoolEasyFlag)
            .AddAutoSave("自门派忠诚不减", LoyalLockFlag)
            .AddAutoSaveRow("门派职位变更CD0", EnableChangeForceJobCdZero,"建筑可拆除", BuildingDestroyFlag)
            .AddButtonRow("添加10块陨铁", () =>
            {
                GameController.Instance.worldData.ChangeSpeEnhanceStoneNum(10,true);
            }, width:150)
            .BeginHorizontal()
            .AddButton("添加所有书籍到星辰楼", OtherHelper.GenAllBookToSpeBookStorage, width:200)
            .Space(10)
            .AddButton("从星辰楼移除所有秘籍", () =>
            {
                var gc = GameController.Instance;
                if (gc == null) return;
                gc.worldData.speBookStorage.allItem.Clear();
                SpeBookStorageController.Instance.RefreshBookStorageSpeAdd();
            },width:200)
            .EndHorizontal()
            .EndFoldout();
        
        builder.BeginFoldout("交互相关").Space(10)
            .AddAutoSaveRow("好感不减",Hgbj,"偷窃偷师必成功:", StealRate)
            .AddAutoSaveRow("好感倍数",FavorTimes,"好感上限:", FavorMax)
            .AddAutoSaveRow("指点满级",TeachNpc,"传授满级:", TeachNewSkillToNpc)
            .AddAutoSaveRow("无限交互",Interaction,"传授任意技能:", TeachAnyNewSkill)
            .AddAutoSaveRow("队友离队天数", TeammateLeaveDay, "不加恶名", BzemFlag)
            .AddAutoSave("关闭玩家伴侣不开心事件", PlayerLoverUnHappyEventFlag)
            .EndFoldout();
        
        builder.BeginFoldout("道具相关").Space(10)
            .AddAutoSaveRow("必定获得完本",RedBook,"一眼鉴宝:",JianBaoFlag)
            .AddAutoSaveRow("珍宝品质变红",GoodTreasure,"必定红色珍宝:", RedTreasure)
            .AddAutoSaveRow("马和马鞍负重倍数",HorseMaxWeightTimes,"马和马鞍视野范围加成倍数", HorseMaxSeeRangeTimes)
            .AddAutoSaveRow("马和马鞍探险耐力加成倍数", HorseStepAddRateTimes,"装备负重倍率(0-1):", EquipmentWeight)
            .AddAutoSave("淬毒不减:", PoisonNumReduceFlag)
            .AddAutoSaveRow("拍卖品质倍率",ShopLvRate,"拍卖物品数量:", ItemNum)
            .AddAutoSaveRow("烹饪铸造炼药倍率",Pzqh,"鬼市商店等级:", ZhongyuanLv)
            .AddAutoSave("获得物品时品质是红色", RedQuality, labelWidth:200)
            .BeginHorizontal()
            .AddLinkedBool("指定材料属性：",()=>RedMaterial, val => RedMaterial = val, labelWidth:130)
            .EndHorizontal()
            .BeginHorizontal()
            .AddLinkedString("红材料属性：", ()=>MaterialAttr, val => MaterialAttr = val, "ma",labelWidth:110, inputWidth:400)
            .EndHorizontal()
            .EndFoldout();
        
        builder.BeginFoldout("其他相关").Space(10) 
            .AddAutoSaveRow("官府功绩倍率", GovernContributionRate, "探险耐力锁定", Explore)
            .AddAutoSaveRow("探险去除迷雾", ExploreSeeAllFlag, "探险随意移动", ExploreFreeMoveFlag)
            .AddAutoSaveRow("跳过战斗",BattleSkipFlag, "跳过战斗加经验", BattleSkipAddExpFlag)
            .AddAutoSaveRow("999回合后进入疲劳", BattleMaxTime999Flag, "DLC解锁", Dlc0Flag)
            .AddAutoSaveRow("按R键重新Roll", _breakRollFlag, "时间暂停", TimeFreezeFlag)
            .AddAutoSaveRow("自动鉴宝",AutoJianBaoFlag, "斗酒一回胜利", DrinkOneWinFlag)
            .AddAutoSaveRow("喝酒自动倒满", DrinkUiAutoFillFlag, "藏宝阁价值容量1亿", ExternalStorageFlag)
            .AddAutoSaveRow("天气锁定晴天", WeatherLockSunnyFlag, "拍卖会自动出价", AuctionAutoOfferFlag)
            .BeginHorizontal()
           
            .AddButton("解锁成就", () =>
            {
                var achs = GameDataController.Instance.AchievementData;
                for (var i = 0; i < achs.Count; i++)
                {
                    GameDataController.Instance.UnlockAchievement(i);
                }
            }, 100)
            .AddButton("解锁图鉴", OtherHelper.UnlockHandBook, 100)
            .EndHorizontal()
            
            .BeginVertical()
            .AddAutoSave("指定玩家门派服装（99999默认为不修改）", SpecifiedSkinId)
            .AddLabelRow("服装ID,名称（只用填入ID）")
            .AddLabelRow("-10,邪道服;-9,杀手服;-8,强盗服;-7,游侠服;-6,才子服;-5,官差服;")
            .AddLabelRow("-4,杂役服;-3,商人服;-2,农民服;-1,常服;0,锦袍;1,夜行衣;")
            .AddLabelRow("2,医袍;3,百衲衣;4,戎装;5,法服;6,甲胄;7,避毒服;")
            .AddLabelRow("8,铁衣;9,襕衫;10,袈裟;11,道袍;12,胡裘;13,鹤氅;")
            .AddLabelRow("14,瑶衣;15,奇装;16,匠服;17,火浣布;18,羌姆袍;19,貂裘;")
            .AddLabelRow("-100,白马红妆;-101,风起青萍")
            .EndVertical()
            
            .AddSlider("窗体/字体缩放", WindowScaling,0.5f, 2.0f, _otherCategory, labelWidth:100, sliderWidth:200, useFixedLayout:true)
            .AddButtonRow("重置缩放", () =>
            {
                WindowScaling.Value = 1;
                _otherCategory.SaveToFile();
            })
            .EndFoldout();
        
        builder.BeginFoldout("难度相关").Space(10)
            .AddLabelRow("自定义难度即时修改")
            .BeginVertical()
            .BeginHorizontal()
            .AddLinkedInt("经验倍率", () => ExpRate, 
                value => ExpRate = value,
                "expRate")
            .AddLabel($"经验倍率 {(ExpRate >= 0 ? "+" : "-")}{(ExpRate > 0 ? ExpRate * 20 : ExpRate * 10)}%", width:200)
            .EndHorizontal()
            .BeginHorizontal()
            .AddLinkedInt("声望倍率", () => FameRate, 
                value => FameRate = value,
                "fameRate")
            .AddLabel($"声望倍率 {(FameRate >= 0 ? "+" : "-")}{(FameRate > 0 ? FameRate * 20 : FameRate * 10)}%", width:200)
            .EndHorizontal()
            .BeginHorizontal()
            .AddLinkedInt("负重倍率", () => MaxweightRate, 
                value => MaxweightRate = value,
                "maxweightRate")
            .AddLabel($"负重倍率 {(MaxweightRate >= 0 ? "+" : "-")}{(MaxweightRate > 0 ? MaxweightRate * 20 : MaxweightRate * 10)}%", width:200)
            .EndHorizontal()
            .BeginHorizontal()
            .AddLinkedInt("本门弟子经验倍率", () => SelfforceExpRate, 
                value => SelfforceExpRate = value,
                "selfforceExpRate", labelWidth:170)
            .AddLabel($"本门弟子经验倍率 {(SelfforceExpRate >= 0 ? "+" : "-")}{(SelfforceExpRate > 0 ? SelfforceExpRate * 20 : SelfforceExpRate * 10)}%", width:250)
            .EndHorizontal()
            .BeginHorizontal()
            .AddLinkedInt("非本门弟子经验倍率", () => OtherforceExpRate, 
                value => OtherforceExpRate = value,
                "otherforceExpRate", labelWidth:170)
            .AddLabel($"非本门弟子经验倍率 {(OtherforceExpRate >= 0 ? "+" : "-")}{(OtherforceExpRate > 0 ? OtherforceExpRate * 40 : OtherforceExpRate * 10)}%", width:250)
            .EndHorizontal()
            .BeginHorizontal()
            .AddLinkedInt("随机敌人强度", () => RandomEnemyStrength, 
                value => RandomEnemyStrength = value,
                "randomEnemyStrength", labelWidth:120)
            .AddLabel($"随机敌人强度 {(RandomEnemyStrength >= 0 ? "+" : "-")}{(RandomEnemyStrength > 0 ? RandomEnemyStrength * 20 : RandomEnemyStrength * 10)}%", width:200)
            .EndHorizontal()
            .BeginHorizontal()
            .AddLinkedInt("随机敌人数量", () => RandomEnemyNum, 
                value => RandomEnemyNum = value,
                "randomEnemyNum", labelWidth:120)
            .AddLabel($"随机敌人数量 {(RandomEnemyNum >= 0 ? "+" : "-")}{(RandomEnemyNum > 0 ? RandomEnemyNum * 20 : RandomEnemyNum * 10)}%", width:200)
            .EndHorizontal()
            .BeginHorizontal()
            .AddLinkedInt("恶名获取", () => BadfameRate, 
                value => BadfameRate = value,
                "badfameRate")
            .AddLabel($"恶名获取 {(BadfameRate >= 0 ? "+" : "-")}{(BadfameRate > 0 ? BadfameRate * 20 : BadfameRate * 10)}%", width:200)
            .EndHorizontal()
            .BeginHorizontal()
            .AddLinkedInt("武学上限", () => MaxSkillNum, 
                value => MaxSkillNum = Math.Clamp(value, 0, 5),
                "maxSkillNum")
            .AddLabel($"武学上限 +{(MaxSkillNum != 5 ? MaxSkillNum.ToString() : "∞")}", width:200)
            .EndHorizontal()
            .BeginHorizontal()
            .AddLinkedInt("组队限制", () => TeammateLimit, 
                value => TeammateLimit = Math.Clamp(value, -3, 1),
                "teammateLimit")
            .AddLabel($"组队限制 {CustomDifficultyData.teammateLimitName[TeammateLimit+3]}", width:200)
            .EndHorizontal()
            .BeginHorizontal()
            .AddLinkedInt("AI门派发展速度", () => AiForceDevelopSpeed, 
                value => AiForceDevelopSpeed = value,
                "aiForceDevelopSpeed", labelWidth:150)
            .AddLabel($"AI门派发展速度 {(AiForceDevelopSpeed >= 0 ? "+" : "-")}{AiForceDevelopSpeed}", width:200)
            .EndHorizontal()
            .AddLabelRow("每级对Al门派产生影响:")
            .AddLabelRow("1.加快建筑建造:每月建造数量+，建造消耗-5%，建造速度+5%")
            .AddLabelRow("2.加快科研:科研消耗-5%，科研速度+5%")
            .AddLabelRow("3.加快弟子晋升:晋升频率+5%，晋升消耗-5%，\n晋升武学等级+，招募消耗-5%，月俸消耗-2.5%")
            .BeginHorizontal()
            .AddLinkedFloat("难度提升速度", () => TimeDifficulty, 
                value => TimeDifficulty = value,
                "timeDifficulty", labelWidth:150)
            .EndHorizontal()
            
            .AddButtonRow("保存自定义难度配置", () =>
            {
                var gc = GameController.Instance;
                if (gc == null) return;
                var dict = new Il2CppSystem.Collections.Generic.Dictionary<int, int>
                {
                    [0] = ExpRate,
                    [1] = FameRate,
                    [2] = MaxweightRate,
                    [3] = SelfforceExpRate,
                    [4] = OtherforceExpRate,
                    [5] = RandomEnemyStrength,
                    [6] = RandomEnemyNum,
                    [7] = BadfameRate,
                    [8] = MaxSkillNum,
                    [9] = TeammateLimit,
                    [10] = AiForceDevelopSpeed
                };
                gc.worldData.customDifficultyData.customDifficultyLv = dict;
                gc.worldData.TimeDifficulty = TimeDifficulty;
            }, width:180)
            .EndVertical()
            
            .AddLabelRow("支持新老版本")
            .AddAutoSaveRow("难度经验倍率", ExpRateMultiplier, "难度经验是否对自门派生效", ExpRateMultiplierSelfForceFlag)
            .EndFoldout();
            
        builder.BeginFoldout("新档相关").Space(10)
            .BeginHorizontal().AddButtonRow("新档人物属性点数999", () =>
            {
                var smc = StartMenuController._instance;
                if (smc == null) return;
                smc.leftAttriPoint = 999;
                smc.leftFightSkillPoint = 999;
                smc.leftLivingSkillPoint = 999;
            }, 225).EndHorizontal()
            .AddAutoSave("新档天赋点数999(选中后重启生效)", NewGameTagNumFlag, labelWidth:280)
            .AddAutoSave("新档天赋无视要求", NewGameAnyTagFlag, labelWidth:150)
            
            .AddLabelRow("新档自定义难度滑块范围：")
            .AddAutoSaveRow("最小值", NewSaveSliderMin,"最大值", NewSaveSliderMax)
            .AddButtonRow("设置生效", () =>
            {
                StartMenuController.Instance.Start();
            }, 100)
            .AddAutoSave("任何难度都可获取成就", AnyDifficultUnlockAchFlag)
            
            .BeginHorizontal()
            .AddLinkedBool("仙霞初建存档地块最大化：", ()=>MaxAreaFlag, val => MaxAreaFlag = val, labelWidth:220)
            .AddLinkedBool("需要城墙：", ()=>MaxAreaFlag1, val => MaxAreaFlag1 = val, labelWidth:95)
            .EndHorizontal()
            .EndFoldout();
        
    }
    #endregion
}

