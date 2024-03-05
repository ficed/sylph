using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame.Field {
    
    public enum ScriptPriority {
        Interrupt = 0,
        High = 1,
        Medium = 2,
        Low = 3,
        Idle = 4,

        LOWEST_PRIO = Idle,
    }

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

    public class FieldScriptWalk : FieldScriptCondition {
        private SpriteObject _obj;
        private IVector2 _target;
        //TODO speed, etc.

        public FieldScriptWalk(SpriteObject obj, IVector2 target) {
            _obj = obj;
            _target = target;
        }

        public override bool IsFulfilled(MapScreen map) {
            if (_obj.Position == _target) {
                //_obj.Sprite.PlayAnimation("Idle", true); //No, leave it up to caller to decide? Hm. TODO.
                return true;
            }

            if (_obj.MoveState == null) {
                map.TryWalk(_obj, (_target - _obj.Position).Direction, true);
            }
            return false;
        }
    }

    public interface IFieldScript {
        void Init(MapScreen map, MapObject obj);
        IEnumerable<FieldScriptCondition> Run();
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

    public class TileMapObject : MapObject {

        public IRect Bounds { get; private set; }

        public TileMapObject(MapScreen map, string id) { 
            Bounds = map.Tilemap.GetObjectBounds(id);
            ID = id;
        }

        public override void Render(Vector2 renderPos, SpriteBatch spriteBatch, Layer layer) {
            //
        }
    }


    public class SpriteObject : MapObject {

        public Entities.Sprite.Instance Sprite { get; private set; }
        public Facing Facing { get; set; }

        public override IEntity Entity => Sprite;

        public IRect Bounds => new IRect(Position.X, Position.Y, 1, 1); //TODO!

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

        public void SetIdle() {
            Sprite.PlayAnimation($"Idle{Facing}", true);
        }
    }

    public class ActiveScripts {
        private class RunningScript {
            public IFieldScript Script { get; set; }
            public IEnumerator<FieldScriptCondition> ScriptEnum { get; set; }
            public FieldScriptCondition WaitingFor { get; set; }
        }

        private RunningScript[] _scripts = new RunningScript[(int)ScriptPriority.LOWEST_PRIO + 1];

        public void Run(MapScreen map) {
            foreach (int prio in Enumerable.Range(0, _scripts.Length)) {
                var active = _scripts[prio];
                if (active != null) {
                    if (active.WaitingFor != null) {
                        if (active.WaitingFor.IsFulfilled(map)) {
                            active.WaitingFor = null;
                        }
                    } else {
                        if (active.ScriptEnum.MoveNext()) {
                            active.WaitingFor = active.ScriptEnum.Current;
                        } else {
                            _scripts[prio] = null;
                        }
                    }
                    break;
                }
            }
        }

        public void Call(IFieldScript script, ScriptPriority priority, MapScreen map, MapObject obj) => TryCall(script, priority, map, obj);
        public bool TryCall(IFieldScript script, ScriptPriority priority, MapScreen map, MapObject obj) {
            if (_scripts[(int)priority] == null) {
                script.Init(map, obj);
                _scripts[(int)priority] = new RunningScript {
                    Script = script,
                    ScriptEnum = script.Run().GetEnumerator(),
                };
                return true;
            } else
                return false;
        }
    }

    public class MapScreen : Screen {

        public static T Get<T>(string entrypoint) where T : MapScreen, new() {
            T t = new T();
            t.Setup(entrypoint);
            return t;
        }

        protected List<MapObject> _objects = new();

        private int _scrollX, _scrollY;
        private Dictionary<MapObject, ActiveScripts> _scripts = new();
        protected string _tilemap, _entrypoint;

        public SpriteObject Player { get; set; }
        public TileMap Tilemap { get; private set; }
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

        public virtual void Setup(string entrypoint) {
            _entrypoint = entrypoint;   
        }

        public override void Init(SGame sgame) {
            base.Init(sgame);
            
            Tilemap = new TileMap(sgame, _tilemap);

            Tilemap.GetPoint(_entrypoint, out var ePos, out int eL);

            Player = new Field.SpriteObject(sgame, "Terra"); //TODO!
            DropToMap(Player, ePos);
            _objects.Add(Player);
            ViewTrackObj = Player;
            RegisterEffect(FadeEffect.In(30));
        }

        public void RegisterEffect(IEntity effect) {
            _entities.Add(effect);
        }

        protected void Call(MapObject obj, ScriptPriority prio, IFieldScript script) {
            TryCall(obj, prio, script);
        }
        protected bool TryCall(MapObject obj, ScriptPriority prio, IFieldScript script) {
            if (!_scripts.TryGetValue(obj, out var scripts))
                scripts = _scripts[obj] = new ActiveScripts();
            return scripts.TryCall(script, prio, this, obj);
        }

        protected void DropToMap(MapObject obj, IVector2 pos) {
            for(int L = Tilemap.LayerCount - 1; L >= 0; L--) {
                if (Tilemap.GetWalkableTile(pos, L, out var props, out int? newLevel)) {
                    obj.Position = pos;
                    obj.Layer = newLevel ?? L;
                }
            }
        }

        protected Vector2 ViewPosFor(MapObject obj) => ViewPosFor(obj.Position); //TODO - needed?
        protected Vector2 ViewPosFor(IVector2 mapPos) {
            return new Vector2(
                mapPos.X * Tilemap.TileWidth + _scrollX,
                mapPos.Y * Tilemap.TileHeight + _scrollY
            );
        }

        public bool CanWalk(SpriteObject obj, IVector2 direction, out Facing newFacing, out IVector2 newPos, out int? newLevel) {
            if (direction.X > 0) //TODO - consider relative X/Y magnitude
                newFacing = Facing.E;
            else if (direction.X < 0)
                newFacing = Facing.W;
            else if (direction.Y > 0)
                newFacing = Facing.S;
            else if (direction.Y < 0)
                newFacing = Facing.N;
            else
                throw new NotImplementedException();

            //Get props for our current tile
            if (Tilemap.GetWalkableTile(obj.Position, obj.Layer, out var props, out _)) {
                newPos = obj.Position;

                switch (newFacing) {
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

                if (Tilemap.GetWalkableTile(newPos, obj.Layer, out var newProps, out newLevel)) {
                    return true;
                }
            }

            newFacing = default;
            newPos = IVector2.Zero;
            newLevel = null;
            return false;
        }

        public bool TryWalk(SpriteObject obj, IVector2 direction, bool animateIfStuck) {
            if (CanWalk(obj, direction, out var newFacing, out var newPos, out int? newLevel)) {
                obj.Facing = newFacing;
                obj.Sprite.PlayAnimation($"Walk{obj.Facing}", true);
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

            if (animateIfStuck) {
                obj.Facing = newFacing;
                obj.Sprite.PlayAnimation($"Walk{obj.Facing}", true);
            }

            return false;
        }

        public bool CanMoveTo(int level, IVector2 pos) {
            var occupied = _objects
                .Where(obj => obj.Position == pos) //TODO - bigger objects?!
                .Where(obj => obj.Flags.HasFlag(ObjectFlags.Solid));
            if (occupied.Any())
                return false;
            if (Tilemap.GetWalkableTile(pos, level, out var props, out int? newLevel))
                return true;

            return false;
        }

        protected override IEnumerable<IEntity> GetActiveEntities() {
            return _objects
                .Select(o => o.Entity)
                .Where(e => e != null)
                .Concat(_entities);
        }

        protected override Matrix GetTransform() {
            return Matrix.CreateTranslation(_scrollX, _scrollY, 0) * base.GetTransform();
        }

        protected override void Render(SpriteBatch spriteBatch) {
            //base.Render(spriteBatch);
            Layer L = Layer.BACKGROUND_BACK;
            foreach(int layer in Enumerable.Range(0, Tilemap.LayerCount)) {
                Tilemap.RenderLayer(spriteBatch, layer, L);
                L = L.Next;
                foreach(var obj in _objects.Where(o => o.Layer == layer).OrderByDescending(o => o.Position.Y)) {
                    var pos = new Vector2(
                        obj.Position.X * Tilemap.TileWidth, 
                        obj.Position.Y * Tilemap.TileHeight + Tilemap.TileHeight
                    );
                    if (obj.MoveState != null) {
                        pos.X += Tilemap.TileWidth * (obj.MoveState.TargetX - obj.Position.X) * obj.MoveState.Progress;
                        pos.Y += Tilemap.TileHeight * (obj.MoveState.TargetY - obj.Position.Y) * obj.MoveState.Progress;
                    }
                    obj.Render(pos, spriteBatch, L);
                }
                L = L.Next;
                L = L.Next;
            }

            foreach (var ent in _entities) //effects, etc.
                ent.Render(spriteBatch);

        }

        public override void Step() {
            base.Step();

            foreach (var scripts in _scripts.Values)
                scripts.Run(this);

            foreach(var obj in _objects) {
                if (obj.MoveState != null) {
                    int largest = Math.Max(
                        Math.Abs((obj.MoveState.TargetX - obj.Position.X) * Tilemap.TileWidth),
                        Math.Abs((obj.MoveState.TargetY - obj.Position.Y) * Tilemap.TileHeight)
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

            if (Player != null) {
                if (Player.MoveState == null) {
                    var direction = _sgame.Input.MovementVector();
                    if ((direction != IVector2.Zero) && CanMoveTo(Player.Layer, Player.Position + direction))
                        TryWalk(Player, direction, true);
                    else
                        Player.SetIdle();
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
