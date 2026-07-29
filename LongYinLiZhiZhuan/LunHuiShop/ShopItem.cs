namespace LunHuiShop;

/// <summary>
///     商店物品数据类（Id 隐藏不显示在表格中）
/// </summary>
public class ShopItem
{
    /// <summary>唯一标识（隐藏字段，不显示在表格）</summary>
    public int Id { get; set; }
    // 名称
    public string Name { get; set; }
    // 物品等级
    public string ItemLevel { get; set; }
    // 筛选大类
    public string Type { get; set; }
    // 筛选小类
    public string SortType { get; set; }
    // 需要银两值
    public int Price { get; set; }
    // 需要声望值
    public float Fame { get; set; }
    // 图标名（对应 UIAtlas 中的 sprite 名）
    public string? IconName { get; set; }
}
