using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SylphGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylph.Core.UI {

    public class AtlasItem {
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
    public class Atlas {
        public string Source { get; set; }
        public List<AtlasItem> Items { get; set; } = new();
    }

    public class LoadedAtlases : ICacheable {

        private Dictionary<string, (Texture2D, Rectangle)> _entries = new(StringComparer.InvariantCultureIgnoreCase);
        private SGame _sgame;

        public LoadedAtlases(SGame sgame) {
            _sgame = sgame;
            foreach (string file in sgame.Data.Scan("UI", "atlas.json")) {
                using(var s = sgame.Data.Open("UI", file)) {
                    var atlas = Util.LoadJson<Atlas>(s);
                    var tex = sgame.LoadTex("UI", atlas.Source);
                    foreach (var entry in atlas.Items)
                        _entries[entry.Name] = (
                            tex,
                            new Rectangle(entry.X, entry.Y, entry.Width, entry.Height)
                        );
                }
            }
        }

        public (Texture2D, Rectangle) Get(string name) => _entries[name];

        public (Texture2D, Rectangle) GetOrLoad(string name) {
            if (_entries.TryGetValue(name, out var value))
                return value;
            var tex = _sgame.LoadTex("UI", name);
            return (tex, new Rectangle(0, 0, tex.Width, tex.Height));
        }
    }
}
