using Il2Cpp;

namespace LYMod.Helpers;

// 「建筑/科技资源零消耗」的统一实现。
//
// 不能把花费直接改成 0：ForceData.CostResource 的核心有 `if (num != 0)` 守卫，
// 金额为 0 时整个函数体（含记账、月度统计等副作用）都会被跳过。
//
// 这里用「垫付 + 还原」：
//   调用前把余额补到刚好够付 → 游戏照常扣、照常记账 → 调用后精确还原余额快照。
// 同时把 HaveResource 对玩家门派恒判为 true，否则资源不足时根本进不到 CostResource。
//
// 只对玩家自己的门派生效（HeroHelper.IsPlayerForce），避免 AI 门派也变成无限资源。
public static class FreeCost
{
    public static bool Enabled => Plugin.Instance.Cost0.Value;

    /// <summary>HaveResource 用：玩家门派直接判为"付得起"，其余走原逻辑。</summary>
    public static bool Allow(ForceData? force, ref bool result)
    {
        if (!Enabled || force == null || !HeroHelper.IsPlayerForce(force)) return true;

        result = true;
        return false;   // 跳过原方法
    }

    /// <summary>按资源 id 的花费清单垫付，返回原始余额快照；不满足条件返回 null。</summary>
    public static Il2CppSystem.Collections.Generic.List<float>? Borrow(
        ForceData? force, Il2CppSystem.Collections.Generic.List<float>? costs)
    {
        if (!Enabled || force?.resourceStore == null || costs == null) return null;
        if (!HeroHelper.IsPlayerForce(force!)) return null;

        var arr = new float[force.resourceStore.Count];
        for (var i = 0; i < costs.Count && i < arr.Length; i++) arr[i] = costs[i];
        return BorrowCore(force, arr);
    }

    public static Il2CppSystem.Collections.Generic.List<float>? Borrow(
        ForceData? force, Il2CppSystem.Collections.Generic.List<ResourceData>? costs)
    {
        if (!Enabled || force?.resourceStore == null || costs == null) return null;
        if (!HeroHelper.IsPlayerForce(force!)) return null;

        var arr = new float[force.resourceStore.Count];
        for (var i = 0; i < costs.Count; i++)
        {
            var r = costs[i];
            if (r != null && r.resourceType >= 0 && r.resourceType < arr.Length) arr[r.resourceType] = r.resourceNum;
        }

        return BorrowCore(force, arr);
    }

    public static Il2CppSystem.Collections.Generic.List<float>? Borrow(ForceData? force, ResourceData? cost)
    {
        if (!Enabled || force?.resourceStore == null || cost == null) return null;
        if (!HeroHelper.IsPlayerForce(force!)) return null;

        var arr = new float[force.resourceStore.Count];
        if (cost.resourceType >= 0 && cost.resourceType < arr.Length) arr[cost.resourceType] = cost.resourceNum;
        return BorrowCore(force, arr);
    }

    public static Il2CppSystem.Collections.Generic.List<float>? Borrow(ForceData? force, int id, float num)
    {
        if (!Enabled || force?.resourceStore == null) return null;
        if (!HeroHelper.IsPlayerForce(force!)) return null;

        var arr = new float[force.resourceStore.Count];
        if (id >= 0 && id < arr.Length) arr[id] = num;
        return BorrowCore(force, arr);
    }

    /// <summary>把缺的部分补到刚好够付，返回原始余额快照。</summary>
    private static Il2CppSystem.Collections.Generic.List<float> BorrowCore(ForceData force, float[] costs)
    {
        var store = force.resourceStore;
        var snap = new Il2CppSystem.Collections.Generic.List<float>();
        for (var i = 0; i < store.Count; i++) snap.Add(store[i]);

        for (var i = 0; i < costs.Length && i < store.Count; i++)
        {
            if (costs[i] > 0f && store[i] < costs[i]) store[i] = costs[i];
        }

        return snap;
    }

    /// <summary>把余额还原成调用前的快照（净效果：没花钱）。</summary>
    public static void Restore(ForceData? force, Il2CppSystem.Collections.Generic.List<float>? snap)
    {
        if (snap == null || force?.resourceStore == null) return;

        var store = force.resourceStore;
        var n = snap.Count < store.Count ? snap.Count : store.Count;
        for (var i = 0; i < n; i++) store[i] = snap[i];
    }
}
