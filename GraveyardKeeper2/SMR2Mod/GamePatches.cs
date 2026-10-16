using HarmonyLib;
using LazyBearTechnology;

namespace SMR2Mod
{
    public class GamePatches
    {
        public static bool CombatGodMode
        {
            get => Main.CombatGodMode != null && Main.CombatGodMode.Value;
            set { if (Main.CombatGodMode != null) Main.CombatGodMode.Value = value; }
        }

        public static bool InfiniteCombatStamina
        {
            get => Main.InfiniteCombatStamina != null && Main.InfiniteCombatStamina.Value;
            set { if (Main.InfiniteCombatStamina != null) Main.InfiniteCombatStamina.Value = value; }
        }

        public static bool InfiniteEnergy
        {
            get => Main.InfiniteEnergy != null && Main.InfiniteEnergy.Value;
            set { if (Main.InfiniteEnergy != null) Main.InfiniteEnergy.Value = value; }
        }

        public static bool LockInsanity
        {
            get => Main.LockInsanity != null && Main.LockInsanity.Value;
            set { if (Main.LockInsanity != null) Main.LockInsanity.Value = value; }
        }

        public static bool FullHappiness
        {
            get => Main.FullHappiness != null && Main.FullHappiness.Value;
            set { if (Main.FullHappiness != null) Main.FullHappiness.Value = value; }
        }

        public static bool OneHitWork
        {
            get => Main.OneHitWork != null && Main.OneHitWork.Value;
            set { if (Main.OneHitWork != null) Main.OneHitWork.Value = value; }
        }

        /// <summary>
        /// 战斗时血量“无敌”锁血：玩家受到伤害时直接跳过
        /// </summary>
        [HarmonyPatch(typeof(HPComponent), nameof(HPComponent.ApplyDamage))]
        [HarmonyPrefix]
        public static bool ApplyDamage_Prefix(HPComponent __instance)
        {
            return !CombatGodMode || MainGame.PlayerData?.hpComponent != __instance;
        }

        /// <summary>
        /// 战斗时体力“无限”：攻击不再消耗耐力
        /// </summary>
        [HarmonyPatch(typeof(StaminaSystem), nameof(StaminaSystem.ConsumeStamina))]
        [HarmonyPrefix]
        public static bool ConsumeStamina_Prefix(StaminaSystem __instance)
        {
            return !InfiniteCombatStamina || MainGame.PlayerData?.staminaSystem != __instance;
        }

        /// <summary>
        /// 体力“无限”：工作能量不再减少
        /// </summary>
        [HarmonyPatch(typeof(PlayerEnergyGameResSystem), nameof(PlayerEnergyGameResSystem.Add))]
        [HarmonyPrefix]
        public static bool EnergyAdd_Prefix(float value)
        {
            return !(InfiniteEnergy && value < 0f);
        }

        /// <summary>
        /// 疯狂“锁定”：疯狂值不再变化
        /// </summary>
        [HarmonyPatch(typeof(PlayerInsanityGameResSystem), nameof(PlayerInsanityGameResSystem.Add))]
        [HarmonyPrefix]
        public static bool InsanityAdd_Prefix()
        {
            return !LockInsanity;
        }

        /// <summary>
        /// 小镇谢礼“总是满值”：每次变动后直接设为上限
        /// </summary>
        [HarmonyPatch(typeof(PlayerHappinessGameResSystem), nameof(PlayerHappinessGameResSystem.Add))]
        [HarmonyPostfix]
        public static void HappinessAdd_Postfix()
        {
            if (!FullHappiness)
                return;

            var pd = MainGame.PlayerData;
            if (pd == null)
                return;

            var quality = MainGame.Instance?.GameSave?.townSystem?.Quality ?? 0;
            var sys = pd.GetResSystem("happiness") as GK2GameResSystem;
            var target = quality > 0 ? (float)quality : (sys?.Max ?? 100f);
            pd.SetRes("happiness", target);
        }

        /// <summary>
        /// “一次完成”工作内容：砍树/挖矿等按 HP 计的工作，每次动作伤害拉满
        /// </summary>
        [HarmonyPatch(typeof(PlayerHPActivity), nameof(PlayerHPActivity.GetActionDamage))]
        [HarmonyPrefix]
        public static bool HpActionDamage_Prefix(ref int __result)
        {
            if (!OneHitWork)
                return true;

            __result = int.MaxValue;
            return false;
        }

        /// <summary>
        /// “一次完成”工作内容：制作/解剖等按进度计的工作，每次动作直接补满剩余进度
        /// </summary>
        [HarmonyPatch(typeof(PlayerCraftActivity), nameof(PlayerCraftActivity.GetActionDamage))]
        [HarmonyPrefix]
        public static bool CraftActionDamage_Prefix(PlayerCraftActivity __instance, ref int __result)
        {
            if (!OneHitWork)
                return true;

            var element = __instance.CraftComponent?.CurrentCraftElement;
            if (element == null)
                return true;

            var remaining = element.TotalProgressTicks - element.ProgressTicks;
            __result = remaining > 0 ? remaining : element.TotalProgressTicks;
            return false;
        }
    }
}