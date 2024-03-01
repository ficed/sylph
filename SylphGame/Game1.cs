using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SylphGame {
    public class Game1 : Game {
        private GraphicsDeviceManager _graphics;
        private SGame _sgame;

        public Game1() {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

        }

        protected override void Initialize() {
            _sgame = new SGame(Environment.GetCommandLineArgs().Skip(1), GraphicsDevice);
            _sgame.DPIScale = Math.Max(1, _graphics.GraphicsDevice.DisplayMode.Width / 1280);
            _graphics.PreferredBackBufferWidth = 1280 * _sgame.DPIScale;
            _graphics.PreferredBackBufferHeight = 720 * _sgame.DPIScale;
            _graphics.ApplyChanges();

            Window.Title = _sgame.Config.Title;
            //_sgame.PushScreen(new TestScreen(_sgame));
            _sgame.PushScreen(new TestMap(_sgame, "Cave1"));
            //            _sgame.PushScreen(new Splash(_sgame));
            //_sgame.PushScreen(new TestUIScreen(_sgame));
            base.Initialize();
        }

        protected override void LoadContent() {
           
        }

        private Dictionary<Keys, InputButton> _inputMap = new Dictionary<Keys, InputButton> {
            [Keys.Left] = InputButton.Left,
            [Keys.Right] = InputButton.Right,
            [Keys.Up] = InputButton.Up,
            [Keys.Down] = InputButton.Down,
            [Keys.Enter] = InputButton.OK,
            [Keys.Space] = InputButton.Cancel,
            [Keys.M] = InputButton.Menu,
            [Keys.F1] = InputButton.Start,
            [Keys.F2] = InputButton.Select,
        };
        private Dictionary<InputButton, bool> _inputDown = Enum.GetValues<InputButton>()
            .Cast<InputButton>()
            .ToDictionary(b => b, _ => false);

        protected override void Update(GameTime gameTime) {
            foreach (var button in _inputMap.Values)
                _inputDown[button] = false;
            foreach(var key in Keyboard.GetState().GetPressedKeys()) {
                if (_inputMap.TryGetValue(key, out var button))
                    _inputDown[button] = true;
            }
            _sgame.Input.Update(_inputDown);

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
