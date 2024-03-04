using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame.Field {

    public class DoorBehaviour : IFieldScript {

        private TileObjectFlags _flags;
        private string _dest, _sfx;
        private IRect _bounds;
        private MapScreen _map;
        private LoadedSfx _loadedSfx;
        private MapObject _obj;

        public DoorBehaviour(TileObjectFlags flags, string dest, string sfx) { 
            _dest = dest;
            _flags = flags;
            _sfx = sfx;
        }

        public void Init(MapScreen map, MapObject obj) {
            map.Tilemap.ChangeObject(obj.ID, TileObjectFlags.None, _flags);
            _bounds = map.Tilemap.GetObjectBounds(obj.ID);
            _map = map;
            _obj = obj;
            _loadedSfx = map.SGame.Load<LoadedSfx>(_sfx);
        }

        public IEnumerable<FieldScriptCondition> Run() {
            while (true) {
                if (_map.Player.Bounds.Overlaps(_bounds)) {
                    //STOP CONTROLS TODO
                    _loadedSfx.Play();
                    _map.Tilemap.ChangeObject(_obj.ID, _flags, TileObjectFlags.None);

                    string[] dest = _dest.Split('.');
                    _map.RegisterEffect(FadeEffect.Out(30, () => {
                        _map.SGame.PopScreen(_map);
                        _map.SGame.PushScreen(new MapScreen(_map.SGame, dest[0], dest[1]));
                    }));
                    break;
                }
                yield return new FieldScriptDelay(0);
                //TODO - wait for player move?
            }
        }
    }

    public class WalkRandomlyBehaviour : IFieldScript {

        private int _maxDistance, _speed, _minDelay, _maxDelay;
        private IVector2 _startPos;
        private MapScreen _map;
        private SpriteObject _obj;

        public WalkRandomlyBehaviour(int maxDistance, int speed, int minDelay, int maxDelay) {
            _maxDistance = maxDistance;
            _speed = speed;
            _minDelay = minDelay;
            _maxDelay = maxDelay;
        }

        public void Init(MapScreen map, MapObject obj) {
            _startPos = obj.Position;
            _map = map;
            _obj = obj as SpriteObject;
        }

        public IEnumerable<FieldScriptCondition> Run() {
            while (true) {
                int delay = _map.SGame.Random.Next(_maxDelay - _minDelay) + _minDelay;
                if (delay > 0) {
                    _obj.SetIdle();
                    yield return new FieldScriptDelay(delay);
                }

                var options = new List<IVector2>();
                foreach(Facing facing in Enum.GetValues<Facing>()) {
                    var direction = IVector2.FromFacing(facing);
                    if (_map.CanWalk(_obj, direction, out _, out var newPos, out _))
                        if ((newPos - _startPos).Length <= _maxDistance)
                            options.Add(direction);
                }

                if (options.Any()) {
                    var direction = options[_map.SGame.Random.Next(options.Count)];
                    yield return new FieldScriptWalk(_obj, _obj.Position + direction);
                }
            }
        }
    }
}
