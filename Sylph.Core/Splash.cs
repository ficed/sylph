using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame {
    public class Splash : Screen {

        private Texture2D _tex;
        private Func<Screen> _launch;

        public Splash(Func<Screen> launch) {
            _launch = launch;
        }

        public override void Init(SGame sgame) {
            base.Init(sgame);
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
            if (_launch != null) {
                _sgame.PushScreen<LoadScreen>();
                _sgame.PushScreen(_launch());
                _launch = null;
            }
        }
    }
}
