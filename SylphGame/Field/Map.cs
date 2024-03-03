using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame.Field {
    
    public interface IPlayerControl {
        bool CanPlayerMove(MapScreen map, IVector2 newPos);
    }

    public abstract class FieldScriptCondition {
        public abstract bool IsFulfilled(MapScreen map);
    }

    public class FieldScriptDelay : FieldScriptCondition {
        private int _frames;

        public FieldScriptDelay(int frames) {
            _frames = frames;
        }

        public override bool IsFulfilled(MapScreen map) {
            --_frames;
            return _frames <= 0;
        }
    }

    public interface IFieldScript {
        void Init(MapScreen map, MapObject obj);
        IEnumerable<FieldScriptDelay> Run();
    }


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
        public IVector2 Position { get; set; }
        public ObjectFlags Flags { get; set; } = ObjectFlags.DEFAULT;
        public MoveState MoveState { get; set; }
        public int Layer { get; set; }

        public abstract void Render(Vector2 renderPos, SpriteBatch spriteBatch, Layer layer);
        public virtual IEntity Entity => null;
    }

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
    }



    public class MapScreen : Screen {

        protected TileMap _tilemap;
        protected List<MapObject> _objects = new();

        private int _scrollX, _scrollY;
        protected SpriteObject _player;

        public MapObject ViewTrackObj { get; set; }

        public SGame SGame => _sgame;

        protected Layer GetLayer(int index, bool entities) {
            Layer L = Layer.BACKGROUND_BACK;
            foreach (int i in Enumerable.Range(0, index))
                L = L.Next.Next.Next;
            if (entities)
                L = L.Next;
            return L;
        }

        public MapScreen(SGame sgame, string tilemap, string entrypoint) : base(sgame) {
            _tilemap = new TileMap(sgame, tilemap);

            _tilemap.GetPoint(entrypoint, out var ePos, out int eL);

            _player = new Field.SpriteObject(sgame, "Terra"); //TODO!
            DropToMap(_player, ePos);
            _objects.Add(_player);
            ViewTrackObj = _player;
        }

        protected void DropToMap(MapObject obj, IVector2 pos) {
            for(int L = _tilemap.LayerCount - 1; L >= 0; L--) {
                if (_tilemap.GetWalkableTile(pos, L, out var props, out int? newLevel)) {
                    obj.Position = pos;
                    obj.Layer = newLevel ?? L;
                }
            }
        }

        protected Vector2 ViewPosFor(MapObject obj) => ViewPosFor(obj.Position); //TODO - needed?
        protected Vector2 ViewPosFor(IVector2 mapPos) {
            return new Vector2(
                mapPos.X * _tilemap.TileWidth + _scrollX,
                mapPos.Y * _tilemap.TileHeight + _scrollY
            );
        }

        public bool TryWalk(SpriteObject obj, IVector2 direction) {
            //Get props for our current tile
            if (_tilemap.GetWalkableTile(obj.Position, obj.Layer, out var props, out _)) {
                var newPos = obj.Position;

                if (direction.X > 0) //TODO - consider relative X/Y magnitude
                    obj.Facing = Facing.E;
                else if (direction.X < 0)
                    obj.Facing = Facing.W;
                else if (direction.Y > 0)
                    obj.Facing = Facing.S;
                else if (direction.Y < 0)
                    obj.Facing = Facing.N;

                switch (obj.Facing) {
                    case Facing.E:
                        newPos.X++;
                        if (props.HasFlag(TileProperties.StairsUpE))
                            newPos.Y--;
                        else if (props.HasFlag(TileProperties.StairsDownE))
                            newPos.Y++;
                        break;
                    case Facing.W:
                        newPos.X--;
                        if (props.HasFlag(TileProperties.StairsUpW))
                            newPos.Y--;
                        else if (props.HasFlag(TileProperties.StairsDownW))
                            newPos.Y++;
                        break;
                    case Facing.S:
                        newPos.Y++;
                        break;
                    case Facing.N:
                        newPos.Y--;
                        break;
                    default:
                        throw new NotImplementedException();
                }
                obj.Sprite.PlayAnimation($"Walk{obj.Facing}", true);

                if (_tilemap.GetWalkableTile(newPos, obj.Layer, out var newProps, out var newLevel)) {
                    obj.MoveState = new MoveState {
                        TargetX = newPos.X,
                        TargetY = newPos.Y,
                        OnComplete = () => {
                            obj.Position = newPos;
                            obj.Layer = newLevel ?? obj.Layer;
                        }
                    };
                    return true;
                }
            }

            return false;
        }

        public bool CanMoveTo(int level, IVector2 pos) {
            var occupied = _objects
                .Where(obj => obj.Position == pos) //TODO - bigger objects?!
                .Where(obj => obj.Flags.HasFlag(ObjectFlags.Solid));
            if (occupied.Any())
                return false;
            if (_tilemap.GetWalkableTile(pos, level, out var props, out int? newLevel))
                return true;

            return false;
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
                foreach(var obj in _objects.Where(o => o.Layer == layer).OrderByDescending(o => o.Position.Y)) {
                    var pos = new Vector2(
                        obj.Position.X * _tilemap.TileWidth, 
                        obj.Position.Y * _tilemap.TileHeight + _tilemap.TileHeight
                    );
                    if (obj.MoveState != null) {
                        pos.X += _tilemap.TileWidth * (obj.MoveState.TargetX - obj.Position.X) * obj.MoveState.Progress;
                        pos.Y += _tilemap.TileHeight * (obj.MoveState.TargetY - obj.Position.Y) * obj.MoveState.Progress;
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
                        Math.Abs((obj.MoveState.TargetX - obj.Position.X) * _tilemap.TileWidth),
                        Math.Abs((obj.MoveState.TargetY - obj.Position.Y) * _tilemap.TileHeight)
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
                    if ((direction != IVector2.Zero) && CanMoveTo(_player.Layer, _player.Position + direction))
                        TryWalk(_player, direction);
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
