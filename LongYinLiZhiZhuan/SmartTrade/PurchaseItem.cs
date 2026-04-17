using System;
using Il2Cpp;

namespace SmartTrade;

public class PurchaseItem : IEquatable<PurchaseItem>
{
    public ItemData ItemData { get; }
    public int PurchasePrice { get; set; }

    public PurchaseItem(ItemData itemData, int purchasePrice)
    {
        ItemData = itemData;
        PurchasePrice = purchasePrice;
    }

    public bool Equals(PurchaseItem other)
    {
        if (other == null) return false;
        if (ItemData == null || other.ItemData == null) return false;
        return ItemData.itemID == other.ItemData.itemID &&
               ItemData.type == other.ItemData.type &&
               ItemData.subType == other.ItemData.subType &&
               ItemData.value == other.ItemData.value;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is PurchaseItem otherItem)) return false;
        return Equals(otherItem);
    }

    public override int GetHashCode()
    {
        if (ItemData == null) return 0;
        return HashCode.Combine(ItemData.itemID, ItemData.type, ItemData.subType);
    }
}
