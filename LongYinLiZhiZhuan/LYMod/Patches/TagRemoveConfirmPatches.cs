using HarmonyLib;
using Il2Cpp;

namespace LYMod.Patches;
public static class TagRemoveConfirmPatches
{
    private static int _pendingTagID = -1;
    private static HeroData _pendingHero = null!;
    private static ManageTagController _mtc = null!;

    private static void ShowConfirm(int tagID, HeroData hero, ManageTagController mtc, float costValue)
    {
        _pendingTagID = tagID;
        _pendingHero = hero;
        _mtc = mtc;
        
        var heroTagDataBase = GameDataController.Instance.GetTagDataBase(tagID);
        if (heroTagDataBase == null) return;
        
        
        string confirmMessage = $"确定要遗忘天赋 [{heroTagDataBase.name}] 吗？\n" +
                               $"将返还天赋点数：{costValue}";
        
        // 使用 SureMenu 自己的 gameObject 来接收消息
        SureMenu.Instance.CallSureMenu(
            confirmMessage,
            "OnConfirmRemoveTag",
            tagID.ToString(),
            SureMenu.Instance.gameObject,
            true,
            false,
            "OnCancelRemoveTag",
            ""
        );
    }
    
    // 处理确认
    private static void HandleConfirm(string param)
    {
        if (_pendingTagID == -1 || _pendingHero == null) return;
        
        var heroTagDataBase = GameDataController.Instance.GetTagDataBase(_pendingTagID);
        if (heroTagDataBase == null)
        {
            ClearPending();
            return;
        }
        
        float costValue = heroTagDataBase.GetCostValue();
        
        
        
        _pendingHero.RemoveTag(_pendingTagID, true);
        _pendingHero.ChangeTagPoint(costValue, false);
        
        var replaceTags = heroTagDataBase.replaceTag;
        var replaceNameLog = "";
        if (replaceTags is { Count: > 0 })
        {
            foreach (var replaceTagName in replaceTags)
            {
                _pendingHero.UnderstandTag(replaceTagName, false);
                replaceNameLog += replaceTagName + ",";
            }

            replaceNameLog.TrimEnd(',');
        }
        Plugin.LOG.Msg($"遗忘天赋：{heroTagDataBase.name}, 返还天赋点数：{costValue}{(replaceNameLog==""? "":", 恢复天赋：" + replaceNameLog)}");
        _mtc?.FreshManageTagUI();
        
        ClearPending();
    }
    
    // 处理取消
    private static void HandleCancel(string param)
    {
        ClearPending();
        SureMenu.Instance.HideSelf();
    }
    
    private static void ClearPending()
    {
        _pendingTagID = -1;
        _pendingHero = null!;
        _mtc = null!;
    }
    
    [HarmonyPatch(typeof(SureMenu), nameof(SureMenu.TryCallFuc))]
    public static class SureMenu_TryCallFuc_Patch
    {
        public static bool Prefix(SureMenu __instance, string targetFucName, string targetFucParam)
        {
            if (targetFucName == "OnConfirmRemoveTag")
            {
                TagRemoveConfirmPatches.HandleConfirm(targetFucParam);
                return false;
            }
        
            if (targetFucName == "OnCancelRemoveTag")
            {
                TagRemoveConfirmPatches.HandleCancel(targetFucParam);
                return false;
            }
        
            return true;
        }
    }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(HeroTagIconController), nameof(HeroTagIconController.OnClick))]
    public static bool HeroTagIconController_OnClick_Prefix(HeroTagIconController __instance)
    {
        // var mtc = ManageTagController.Instance;
        // if (mtc == null || __instance == null || !Plugin.Instance.FastRemoveTag.Value) return true;
        //
        // HeroData targetHero = mtc.targetHero;
        // HeroTagData targetTag = __instance.targetTag;
        // if (targetTag == null || targetHero == null) return true;
        // int tagID = targetTag.tagID;
        // HeroTagDataBase heroTagDataBase = targetTag.DataBase();
        // if (heroTagDataBase == null) return true;
        // if (!targetHero.HaveTag(tagID)) return true;
        // float costValue = heroTagDataBase.GetCostValue();
        // if (costValue <= 0) return true;
        // Plugin.LOG.Msg($"遗忘天赋：{targetTag.DataBase().name}, 返还天赋点数：{costValue}");
        // targetHero.RemoveTag(tagID, true);
        // targetHero.ChangeTagPoint(costValue, true);
        //
        // var replaceTags = heroTagDataBase.replaceTag;
        // Plugin.LOG.Msg($"replaceTags:{replaceTags.Count}");
        // if (replaceTags is { Count: > 0 })
        // {
        //     foreach (var replaceTagName in replaceTags)
        //     {
        //         targetHero.UnderstandTag(replaceTagName, true);
        //         Plugin.LOG.Msg($"恢复替换天赋：{replaceTagName}");
        //     }
        // }
        // mtc.FreshManageTagUI();
        // return false;
        var mtc = ManageTagController.Instance;
        if (mtc == null || __instance == null || !Plugin.Instance.FastRemoveTag.Value || !mtc.manageTagUIPanel.active) return true;
    
        HeroData targetHero = mtc.targetHero;
        HeroTagData targetTag = __instance.targetTag;
        if (targetTag == null || targetHero == null) return true;
    
        int tagID = targetTag.tagID;
        HeroTagDataBase heroTagDataBase = targetTag.DataBase();
        if (heroTagDataBase == null) return true;
        if (!targetHero.HaveTag(tagID)) return true;
    
        float costValue = heroTagDataBase.GetCostValue();
        if (costValue <= 0) return true;
    
        // 显示确认对话框，而不是直接执行
        TagRemoveConfirmPatches.ShowConfirm(tagID, targetHero, mtc, costValue);
    
        return false;
    }
}