using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame.UI {
    public class DialogBox : IEntity {

        private enum State {
            Opening,
            Drawing,
            Waiting,
            Closing,
        }

        public int StepFrames => 1;
        public Layer Layer {
            get => _box.Layer;
            set => _box.Layer = value;
        }

        private Boxes.Instance _box;
        private string[] _texts;
        private int _textProgress;
        private DynamicSpriteFont _font;
        private State _state = State.Opening;
        private SGame _sgame;

        public DialogBox(SGame sgame, string text, Rectangle location) {
            _sgame = sgame;
            _font = sgame.DefaultFont;
            _texts = _font.WrapText(sgame, text, location.Width - 8).ToArray();
            _box = sgame.Boxes.New();
            _box.Location = location;
            _box.Expand(45);
        }

        public void Render(SpriteBatch spriteBatch) {
            _box.Render(spriteBatch);

            int textLen = _state == State.Drawing ? _textProgress : int.MaxValue;

            switch (_state) {
                case State.Drawing:
                case State.Waiting:
                    float y = _box.Location.Y;
                    foreach (string line in _texts) {
                        string toDraw = line.Substring(0, Math.Min(line.Length, textLen));
                        _font.DrawText(
                            spriteBatch, toDraw, 
                            new Vector2(_box.Location.X + 6, (int)y), Color.White,
                            layerDepth: _box.Layer.Next.Next,
                            scale: new Vector2(1f / _sgame.Config.Scale)
                        );
                        y += _font.LineHeight / _sgame.Config.Scale;
                        textLen -= toDraw.Length;
                    }
                    break;
            }
        }

        public void Step() {
            switch (_state) {
                case State.Opening:
                    if (!_box.IsAnimating)
                        _state = State.Drawing;
                    break;
                case State.Drawing:
                    _textProgress++;
                    break;
            }
            _box.Step();
        }
    }
}
