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
        public void SaveBehaviours(Func<string, Stream> getWritable) {
            foreach (var behaviour in _behaviours.Values) {
                using (var bs = getWritable(behaviour.GetType().Name + ".json")) {
                    Util.SaveJson(behaviour, bs);
                }
            }
        }
    }


    public interface IPartyBehaviour {

    }

    public enum StdIFlags {
        SaveEnabled = 1,
        Money = 2,
    }


    public class Party : BehaviourOwner<IPartyBehaviour> {

        public class StdPartyIFlags {
            private Party _party;

            internal StdPartyIFlags(Party party) {
                _party = party;
            }

            public int this[StdIFlags flags] {
                get => _party.IFlags.GetValueOrDefault(nameof(StdIFlags) + "." + flags.ToString());
                set => _party.IFlags[nameof(StdIFlags) + "." + flags.ToString()] = value;
            }
        }

        public long Frames { get; set; }
        public TimeSpan GameTime => TimeSpan.FromSeconds(Frames / 60.0);
        public string ID { get; set; }
        public Dictionary<string, int> IFlags { get; set; } = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
        public Dictionary<string, string> SFlags { get; set; } = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        public List<Character> Characters { get; set; } = new();

        public StdPartyIFlags StdIFlags { get; private set; } 

        [JsonIgnore]
        public IEnumerable<Character> ActiveChars => Characters.Where(c => c.Flags.HasFlag(StandardCharacterFlags.InParty));

        public Party() {
            StdIFlags = new StdPartyIFlags(this);
        }

        public static Party Load(Func<string, Stream> getReadable) {
            using (var s = getReadable("party.json")) {
                var party = Util.LoadJson<Party>(s);
                party.LoadBehaviours(getReadable);
                foreach (var chr in party.Characters) {
                    chr.LoadBehaviours(str => getReadable(chr.ID + "." + str));
                }
                return party;
            }
        }

        public void Save(Func<string, Stream> getWritable) {
            using (var s = getWritable("party.json"))
                Util.SaveJson(this, s);
            SaveBehaviours(getWritable);
            foreach (var chr in Characters) {
                chr.SaveBehaviours(str => getWritable(chr.ID + "." + str));
            }
        }
    }
}
