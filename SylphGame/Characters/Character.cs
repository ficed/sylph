using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame.Characters {
    public class Character : BehaviourOwner<ICharBehaviour> {

        public int ID { get; set; }
        public string Name { get; set; }
        public string SmallImage { get; set; }
        public string LargeImage { get; set; }

    }

    public interface ICharBehaviour {
    }
}
