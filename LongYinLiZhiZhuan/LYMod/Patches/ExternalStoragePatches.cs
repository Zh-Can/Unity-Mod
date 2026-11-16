using System;
using System.Collections.Generic;
using HarmonyLib;
using Il2Cpp;
using LYMod.Helpers;

namespace LYMod.Patches;

// 藏宝阁（ExternalStorage）禁止放入「自创武学秘籍」。
// 游戏判定条件：item.type == Book && BookData.skillID >= GlobalData.CustomSkillIDStart
//（自创武学的 id 是从 20000 开始往后分配的）。
//
// 做法：在判定执行期间把 GlobalData.CustomSkillIDStart 临时抬到 int.MaxValue，
// 让"skillID >= 起始 id"恒为 false；方法结束（含异常）立刻还原。
// 物品自身的 skillID 完全不动，存档里保存的仍是真实 id。
//
// 另一个问题：藏宝阁是全局单文件，自创武学定义只在单个存档里。
// 所以放入时把定义导出到 UserData/ExternalStorage/<skillID>.bin，
// 读档时再重建到当前存档（见下方 Reconcile）。
public class ExternalStoragePatches
{
    private static int _savedIdStart = -1;
    private static int _raiseDepth;

    /// <summary>抬高起始 id，让自创秘籍通过检查。可重入：嵌套调用只抬一次、只在最外层还原。</summary>
    private static void Raise()
    {
        try
        {
            if (!Plugin.Instance.CustomSkillIntoShowRoomFlag.Value) return;

            _raiseDepth++;
            if (_savedIdStart >= 0) return;   // 已经抬过了，只记层数

            _savedIdStart = GlobalData.CustomSkillIDStart;
            GlobalData.CustomSkillIDStart = int.MaxValue;
        }
        catch (Exception ex)
        {
            Plugin.LOG.Warning("[LYMod] 藏宝阁自创武学限制处理失败: " + ex.Message);
        }
    }

    /// <summary>把起始 id 还原成游戏原值（只在最外层调用结束时执行）。</summary>
    private static void Restore()
    {
        try
        {
            if (_raiseDepth > 0) _raiseDepth--;
            if (_raiseDepth > 0) return;      // 外层还没返回，继续维持抬高状态
            if (_savedIdStart < 0) return;

            GlobalData.CustomSkillIDStart = _savedIdStart;
            _savedIdStart = -1;
        }
        catch (Exception ex)
        {
            // 还原失败也不能影响主流程，只记日志
            Plugin.LOG.Warning("[LYMod] 还原 CustomSkillIDStart 失败: " + ex.Message);
        }
    }

    // 选中物品后、弹"确认要将这#name#复制到藏宝阁内吗？"之前的那道判定
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlotController), nameof(PlotController.ItemIntoExternalStorageChoosen))]
    public static void PlotController_ItemIntoExternalStorageChoosen_Prefix() => Raise();

    [HarmonyFinalizer]
    [HarmonyPatch(typeof(PlotController), nameof(PlotController.ItemIntoExternalStorageChoosen))]
    public static Exception PlotController_ItemIntoExternalStorageChoosen_Finalizer(Exception __exception)
    {
        Restore();
        return __exception;
    }

    // 真正写入藏宝阁数据时的判定（原版命中只打日志然后 return，不写入）
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameDataController), nameof(GameDataController.ItemIntoExternalStorage))]
    public static void GameDataController_ItemIntoExternalStorage_Prefix() => Raise();

    [HarmonyFinalizer]
    [HarmonyPatch(typeof(GameDataController), nameof(GameDataController.ItemIntoExternalStorage))]
    public static Exception GameDataController_ItemIntoExternalStorage_Finalizer(Exception __exception)
    {
        Restore();
        return __exception;
    }

    /// <summary>
    /// 藏宝阁价值容量。原版 = 500 × (展示类建筑数 + 8)，装不下更贵的物品时列表里直接看不到。
    /// 开关打开时锁成 1 亿，等于取消容量限制。
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameDataController), nameof(GameDataController.GetExternalStorageMaxValue))]
    public static void GameDataController_GetExternalStorageMaxValue_Postfix(GameDataController __instance, ref int __result)
    {
        if (__instance == null) return;
        if (!Plugin.Instance.ExternalStorageFlag.Value) return;

        __result = 100000000;
    }

    /// <summary>
    /// 藏宝阁每次写盘时，把当前列表里所有自创武学秘籍的定义导出成 sidecar 文件。
    /// 挂在这里而不是挂 ItemIntoExternalStorage，是因为不论从哪条路径加进来的
    /// （放入、批量、其它 mod），最后都会走这个方法写盘，一处覆盖全部。
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameDataController), nameof(GameDataController.SaveExternalStorageData))]
    public static void GameDataController_SaveExternalStorageData_Postfix()
    {
        try
        {
            if (!Plugin.Instance.CustomSkillIntoShowRoomFlag.Value) return;

            var storage = GameDataController.ExternalStorage;
            var wd = GameController.Instance?.worldData;
            var dict = wd?.customSkills;
            if (storage?.allItem == null || dict == null) return;

            var saved = new HashSet<int>();
            var unknown = 0;

            for (var i = 0; i < storage.allItem.Count; i++)
            {
                ItemData item;
                try
                {
                    item = storage.allItem[i];
                }
                catch
                {
                    break;
                }

                if (item == null || item.type != ItemType.Book) continue;

                var book = item.bookData;
                if (book == null) continue;

                var id = book.skillID;
                if (id < GlobalData.CustomSkillIDStart) continue;

                if (!dict.ContainsKey(id))
                {
                    unknown++;
                    continue;
                }

                if (!saved.Add(id)) continue;   // 同一个 id 只写一次

                var def = dict[id];
                if (ExternalStorageDefStore.Save(id, def))
                {
                    Plugin.LOG.Msg($"[LYMod] 已导出自创武学定义: id={id} name={def.skillData?.name}");
                }
            }

            if (unknown > 0)
            {
                Plugin.LOG.Warning($"[LYMod] 藏宝阁里有 {unknown} 本自创秘籍在当前存档查不到定义，将在对账时处理");
            }
        }
        catch (Exception ex)
        {
            Plugin.LOG.Warning("[LYMod] 导出自创武学定义失败: " + ex.Message);
        }
    }

    /// <summary>
    /// 读档后触发（和 LoadSavePostPatches 里同名 postfix 并存，Harmony 允许多个 postfix）。
    /// 此时 worldData 已经就绪，直接把藏宝阁里的自创秘籍对账到当前存档。
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameController), nameof(GameController.GameStartTeleportPlayer))]
    public static void GameController_GameStartTeleportPlayer_Postfix() => TryReconcile("读档");

    /// <summary>打开藏宝阁界面时也对一次账：顺便把缺失的 sidecar 补出来，切换存档前就齐了。</summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ShowRoomController), nameof(ShowRoomController.Init))]
    public static void ShowRoomController_Init_Postfix() => TryReconcile("打开藏宝阁");

    /// <summary>藏宝阁是全局文件，读盘后再对一次账（存档还没进来时会等读档钩子）。</summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameDataController), nameof(GameDataController.LoadExternalStorageData))]
    public static void GameDataController_LoadExternalStorageData_Postfix() => TryReconcile("藏宝阁读盘");

    /// <summary>worldData 就绪才动，否则留给读档钩子；对账本身是幂等的，重复调用没副作用。</summary>
    private static void TryReconcile(string reason)
    {
        try
        {
            var wd = GameController.Instance?.worldData;
            if (wd == null)
            {
                Plugin.LOG.Msg($"[LYMod] 藏宝阁同步等待读档（{reason}）");
                return;
            }

            Reconcile(wd);
        }
        catch (Exception ex)
        {
            Plugin.LOG.Warning($"[LYMod] 藏宝阁定义同步失败（{reason}）: " + ex.Message);
        }
    }

    /// <summary>
    /// 把藏宝阁里每本自创秘籍对到当前存档的 customSkills：
    ///  - id 空闲 → 直接把定义注入原 id，书不用改（跨存档来回走也不漂移）
    ///  - id 被别的武学占了 → 用 AddCustomSkill 分配新 id，改写书的 skillID
    ///  - 找不到定义 → 从藏宝阁移除并记 orphans.log，避免虚空物品/崩溃
    /// </summary>
    private static void Reconcile(WorldData wd)
    {
        if (!Plugin.Instance.CustomSkillIntoShowRoomFlag.Value) return;

        var storage = GameDataController.ExternalStorage;
        var dict = wd.customSkills;
        if (storage?.allItem == null || dict == null) return;

        var remap = new Dictionary<int, int>();   // 旧 id → 新 id（同一份定义被多本书引用时复用）
        var injected = 0;
        var remapped = 0;
        var orphans = 0;
        var dirty = false;

        for (var i = storage.allItem.Count - 1; i >= 0; i--)
        {
            ItemData item;
            try
            {
                item = storage.allItem[i];
            }
            catch
            {
                break;
            }

            if (item == null || item.type != ItemType.Book) continue;

            var book = item.bookData;
            if (book == null) continue;

            var id = book.skillID;
            if (id < GlobalData.CustomSkillIDStart) continue;   // 静态武学，不归我们管

            CustomSkillData current = null;
            if (dict.ContainsKey(id)) current = dict[id];

            var def = ExternalStorageDefStore.Load(id);

            if (def == null)
            {
                // 定义缺失但本存档有这门武学（id 一致）→ 说明只是没导出过，补一份 sidecar 即可
                if (current != null)
                {
                    if (ExternalStorageDefStore.Save(id, current))
                    {
                        Plugin.LOG.Msg($"[LYMod] 补导出自创武学定义: id={id} name={current.skillData?.name}");
                    }

                    injected++;
                    continue;
                }

                // 还没导出过，但当前存档里确实有这门武学（按书名认）→ 就地补导出，别误删
                var byName = FindSameSkillIdByName(dict, item.name);
                if (byName > 0)
                {
                    ExternalStorageDefStore.Save(id, dict[byName]);
                    if (byName != id)
                    {
                        book.skillID = byName;
                        remapped++;
                        dirty = true;
                    }
                    else
                    {
                        injected++;
                    }

                    continue;
                }

                if (remap.TryGetValue(id, out var mapped))
                {
                    book.skillID = mapped;
                    dirty = true;
                    continue;
                }

                ExternalStorageDefStore.WriteOrphan(id, item.name);
                ExternalStorageDefStore.SaveOrphanItem(id, item);
                storage.LoseItem(item, false);
                orphans++;
                dirty = true;
                continue;
            }

            if (current != null && SameSkill(current, def)) continue;   // 已经是同一门武学

            if (def.skillData == null)
            {
                ExternalStorageDefStore.WriteOrphan(id, item.name);
                ExternalStorageDefStore.SaveOrphanItem(id, item);
                storage.LoseItem(item, false);
                orphans++;
                dirty = true;
                continue;
            }

            // 本存档里已经有这门武学（只是 id 不同，比如上次在别的存档里被重排过）
            // → 直接把书指回已有 id，不再造副本，避免两个存档来回切换越堆越多
            var existingId = FindSameSkillId(dict, def);
            if (existingId > 0 && existingId != id)
            {
                book.skillID = existingId;
                remapped++;
                dirty = true;
                continue;
            }

            if (current == null)
            {
                // id 空闲：沿用原 id 注入，书一个字都不用改
                def.skillData.skillID = id;
                dict[id] = def;
                injected++;
                continue;
            }

            // id 被别的武学占了：换新 id
            if (!remap.TryGetValue(id, out var newId))
            {
                def.skillData.skillID = 0;
                newId = wd.AddCustomSkill(def);
                if (newId < 0)
                {
                    ExternalStorageDefStore.WriteOrphan(id, item.name);
                    ExternalStorageDefStore.SaveOrphanItem(id, item);
                    storage.LoseItem(item, false);
                    orphans++;
                    dirty = true;
                    continue;
                }

                remap[id] = newId;
                ExternalStorageDefStore.Save(newId, def);   // 让这本书下次进来能稳定命中
            }

            book.skillID = newId;
            remapped++;
            dirty = true;
        }

        if (dirty)
        {
            GameDataController.Instance?.SaveExternalStorageData();
        }

        if (injected + remapped + orphans > 0)
        {
            Plugin.LOG.Msg($"[LYMod] 藏宝阁同步: 注入 {injected} 门、换 id {remapped} 本、清理无定义 {orphans} 本");
        }
    }

    /// <summary>判断两份定义是不是同一门武学（id 会变，靠名字 + 品质 + 类型认）</summary>
    private static bool SameSkill(CustomSkillData a, CustomSkillData b)
    {
        if (a?.skillData == null || b?.skillData == null) return false;
        return a.skillData.name == b.skillData.name
               && a.skillData.rareLv == b.skillData.rareLv
               && a.skillData.type == b.skillData.type;
    }

    /// <summary>在当前存档里找同名的自创武学，找不到返回 -1</summary>
    private static int FindSameSkillId(Il2CppSystem.Collections.Generic.Dictionary<int, CustomSkillData> dict,
        CustomSkillData def)
    {
        try
        {
            foreach (var kv in dict)
            {
                if (SameSkill(kv.Value, def)) return kv.Key;
            }
        }
        catch (Exception ex)
        {
            Plugin.LOG.Warning("[LYMod] 查找同名武学失败: " + ex.Message);
        }

        return -1;
    }

    /// <summary>按书名在当前存档的自创武学里反查 id（书上的 name 就是武学名，允许带后缀）</summary>
    private static int FindSameSkillIdByName(Il2CppSystem.Collections.Generic.Dictionary<int, CustomSkillData> dict,
        string name)
    {
        if (string.IsNullOrEmpty(name)) return -1;

        try
        {
            foreach (var kv in dict)
            {
                var skillName = kv.Value?.skillData?.name;
                if (string.IsNullOrEmpty(skillName)) continue;

                if (skillName == name || name.Contains(skillName) || skillName.Contains(name))
                {
                    return kv.Key;
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.LOG.Warning("[LYMod] 按名字查找武学失败: " + ex.Message);
        }

        return -1;
    }
}
