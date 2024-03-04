using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
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

        public abstract void Render(SpriteBatch spriteBatch, Layer layer);
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

        public override void Render(SpriteBatch spriteBatch, Layer layer) {
            var childLayer = layer.Next;
            foreach (var child in Children.Where(c => c.Visible.Value))
                child.Render(spriteBatch, childLayer);
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
        public override void Render(SpriteBatch spriteBatch, Layer layer) {
            base.Render(spriteBatch, layer);
            _box.Layer = layer;
            _box.Location = new Rectangle(X.Value, Y.Value, W.Value, H.Value);
            _box.Render(spriteBatch);
        }
    }

    public class Label : Component {
        public Prop<string> Text { get; set; }
        public Prop<uint> Color { get; set; } = 0xffffffff;

        private DynamicSpriteFont _font;
        private float _scale;

        public override void Init(SGame sgame) {
            base.Init(sgame);
            _font = sgame.DefaultFont;
            _scale = 1f / sgame.Config.Scale;
        }

        public override void Render(SpriteBatch spriteBatch, Layer layer) {
            _font.DrawText(
                spriteBatch, Text.Value, new Vector2(X.Value, Y.Value), new Color(Color.Value),
                layerDepth: layer,
                scale: new Vector2(_scale)
            );
        }
    }

    public class Image : SizedComponent {
        public Prop<string> Source { get; set; }

        private Texture2D _tex;

        public override void Init(SGame sgame) {
            base.Init(sgame);
            _tex = sgame.LoadTex("UI", Source.Value);
            if (W.Value == 0)
                W = _tex.Width;
            if (H.Value == 0)
                H = _tex.Height;
        }

        public override void Render(SpriteBatch spriteBatch, Layer layer) {
            spriteBatch.Draw(
                _tex, new Rectangle(X.Value, Y.Value, W.Value, H.Value), 
                null, Color.White, 0, Vector2.Zero, SpriteEffects.None, layer
            );
        }
    }

    public class Gauge : SizedComponent {
        public Prop<int> Value { get; set; }
        public Prop<int> Max { get; set; }

        public override void Render(SpriteBatch spriteBatch, Layer layer) {
            int fillW = W.Value * Value.Value / Max.Value;
            spriteBatch.FillRectangle(
                new RectangleF(X.Value, Y.Value, fillW, H.Value),
                Color.LightBlue, //TODO!!!!
                layer
            );
            spriteBatch.FillRectangle(
                new RectangleF(X.Value + fillW, Y.Value, W.Value - fillW, H.Value),
                Color.DarkBlue, //TODO!!!!
                layer
            );
        }
    }

}
