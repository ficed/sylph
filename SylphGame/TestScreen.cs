using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SylphGame.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame {

    public class TestMap : Field.Map {
        public TestMap(SGame sgame, string tilemap) : base(sgame, tilemap) {
            var sprite = new Field.SpriteObject(sgame, "Terra");
            sprite.X = 18;
            sprite.Y = 10;
            sprite.PlayAnimation("WalkS", true);
            _objects.Add(sprite);
        }
    }

    public class TestScreen : Screen {

        private TestMap _map;
        private int _scrollX, _scrollY;

        public TestScreen(SGame sgame) : base(sgame) {
            _map = new TestMap(sgame, "Cave1");
        }

        protected override IEnumerable<IEntity> GetActiveEntities() {
            return _map.Entities;
        }

        protected override Matrix GetTransform() {
            return Matrix.CreateTranslation(_scrollX, _scrollY, 0) * base.GetTransform();
        }

        protected override void Render(SpriteBatch spriteBatch) {
            //base.Render(spriteBatch);
            _map.Render(spriteBatch);
        }

        public override void Step() {
            base.Step();
            _map.Step();
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
