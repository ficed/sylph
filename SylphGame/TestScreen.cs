using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SylphGame.Field;
using SylphGame.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame {

    public class TestMap : Field.MapScreen {
        public TestMap(SGame sgame, string tilemap) : base(sgame, tilemap, "entry1") {

            var locke = new Field.SpriteObject(sgame, "Locke");
            DropToMap(locke, new IVector2(20, 13));
            locke.Sprite.PlayAnimation("FingerWag", true);
            _objects.Add(locke);

            Call(locke, Field.ScriptPriority.Idle, new WalkRandomlyBehaviour(4, 1, 30, 180));
        }
    }

    public class TestScreen : Screen {

        private Field.TileMap _map;
        private int _scrollX, _scrollY;

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

            _map = new Field.TileMap(sgame, "cave1");
        }

        protected override Matrix GetTransform() {
            return Matrix.CreateTranslation(_scrollX, _scrollY, 0) * base.GetTransform();
        }

        protected override void Render(SpriteBatch spriteBatch) {
            base.Render(spriteBatch);
            Layer depth = Layer.BACKGROUND_BACK;
            foreach(int layer in Enumerable.Range(0, _map.LayerCount)) {
                _map.RenderLayer(spriteBatch, layer, depth);
                depth = depth.Next;
                //Draw sprites
                depth = depth.Next;
                //Draw overlay
                depth = depth.Next;
            }
        }

        public override void Step() {
            base.Step();
            if (_sgame.Input.IsDown(InputButton.Left)) {
                _scrollX--;
            } else if (_sgame.Input.IsDown(InputButton.Right)) {
                _scrollX++;
            } else if (_sgame.Input.IsDown(InputButton.Up)) {
                _scrollY--;
            } else if (_sgame.Input.IsDown(InputButton.Down)) {
                _scrollY++;
            }

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
