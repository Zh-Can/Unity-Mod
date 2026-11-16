using System;
using System.IO;
using Il2Cpp;
using MelonLoader.Utils;
using Il2CppBinaryFormatter = Il2CppSystem.Runtime.Serialization.Formatters.Binary.BinaryFormatter;
using Il2CppMemoryStream = Il2CppSystem.IO.MemoryStream;

namespace LYMod.Helpers;

// 藏宝阁（ExternalStorateData.dat）是全局单文件，而自创武学的定义只活在单个存档的
// worldData.customSkills 里；书上只带 BookData.skillID，换存档后这个 id 查不到任何定义。
//
// 这里把整份 CustomSkillData 按 skillID 落盘成 sidecar 文件，读档时由
// ExternalStoragePatches 重建到当前存档（id 空闲就沿用原 id，被别的武学占了就换新 id）。
// 目录与游戏内容对应：UserData/ExternalStorage/<skillID>.bin
public class ExternalStorageDefStore
{
    private const string FolderName = "ExternalStorage";
    private const string OrphanFileName = "orphans.log";

    public static string FolderPath => Path.Combine(MelonEnvironment.UserDataDirectory, FolderName);

    private static string PathOf(int skillID) => Path.Combine(FolderPath, skillID + ".bin");

    public static bool Exists(int skillID) => File.Exists(PathOf(skillID));

    /// <summary>把一份自创武学定义写到 UserData/ExternalStorage/&lt;skillID&gt;.bin。</summary>
    public static bool Save(int skillID, CustomSkillData def)
    {
        try
        {
            if (def == null || skillID <= 0) return false;

            var ms = new Il2CppMemoryStream();
            new Il2CppBinaryFormatter().Serialize(ms, def.Cast<Il2CppSystem.Object>());
            var bytes = ms.ToArray();
            ms.Dispose();

            if (bytes == null || bytes.Length == 0) return false;

            Directory.CreateDirectory(FolderPath);
            File.WriteAllBytes(PathOf(skillID), bytes);
            return true;
        }
        catch (Exception ex)
        {
            Plugin.LOG.Warning("[LYMod] 导出武学定义失败: id=" + skillID + " " + ex.Message);
            return false;
        }
    }

    /// <summary>读回定义；文件不存在或反序列化失败都返回 null，由调用方决定怎么兜底。</summary>
    public static CustomSkillData Load(int skillID)
    {
        try
        {
            var path = PathOf(skillID);
            if (!File.Exists(path)) return null;

            var bytes = File.ReadAllBytes(path);
            if (bytes == null || bytes.Length == 0) return null;

            var ms = new Il2CppMemoryStream(bytes);
            var obj = new Il2CppBinaryFormatter().Deserialize(ms);
            ms.Dispose();

            return obj == null ? null : obj.TryCast<CustomSkillData>();
        }
        catch (Exception ex)
        {
            Plugin.LOG.Warning("[LYMod] 读取武学定义失败: id=" + skillID + " " + ex.Message);
            return null;
        }
    }

    /// <summary>记一条"这本书找不到定义"的流水，方便事后从别处找回。</summary>
    public static void WriteOrphan(int skillID, string itemName)
    {
        try
        {
            Directory.CreateDirectory(FolderPath);
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\tskillID={skillID}\t{itemName}";
            File.AppendAllText(Path.Combine(FolderPath, OrphanFileName), line + Environment.NewLine);
        }
        catch (Exception ex)
        {
            Plugin.LOG.Warning("[LYMod] 记录孤儿秘籍失败: " + ex.Message);
        }
    }

    /// <summary>
    /// 净化前把整本书序列化一份快照到 ExternalStorage/orphans/，万一以后要找回还有原始字节。
    /// 快照失败也不阻断净化（净化本身是为了避免虚空物品）。
    /// </summary>
    public static void SaveOrphanItem(int skillID, ItemData item)
    {
        try
        {
            if (item == null) return;

            var ms = new Il2CppMemoryStream();
            new Il2CppBinaryFormatter().Serialize(ms, item.Cast<Il2CppSystem.Object>());
            var bytes = ms.ToArray();
            ms.Dispose();
            if (bytes == null || bytes.Length == 0) return;

            var dir = Path.Combine(FolderPath, "orphans");
            Directory.CreateDirectory(dir);
            var file = Path.Combine(dir, $"{skillID}_{DateTime.Now:yyyyMMdd_HHmmss}.bin");
            File.WriteAllBytes(file, bytes);
        }
        catch (Exception ex)
        {
            Plugin.LOG.Warning("[LYMod] 备份孤儿秘籍失败: id=" + skillID + " " + ex.Message);
        }
    }
}
