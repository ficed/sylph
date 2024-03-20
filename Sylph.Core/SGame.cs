using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SylphGame {

    public enum Facing { N, S, E, W };


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

    public interface ICacheable { }

    public abstract class SGame {

        private static Dictionary<Type, Func<SGame, string, ICacheable>> _cacheCreate = new();
        public static void Register<T>(Func<SGame, string, T> create) where T : ICacheable {
            _cacheCreate[typeof(T)] = (sg, str) => create(sg, str);
        }

        static SGame() {
            Register((sg, str) => new Entities.Sprite(sg, str));
            Register((sg, str) => new LoadedSfx(sg, str));
            Register((sg, str) => new Sylph.Core.UI.LoadedAtlases(sg));
        }


        private Dictionary<Type, Dictionary<string, WeakReference<ICacheable>>> _cache = new();
        public Random Random { get; private set; } = new();
        public Characters.Party Party { get; private set; }

        public T Load<T>(string which) where T : ICacheable {
            if (!_cache.TryGetValue(typeof(T), out var dict))
                dict = _cache[typeof(T)] = new Dictionary<string, WeakReference<ICacheable>>(StringComparer.InvariantCultureIgnoreCase);
            if (dict.TryGetValue(which, out var wr) && wr.TryGetTarget(out var result))
                return (T)result;
            result = _cacheCreate[typeof(T)](this, which);
            dict[which] = new WeakReference<ICacheable>(result);
            return (T)result;
        }

        private Dictionary<string, FontSystem> _fonts = new(StringComparer.InvariantCultureIgnoreCase);
        private Dictionary<string, DynamicSpriteFont> _fontInstances = new(StringComparer.InvariantCultureIgnoreCase);

        public GraphicsDevice Graphics { get; private set; }
        public Data Data { get; private set; }
        public UI.Boxes Boxes { get; private set; }
        public SylphConfig Config { get; private set; }
        public DynamicSpriteFont DefaultFont { get; private set; }
        public InputState Input { get; private set; } = new InputState();

        private Stack<Screen> _screens = new();
        public Screen ActiveScreen => _screens.Peek();
        public int DPIScale { get; set; }

        public Vector2 ScreenBounds => new Vector2(1280f / Config.Scale, 720f / Config.Scale);

        public SGame(IEnumerable<string> roots, GraphicsDevice graphics, Func<Screen> launch) {
            Data = new Data(roots);
            Graphics = graphics;

            Config = LoadJson<SylphConfig>("Game", "Sylph");

            FontSystemDefaults.FontResolutionFactor = 2f;
            FontSystemDefaults.KernelWidth = 2;
            FontSystemDefaults.KernelHeight = 2;

            foreach(string file in Data.Scan("Font", ".ttf")) {
                var fonts = new FontSystem();
                using (var s = Data.Open("Font", file))
                    fonts.AddFont(s);
                _fonts[Path.GetFileNameWithoutExtension(file)] = fonts;
                _fontInstances[Path.GetFileNameWithoutExtension(file)] = fonts.GetFont(16 * Config.Scale);
            }
            DefaultFont = _fontInstances[Config.UIDefaults.DefaultFont];

            Boxes = new UI.Boxes(this);

            PushScreen(new Splash(launch));
        }

        public DynamicSpriteFont GetFont(string name) => _fontInstances[name];

        public void PushScreen<T>() where T : Screen, new() {
            var s = new T();
            _screens.Push(s);
            s.Init(this);
            ActiveScreen.Activated();
        }
        public void PushScreen(Screen s) {
            _screens.Push(s);
            s.Init(this);
            ActiveScreen.Activated();
        }

        public void PopScreen(Screen s) {
            Trace.Assert(s == _screens.Pop());
            ActiveScreen.Activated();
        }


        private Dictionary<string, WeakReference<Texture2D>> _textures = new(StringComparer.InvariantCultureIgnoreCase);
        public Texture2D LoadTex(string category, string file) {
            string key = category + "\\" + file;
            if (_textures.TryGetValue(key, out var wr) && wr.TryGetTarget(out var tex))
                return tex;
            using (var s = Data.Open(category, AddExtIfNeeded(file, ".png"))) {
                tex = Texture2D.FromStream(Graphics, s);
                _textures[key] = new WeakReference<Texture2D>(tex);
                return tex;
            }
        }

        private string AddExtIfNeeded(string file, string ext) {
            if (file.Contains('.'))
                return file;
            else
                return file + ext;
        }

        public T LoadJson<T>(string category, string file) {
            using (var s = Data.Open(category, AddExtIfNeeded(file, ".json"))) {
                return Util.LoadJson<T>(s); 
            }
        }

        private void DoLoad(Func<string, Stream> getReadable) {
            Party = Characters.Party.Load(getReadable);
        }

        private void DoSave(string path) {
            Directory.CreateDirectory(path);
            Party.Save(s => new FileStream(Path.Combine(path, s), FileMode.Create));
        }

        private string SlotPath(int slot) {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Sylph",
                Config.GameID,
                $"save{slot}"
            );
        }

        public virtual void NewGame() {
            DoLoad(s => Data.Open("NewGame", s));
            Party.ID = Guid.NewGuid().ToString("N");
        }
        public void LoadGame(int slot) {
            DoLoad(s => File.OpenRead(Path.Combine(SlotPath(slot), s)));
        }

        public void SaveGame(int slot) => DoSave(SlotPath(slot));

        public abstract void MainMenu();
    }

    public class LoadedSfx : ICacheable {

        private Microsoft.Xna.Framework.Audio.SoundEffect _sfx;

        public LoadedSfx(SGame sgame, string which) {
            using (var s = sgame.Data.Open("Sfx", which + ".wav")) {
                _sfx = Microsoft.Xna.Framework.Audio.SoundEffect.FromStream(s);
            }
        }

        public void Play() {
            var instance = _sfx.CreateInstance();
            instance.Play();
            //TODO loop, volume, cancel, ...
        }
    }
}
