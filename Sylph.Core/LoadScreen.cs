using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame {
    public class LoadScreen : Screen {
        private Texture2D _tex;

        public override void Init(SGame sgame) {
            base.Init(sgame);
            _tex = sgame.LoadTex("UI", sgame.Config.LoadGraphic);
        }

        protected override void Render(SpriteBatch spriteBatch) {
            base.Render(spriteBatch);
            spriteBatch.Draw(
                _tex,
                new Rectangle(0, 0, (int)(_tex.Width / _sgame.Config.Scale), (int)(_tex.Height / _sgame.Config.Scale)),
                Color.White
            );
        }

        public override void Step() {
            base.Step();
        }
    }
}
