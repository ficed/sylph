using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame {
    public class TestScreen : Screen {
        public TestScreen(SGame sgame) : base(sgame) {
            var sprite = new Entities.Sprite(_sgame, "Terra");
            var terra = sprite.New();
            terra.Layer = Layer.BACKGROUND_MID;
            terra.Position = new Vector2(64, 32);
            terra.PlayAnimation("WalkS", true);
            _entities.Add(terra);

            var dlg = new UI.DialogBox(
                _sgame,
                "Hello, here is some text that will hopefully have to wrap",
                new Rectangle(200, 180, 128, 60)
            ) {
                Layer = Layer.UI_MID
            };
            _entities.Add(dlg);
        }
    }
}
