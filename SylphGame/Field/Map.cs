using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame.Field {
    
    public class MoveState {
        public int TargetX { get; set; }
        public int TargetY { get; set; }
        public float Progress { get; set; }
        public int Steps { get; set; }
        public Action OnComplete { get; set; }
    }

    [Flags]
    public enum ObjectFlags {
        None = 0,
        Visible = 0x1,
        Solid = 0x2,
        Interactable = 0x4,

        DEFAULT = Visible | Solid | Interactable,
    }

    public abstract class MapObject {
        public string ID { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public ObjectFlags Flags { get; set; } = ObjectFlags.DEFAULT;
        public MoveState MoveState { get; set; }
        public int Layer { get; set; }

        public abstract void Render(Vector2 renderPos, SpriteBatch spriteBatch, Layer layer);
        public virtual IEntity Entity => null;
    }

    public class SpriteObject : MapObject {

        private Entities.Sprite.Instance _sprite;

        public override IEntity Entity => _sprite;

        public SpriteObject(SGame sgame, string sprite) {
            _sprite = sgame.Load<Entities.Sprite>(sprite).New();
        }

        public override void Render(Vector2 renderPos, SpriteBatch spriteBatch, Layer layer) {
            _sprite.Position = renderPos;
            _sprite.Layer = layer;
            _sprite.Render(spriteBatch);
        }

        public void Step() {
        }

        public void PlayAnimation(string which, bool loop) {
            _sprite.PlayAnimation(which, loop);
        }
    }



    public class Map {

        protected TileMap _tilemap;
        protected List<MapObject> _objects = new();

        public IEnumerable<IEntity> Entities => _objects.Select(o => o.Entity).Where(e => e != null);

        public Map(SGame sgame, string tilemap) {
            _tilemap = new TileMap(sgame, tilemap);
        }

        public void Render(SpriteBatch spriteBatch) {
            Layer L = Layer.BACKGROUND_BACK;
            foreach(int layer in Enumerable.Range(0, _tilemap.LayerCount)) {
                _tilemap.RenderLayer(spriteBatch, layer, L);
                L = L.Next;
                foreach(var obj in _objects.Where(o => o.Layer == layer)) {
                    var pos = new Vector2(obj.X * _tilemap.TileWidth, obj.Y * _tilemap.TileHeight);
                    if (obj.MoveState != null) {
                        pos.X += _tilemap.TileWidth * (obj.MoveState.TargetX - obj.X) * obj.MoveState.Progress;
                        pos.Y += _tilemap.TileHeight * (obj.MoveState.TargetY - obj.Y) * obj.MoveState.Progress;
                    }
                    obj.Render(pos, spriteBatch, L);
                }
                L = L.Next;
                L = L.Next;
            }
        }

        public void Step() {
            foreach(var obj in _objects) {
                if (obj.MoveState != null) {
                    int largest = Math.Max(
                        (obj.MoveState.TargetX - obj.X) * _tilemap.TileWidth,
                        (obj.MoveState.TargetY - obj.Y) * _tilemap.TileHeight
                    );
                    if (obj.MoveState.Steps == largest) {
                        var ms = obj.MoveState;
                        obj.MoveState = null;
                        ms.OnComplete?.Invoke();
                    } else {
                        obj.MoveState.Steps++;
                        obj.MoveState.Progress = 1f * obj.MoveState.Steps / largest;
                    }
                }
            }

        }
    }
}
