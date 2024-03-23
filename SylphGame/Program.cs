using SylphGame;
using SylphGame.Field;
using System;
using System.Linq;

using (var game = new Sylph.Core.SylphGame(
    gfx => new TestGame(
        Environment.GetCommandLineArgs().Skip(1), gfx, () => MapScreen.Get<TestMap>("Entry1"))
    ))
    game.Run();
