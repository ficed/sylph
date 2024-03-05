using SylphGame;
using SylphGame.Field;

using (var game = new Sylph.Core.SylphGame(() => MapScreen.Get<TestMap>("Entry1")))
    game.Run();
