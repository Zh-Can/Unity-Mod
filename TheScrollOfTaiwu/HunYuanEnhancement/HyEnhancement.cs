using System;
using Config;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Taiwu;
using GameData.Utilities;
using TaiwuModdingLib.Core.Plugin;

namespace HunYuanEnhancement
{
    [PluginConfig("混元增强Mod", "Can", "1.2.0")]
    public class HyEnhancement : TaiwuRemakePlugin
    {
        private static int _requirementChange = -20;
        private static int _maxPowerChange = 20;
        
        // 攻击属性
        private static int _penetrateOuter = 5;
        private static int _penetrateInner = 5;
        
        // 防御属性
        private static int _penResistOuter = 5;
        private static int _penResistInner = 5;
        
        // 命中属性
        private static int _hitLiDao = 3;
        private static int _hitJingMiao = 3;
        private static int _hitXunJi = 3;
        private static int _hitDongXin = 3;
        
        // 化解属性
        private static int _avoidXieLi = 3;
        private static int _avoidChaiZhao = 3;
        private static int _avoidShanBi = 3;
        private static int _avoidShouXin = 3;
        
        // 次要属性
        private static int _stanceRecovery = 5;
        private static int _breathRecovery = 5;
        private static int _moveSpeed = 5;
        private static int _flawRecovery = 5;
        private static int _castSpeed = 5;
        private static int _blockedAcupointRecovery = 5;
        private static int _weaponSwitchSpeed = 5;
        private static int _attackSpeed = 5;
        private static int _innerRatio = 5;
        private static int _qiDisorderRecovery = 5;
        
        // 毒素抵抗
        private static int _hotPoisonResist = 2;
        private static int _gloomyPoisonResist = 2;
        private static int _coldPoisonResist = 2;
        private static int _redPoisonResist = 2;
        private static int _rottenPoisonResist = 2;
        private static int _illusoryPoisonResist = 2;
        
        public override void Initialize()
        {
            // AdaptableLog.Info("123");
            ReadSetting();
            // ReplaceNeiliTypeEffect();
        }

        public override void OnModSettingUpdate()
        {
            ReadSetting();
            ReplaceNeiliTypeEffect();
        }

        private void ReadSetting()
        {
            DomainManager.Mod.GetSetting(ModIdStr, "requirementChange", ref _requirementChange);
            DomainManager.Mod.GetSetting(ModIdStr, "maxPowerChange", ref _maxPowerChange);
            
            // 读取攻击属性配置
            DomainManager.Mod.GetSetting(ModIdStr, "penetrateOuter", ref _penetrateOuter);
            DomainManager.Mod.GetSetting(ModIdStr, "penetrateInner", ref _penetrateInner);
            
            // 读取防御属性配置
            DomainManager.Mod.GetSetting(ModIdStr, "penResistOuter", ref _penResistOuter);
            DomainManager.Mod.GetSetting(ModIdStr, "penResistInner", ref _penResistInner);
            
            // 读取命中属性配置
            DomainManager.Mod.GetSetting(ModIdStr, "hitLiDao", ref _hitLiDao);
            DomainManager.Mod.GetSetting(ModIdStr, "hitJingMiao", ref _hitJingMiao);
            DomainManager.Mod.GetSetting(ModIdStr, "hitXunJi", ref _hitXunJi);
            DomainManager.Mod.GetSetting(ModIdStr, "hitDongXin", ref _hitDongXin);
            
            // 读取化解属性配置
            DomainManager.Mod.GetSetting(ModIdStr, "avoidXieLi", ref _avoidXieLi);
            DomainManager.Mod.GetSetting(ModIdStr, "avoidChaiZhao", ref _avoidChaiZhao);
            DomainManager.Mod.GetSetting(ModIdStr, "avoidShanBi", ref _avoidShanBi);
            DomainManager.Mod.GetSetting(ModIdStr, "avoidShouXin", ref _avoidShouXin);
            
            // 读取次要属性配置
            DomainManager.Mod.GetSetting(ModIdStr, "stanceRecovery", ref _stanceRecovery);
            DomainManager.Mod.GetSetting(ModIdStr, "breathRecovery", ref _breathRecovery);
            DomainManager.Mod.GetSetting(ModIdStr, "moveSpeed", ref _moveSpeed);
            DomainManager.Mod.GetSetting(ModIdStr, "flawRecovery", ref _flawRecovery);
            DomainManager.Mod.GetSetting(ModIdStr, "castSpeed", ref _castSpeed);
            DomainManager.Mod.GetSetting(ModIdStr, "blockedAcupointRecovery", ref _blockedAcupointRecovery);
            DomainManager.Mod.GetSetting(ModIdStr, "weaponSwitchSpeed", ref _weaponSwitchSpeed);
            DomainManager.Mod.GetSetting(ModIdStr, "attackSpeed", ref _attackSpeed);
            DomainManager.Mod.GetSetting(ModIdStr, "innerRatio", ref _innerRatio);
            DomainManager.Mod.GetSetting(ModIdStr, "qiDisorderRecovery", ref _qiDisorderRecovery);
            
            // 读取毒素抵抗配置
            DomainManager.Mod.GetSetting(ModIdStr, "hotPoisonResist", ref _hotPoisonResist);
            DomainManager.Mod.GetSetting(ModIdStr, "gloomyPoisonResist", ref _gloomyPoisonResist);
            DomainManager.Mod.GetSetting(ModIdStr, "coldPoisonResist", ref _coldPoisonResist);
            DomainManager.Mod.GetSetting(ModIdStr, "redPoisonResist", ref _redPoisonResist);
            DomainManager.Mod.GetSetting(ModIdStr, "rottenPoisonResist", ref _rottenPoisonResist);
            DomainManager.Mod.GetSetting(ModIdStr, "illusoryPoisonResist", ref _illusoryPoisonResist);
        }

        private void ReplaceNeiliTypeEffect()
        {
            // 配置数据处理
            // 发挥需求
            var requirement = Convert.ToSByte(_requirementChange);
            var requirementChange = new[] { requirement, requirement, requirement, requirement, requirement, requirement };
            // 功法伤害上限
            var maxPower = Convert.ToSByte(_maxPowerChange);
            var maxPowerChange = new[] { maxPower, maxPower, maxPower, maxPower, maxPower, maxPower };
            
            // 数据整合：将分散的配置项整合为结构体/数组
            // 攻击属性：破体、破气 -> OuterAndInnerShorts
            var newPenetrations = new OuterAndInnerShorts((short)_penetrateOuter, (short)_penetrateInner);
            // 防御属性：御体、御气 -> OuterAndInnerShorts
            var newPenetrationResists = new OuterAndInnerShorts((short)_penResistOuter, (short)_penResistInner);
            // 命中属性：力道、精妙、迅疾、动心 -> HitOrAvoidShorts
            var newHitValues = new HitOrAvoidShorts((short)_hitLiDao, (short)_hitJingMiao, (short)_hitXunJi, (short)_hitDongXin);
            // 化解属性：卸力、拆招、闪避、守心 -> HitOrAvoidShorts
            var newAvoidValues = new HitOrAvoidShorts((short)_avoidXieLi, (short)_avoidChaiZhao, (short)_avoidShanBi, (short)_avoidShouXin);
            // 架势/提气恢复 -> OuterAndInnerShorts
            var newRecoveryOfStanceAndBreath = new OuterAndInnerShorts((short)_stanceRecovery, (short)_breathRecovery);
            // 毒素抵抗：烈度、郁毒、寒毒、赤毒、腐毒、幻毒 -> PoisonShorts
            var newPoisonResists = new PoisonShorts(_hotPoisonResist, _gloomyPoisonResist, _coldPoisonResist, _redPoisonResist, _rottenPoisonResist, _illusoryPoisonResist);
            
            // 原始混元属性
            var o = NeiliType.Instance[5];
            
            // AdaptableLog.Info($"[HyEnhancement] ===== 混元原始值 =====");
            // AdaptableLog.Info($"功法威力上限(金/木/水/火/土/混元): [{string.Join(", ", o.MaxPowerChange)}]");
            // AdaptableLog.Info($"功法发挥需求(金/木/水/火/土/混元): [{string.Join(", ", o.RequirementChange)}]");
            // AdaptableLog.Info($"命中(力道/精妙/迅疾/动心): [{o.HitValues[0]}, {o.HitValues[1]}, {o.HitValues[2]}, {o.HitValues[3]}]");
            // AdaptableLog.Info($"破体/破气: [{o.Penetrations.Outer}, {o.Penetrations.Inner}]");
            // AdaptableLog.Info($"化解(卸力/拆招/闪避/守心): [{o.AvoidValues[0]}, {o.AvoidValues[1]}, {o.AvoidValues[2]}, {o.AvoidValues[3]}]");
            // AdaptableLog.Info($"御体/御气: [{o.PenetrationResists.Outer}, {o.PenetrationResists.Inner}]");
            // AdaptableLog.Info($"架势恢复/提气恢复: [{o.RecoveryOfStanceAndBreath.Outer}, {o.RecoveryOfStanceAndBreath.Inner}]");
            // AdaptableLog.Info($"移动速度={o.MoveSpeed}, 步伐稳健={o.RecoveryOfFlaw}, 施展速度={o.CastSpeed}");
            // AdaptableLog.Info($"引气冲关={o.RecoveryOfBlockedAcupoint}, 武具运用={o.WeaponSwitchSpeed}");
            // AdaptableLog.Info($"攻击速度={o.AttackSpeed}, 内功发挥={o.InnerRatio}, 调息吐纳={o.RecoveryOfQiDisorder}");
            // AdaptableLog.Info($"毒素抵抗(烈/郁/寒/赤/腐/幻): [{o.PoisonResists[0]}, {o.PoisonResists[1]}, {o.PoisonResists[2]}, {o.PoisonResists[3]}, {o.PoisonResists[4]}, {o.PoisonResists[5]}]");
            // AdaptableLog.Info($"五行比例(金/木/水/火/土): {o.NeiliProportionOfFiveElements}");
            
            // 构建新的混元属性项
            var newNeiliTypeItem = new NeiliTypeItem(5, o.Name, o.Desc, o.FiveElements, o.IdeaAllocationProportion,
                maxPowerChange,
                requirementChange,
                o.InjuryOnUseType, o.ShowConflictingWorldState,
                newHitValues, newPenetrations, newAvoidValues,
                newPenetrationResists, newRecoveryOfStanceAndBreath,
                (short)_moveSpeed, (short)_flawRecovery, (short)_castSpeed,
                (short)_blockedAcupointRecovery, (short)_weaponSwitchSpeed,
                (short)_attackSpeed, (short)_innerRatio, (short)_qiDisorderRecovery,
                newPoisonResists, o.ColorType, o.LinePos, o.LineAngle,
                o.TypeIconPos, o.NeiliTypeConditionText,
                o.SimpleDesc, o.EffectDesc,
                o.LifeGateFeatures, o.DeathGateFeatures,
                o.NeiliProportionOfFiveElements);
            NeiliType.Instance.AddOrModifyItem(newNeiliTypeItem);
            RefreshTaiwuNeiliCache();
        }
        private void RefreshTaiwuNeiliCache()
        {
            TaiwuDomain taiwu = DomainManager.Taiwu;
            var character = taiwu?.GetTaiwu();
            if (character == null) return;
            character.SetBaseNeiliProportionOfFiveElements(character.GetBaseNeiliProportionOfFiveElements(), DataContextManager.GetCurrentThreadDataContext());
        }

        public override void Dispose()
        {
        }
    }
}
