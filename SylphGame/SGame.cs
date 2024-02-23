using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame {
    public class SGame {

        public GraphicsDevice Graphics { get; private set; }
        public Data Data { get; private set; }

        public SGame(string root, GraphicsDevice graphics) {
            Data = new Data(root);
            Graphics = graphics;
        }

        private Dictionary<string, WeakReference<Texture2D>> _textures = new(StringComparer.InvariantCultureIgnoreCase);
        public Texture2D LoadTex(string category, string file) {
            string key = category + "\\" + file;
            if (_textures.TryGetValue(key, out var wr) && wr.TryGetTarget(out var tex))
                return tex;
            using (var s = Data.Open(category, file + ".png")) {
                tex = Texture2D.FromStream(Graphics, s);
                _textures[key] = new WeakReference<Texture2D>(tex);
                return tex;
            }
        }

        public T Load<T>(string category, string file) {
            var serializer = new JsonSerializer();
            using (var s = Data.Open(category, file + ".json")) {
                using (var streamReader = new StreamReader(s))
                using (var jsonReader = new JsonTextReader(streamReader)) {
                    return serializer.Deserialize<T>(jsonReader);
                }
            }
        }
    }
}
