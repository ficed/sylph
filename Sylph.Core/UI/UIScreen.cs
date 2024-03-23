using FontStashSharp;
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
        protected Component _popup;
        private Stack<Component> _focus = new();
        private Texture2D _cursor;
        private LoadedSfx _sfxMove, _sfxConfirm, _sfxCancel;
        protected bool _inputEnabled = true;

        protected static Prop<T> Dyn<T>(Func<T> getValue) => getValue;

        public Component Focused {
            get {
                _focus.TryPeek(out var f);
                return f;
            }
        }

        public override void Activated() {
            base.Activated();
            _container.Init(_sgame);
            _cursor = _sgame.LoadTex("UI", _sgame.Config.UIDefaults.CursorGraphic);
            _sfxMove = _sgame.Load<LoadedSfx>(_sgame.Config.UIDefaults.MoveSfx);
            _sfxCancel = _sgame.Load<LoadedSfx>(_sgame.Config.UIDefaults.CancelSfx);
            _sfxConfirm = _sgame.Load<LoadedSfx>(_sgame.Config.UIDefaults.ConfirmSfx);
        }

        protected virtual void DoCancel() {
            if (_focus.Any()) {
                PopFocus();
                _sfxCancel.Play();
                if (!_focus.Any()) {
                    _inputEnabled = false;
                    _entities.Add(FadeEffect.Out(30, () => {
                        _sgame.PopScreen(this);
                    }));
                }
            }
        }

        protected void FadeOutAndSwitchTo<T>() where T : UIScreen, new() {
            _inputEnabled = false;
            _entities.Add(FadeEffect.Out(30, () => {
                _sgame.PushScreen<T>();
                _inputEnabled = true;
            }));
        }

        public override void Step() {
            base.Step();
            if (_focus.TryPeek(out var focus) && _inputEnabled) {
                if (focus is IInputComponent ic) {
                    var results = ic.Process(_sgame.Input);
                    if (results.HasFlag(InputProcessResults.PlayMoveSfx))
                        _sfxMove.Play();
                    if (results.HasFlag(InputProcessResults.PlayConfirmSfx))
                        _sfxConfirm.Play();
                    if (results.HasFlag(InputProcessResults.PlayCancelSfx))
                        _sfxCancel.Play();
                    if (results.HasFlag(InputProcessResults.StopProcessing))
                        return;
                }

                if (_sgame.Input.IsDownRepeat(InputButton.Left))
                    SwitchFocus(-1, 0);
                else if (_sgame.Input.IsDownRepeat(InputButton.Right))
                    SwitchFocus(1, 0);
                else if (_sgame.Input.IsDownRepeat(InputButton.Up))
                    SwitchFocus(0, -1);
                else if (_sgame.Input.IsDownRepeat(InputButton.Down))
                    SwitchFocus(0, 1);
                else if (_sgame.Input.IsJustDown(InputButton.OK)) {
                    focus.OnSelect?.Invoke();
                    _sfxConfirm.Play();
                }

                if (_sgame.Input.IsJustDown(InputButton.Cancel))
                    DoCancel();
            }
        }

        protected void SwitchFocus(int dx, int dy) {
            var focus = _focus.Peek();
            var candidates = focus.Owner.Children
                .Where(c => c.OnSelect != null)
                .Where(c => c != focus)
                .Select(c => new { Component = c, X = c.X.Value - focus.X.Value, Y = c.Y.Value - focus.Y.Value });

            if (dx != 0)
                candidates = candidates.Where(c => Math.Sign(c.X) == dx)
                    .OrderBy(c => Math.Abs(c.Y))
                    .ThenBy(c => Math.Abs(c.X));
            if (dy != 0)
                candidates = candidates.Where(c => Math.Sign(c.Y) == dy)
                    .OrderBy(c => Math.Abs(c.X))
                    .ThenBy(c => Math.Abs(c.Y));

            if (candidates.Any()) {
                _focus.Pop();
                _focus.Push(candidates.First().Component);
                _sfxMove.Play();
            }
        }

        protected override void Render(SpriteBatch spriteBatch) {
            base.Render(spriteBatch);
            _container.Render(spriteBatch, Layer.UI_BACK, 0, 0);
            _popup?.Render(spriteBatch, Layer.UI_FRONT, 0, 0);
            if (_focus.TryPeek(out var focus)) {
                var pos = RenderPos(focus);
                if (focus is ICustomFocus loc) {
                    var offset = loc.FocusLocation;
                    pos.X += offset.X;
                    pos.Y += offset.Y;
                } else {
                    pos.X -= 1;
                    pos.Y += 2;
                }
                spriteBatch.Draw(
                    _cursor, new Vector2(pos.X - _cursor.Width, pos.Y), null, Color.White,
                    0, Vector2.Zero, 1f, SpriteEffects.None, Layer.UI_DECORATIONS
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

            bool CanFocus(Component c) => (c.OnSelect != null) || (c is IInputComponent);

            Component FindFocusable(Component c) {
                if (CanFocus(c))
                    return c;
                else if (c is Container container) {
                    return container.Children
                        .Select(child => FindFocusable(child))
                        .FirstOrDefault();
                } else
                    return null;
            }

            var focus = FindFocusable(component);
            if (focus != null) 
                _focus.Push(focus);

            DoFocusChanged();
            return component;
        }
        protected void PopFocus() {
            var old = _focus.Pop();
            if (old is ICustomFocus cf)
                cf.LostFocus();
            DoFocusChanged();
        }

        private void DoFocusChanged() {
            if (_container != null) {
                if (_focus.TryPeek(out var focus)) {
                    if (focus is ICustomFocus cf)
                        cf.Focussed();
                    if (_popup != null) {
                        if (!focus.Ancestors.Contains(_popup)) {
                            _popup.Visible = false;
                            _popup = null;
                        }
                    }
                }
                FocusChanged(); //Try to avoid firing events until init is complete
            }
        }
        protected virtual void FocusChanged() { }

        protected T Popup<T>(T t) where T : Component {
            _popup = t;
            t.Visible = true;
            Focus(t);
            return t;
        }

        protected Label Label(Prop<int> x, Prop<int> y, Prop<string> text, 
            Prop<TextAlign>? alignment = null, Prop<Color>? color = null, Prop<DynamicSpriteFont>? font = null) {
            return new Label {
                X = x,
                Y = y,
                Text = text,
                Alignment = alignment ?? TextAlign.Left,
                Color = color ?? Color.White,
                Font = font ?? _sgame.DefaultFont,
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

        protected ListBox<T> ListBox<T>(Prop<int> x, Prop<int> y, Prop<int> w, Prop<int> h, 
            Prop<DynamicSpriteFont> textFont, Prop<DynamicSpriteFont> annotateFont,
            IEnumerable<T> source, Action<T> onFocus, Action<T> onSelect, Func<T, string> getName, Func<T, string> getAnnotation) {
            return new ListBox<T>(source, onFocus, onSelect, getName, getAnnotation) {
                X = x,
                Y = y,
                W = w,
                H = h,
                TextFont = textFont,
                AnnotateFont = annotateFont,
            };
        }

        protected Group FullGauge(Prop<int> x, Prop<int> y, Prop<string> name, 
            Func<int> current, Func<int> max) {
            return Group(
                x, y,
                Label(5, 5, name, color: Color.Aqua),
                Label(50, 5, (Func<string>)(() => current().ToString()), alignment: TextAlign.Right, font: _sgame.NumericFont),
                Label(55, 5, "/"),
                Label(90, 5, (Func<string>)(() => max().ToString()), alignment: TextAlign.Right, font: _sgame.NumericFont),
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
