using SylphGame.Characters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame.UI {
    public class ItemScreen : BaseUI {

        private Container _arrangeMenu, _topMenu;
        private Component _keyItemsList, _itemsList;
        private Label _lDescription;
        private InventoryStore _store;

        private void FocusTop() {
            _keyItemsList.Visible = false;
            _itemsList.Visible = true;
        }
        private void SelItems() {
            _keyItemsList.Visible = false;
            _itemsList.Visible = true;
            Focus(_itemsList);
        }

        private void SelArrange() {
            _keyItemsList.Visible = false;
            _itemsList.Visible = true;
            Popup(_arrangeMenu);
        }

        private void SelKey() {
            _arrangeMenu.Visible = _itemsList.Visible = false;
            _keyItemsList.Visible = true;
            Focus(_keyItemsList);
        }


        
        private void SortBy(Func<InventoryItem, IComparable> sortKey) {
            var inv = _sgame.Party.As<CoreInventory>();
            var sorted = inv.Items 
                .OrderBy(sortKey)
                .ThenBy(inv => inv.ID)
                .ToList();

            if (inv.Items.Zip(sorted).All(pair => pair.First == pair.Second))
                sorted.Reverse();

            inv.Items.Clear();
            inv.Items.AddRange(sorted);
        }
        private void SortBy(Func<ItemDetails, IComparable> sortKey) {
            SortBy((InventoryItem inv) => sortKey(_store.GetItem(inv.ID)));
        }

        private void Select(InventoryItem item) {

        }

        protected override void FocusChanged() {
            base.FocusChanged();
            if (Focused?.Owner == _topMenu)
                FocusTop();
        }

        public override void Init(SGame sgame) {
            base.Init(sgame);

            _store = sgame.Load<InventoryStore>("");

            _container = Group(0, 0,
                Box(5, 5, 400, 25,
                    Focus(Ref(out _topMenu, MenuH(10, 2, 390, 15,
                        ("Items", () => true, SelItems),
                        ("Arrange", () => true, SelArrange),
                        ("Key", () => true, SelKey)
                    )))
                ),

                Box(5, 35, 160, 200,
                    Group(8, 5, sgame.Party.ActiveChars.Select((chr, index) => SmallChar(chr, chr, 0, index * 50)).ToArray())
                ),

                Box(170, 35, 240, 25,
                    Ref(out _lDescription, Label(10, 2, ""))
                ),

                Box(170, 60, 240, 175,
                    Ref(out _itemsList, ListBox(
                        10, 5, 220, 170,
                        sgame.DefaultFont, sgame.NumericFont,
                        sgame.Party.As<CoreInventory>().Items,
                        item => _lDescription.Text = item == null ? "" : _store.GetItem(item.ID).Description,
                        Select,
                        item => _store.GetItem(item.ID).Name,
                        item => item.Quantity.ToString()
                    )),
                    Ref(out _keyItemsList, ListBox(
                        10, 5, 225, 170,
                        sgame.DefaultFont, sgame.NumericFont,
                        sgame.Party.As<CoreInventory>().KeyItems,
                        id => _lDescription.Text = id == 0 ? "" : _store.GetKey(id).Description,
                        null,
                        id => _store.GetKey(id).Name,
                        null
                    ))
                ),

                Ref(out _arrangeMenu, Box(150, 25, 60, 80,
                    MenuV(10, 5, 40, 70,
                        ("Quantity", () => true, () => SortBy(inv => inv.Quantity)),
                        ("Name", () => true, () => SortBy(item => item.Name))
                    )
                )).WithVisible(false)
            );

            FocusTop();
        }
    }
}
