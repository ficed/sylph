using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame.Characters {

    [Flags]
    public enum StandardCharacterFlags {
        None = 0,
        InParty = 0x1,
        Available = 0x2,
    }

    public class Character : BehaviourOwner<ICharBehaviour> {

        public int ID { get; set; }
        public string Name { get; set; }
        public string SmallImage { get; set; }
        public string LargeImage { get; set; }
        public StandardCharacterFlags Flags { get; set; }
    }

    public interface ICharBehaviour {
    }
}
