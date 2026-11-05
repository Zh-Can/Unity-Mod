using Il2Cpp;
using UnityEngine;

namespace SmartTrade
{
    /// <summary>
    /// 新版 ItemListController 是「虚拟列表 + 图标对象池」：
    /// 只有落在可视区间里的条目才会 RentIcon 出 GameObject，其它物品根本不存在，
    /// 所以自动化买卖会"看不到就点不到"。
    ///
    /// 这里按游戏自己的绑定方式（UpdateVisibleCells 内部逻辑）把整张列表都租出图标：
    /// 超出视口的格子只是被遮罩裁掉，看不见但能点，不用再滚动去"刷"物品。
    /// 每次列表刷新后虚拟化会把多余格子回收，所以每轮扫描前补齐一次即可。
    /// </summary>
    internal static class BulkIconBinder
    {
        public static bool CanBind(ItemListController list)
        {
            if (list == null) return false;
            return list.itemGrid != null && list.currentDataList != null && list.activeItemMap != null;
        }

        /// <summary>把 currentDataList 里所有还没有图标的条目都绑定好，返回本次新绑定的数量</summary>
        public static int EnsureAllBound(ItemListController list)
        {
            if (!CanBind(list)) return 0;

            var data = list.currentDataList;
            var map = list.activeItemMap;

            var interact = (int)list.itemListInteractType;
            var isTrade = interact >= (int)ItemListInteractType.TradeLeft &&
                          interact <= (int)ItemListInteractType.TradeRightOut;
            // 游戏内部是 Enum.Parse(TradeIconType, Enum.GetName(ItemListInteractType, ...))，二者只差 1
            var tradeIconType = isTrade ? (TradeIconType)(interact - 1) : TradeIconType.None;

            var count = data.Count;
            var bound = 0;

            for (var i = 0; i < count; i++)
            {
                if (map.ContainsKey(i)) continue;

                var item = data[i];
                if (item == null) continue;

                var go = list.RentIcon();
                if (go == null) continue;

                var rect = go.GetComponent<RectTransform>();
                if (rect != null) list.PositionItem(rect, i);

                var icon = go.GetComponent<ItemIconController>();
                if (icon == null) continue;

                icon.itemData = item;
                icon.itemListID = i;
                icon.itemIconType = isTrade ? ItemIconType.Trade : ItemIconType.Show;
                icon.tradeIconType = tradeIconType;
                icon.needRefreshPriceIcon = true;
                icon.inited = false;

                map.Add(i, go);
                bound++;
            }

            return bound;
        }

        /// <summary>操作结束后让虚拟化把多余格子回收掉</summary>
        public static void ReleaseExtra(ItemListController list)
        {
            if (list == null) return;
            list.UpdateVisibleCells(true);
        }
    }
}
