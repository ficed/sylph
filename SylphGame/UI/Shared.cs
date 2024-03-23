using Microsoft.Xna.Framework;
using SylphGame.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame.UI {
    public abstract class BaseUI : UIScreen {

        protected Group SmallChar(Character chr, CoreBattle battle, Prop<int> x, Prop<int> y) {
            return Group(x, y,
                Label(50, 0, chr.Name),
                Label(90, 0, "LV", color: Color.Aqua),
                Label(120, 0, Dyn(() => battle.Level.ToString()), alignment: TextAlign.Right, font: _sgame.NumericFont),
                Image(0, 0, 40, 40, chr.SmallImage),
                FullGauge(50, 6, "HP", () => battle.CurrentHP, () => battle.MaxHP),
                FullGauge(50, 20, "MP", () => battle.CurrentMP, () => battle.MaxMP)
            );
        }
    }
}
