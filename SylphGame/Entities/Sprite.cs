using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SylphGame.Entities {

    [Flags]
    public enum SpriteAnimationFlags {
        None = 0,
        FlipH = 0x1,
        FlipV = 0x2,
    }

    public class SpriteAnimation {
        public string Name { get; set; }
        public List<int> Frames { get; set; } = new();
        public SpriteAnimationFlags Flags { get; set; }
    }
    public class SpriteData {
        public int FrameWidth { get; set; }
        public int FrameHeight { get; set; }
        public List<SpriteAnimation> Animations { get; set; } = new();
    }

    public class Sprite {
        private SpriteData _data;
        private Texture2D _tex;

        private Rectangle GetFrameRect(int frame) {
            int framesPerRow = _tex.Width / _data.FrameWidth;
            return new Rectangle(
                (frame % framesPerRow) * _data.FrameWidth,
                (frame / framesPerRow) * _data.FrameHeight,
                _data.FrameWidth, _data.FrameHeight
            );
        }

        public class Instance : IEntity {
            private SpriteAnimation _animation;
            private int _frame;
            private bool _loop;
            private Sprite _sprite;
            
            public Vector2 Position { get; set; }
            public int StepFrames => 15;
            public Layer Layer { get; set; }

            public Instance(Sprite sprite) {
                _sprite = sprite;
                PlayAnimation("idle", true);
            }

            public void PlayAnimation(string animation, bool loop) {
                _animation = _sprite._data.Animations.Single(a => a.Name.Equals(animation, StringComparison.InvariantCultureIgnoreCase));
                _frame = 0;
                _loop = loop;
            }

            public void Render(SpriteBatch batch) {
                SpriteEffects fx = SpriteEffects.None;
                if (_animation.Flags.HasFlag(SpriteAnimationFlags.FlipH))
                    fx |= SpriteEffects.FlipHorizontally;
                if (_animation.Flags.HasFlag(SpriteAnimationFlags.FlipV))
                    fx |= SpriteEffects.FlipVertically;

                batch.Draw(
                    _sprite._tex,
                    new Rectangle((int)Position.X, (int)Position.Y, _sprite._data.FrameWidth, _sprite._data.FrameHeight),
                    _sprite.GetFrameRect(_animation.Frames[_frame]),
                    Color.White,
                    0,
                    Vector2.Zero,
                    fx,
                    Layer
                );
            }

            public void Step() {
                if (_frame < (_animation.Frames.Count - 1))
                    _frame++;
                else if (_loop)
                    _frame = 0;
            }
        }


        public Sprite(SGame sgame, string name) {
            _data = sgame.Load<SpriteData>("Sprite", name);
            _tex = sgame.LoadTex("Sprite", name);
        }

        public Instance New() {
            return new Instance(this);
        }
    }
}
