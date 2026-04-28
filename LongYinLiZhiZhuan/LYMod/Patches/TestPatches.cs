using HarmonyLib;
using Il2Cpp;

namespace LYMod.Patches;

[HarmonyPatch]
public class TestPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(SteamStatsAndAchievements), nameof(SteamStatsAndAchievements.CheckDLCState))]
    public static bool SteamStatsAndAchievements_CheckDLCState_Prefix(SteamStatsAndAchievements __instance)
    {
        if (!Plugin.Instance.Dlc0Flag.Value) return true;
        var ppd = GameDataController.playerPrefData.playerPrefData;
        var dlc0 = ppd.GetInt("DLC0");
        if (dlc0 != 0) return true;
        ppd.SetKey("DLC0", 1);
        return false;
    }
    
    private static readonly int[] MaximizedLayout =
    {
        -1, -1, -1, -1, -1, -1, -1, -2, -1, -1, -1, -1, -1, -1, -1,
        -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, -1,
        -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, -1,
        -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, -1,
        -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, -1,
        -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, -1,
        -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, -1,
        -2, 1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, -2,
        -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, -1,
        -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, -1,
        -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, -1,
        -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, -1,
        -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, -1,
        -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, -1,
        -1, -1, -1, -1, -1, -1, -1, -2, -1, -1, -1, -1, -1, -1, -1
    };

    private static AreaTileType IntToTileType(int value)
    {
        if (Plugin.Instance.MaxAreaFlag1)
        {
            return value switch
            {
                -2 => AreaTileType.CityGate,
                -1 => AreaTileType.CityWall,
                0 => AreaTileType.EmptySpace,
                1 => AreaTileType.Road,
                2 => AreaTileType.MainBuilding,
                _ => AreaTileType.Null
            };
        }

        return value switch
        {
            -2 => AreaTileType.CityGate,
            -1 or 0 => AreaTileType.EmptySpace,
            1 => AreaTileType.Road,
            2 => AreaTileType.MainBuilding,
            _ => AreaTileType.Null
        };
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameController), nameof(GameController.LoadAreaMapData))]
    public static void LoadAreaMapDataPostfix(GameController __instance, AreaData? areaData)
    {
        if (__instance != null && areaData != null && Plugin.Instance.MaxAreaFlag)
        {
            var tiles = areaData.areaTiles;
            if (tiles == null || tiles.Count == 0) return;


            if (areaData.areaID != 55)
            {
               return;
            }
            Plugin.LOG.Msg($"[LoadAreaMapData] 已跳过非仙霞派区域 (ID={areaData.areaID})");
            
            Plugin.LOG.Msg($"[LoadAreaMapData] 区域: {areaData.GetAreaName()} (ID={areaData.areaID}), " +
                           $"尺寸: {areaData.mapWidth}x{areaData.mapHeight}");

            var targetSize = 15;
            Plugin.LOG.Msg($"[LoadAreaMapData] 目标尺寸: {targetSize}x{targetSize}, " +
                           $"当前尺寸: {areaData.mapWidth}x{areaData.mapHeight}");
            Plugin.LOG.Msg($"[LoadAreaMapData] MaxAreaFlag1: {Plugin.Instance.MaxAreaFlag1}");

            if (areaData.mapWidth == targetSize && areaData.mapHeight == targetSize)
            {
                if (Plugin.Instance.MaxAreaFlag1)
                {
                    Plugin.LOG.Msg("[LoadAreaMapData] 已是最大尺寸，保留城墙");
                }
                else
                {
                    Plugin.LOG.Msg("[LoadAreaMapData] 已是最大尺寸，去掉城墙");
                    var modifiedCount = 0;
                    foreach (var tile in tiles)
                        if (tile.tileType == AreaTileType.CityWall || tile.tileType == AreaTileType.Null)
                        {
                            tile.tileType = AreaTileType.EmptySpace;
                            modifiedCount++;
                        }
                    Plugin.LOG.Msg($"[LoadAreaMapData] 修改 {modifiedCount} 个地块");
                }
                return;
            }

            Plugin.LOG.Msg($"[LoadAreaMapData] 扩展地图到 {targetSize}x{targetSize}");

            var newTiles = new Il2CppSystem.Collections.Generic.List<AreaTileData>();
            for (var i = 0; i < MaximizedLayout.Length; i++)
            {
                var row = i / targetSize;
                var col = i % targetSize;
                var tileType = IntToTileType(MaximizedLayout[i]);
                var tile = new AreaTileData(areaData.areaID, tileType);
                tile.row = row;
                tile.column = col;
                newTiles.Add(tile);
            }

            areaData.areaTiles = newTiles;
            areaData.mapWidth = targetSize;
            areaData.mapHeight = targetSize;

            Plugin.LOG.Msg($"[LoadAreaMapData] 完成！新建 {newTiles.Count} 个地块");
            Plugin.Instance.MaxAreaFlag = false;
            Plugin.Instance.MaxAreaFlag1 = false;
        }
    }
    
}