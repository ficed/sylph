using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame {

    public class UIDefaults {
        public string DefaultFont { get; set; }
        public string NumericFont { get; set; }
        public string ConfirmSfx { get; set; }
        public string CancelSfx { get; set; }
        public string MoveSfx { get; set; }
        public string CursorGraphic { get; set; }
    }

    public class SylphConfig {
        public string GameID { get; set; }
        public string Title { get; set; }
        public float Scale { get; set; }
        public string SplashGraphic { get; set; }
        public string LoadGraphic { get; set; }
        public UIDefaults UIDefaults { get; set; }
    }
}
