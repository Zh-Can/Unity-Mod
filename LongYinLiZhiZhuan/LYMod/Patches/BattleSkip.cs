using HarmonyLib;
using Il2Cpp;

namespace LYMod.Patches;

public static class BattleSkip
{
    private static int _storedEnemyCount;
    private static float _storedEnemyTotalHp;
    private static bool _isSkipping = false;

    [HarmonyPatch(typeof(BattleController), nameof(BattleController.StartBattleButtonClicked))]
    public static class StartBattleButtonClickedPatch
    {
        public static void Postfix(BattleController __instance)
        {
            if (!Plugin.Instance.BattleSkipFlag.Value) return;
            
            // 重置跳过状态，确保新战斗可以正常跳过
            _isSkipping = false;
            
            // 不再在这里显示按钮，等待人物就位后在 Update 中显示
        }
    }

    /// <summary>
    /// 在 Update 中检查战斗状态，当人物就位后显示跳过按钮
    /// </summary>
    [HarmonyPatch(typeof(BattleController), nameof(BattleController.Update))]
    public static class BattleControllerUpdatePatch
    {
        public static void Postfix(BattleController __instance)
        {
            if (!Plugin.Instance.BattleSkipFlag.Value) return;
            
            // 只在战斗准备完成且未跳过时显示按钮
            if (__instance.battleState != BattleState.Ready && __instance.battleState != BattleState.Fighting) 
                return;
            
            var skipButton = __instance.battleSkipButton;
            if (skipButton == null) return;
            
            // 如果按钮已经显示，不需要再处理
            if (skipButton.activeSelf) return;
            
            var rect = skipButton.GetComponent<UnityEngine.UI.Image>()?.rectTransform;
            if (rect == null) return;
            
            // 修复缩放问题
            if (rect.localScale.x == 0 || rect.localScale.y == 0)
            {
                rect.localScale = new UnityEngine.Vector3(1f, 1f, 1f);
            }
            
            skipButton.SetActive(true);
        }
    }

    /// <summary>
    /// 在点击跳过按钮时，直接调用 SureSkipBattle 跳过战斗
    /// </summary>
    [HarmonyPatch(typeof(BattleController), nameof(BattleController.BattleSkipButtonClicked))]
    public static class BattleSkipButtonClickedPatch
    {
        public static void Prefix(BattleController __instance)
        {
            if (!Plugin.Instance.BattleSkipFlag.Value) return;
            
            // 防止重复点击
            if (_isSkipping) return;
            _isSkipping = true;
            
            // 清除 noSkip 标志
            __instance.noSkip = false;
        }
    }

    [HarmonyPatch(typeof(BattleController), nameof(BattleController.SureSkipBattle))]
    public static class SureSkipBattlePatch
    {
        public static void Prefix(BattleController __instance)
        {
            var playerTeamID = __instance.GetPlayerControlTeamID();
            GetEnemyInfo(__instance, playerTeamID, out var enemyCount, out var enemyTotalHP);
            _storedEnemyCount = enemyCount;
            _storedEnemyTotalHp = enemyTotalHP;
            
            var playerUnit = __instance.playerBattleUnit;
            if (playerUnit != null)
            {
                if (playerUnit.battleInfo == null) playerUnit.battleInfo = new BattleInfoData();
                playerUnit.battleInfo.makeDamage = _storedEnemyTotalHp;
                playerUnit.battleInfo.takeDamage = 0f;
                playerUnit.battleInfo.enemyKilled = _storedEnemyCount;
                playerUnit.battleInfo.enemyKillScorePercent = _storedEnemyTotalHp;
            }

            var teams = __instance.teams;
            foreach (var team in teams)
            {
                foreach (var unit in team.battleUnits)
                {
                    unit.heroData.AutoGetFightExp();
                }
            }
        }
    }

    private static void GetEnemyInfo(BattleController bc, int playerTeamID, out int enemyCount, out float enemyTotalHP)
    {
        enemyCount = 0;
        enemyTotalHP = 0f;

        var teams = bc.teams;
        if (teams == null)
        {
            return;
        }

        foreach (var team in teams)
        {
            if (team == null || team.ID == playerTeamID) continue;

            var units = team.battleUnits;
            if (units == null) continue;

            foreach (var unit in units)
            {
                if (unit == null || unit.heroData == null) continue;
                enemyCount++;
                enemyTotalHP += unit.heroData.maxhp;
            }
        }
    }

    [HarmonyPatch(typeof(BattleController), nameof(BattleController.CountPlayerBattleScore))]
    public static class CountPlayerBattleScorePatch
    {
        public static void Postfix(BattleController __instance, float enemyTotalScore, ref bool heroWin, ref float __result)
        {
            if (!Plugin.Instance.BattleSkipFlag.Value) return;
            if (heroWin) __result = 100f;

        }
    }

    [HarmonyPatch(typeof(BattleController), nameof(BattleController.CountHeroBattleContribution))]
    public static class CountHeroBattleContributionPatch
    {
        public static void Prefix(BattleController __instance, BattleUnit targetUnit, bool win)
        {
            if (!Plugin.Instance.BattleSkipFlag.Value) return;
            
            if (targetUnit == null || targetUnit.battleInfo == null) return;
            if (targetUnit.heroData == null || targetUnit.heroData.heroID != 0) return;

            targetUnit.battleInfo.makeDamage = _storedEnemyTotalHp;
            targetUnit.battleInfo.takeDamage = 0f;
            targetUnit.battleInfo.enemyKilled = _storedEnemyCount;
            targetUnit.battleInfo.enemyKillScorePercent = _storedEnemyTotalHp;
        }
    }
    
    /// <summary>
    /// 战斗结束时重置跳过状态
    /// </summary>
    [HarmonyPatch(typeof(BattleController), nameof(BattleController.BattleEnd))]
    public static class BattleEndPatch
    {
        public static void Postfix()
        {
            _isSkipping = false;
        }
    }

  
}