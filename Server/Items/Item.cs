using Server.Data;
using Server.DB;

namespace Server.Items {
    public class Item {
        public int ItemDBId { get; set; }
        public int TemplateId { get; set; }
        public int Count { get; set; }
        public int Slot { get; set; }
        public bool Equipped { get; set; }
        public ItemType ItemType { get; private set; }

        public Item(ItemType itemType, int templateId) {
            ItemType = itemType;
            TemplateId = templateId;
        }

        public static Item MakeItem(ItemDB itemDB) {
            Item item = null;

            ItemData itemData = null;
            DataManager.ItemDict.TryGetValue(itemDB.TemplateId, out itemData);
            if (itemData == null) {
                return null;
            }

            item = new Item(itemData.itemType, itemDB.TemplateId);

            if (item != null) {
                item.ItemDBId = itemDB.ItemDBId;
                item.Count = itemDB.Count;
                item.Slot = itemDB.Slot;
                item.Equipped = itemDB.Equipped;
            }

            return item;
        }
    }
}
