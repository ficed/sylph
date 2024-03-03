using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame {

    public enum InputButton {
        OK,
        Cancel,
        Menu,
        Left,
        Right,
        Up,
        Down,
        Start,
        Select,
    }

    public class InputState {
        private Dictionary<InputButton, int> _downFor = new();

        public InputState() {
            foreach(InputButton b in Enum.GetValues<InputButton>())
                _downFor[b] = 0;
        }

        public void Update(Dictionary<InputButton, bool> isDown) {
            foreach (InputButton b in isDown.Keys) {
                if (isDown[b])
                    _downFor[b]++;
                else
                    _downFor[b] = 0;
            }
        }

        public bool IsDown(InputButton b) => _downFor[b] > 0;
        public bool IsJustDown(InputButton b) => _downFor[b] == 1;
        public bool IsDownRepeat(InputButton b) => (_downFor[b] % 20) == 1;

        public IVector2 MovementVector() {
            return new IVector2(
                IsDown(InputButton.Left) ? -1 : IsDown(InputButton.Right) ? 1 : 0,
                IsDown(InputButton.Up) ? -1 : IsDown(InputButton.Down) ? 1 : 0
            );
        }
    }
}
