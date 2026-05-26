using Il2Cpp;

namespace LYMod.Helpers;

public static class HeroHelper
{
    /*
     * 当前人物属性
     */
    public static bool TryReadNowHero(out HeroData nowHero)
    {
        nowHero = null;
        var hdc = HeroDetailController.Instance;
        if (hdc == null) return false;
        nowHero = hdc.nowShowHero;
        return nowHero != null;
    }
    /**
     * 玩家属性
     */
    public static bool TryReadPlayer(out HeroData player)
    {
        player = null;
        var gc = GameController.Instance;
        if (gc == null) return false;
        var wd = gc.worldData;
        if (wd == null) return false;
        player = wd.Player();
        return player != null;
    }

    /**
     * 根据人物id获取人物信息
     */
    public static bool TryGetHeroByID(int id, out HeroData heroData)
    {
        heroData = null;
        var gc = GameController.Instance;
        if (gc == null) return false;
        heroData = gc.worldData.GetHero(id);
        return heroData != null;
    }
    
    /**
     * 解锁所有皮肤
     */
    public static void UnlockSkins()
    {
        var list = GameDataController.Instance.skinDataBase;
        foreach (var skin in list)
        {
            if (skin.DLC == 0) skin.DLC = -1;
        }
        var gc = GameController.Instance;
        if (gc == null) return;
        int[] skinIds = {-10,-9,-8,-7,-6,-5,-4,-3,-2,-1,0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,-100,-101};
        int[] lvs = { 0, 1, 2, 3, 4, 5 };
        for (var i = 0; i < skinIds.Length; i++)
        {
            for (var j = 0; j < lvs.Length; j++)
            {
                gc.worldData.UnlockSkin(skinIds[i], lvs[j], true);
            }
        }
    }
}