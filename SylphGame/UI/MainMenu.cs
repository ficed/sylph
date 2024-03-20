using Microsoft.Xna.Framework;
using SylphGame.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame.UI {
    public class MainMenu : UIScreen {

        private static readonly (CoreStatuses status, string icon)[] STATUS_ICONS = new[] {
            (CoreStatuses.Poison, "status_poison"),
            (CoreStatuses.Blind, "status_dark"),
        };

        private Group BuildChar(Character chr, CoreBattle battle, int yOffset) {

            IEnumerable<Component> GetIcons() {
                int x = 170;
                foreach(var entry in STATUS_ICONS) {
                    if (battle.Statuses.HasFlag(entry.status)) {
                        yield return Image(x, 0, 0, 0, entry.icon);
                        x += 16;
                    }
                }
            }

            return Group(10, yOffset,
                Label(60, 0, chr.Name),
                Label(100, 0, "LV", color: Color.Aqua),
                Label(130, 0, Dyn(() => battle.Level.ToString()), alignment: TextAlign.Right, font: "FF6SnesA"),
                Image(0 + (battle.Statuses.HasFlag(CoreStatuses.BackRow) ? 10 : 0), 0, 40, 40, chr.SmallImage),

                FullGauge(60, 10, "HP", () => battle.CurrentHP, () => battle.MaxHP, "FF6SnesA"),
                FullGauge(160, 10, "MP", () => battle.CurrentMP, () => battle.MaxMP, "FF6SnesA")
            )
            .AddChildren(GetIcons());
        }

        private void SelectItem() {

        }

        private void Quit() {
            Environment.Exit(0);
        }

        private string GameTime() {
            var time = _sgame.Party.GameTime;
            return $"{(int)time.TotalHours}:{time:mm}:{time:ss}";
        }

        public override void Init(SGame sgame) {
            base.Init(sgame);

            _container = Group(0, 0,
                Box(10, 10, 300, 220,
                    sgame.Party.ActiveChars
                    .Select((c, index) => BuildChar(c, c, 10 + index * 50))
                    .ToArray()
                ),
                Box(320, 10, 100, 160,
                    Focus(MenuV(10, 5, 80, 150,
                        ("Item", () => true, SelectItem),
                        ("Ability", () => true, SelectItem),
                        ("Equip", () => true, SelectItem),
                        ("Accessory", () => true, SelectItem),
                        ("Status", () => true, SelectItem),
                        ("Formation", () => true, SelectItem),
                        ("Options", () => true, SelectItem),
                        ("Save", () => _sgame.Party.StdIFlags[StdIFlags.SaveEnabled] != 0, SelectItem),
                        ("Quit", () => true, Quit)
                    ))
                ),
                Box(320, 180, 100, 50,
                    Label(10, 5, "Time", color: Color.Aqua),
                    Label(90, 5, (Func<string>)GameTime, TextAlign.Right, font: "FF6SnesA"),

                    Label(10, 20, "Gil", color: Color.Aqua),
                    Label(90, 20, (Func<string>)(() => _sgame.Party.StdIFlags[StdIFlags.Money].ToString()), TextAlign.Right, font: "FF6SnesA")
                )
            );

        }
    }
}
