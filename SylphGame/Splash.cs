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
            spriteBatch.Draw(_tex, new Rectangle(0, 0, 426, 240), Color.White);
        }
    }
}
