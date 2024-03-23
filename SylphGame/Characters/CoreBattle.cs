using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame.Characters {

    [Flags]
    public enum CoreStatuses {
        None = 0,
        Poison = 0x1,
        Slow = 0x2,
        Haste = 0x4,
        Confused = 0x8,
        Stop = 0x10,
        Blind = 0x20,
        Mute = 0x40,
        BackRow = 0x100,
    }

    public class CoreBattle : ICharBehaviour {
        public int XP { get; set; }
        public int Level { get; set; }
        public int CurrentHP { get; set; }
        public int MaxHP { get; set; }
        public int CurrentMP { get; set; }
        public int MaxMP { get; set; }
        public CoreStatuses Statuses { get; set; }

        public static implicit operator CoreBattle(Character c) => c.As<CoreBattle>();
    }

    public class CoreEquipment : ICharBehaviour {
        public int LeftID { get; set; }
        public int RightID { get; set; }
        public int ArmorID { get; set; }
        public int AccessoryID { get; set; }
    }
}
