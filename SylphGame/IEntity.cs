using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame {
    public interface IEntity {

        int StepFrames { get; }
        Layer Layer { get; set; }

        void Render(SpriteBatch spriteBatch);
        void Step();
    }
}
