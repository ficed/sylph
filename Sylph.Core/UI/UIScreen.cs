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

        protected static Prop<T> Dyn<T>(Func<T> getValue) => getValue;

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
            _container.Render(spriteBatch, Layer.UI_BACK, 0, 0);
            if (_focus != null) {
                var pos = RenderPos(_focus);
                spriteBatch.Draw(
                    _cursor, new Vector2(pos.X - _cursor.Width - 1, pos.Y + 2), null, Color.White,
                    0, Vector2.Zero, 1f, SpriteEffects.None, Layer.UI_FRONT
                );
            }
        }

        protected Vector2 RenderPos(Component c) {
            float x = 0, y = 0;
            while (c != null) {
                x += c.X.Value;
                y += c.Y.Value;
                c = c.Owner;
            }
            return new Vector2(x, y);
        }

        protected T Ref<T>(out T tRef, T value) {
            tRef = value;
            return value;
        }

        protected T Focus<T>(T component) where T : Component {
            if (component.OnSelect != null) {
                _focus = component;
            } else if (component is Container c) {
                _focus = c.Children.First(child => child.OnSelect != null);
            }
            return component;
        }

        protected Label Label(Prop<int> x, Prop<int> y, Prop<string> text, 
            Prop<TextAlign>? alignment = null, Prop<Color>? color = null, Prop<string>? font = null) {
            return new Label {
                X = x,
                Y = y,
                Text = text,
                Alignment = alignment ?? TextAlign.Left,
                Color = color ?? Color.White,
                Font = font ?? (string)null,
            };
        }

        protected Gauge Gauge(Prop<int> x, Prop<int> y, Prop<int> w, Prop<int> h, Prop<int> value, Prop<int> max) {
            return new Gauge {
                X = x,
                Y = y,
                W = w,
                H = h,
                Value = value,
                Max = max
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

        protected Group FullGauge(Prop<int> x, Prop<int> y, Prop<string> name, 
            Func<int> current, Func<int> max, string numFont) {
            return Group(
                x, y,
                Label(5, 5, name, color: Color.Aqua),
                Label(50, 5, (Func<string>)(() => current().ToString()), alignment: TextAlign.Right, font: numFont),
                Label(55, 5, "/"),
                Label(90, 5, (Func<string>)(() => max().ToString()), alignment: TextAlign.Right, font: numFont),
                Gauge(5, 20, 90, 3, current, max)
            );
        }


        protected Group Menu(Prop<int> x, Prop<int> y,
            Prop<int> w, Prop<int> h,
            int offsetX, int offsetY,
            params (string caption, Func<bool> enabled, Action onSelect)[] items) {
            return Group(x, y,
                items.Select((i, index) => new Label {
                    X = (Func<int>)(() => offsetX * index),
                    Y = (Func<int>)(() => offsetY * index),
                    Text = i.caption,
                    Color = (Func<Color>)(() => i.enabled() ? Color.White : Color.Gray),
                    OnSelect = () => {
                        if (i.enabled())
                            i.onSelect();
                        else
                            _sfxCancel.Play();
                    }
                })
                .ToArray()
            );
        }

        protected Group MenuV(Prop<int> x, Prop<int> y, 
            Prop<int> w, Prop<int> h,
            params (string caption, Func<bool> enabled, Action onSelect)[] items) {
            return Menu(x, y, w, h, 0, h.Value / items.Length, items);
        }
        protected Group MenuH(Prop<int> x, Prop<int> y,
            Prop<int> w, Prop<int> h,
            params (string caption, Func<bool> enabled, Action onSelect)[] items) {
            return Menu(x, y, w, h, w.Value / items.Length, 0, items);
        }
    }
}
