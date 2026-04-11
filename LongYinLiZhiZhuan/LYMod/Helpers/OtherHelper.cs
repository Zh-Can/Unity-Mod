using Il2Cpp;
using UnityEngine;

namespace LYMod.Helpers;

public static class OtherHelper
{
    
    /// <summary>
    /// 重置 _mainCategory 下的所有配置项为默认值
    /// </summary>
    public static void ResetAllMainConfig()
    {
        foreach (var entry in Plugin.Instance.MainCategory.Entries)
        {
            entry.ResetToDefault();
        }
        Plugin.Instance.MainCategory.SaveToFile();
    }
    
    // 输入框文本转字典
    public static Dictionary<int, float>? ParseInputBox(string inputText)
    {
        if (string.IsNullOrWhiteSpace(inputText)) 
            return null;

        return inputText
            // 先替换所有空格
            .Replace(" ", "")
            // 按分号分割键值对
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            // 按等号分割key/value
            .Select(pair => pair.Split('='))
            // 过滤无效格式（必须是key=value）
            .Where(kv => kv.Length == 2)
            // 安全转换类型（避免输错数字导致崩溃）
            .Where(kv => int.TryParse(kv[0], out _) && float.TryParse(kv[1], out _))
            // 转字典
            .ToDictionary(
                kv => int.Parse(kv[0]),
                kv => float.Parse(kv[1])
            );
    }
    public static Il2CppSystem.Collections.Generic.Dictionary<int, float>? ToIl2CppDictionary(Dictionary<int, float>? systemDict)
    {
        // 初始化 IL2CPP 字典
        var il2CPPDict = new Il2CppSystem.Collections.Generic.Dictionary<int, float>();

        // 空值判断，避免崩溃
        if (systemDict == null || systemDict.Count == 0)
        {
            return null;
        }

        // 遍历原生字典，逐个添加到 IL2CPP 字典
        foreach (var kvp in systemDict)
        {
            // 避免重复key（IL2CPP字典添加重复key会抛异常）
            if (!il2CPPDict.ContainsKey(kvp.Key))
            {
                il2CPPDict.Add(kvp.Key, kvp.Value);
            }
        }

        return il2CPPDict;
    }

    
    /// <summary>
    /// 添加游戏内提示信息
    /// </summary>
    /// <param name="infoText"></param>
    /// <param name="atlasName"></param>
    /// <param name="infoPic"></param>
    /// <param name="soundName"></param>
    /// <param name="volumn"></param>
    /// <param name="lastTime"></param>
    /// <param name="picColor"></param>
    public static void AddInfoTab(string infoText, string atlasName = "UIAtlas", string infoPic = null,
        string soundName = "Woosh", float volumn = 1f, float lastTime = 5f, Color picColor = default (Color))
    {
        var infoController = InfoController.Instance;
        if (infoController == null) return;
        infoController.AddInfoTab(infoText, atlasName, infoPic, soundName, volumn, lastTime, picColor);
    }
    
    /// <summary>
    /// 修改武学修炼数量限制倍数
    /// </summary>
    public static void ChaneMaxNum()
    {
        if (ModConfig.HaveNpcMod) return;
        
        List<float> skillBaseNum = new() {12,10,8,6,4,2};

        var maxSkillNum = GlobalData.MaxSkillNum;
        if (maxSkillNum.Count == 6)
        {
            for (int i = 0; i < 6; i++)
            {
                maxSkillNum[i] = skillBaseNum[i] * Plugin.Instance.KungFuMaxLimitTimes.Value;
            }
        }
        GlobalData.MaxSkillNum = maxSkillNum;
    }
}