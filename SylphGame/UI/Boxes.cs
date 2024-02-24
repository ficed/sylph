using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame.UI {
    public class Boxes {

        private Texture2D _border, _background;

        public Boxes(SGame sgame) {
            _border = sgame.LoadTex("UI", "Box");

            //TODO
            float rFrom = 86f / 255f, gFrom = 88f / 255f, bFrom = 169f / 255f,
                rTo = 19f / 255f, gTo = 21f / 255f, bTo = 93f / 255f;

            var colors = Enumerable.Range(0, 256)
                .Select(i => new Color(
                    rFrom + ((rTo - rFrom) / 256f),
                    gFrom + ((gTo - gFrom) / 256f),
                    bFrom + ((bTo - bFrom) / 256f)
                ))
                .ToArray();
            _background = new Texture2D(sgame.Graphics, 1, 256, false, SurfaceFormat.Color);
            _background.SetData(colors);
        }

        public Instance New() {
            return new Instance(this);
        }

        public class Instance : IEntity {
            private Boxes _boxes;

            private int _current, _total;
            private bool _growing;

            public Rectangle Location { get; set; }
            public bool IsAnimating => _current < _total;

            public int StepFrames => 1;

            public Instance(Boxes boxes) {
                _boxes = boxes;
            }

            public void Expand(int frames) {
                _growing = true;
                _current = 0;
                _total = frames;
            }
            public void Shrink(int frames) {
                _growing = false;
                _current = 0;
                _total = frames;
            }

            public void Step() {
                if (_current < _total) 
                    _current++;
            }

            public void Render(SpriteBatch spriteBatch) {

                Rectangle current;
                if (_current < _total) {
                    var center = Location.Center;
                    float progress = _growing ? (float)_current / _total : 1 - ((float)_current / _total);
                    int xd = (int)((Location.Width - 16) * 0.5f * progress) + 8,
                        yd = (int)((Location.Height - 16) * 0.5f * progress) + 8;
                    current = new Rectangle(center.X - xd, center.Y - yd, xd * 2, yd * 2);
                } else
                    current = Location;

                var bg = current;
                bg.Inflate(-2, -2);
                spriteBatch.Draw(
                    _boxes._background,
                    bg,
                    Color.White
                );

                //TL
                spriteBatch.Draw(
                    _boxes._border,
                    new Rectangle(current.X, current.Y, 8, 8),
                    new Rectangle(0, 0, 8, 8),
                    Color.White
                );
                //TR
                spriteBatch.Draw(
                    _boxes._border,
                    new Rectangle(current.Right - 8, current.Y, 8, 8),
                    new Rectangle(_boxes._border.Width - 8, 0, 8, 8),
                    Color.White
                );
                //BL
                spriteBatch.Draw(
                    _boxes._border,
                    new Rectangle(current.X, current.Bottom - 8, 8, 8),
                    new Rectangle(0, _boxes._border.Height - 8, 8, 8),
                    Color.White
                );
                //BR
                spriteBatch.Draw(
                    _boxes._border,
                    new Rectangle(current.Right - 8, current.Bottom - 8, 8, 8),
                    new Rectangle(_boxes._border.Width - 8, _boxes._border.Height - 8, 8, 8),
                    Color.White
                );

                //T
                spriteBatch.Draw(
                    _boxes._border,
                    new Rectangle(current.X + 8, current.Y, current.Width - 16, 8),
                    new Rectangle(8, 0, _boxes._border.Width - 16, 8),
                    Color.White
                );
                //B
                spriteBatch.Draw(
                    _boxes._border,
                    new Rectangle(current.X + 8, current.Bottom - 8, current.Width - 16, 8),
                    new Rectangle(8, _boxes._border.Height - 8, _boxes._border.Width - 16, 8),
                    Color.White
                );
                //L
                spriteBatch.Draw(
                    _boxes._border,
                    new Rectangle(current.X, current.Y + 8, 8, current.Height - 16),
                    new Rectangle(0, 8, 8, _boxes._border.Height - 16),
                    Color.White
                );
                //R
                spriteBatch.Draw(
                    _boxes._border,
                    new Rectangle(current.Right - 8, current.Y + 8, 8, current.Height - 16),
                    new Rectangle(_boxes._border.Width - 8, 8, 8, _boxes._border.Height - 16),
                    Color.White
                );
            }
        }
    }
}
