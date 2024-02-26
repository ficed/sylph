using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame {
    public abstract class Screen {
        private SpriteBatch _spriteBatch;

        protected SGame _sgame;
        protected List<IEntity> _entities = new();
        protected int _frame;


        public virtual Color Background => Color.CornflowerBlue;

        public Screen(SGame sgame) {
            _sgame = sgame;
            _spriteBatch = new SpriteBatch(_sgame.Graphics);
        }

        public virtual void Activated() { }

        protected virtual void Render(SpriteBatch spriteBatch) { 
            foreach(var ent in _entities)
                ent.Render(spriteBatch);
        }

        protected virtual Matrix GetTransform() {
            return Matrix.CreateScale(_sgame.Config.Scale, _sgame.Config.Scale, 1);
        }

        public virtual void Render() {
            _sgame.Graphics.Clear(Background);

            _spriteBatch.Begin(
                transformMatrix: GetTransform(),
                samplerState: SamplerState.PointClamp,
                sortMode: SpriteSortMode.FrontToBack,
                depthStencilState: DepthStencilState.None,
                blendState: BlendState.AlphaBlend
            );
            Render(_spriteBatch);
            _spriteBatch.End();
        }

        public virtual void Step() {
            var current = _entities.ToArray();
            foreach(var ent in current) {
                if ((ent is ITransientEntity trEnt) && trEnt.IsComplete)
                    _entities.Remove(ent);
                else {
                    if ((_frame % ent.StepFrames) == 0)
                        ent.Step();
                }
            }

            _frame++;
        }
    }
}
