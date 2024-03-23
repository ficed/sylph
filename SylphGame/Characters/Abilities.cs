using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame.Characters {
    public class Thief : ICharBehaviour {
    }


    public class LearnedSpell {
        public int SpellID { get; set; }
        public int Progress { get; set; }
    }
    public class Magic : ICharBehaviour {
        public List<LearnedSpell> Spells { get; set; } = new();
    }
}
