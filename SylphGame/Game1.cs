using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace SylphGame {
    public class Game1 : Game {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SGame _sgame;

        private Entities.Sprite.Instance _terra;

        public Game1() {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1280 * 2;
            _graphics.PreferredBackBufferHeight = 720 * 2;
            Window.Title = "Sylph";
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

        }

        protected override void Initialize() {
            _sgame = new SGame(Environment.GetCommandLineArgs()[1], GraphicsDevice);
            base.Initialize();
        }

        protected override void LoadContent() {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            var sprite = new Entities.Sprite(_sgame, "Terra");
            _terra = sprite.New();
            _terra.Position = new Vector2(64, 32);
            _terra.PlayAnimation("WalkS", true);
        }

        int _frame;

        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _frame++;

            if ((_frame % 15) == 0) {

                _terra.Step();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(
                transformMatrix: Matrix.CreateScale(8),
                samplerState: SamplerState.PointClamp
            );

            _terra.Render(_spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
