using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame {
    public class FadeEffect : ITransientEntity {
        public int StepFrames => 1;
        public Layer Layer { get; set; }
        public bool IsComplete => _progress >= _total;

        private int _total, _progress;
        private float _alphaFrom, _alphaTo;
        private Color _color;
        private Action _then;

        public void Render(SpriteBatch spriteBatch) {
            var c = _color;
            c.A = (byte)(255 * (_alphaFrom + (_alphaTo - _alphaFrom) * _progress / _total));
            System.Diagnostics.Debug.WriteLine($"Fade alpha {c.A}");
            spriteBatch.FillRectangle(new RectangleF(0, 0, 1280, 720), c, Layer);
        }

        public void Step() {
            _progress++;
            if (IsComplete)
                _then?.Invoke();
        }

        public static FadeEffect In(int frames, Action then = null, Color? color = null) {
            return new FadeEffect {
                _total = frames,
                _then = then,
                _alphaFrom = 1,
                _alphaTo = 0,
                _color = color ?? Color.Black,
                Layer = Layer.MAX_OVERLAY,
            };
        }
        public static FadeEffect Out(int frames, Action then = null, Color? color = null) {
            return new FadeEffect {
                _total = frames,
                _then = then,
                _alphaFrom = 0,
                _alphaTo = 1,
                _color = color ?? Color.Black,
                Layer = Layer.MAX_OVERLAY,
            };
        }
    }
}
