using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame.UI {
    public abstract class UIScreen : Screen {

        protected Container _container;
        protected Component _focus;
        private Texture2D _cursor;
        private LoadedSfx _sfxMove, _sfxConfirm, _sfxCancel;

        public UIScreen(SGame sgame) : base(sgame) {
        }

        public override void Activated() {
            base.Activated();
            _container.Init(_sgame);
            _cursor = _sgame.LoadTex("UI", _sgame.Config.UIDefaults.CursorGraphic);
            _sfxMove = _sgame.Load<LoadedSfx>(_sgame.Config.UIDefaults.MoveSfx);
            _sfxCancel = _sgame.Load<LoadedSfx>(_sgame.Config.UIDefaults.CancelSfx);
            _sfxConfirm = _sgame.Load<LoadedSfx>(_sgame.Config.UIDefaults.ConfirmSfx);
        }

        public override void Step() {
            base.Step();
            if (_focus != null) {
                if (_sgame.Input.IsDownRepeat(InputButton.Left))
                    SwitchFocus(-1, 0);
                else if (_sgame.Input.IsDownRepeat(InputButton.Right))
                    SwitchFocus(1, 0);
                else if (_sgame.Input.IsDownRepeat(InputButton.Up))
                    SwitchFocus(0, -1);
                else if (_sgame.Input.IsDownRepeat(InputButton.Down))
                    SwitchFocus(0, 1);
                else if (_sgame.Input.IsJustDown(InputButton.OK)) {
                    _focus.OnSelect?.Invoke();
                    _sfxConfirm.Play();
                }
            }
        }

        protected void SwitchFocus(int dx, int dy) {
            var candidates = _focus.Owner.Children
                .Where(c => c.OnSelect != null)
                .Where(c => c != _focus)
                .Select(c => new { Component = c, X = c.X.Value - _focus.X.Value, Y = c.Y.Value - _focus.Y.Value });

            if (dx != 0)
                candidates = candidates.Where(c => Math.Sign(c.X) == dx)
                    .OrderBy(c => Math.Abs(c.Y))
                    .ThenBy(c => Math.Abs(c.X));
            if (dy != 0)
                candidates = candidates.Where(c => Math.Sign(c.Y) == dy)
                    .OrderBy(c => Math.Abs(c.X))
                    .ThenBy(c => Math.Abs(c.Y));

            if (candidates.Any()) {
                _focus = candidates.First().Component;
                _sfxMove.Play();
            }
        }

        protected override void Render(SpriteBatch spriteBatch) {
            base.Render(spriteBatch);
            _container.Render(spriteBatch, Layer.UI_BACK);
            if (_focus != null) {
                spriteBatch.Draw(
                    _cursor, new Vector2(_focus.X.Value - _cursor.Width - 1, _focus.Y.Value + 2), Color.White
                );
            }
        }

        protected T Ref<T>(out T tRef, T value) {
            tRef = value;
            return value;
        }

        protected T Focus<T>(T component) where T : Component {
            _focus = component;
            return component;
        }

        protected Label Label(Prop<int> x, Prop<int> y, Prop<string> text) {
            return new Label {
                X = x,
                Y = y,
                Text = text,
            };
        }

        protected Image Image(Prop<int> x, Prop<int> y, Prop<int> w, Prop<int> h, Prop<string> source) {
            return new Image {
                X = x,
                Y = y,
                W = w,
                H = h,
                Source = source
            };
        }

        protected Group Group(Prop<int> x, Prop<int> y, params Component[] components) {
            var g = new Group {
                X = x,
                Y = y,
            };
            g.AddRange(components);
            return g;
        }

        protected Box Box(Prop<int> x, Prop<int> y, Prop<int> w, Prop<int> h, params Component[] components) {
            var b = new Box {
                X = x,
                Y = y,
                W = w, 
                H = h,
            };
            b.AddRange(components);
            return b;
        }
    }
}
