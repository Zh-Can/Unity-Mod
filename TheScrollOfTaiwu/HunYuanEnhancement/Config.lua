return {
    Title = "混元增强Mod",
    Source = 0,
    Version = "1.0.0",
    GameVersion = "1.0.44",
    Author = "Can",
    Description = "混元增强Mod说明：\n\n1.修改数据后，需要重启才能生效\n2.数据可调\n3.对全局所有人物的混元属性都会增强，所以不要改的过强\n4.字段描述的括号内是游戏原值“功法发挥需求(-20)”",
    TagList = {
		[1] = "Modifications",
		[2] = "Compatible Mods",
	},
    BackendPlugins = {
        [1] = "HunYuanEnhancement.dll",
    },
    Cover = "Cover.png",
    WorkshopCover = "Cover.png",
    NeedRestartWhenSettingChanged = false,
    DefaultSettings = {
        [1] = {
            SettingType = "Slider",
            Key = "requirementChange",
            DisplayName = "功法发挥需求(-20)",
            GroupName = "混元效果",
            MinValue = -100,
            MaxValue = 0,
            StepSize = 5,
            DefaultSettings = -20
        },
        [2] = {
            SettingType = "Slider",
            Key = "maxPowerChange",
            DisplayName = "功法威力上限(0)",
            GroupName = "混元效果",
            MinValue = 0,
            MaxValue = 100,
            StepSize = 5,
            DefaultSettings = 20
        },
        [3] = {
            SettingType = "Slider",
            Key = "penetrateOuter",
            DisplayName = "破体(10)",
            GroupName = "攻击属性",
            MinValue = 0,
            MaxValue = 20,
            StepSize = 1,
            DefaultSettings = 15
        },
        [4] = {
            SettingType = "Slider",
            Key = "penetrateInner",
            DisplayName = "破气(10)",
            GroupName = "攻击属性",
            MinValue = 0,
            MaxValue = 20,
            StepSize = 1,
            DefaultSettings = 15
        },
        [5] = {
            SettingType = "Slider",
            Key = "penResistOuter",
            DisplayName = "御体(10)",
            GroupName = "防御属性",
            MinValue = 0,
            MaxValue = 20,
            StepSize = 1,
            DefaultSettings = 15
        },
        [6] = {
            SettingType = "Slider",
            Key = "penResistInner",
            DisplayName = "御气(10)",
            GroupName = "防御属性",
            MinValue = 0,
            MaxValue = 20,
            StepSize = 1,
            DefaultSettings = 15
        },
        [7] = {
            SettingType = "Slider",
            Key = "hitLiDao",
            DisplayName = "力道(6)",
            GroupName = "命中属性",
            MinValue = 0,
            MaxValue = 20,
            StepSize = 1,
            DefaultSettings = 7
        },
        [8] = {
            SettingType = "Slider",
            Key = "hitJingMiao",
            DisplayName = "精妙(6)",
            GroupName = "命中属性",
            MinValue = 0,
            MaxValue = 20,
            StepSize = 1,
            DefaultSettings = 7
        },
        [9] = {
            SettingType = "Slider",
            Key = "hitXunJi",
            DisplayName = "迅疾(6)",
            GroupName = "命中属性",
            MinValue = 0,
            MaxValue = 20,
            StepSize = 1,
            DefaultSettings = 7
        },
        [10] = {
            SettingType = "Slider",
            Key = "hitDongXin",
            DisplayName = "动心(6)",
            GroupName = "命中属性",
            MinValue = 0,
            MaxValue = 20,
            StepSize = 1,
            DefaultSettings = 7
        },
        [11] = {
            SettingType = "Slider",
            Key = "avoidXieLi",
            DisplayName = "卸力(6)",
            GroupName = "化解属性",
            MinValue = 0,
            MaxValue = 20,
            StepSize = 1,
            DefaultSettings = 7
        },
        [12] = {
            SettingType = "Slider",
            Key = "avoidChaiZhao",
            DisplayName = "拆招(6)",
            GroupName = "化解属性",
            MinValue = 0,
            MaxValue = 20,
            StepSize = 1,
            DefaultSettings = 7
        },
        [13] = {
            SettingType = "Slider",
            Key = "avoidShanBi",
            DisplayName = "闪避(6)",
            GroupName = "化解属性",
            MinValue = 0,
            MaxValue = 20,
            StepSize = 1,
            DefaultSettings = 7
        },
        [14] = {
            SettingType = "Slider",
            Key = "avoidShouXin",
            DisplayName = "守心(6)",
            GroupName = "化解属性",
            MinValue = 0,
            MaxValue = 20,
            StepSize = 1,
            DefaultSettings = 7
        },
        [15] = {
            SettingType = "Slider",
            Key = "stanceRecovery",
            DisplayName = "架势恢复(10)",
            GroupName = "次要属性",
            MinValue = 0,
            MaxValue = 20,
            StepSize = 1,
            DefaultSettings = 15
        },
        [16] = {
            SettingType = "Slider",
            Key = "breathRecovery",
            DisplayName = "提气恢复(10)",
            GroupName = "次要属性",
            MinValue = 0,
            MaxValue = 20,
            StepSize = 1,
            DefaultSettings = 15
        },
        [17] = {
            SettingType = "Slider",
            Key = "moveSpeed",
            DisplayName = "移动速度(10)",
            GroupName = "次要属性",
            MinValue = 0,
            MaxValue = 20,
            StepSize = 1,
            DefaultSettings = 15
        },
        [18] = {
            SettingType = "Slider",
            Key = "flawRecovery",
            DisplayName = "步伐稳健(10)",
            GroupName = "次要属性",
            MinValue = 0,
            MaxValue = 20,
            StepSize = 1,
            DefaultSettings = 15
        },
        [19] = {
            SettingType = "Slider",
            Key = "castSpeed",
            DisplayName = "施展速度(10)",
            GroupName = "次要属性",
            MinValue = 0,
            MaxValue = 20,
            StepSize = 1,
            DefaultSettings = 15
        },
        [20] = {
            SettingType = "Slider",
            Key = "blockedAcupointRecovery",
            DisplayName = "引气冲关(10)",
            GroupName = "次要属性",
            MinValue = 0,
            MaxValue = 20,
            StepSize = 1,
            DefaultSettings = 15
        },
        [21] = {
            SettingType = "Slider",
            Key = "weaponSwitchSpeed",
            DisplayName = "武具运用(10)",
            GroupName = "次要属性",
            MinValue = 0,
            MaxValue = 20,
            StepSize = 1,
            DefaultSettings = 15
        },
        [22] = {
            SettingType = "Slider",
            Key = "attackSpeed",
            DisplayName = "攻击速度(10)",
            GroupName = "次要属性",
            MinValue = 0,
            MaxValue = 20,
            StepSize = 1,
            DefaultSettings = 15
        },
        [23] = {
            SettingType = "Slider",
            Key = "innerRatio",
            DisplayName = "内功发挥(10)",
            GroupName = "次要属性",
            MinValue = 0,
            MaxValue = 20,
            StepSize = 1,
            DefaultSettings = 15
        },
        [24] = {
            SettingType = "Slider",
            Key = "qiDisorderRecovery",
            DisplayName = "调息吐纳(10)",
            GroupName = "次要属性",
            MinValue = 0,
            MaxValue = 20,
            StepSize = 1,
            DefaultSettings = 15
        },
        [25] = {
            SettingType = "Slider",
            Key = "hotPoisonResist",
            DisplayName = "烈度抵抗(4)",
            GroupName = "毒素抵抗",
            MinValue = 0,
            MaxValue = 10,
            StepSize = 1,
            DefaultSettings = 4
        },
        [26] = {
            SettingType = "Slider",
            Key = "gloomyPoisonResist",
            DisplayName = "郁毒抵抗(4)",
            GroupName = "毒素抵抗",
            MinValue = 0,
            MaxValue = 10,
            StepSize = 1,
            DefaultSettings = 4
        },
        [27] = {
            SettingType = "Slider",
            Key = "coldPoisonResist",
            DisplayName = "寒毒抵抗(4)",
            GroupName = "毒素抵抗",
            MinValue = 0,
            MaxValue = 10,
            StepSize = 1,
            DefaultSettings = 4
        },
        [28] = {
            SettingType = "Slider",
            Key = "redPoisonResist",
            DisplayName = "赤毒抵抗(4)",
            GroupName = "毒素抵抗",
            MinValue = 0,
            MaxValue = 10,
            StepSize = 1,
            DefaultSettings = 4
        },
        [29] = {
            SettingType = "Slider",
            Key = "rottenPoisonResist",
            DisplayName = "腐毒抵抗(4)",
            GroupName = "毒素抵抗",
            MinValue = 0,
            MaxValue = 10,
            StepSize = 1,
            DefaultSettings = 4
        },
        [30] = {
            SettingType = "Slider",
            Key = "illusoryPoisonResist",
            DisplayName = "幻毒抵抗(4)",
            GroupName = "毒素抵抗",
            MinValue = 0,
            MaxValue = 10,
            StepSize = 1,
            DefaultSettings = 4
        },
    },
    SettingGroups = {
        [1] = "混元效果",
        [2] = "攻击属性",
        [3] = "防御属性",
        [4] = "命中属性",
        [5] = "化解属性",
        [6] = "次要属性",
        [7] = "毒素抵抗",
    }
}