using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SylphGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylph.Core {
    public class SylphGame : Game {
        private GraphicsDeviceManager _graphics;
        private Func<GraphicsDevice, SGame> _launch;

        public SGame SGame { get; private set; }

        public SylphGame(Func<GraphicsDevice, SGame> launch) {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _launch = launch;
        }

        protected override void Initialize() {
            SGame = _launch(_graphics.GraphicsDevice);
            SGame.DPIScale = Math.Max(1, _graphics.GraphicsDevice.DisplayMode.Width / 1280);
            _graphics.PreferredBackBufferWidth = 1280 * SGame.DPIScale;
            _graphics.PreferredBackBufferHeight = 720 * SGame.DPIScale;
            _graphics.ApplyChanges();

            Window.Title = SGame.Config.Title;
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
            foreach (var key in Keyboard.GetState().GetPressedKeys()) {
                if (_inputMap.TryGetValue(key, out var button))
                    _inputDown[button] = true;
            }
            SGame.Input.Update(_inputDown);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            SGame.ActiveScreen.Step();
            SGame.Party.Frames++;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            SGame.ActiveScreen.Render();
            base.Draw(gameTime);
        }
    }
}
