using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Sylph.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SylphGame.UI {

    public struct Prop<T> {
        private Func<T> _get;
        private T _static;

        public T Value {
            get {
                if (_get != null)
                    return _get();
                else
                    return _static;
            }
        }

        public static implicit operator Prop<T>(T value) {
            return new Prop<T> { _static = value };
        }
        public static implicit operator Prop<T>(Func<T> get) {
            return new Prop<T> { _get = get };
        }
    }

    public interface ICustomFocus {
        (int X, int Y) FocusLocation { get; }
        void Focussed();
        void LostFocus();
    }

    public abstract class Component {
        public Prop<int> X { get; set; }
        public Prop<int> Y { get; set; }
        public Prop<bool> Visible { get; set; } = true;
        public Action OnSelect { get; set; }
        public Container Owner { get; set; }

        public abstract void Render(SpriteBatch spriteBatch, Layer layer, int xOffset, int yOffset);
        public virtual void Init(SGame sgame) { }

        public IEnumerable<Component> Ancestors {
            get {
                Container o = Owner;
                while (o != null) {
                    yield return o;
                    o = o.Owner;
                }
            }
        }
    }

    [Flags]
    public enum InputProcessResults {
        None = 0,
        PlayMoveSfx = 0x1,
        PlayConfirmSfx = 0x2,
        PlayCancelSfx = 0x4,
        StopProcessing = 0x8,
    }

    public interface IInputComponent {
        InputProcessResults Process(InputState input);
    }

    public static class ComponentUtil {
        public static T WithVisible<T>(this T t, Prop<bool> visible) where T : Component {
            t.Visible = visible;
            return t;
        }
        public static T WithOnSelect<T>(this T t, Action onSelect) where T : Component {
            t.OnSelect = onSelect;
            return t;
        }

        public static T AddChildren<T>(this T t, IEnumerable<Component> components) where T : Container {
            t.AddRange(components);
            return t;
        }
    }

    public abstract class SizedComponent : Component {
        public Prop<int> W { get; set; }
        public Prop<int> H { get; set; }
    }

    public abstract class Container : SizedComponent {
        private List<Component> _children = new();
        public IEnumerable<Component> Children => _children.AsReadOnly();

        public void Add(Component child) {
            child.Owner = this;
            _children.Add(child);
        }
        public void AddRange(IEnumerable<Component> children) {
            foreach (var child in children)
                Add(child);
        }

        public override void Render(SpriteBatch spriteBatch, Layer layer, int xOffset, int yOffset) {
            var childLayer = layer.Next;
            foreach (var child in Children.Where(c => c.Visible.Value))
                child.Render(spriteBatch, childLayer, xOffset + X.Value, yOffset + Y.Value);
        }

        public override void Init(SGame sgame) {
            base.Init(sgame);
            foreach(var child in Children)
                child.Init(sgame);
        }

    }

    public class Group : Container {

    }

    public class Box : Container {
        private Boxes.Instance _box;

        public override void Init(SGame sgame) {
            base.Init(sgame);
            _box = sgame.Boxes.New();
        }
        public override void Render(SpriteBatch spriteBatch, Layer layer, int xOffset, int yOffset) {
            base.Render(spriteBatch, layer, xOffset, yOffset);
            _box.Layer = layer;
            _box.Location = new Rectangle(X.Value, Y.Value, W.Value, H.Value);
            _box.Render(spriteBatch);
        }
    }

    public enum TextAlign {
        Left,
        Center,
        Right,
    }

    public class Label : Component {
        public Prop<string> Text { get; set; }
        public Prop<DynamicSpriteFont> Font { get; set; }
        public Prop<Color> Color { get; set; } = Microsoft.Xna.Framework.Color.White;
        public Prop<TextAlign> Alignment { get; set; }

        private float _scale;
        private SGame _sgame;
        
        public override void Init(SGame sgame) {
            base.Init(sgame);
            _sgame = sgame;
            _scale = 1f / sgame.Config.Scale;
        }

        public override void Render(SpriteBatch spriteBatch, Layer layer, int xOffset, int yOffset) {
            int x = xOffset + X.Value;            
            (Font.Value ?? _sgame.DefaultFont).DrawText(
                spriteBatch, Text.Value, new Vector2(x, yOffset + Y.Value), Color.Value,
                layerDepth: layer,
                scale: new Vector2(_scale),
                textAlign: Alignment.Value
            );
        }
    }

    public class Image : SizedComponent {
        public Prop<string> Source { get; set; }

        private Texture2D _tex;
        private Rectangle _source;

        public override void Init(SGame sgame) {
            base.Init(sgame);
            var atlases = sgame.Load<LoadedAtlases>(string.Empty);
            (_tex, _source) = atlases.GetOrLoad(Source.Value);
            if (W.Value == 0)
                W = _source.Width;
            if (H.Value == 0)
                H = _source.Height;
        }

        public override void Render(SpriteBatch spriteBatch, Layer layer, int xOffset, int yOffset) {
            spriteBatch.Draw(
                _tex, new Rectangle(X.Value + xOffset, Y.Value + yOffset, W.Value, H.Value), 
                _source, Color.White, 0, Vector2.Zero, SpriteEffects.None, layer
            );
        }
    }

    public class Gauge : SizedComponent {
        public Prop<int> Value { get; set; }
        public Prop<int> Max { get; set; }

        public override void Render(SpriteBatch spriteBatch, Layer layer, int xOffset, int yOffset) {
            int fillW = (W.Value - 1) * Value.Value / Max.Value;
            spriteBatch.FillRectangle(
                new RectangleF(X.Value + xOffset, Y.Value + yOffset, fillW, H.Value - 1),
                Color.LightBlue, //TODO!!!!
                layer
            );
            spriteBatch.FillRectangle(
                new RectangleF(X.Value + fillW + xOffset, Y.Value + yOffset, W.Value - 1 - fillW, H.Value - 1),
                Color.DarkBlue, //TODO!!!!
                layer
            );
            spriteBatch.FillRectangle(
                new RectangleF(X.Value + fillW + xOffset, Y.Value + yOffset, W.Value - 1 - fillW, H.Value - 1),
                Color.DarkBlue, //TODO!!!!
                layer
            );
            spriteBatch.FillRectangle(
                new RectangleF(X.Value + xOffset + 1, Y.Value + yOffset + H.Value - 1, W.Value - 1, 1),
                Color.Black.WithAlpha(0.5f),
                layer
            );
            spriteBatch.FillRectangle(
                new RectangleF(X.Value + xOffset + W.Value - 1, Y.Value + yOffset + 1, 1, H.Value - 1),
                Color.Black.WithAlpha(0.5f),
                layer
            );
        }
    }

    public class ListBox<T> : SizedComponent, IInputComponent, ICustomFocus {

        private const int ITEM_HEIGHT = 15; //TODO??

        public Prop<DynamicSpriteFont> TextFont { get; set; }
        public Prop<DynamicSpriteFont> AnnotateFont { get; set; }

        private int _top, _sel;
        private IEnumerable<T> _source;
        private Action<T> _onFocus, _onSelect;
        private Func<T, string> _name, _annotation;
        private float _scale;

        public (int X, int Y) FocusLocation => (0, (_sel - _top) * ITEM_HEIGHT + ITEM_HEIGHT / 2);

        public ListBox(IEnumerable<T> source, Action<T> onFocus, Action<T> onSelect, Func<T, string> getName, Func<T, string> getAnnotation) {
            _source = source;
            _onFocus = onFocus;
            _onSelect = onSelect;
            _name = getName;    
            _annotation = getAnnotation;
        }

        public override void Init(SGame sgame) {
            base.Init(sgame);
            _scale = 1f / sgame.Config.Scale;
        }

        public InputProcessResults Process(InputState input) {
            if (input.IsJustDown(InputButton.Cancel)) {
                return InputProcessResults.None;
            }

            if (!_source.Any()) return InputProcessResults.StopProcessing;

            if (input.IsDownRepeat(InputButton.Up)) {
                if (_sel > 0) {
                    _sel--;
                    _onFocus?.Invoke(_source.ElementAt(_sel));
                    return InputProcessResults.PlayMoveSfx | InputProcessResults.StopProcessing;
                }
            } else if (input.IsDownRepeat(InputButton.Down)) {
                if (_sel < (_source.Count() - 1)) {
                    _sel++;
                    _onFocus?.Invoke(_source.ElementAt(_sel));
                    return InputProcessResults.PlayMoveSfx | InputProcessResults.StopProcessing;
                }
            } else if (input.IsJustDown(InputButton.OK)) {
                _onSelect?.Invoke(_source.ElementAt(_sel));
                return InputProcessResults.PlayConfirmSfx | InputProcessResults.StopProcessing;
            }

            return InputProcessResults.StopProcessing;
        }

        public override void Render(SpriteBatch spriteBatch, Layer layer, int xOffset, int yOffset) {
            if (_sel < _top) _top = _sel;

            int num = H.Value / ITEM_HEIGHT;

            if (_sel > (_top + num - 1))
                _top = _sel - num + 1;

            int y = 0;
            foreach(T item in _source.Skip(_top).Take(num)) {
                TextFont.Value.DrawText(
                    spriteBatch, _name(item), new Vector2(xOffset + X.Value, yOffset + Y.Value + y), Color.White,
                    layerDepth: layer,
                    scale: new Vector2(_scale)
                );
                if (_annotation != null) {
                    AnnotateFont.Value.DrawText(
                        spriteBatch, _annotation(item), new Vector2(xOffset + X.Value + W.Value, yOffset + Y.Value + y), Color.White,
                        textAlign: TextAlign.Right,
                        layerDepth: layer,
                        scale: new Vector2(_scale)
                    );
                }
                y += ITEM_HEIGHT;
            }
        }

        public void Focussed() {
            if (_source.Any())
                _onFocus?.Invoke(_source.ElementAt(_sel));
        }

        public void LostFocus() {
            _onFocus?.Invoke(default(T));
        }
    }
}
