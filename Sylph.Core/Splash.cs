using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame {
    public class Splash : Screen {

        private Texture2D _tex;

        public Splash(SGame sgame) : base(sgame) {
            _tex = sgame.LoadTex("UI", sgame.Config.SplashGraphic);
            _entities.Add(FadeEffect.In(60));
        }

        protected override void Render(SpriteBatch spriteBatch) {
            base.Render(spriteBatch);
            spriteBatch.Draw(
                _tex, 
                new Rectangle(0, 0, (int)(1280 / _sgame.Config.Scale), (int)(240 / _sgame.Config.Scale)), 
                Color.White
            );
        }

        public override void Step() {
            base.Step();
            _sgame.PushScreen(new LoadScreen(_sgame,
                () => _sgame.PushScreen(new TestMap(_sgame, "Cave1"))
            ));
        }
    }
}
