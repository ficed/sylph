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

    public enum Facing { N, S, E, W };

    public class SpriteObject : MapObject {

        public Entities.Sprite.Instance Sprite { get; private set; }
        public Facing Facing { get; set; }

        public override IEntity Entity => Sprite;

        public SpriteObject(SGame sgame, string sprite) {
            Sprite = sgame.Load<Entities.Sprite>(sprite).New();
        }

        public override void Render(Vector2 renderPos, SpriteBatch spriteBatch, Layer layer) {
            Sprite.Position = renderPos;
            Sprite.Layer = layer;
            Sprite.Render(spriteBatch);
        }

        public void Step() {
        }

        public void Walk(Vector2 direction) {
            string anim;
            switch (Math.Sign(direction.X)) {
                case -1:
                    Facing = Facing.W; break;
                case +1:
                    Facing = Facing.E; break;
                default:
                    if (Math.Sign(direction.Y) == -1)
                        Facing = Facing.N;
                    else
                        Facing = Facing.S;
                    break;
            }
            Sprite.PlayAnimation($"Walk{Facing}", true);
            int newX = X + (int)direction.X,
                newY = Y + (int)direction.Y;
            MoveState = new MoveState {
                TargetX = newX,
                TargetY = newY,
                OnComplete = () => {
                    X = newX; Y = newY;
                }
            };
        }
    }



    public class MapScreen : Screen {

        protected TileMap _tilemap;
        protected List<MapObject> _objects = new();

        private int _scrollX, _scrollY;
        protected SpriteObject _player;

        public MapObject ViewTrackObj { get; set; }

        public MapScreen(SGame sgame, string tilemap) : base(sgame) {
            _tilemap = new TileMap(sgame, tilemap);
        }

        protected Vector2 ViewPosFor(MapObject obj) {
            return ViewPosFor(new Vector2(obj.X, obj.Y));
        }
        protected Vector2 ViewPosFor(Vector2 mapPos) {
            return new Vector2(
                mapPos.X * _tilemap.TileWidth + _scrollX,
                mapPos.Y * _tilemap.TileHeight + _scrollY
            );
        }

        protected override IEnumerable<IEntity> GetActiveEntities() {
            return _objects.Select(o => o.Entity).Where(e => e != null);
        }

        protected override Matrix GetTransform() {
            return Matrix.CreateTranslation(_scrollX, _scrollY, 0) * base.GetTransform();
        }

        protected override void Render(SpriteBatch spriteBatch) {
            //base.Render(spriteBatch);
            Layer L = Layer.BACKGROUND_BACK;
            foreach(int layer in Enumerable.Range(0, _tilemap.LayerCount)) {
                _tilemap.RenderLayer(spriteBatch, layer, L);
                L = L.Next;
                foreach(var obj in _objects.Where(o => o.Layer == layer)) {
                    var pos = new Vector2(
                        obj.X * _tilemap.TileWidth, 
                        obj.Y * _tilemap.TileHeight + _tilemap.TileHeight
                    );
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

        public override void Step() {
            base.Step();
            foreach(var obj in _objects) {
                if (obj.MoveState != null) {
                    int largest = Math.Max(
                        Math.Abs((obj.MoveState.TargetX - obj.X) * _tilemap.TileWidth),
                        Math.Abs((obj.MoveState.TargetY - obj.Y) * _tilemap.TileHeight)
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

            if (_player != null) {
                if (_player.MoveState == null) {
                    var direction = _sgame.Input.MovementVector();
                    if (direction != Vector2.Zero)
                        _player.Walk(direction);
                    else
                        _player.Sprite.PlayAnimation($"Idle{_player.Facing}", true);
                }
            }

            if (ViewTrackObj != null) {
                var pos = ViewPosFor(ViewTrackObj);
                float xMargin = _sgame.ScreenBounds.X / 4,
                    yMargin = _sgame.ScreenBounds.Y / 4;

                if (pos.X < xMargin)
                    _scrollX++;
                else if (pos.X > (_sgame.ScreenBounds.X - xMargin))
                    _scrollX--;
                if (pos.Y < yMargin)
                    _scrollY++;
                else if (pos.Y > (_sgame.ScreenBounds.Y - yMargin))
                    _scrollY--;
            }
        }
    }
}
