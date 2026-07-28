using UnityEngine;

namespace LunHuiShop;

public class LyHelper
{
    /// <summary>
    /// 获取玩家信息
    /// </summary>
    public static bool TryReadPlayer(out HeroData player)
    {
        player = null!;
        var gc = GameController.Instance;
        if (gc == null) return false;
        var wd = gc.worldData;
        if (wd == null) return false;
        player = wd.Player();
        return player != null;
    }
    
    /// <summary>
    /// 添加游戏内提示信息
    /// </summary>
    public static void AddInfoTab(string infoText, string atlasName = "UIAtlas", string infoPic = null!,
        string soundName = "Woosh", float volumn = 1f, float lastTime = 5f, Color picColor = default)
    {
        var infoController = InfoController.Instance;
        if (infoController == null) return;
        infoController.AddInfoTab(infoText, atlasName, infoPic, soundName, volumn, lastTime, picColor);
    }

}