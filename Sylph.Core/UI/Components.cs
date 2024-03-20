using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Sylph.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public abstract class Component {
        public Prop<int> X { get; set; }
        public Prop<int> Y { get; set; }
        public Prop<bool> Visible { get; set; } = true;
        public Action OnSelect { get; set; }
        public Container Owner { get; set; }

        public abstract void Render(SpriteBatch spriteBatch, Layer layer, int xOffset, int yOffset);
        public virtual void Init(SGame sgame) { }
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
        public Prop<string> Font { get; set; }
        public Prop<Color> Color { get; set; } = Microsoft.Xna.Framework.Color.White;
        public Prop<TextAlign> Alignment { get; set; }

        private DynamicSpriteFont _font;
        private float _scale;

        public override void Init(SGame sgame) {
            base.Init(sgame);
            if (!string.IsNullOrEmpty(Font.Value))
                _font = sgame.GetFont(Font.Value);
            else
                _font = sgame.DefaultFont;
            _scale = 1f / sgame.Config.Scale;
        }

        public override void Render(SpriteBatch spriteBatch, Layer layer, int xOffset, int yOffset) {
            int x = xOffset + X.Value;

            if (Alignment.Value != TextAlign.Left) {
                float width = _font.MeasureString(Text.Value, new Vector2(_scale)).X;
                switch (Alignment.Value) {
                    case TextAlign.Center:
                        x -= (int)(width * 0.5f);
                        break;
                    case TextAlign.Right:
                        x -= (int)width;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            _font.DrawText(
                spriteBatch, Text.Value, new Vector2(x, yOffset + Y.Value), Color.Value,
                layerDepth: layer, 
                scale: new Vector2(_scale)
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

}
