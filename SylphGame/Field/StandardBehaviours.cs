using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame.Field {
    public class WalkRandomlyBehaviour : IFieldScript {

        private int _maxDistance, _speed, _minDelay, _maxDelay;
        private IVector2 _startPos;
        private MapScreen _map;

        public WalkRandomlyBehaviour(int maxDistance, int speed, int minDelay, int maxDelay) {
            _maxDistance = maxDistance;
            _speed = speed;
            _minDelay = minDelay;
            _maxDelay = maxDelay;
        }

        public void Init(MapScreen map, MapObject obj) {
            _startPos = obj.Position;
            _map = map;
        }

        public IEnumerable<FieldScriptDelay> Run() {
            while (true) {
                int delay = _map.SGame.Random.Next(_maxDelay - _minDelay) + _minDelay;
                yield return new FieldScriptDelay(delay);

                foreach(Facing facing in Enum.GetValues<Facing>()) {

                }
            }
        }
    }
}
