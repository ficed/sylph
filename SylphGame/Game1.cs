using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace SylphGame {
    public class Game1 : Game {
        private GraphicsDeviceManager _graphics;
        private SGame _sgame;

        public Game1() {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1280 * 2;
            _graphics.PreferredBackBufferHeight = 720 * 2;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

        }

        protected override void Initialize() {
            _sgame = new SGame(Environment.GetCommandLineArgs()[1], GraphicsDevice);
            Window.Title = _sgame.Config.Title;
//            _sgame.PushScreen(new TestScreen(_sgame));
            _sgame.PushScreen(new Splash(_sgame));
            base.Initialize();
        }

        protected override void LoadContent() {
           
        }

        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            _sgame.ActiveScreen.Step();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            _sgame.ActiveScreen.Render();
            base.Draw(gameTime);
        }
    }
}
