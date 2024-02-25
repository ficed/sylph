using Microsoft.Xna.Framework;
using SylphGame.UI;
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

    public class TestUIScreen : UI.UIScreen {

        public override Color Background => Color.Black;

        private void NewGame() {

        }
        private void LoadGame() {

        }
        private void Options() {

        }
        private void Quit() {
            Environment.Exit(0);
        }

        public TestUIScreen(SGame sgame) : base(sgame) {
            _container = Group(0, 0,
                Image(100, 20, 0, 0, "Title"),
                Focus(Label(200, 120, "New Game").WithOnSelect(NewGame)),
                Label(200, 135, "Load Game").WithOnSelect(LoadGame),
                Label(200, 150, "Options").WithOnSelect(Options),
                Label(200, 165, "Quit").WithOnSelect(Quit)
            );
        }
    }
}
