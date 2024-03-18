﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SylphGame.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame {
    internal class TestGame : SGame {
        public TestGame(IEnumerable<string> roots, GraphicsDevice graphics, Func<Screen> launch) : base(roots, graphics, launch) {
        }

        public override void NewGame() {
            base.NewGame();
        }

        public override void MainMenu() {
            PushScreen<MainMenu>();
        }
    }
}
