using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame.Characters {

    public abstract class BehaviourOwner<TBehaviour> {
        private Dictionary<Type, TBehaviour> _behaviours = new();

        public List<string> Behaviours { get; set; } = new();

        public T As<T>() where T : TBehaviour {
            return (T)_behaviours[typeof(T)];
        }

        public void Register(TBehaviour behaviour) {
            _behaviours[behaviour.GetType()] = behaviour;
            Behaviours.Add(behaviour.GetType().AssemblyQualifiedName);
        }

        public void LoadBehaviours(Func<string, Stream> getReadable) {
            foreach (string b in Behaviours) {
                Type t = Type.GetType(b);
                using (var bs = getReadable(t.Name + ".json")) {
                    _behaviours[t] = (TBehaviour)Util.LoadJson(bs, t);
                }
            }
        }
    }


    public interface IPartyBehaviour {

    }

    public class Party : BehaviourOwner<IPartyBehaviour> {

        public long Frames { get; set; }
        public string ID { get; set; }
        public Dictionary<string, int> IFlags { get; set; } = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
        public Dictionary<string, string> SFlags { get; set; } = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        public List<Character> Characters { get; set; } = new();

        public static Party Load(Func<string, Stream> getReadable) {
            using (var s = getReadable("party.json")) {
                var party = Util.LoadJson<Party>(s);
                party.LoadBehaviours(getReadable); 
                foreach(var chr in party.Characters) {
                    chr.LoadBehaviours(str => getReadable(chr.ID + "." + str));
                }
                return party;
            }
        }

        public void Save(Func<string, Stream> getWritable) {

        }
    }
}
