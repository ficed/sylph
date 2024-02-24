using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace SylphGame {
    public class Game1 : Game {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SGame _sgame;

        private List<IEntity> _entities = new();

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
            base.Initialize();
        }

        protected override void LoadContent() {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            var sprite = new Entities.Sprite(_sgame, "Terra");
            var terra = sprite.New();
            terra.Position = new Vector2(64, 32);
            terra.PlayAnimation("WalkS", true);
            _entities.Add(terra);

            var box = _sgame.Boxes.New();
            box.Location = new Rectangle(200, 180, 96, 48);
            box.Expand(180);
            _entities.Add(box);
        }

        int _frame;

        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _frame++;

            foreach (var entity in _entities)
                if ((_frame % entity.StepFrames) == 0)
                    entity.Step();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(
                transformMatrix: Matrix.CreateScale(_sgame.Config.Scale),
                samplerState: SamplerState.PointClamp
            );

            foreach (var entity in _entities)
                entity.Render(_spriteBatch);

            _spriteBatch.DrawString(
                _sgame.Fonts.GetFont(16 * _sgame.Config.Scale), "Welcome to Sylph!", new Vector2(32, 128), Color.White,
                effect: FontSystemEffect.None,
                scale: new Vector2(1f / _sgame.Config.Scale)
            );

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
