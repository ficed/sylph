using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame.Characters {

    public class ItemDetails {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? WeaponID { get; set; }
        public int? ArmourID { get; set; }
        public int? AccessoryID { get; set; }
        public int? AttackID { get; set; }
    }

    public class InventoryItem {
        public int ID { get; set; }
        public int Quantity { get; set; }
    }

    public class CoreInventory : IPartyBehaviour {
        public List<InventoryItem> Items { get; set; } = new();
        public List<int> KeyItems { get; set; } = new();
    }

    public class KeyItem {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }


    public class InventoryStore : ICacheable {
        public List<ItemDetails> Items { get; set; } = new();
        public List<KeyItem> KeyItems { get; set; } = new();

        private Dictionary<int, ItemDetails> _itemsDict;
        private Dictionary<int, KeyItem> _keyDict;

        public ItemDetails GetItem(int id) => _itemsDict[id];
        public KeyItem GetKey(int id) => _keyDict[id];

        private void Key() {
            _itemsDict = Items.ToDictionary(i => i.ID, i => i);
            _keyDict = KeyItems.ToDictionary(k => k.ID, k => k);
        }

        public static InventoryStore Load(SGame sgame) {
            using (var s = sgame.Data.Open("data", "items.json")) {
                var store = Util.LoadJson<InventoryStore>(s);
                store.Key();
                return store;
            }
        }
    }
}
