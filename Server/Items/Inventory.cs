namespace Server.Items {
    public class Inventory {
        const int InvenSize = 350;
        public Dictionary<int, Item> Items { get; } = new Dictionary<int, Item>();

        public void Add(Item item) {
            Items.Add(item.ItemDBId, item);
        }

        public Item Get(int itemDbId) {
            Item item = null;
            Items.TryGetValue(itemDbId, out item);
            return item;
        }

        public List<Item> GetEquipItem() {
            List<Item> eqiupItems = new List<Item>();
            foreach (Item item in Items.Values) {
                if (item.Equipped) {
                    eqiupItems.Add(item);
                }
            }
            return eqiupItems;
        }

        public List<Item> GetAll() {
            return Items.Values.ToList();
        }

        public bool Remove(int itemDbId, int count = 0) {
            Item item = null;
            if (Items.TryGetValue(itemDbId, out item)) {
                item.Count = count;
                if (item.Count == 0) {
                    return Items.Remove(itemDbId);
                }
                return true;
            }
            return false;
        }


        public Item Find(Func<Item, bool> condition) {
            foreach (Item item in Items.Values) {
                if (condition(item)) {
                    return item;
                }
            }
            return null;
        }

        public int GetEmptySlot() {
            for (int slot=0; slot<InvenSize; slot++) {
                Item item = Items.Values.FirstOrDefault(i => i.Slot == slot);
                if (item == null) {
                    return slot;
                }
            }
            return -1;
        }
    }
}
