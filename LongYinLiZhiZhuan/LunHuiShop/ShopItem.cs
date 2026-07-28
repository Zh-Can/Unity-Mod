namespace LunHuiShop;

/// <summary>
///     商店物品数据类（Id 隐藏不显示在表格中）
/// </summary>
public class ShopItem
{
    /// <summary>唯一标识（隐藏字段，不显示在表格）</summary>
    public int Id { get; set; }

    public string Name { get; set; }
    public string Type { get; set; }
    public string Level { get; set; }
    public string Quality { get; set; }
    public int Price { get; set; }
    public int Reputation { get; set; }
}
