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
                Image(0 + (battle.Statuses.HasFlag(CoreStatuses.BackRow) ? 10 : 0), 0, 40, 40, chr.SmallImage),

                Gauge(60, 25, 80, 4, Dyn(() => battle.CurrentHP), Dyn(() => battle.MaxHP)),
                Label(90, 10, Dyn(() => battle.CurrentHP.ToString()), TextAlign.Right),
                Label(100, 10, "/"),
                Label(140, 10, Dyn(() => battle.MaxHP.ToString()), TextAlign.Right),

                Gauge(170, 25, 80, 4, Dyn(() => battle.CurrentMP), Dyn(() => battle.MaxMP))
            )
            .AddChildren(GetIcons());
        }

        public override void Init(SGame sgame) {
            base.Init(sgame);

            _container = Group(0, 0,
                Box(10, 10, 300, 220,
                    sgame.Party.ActiveChars
                    .Select((c, index) => BuildChar(c, c, 10 + index * 50))
                    .ToArray()
                )
            );
        }
    }
}
