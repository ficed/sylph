using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame {

    public struct Layer {
        private const int LAYER_COUNT = 32;
        private const float LAYER_AMOUNT = 1f / LAYER_COUNT;

        public static readonly Layer BACKGROUND_BACK = new Layer(1);
        public static readonly Layer BACKGROUND_MID = new Layer(4);
        public static readonly Layer BACKGROUND_FRONT = new Layer(7);
        public static readonly Layer BACKGROUND_OVERLAY = new Layer(10);

        public static readonly Layer UI_BACK = new Layer(16);
        public static readonly Layer UI_MID = new Layer(20);
        public static readonly Layer UI_FRONT = new Layer(24);

        public static readonly Layer MAX_OVERLAY = new Layer(31);

        private int _index;

        private Layer(int index) {
            if ((index < 0) || (index > LAYER_COUNT))
                throw new IndexOutOfRangeException();
            _index = index;
        }

        public Layer Next => new Layer(_index + 1);
        public Layer Prev => new Layer(_index - 1);

        public static implicit operator float(Layer L) {
            return LAYER_AMOUNT * L._index;
        }
    }

    public class SGame {

        public GraphicsDevice Graphics { get; private set; }
        public Data Data { get; private set; }
        public FontSystem Fonts { get; private set; }
        public UI.Boxes Boxes { get; private set; }
        public SylphConfig Config { get; private set; }
        public DynamicSpriteFont DefaultFont { get; private set; }


        public SGame(string root, GraphicsDevice graphics) {
            Data = new Data(root);
            Graphics = graphics;

            Config = Load<SylphConfig>("Game", "Sylph");

            FontSystemDefaults.FontResolutionFactor = 2f;
            FontSystemDefaults.KernelWidth = 2;
            FontSystemDefaults.KernelHeight = 2;

            Fonts = new FontSystem();
            //TODO configure!
            using(var s = Data.Open("Font", "FF6Snes.ttf"))
                Fonts.AddFont(s);
            DefaultFont = Fonts.GetFont(16 * Config.Scale);

            Boxes = new UI.Boxes(this);
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
