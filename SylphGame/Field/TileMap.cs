using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SylphGame.Field {

    [Flags]
    public enum TileProperties {
        None = 0,
        Walkable = 0x1,
        StairsUpE = 0x2,
        StairsUpW = 0x4,
        StairsDownE = 0x8,
        StairsDownW = 0x10,
    }

    [Flags]
    public enum TileObjectFlags {
        None = 0,
        ShiftLeft = 0x1,
        ShiftRight = 0x2,
        Hidden = 0x4,
    }

    public class TsjTileset {
        public int Columns { get; set; }
        public string Image { get; set; }
        public int ImageHeight { get; set; }
        public int ImageWidth { get; set; }
        public int Margin { get; set; }
        public string Name { get; set; }
        public int Spacing { get; set; }
        public int TileCount { get; set; }
        public string TiledVersion { get; set; }
        public int TileHeight { get; set; }
        public List<TsjTile> Tiles { get; set; }
        public int TileWidth { get; set; }
        public string Type { get; set; }
        public string Version { get; set; }
    }

    public class TsjTile {
        public int Id { get; set; }
        public List<TsjProperty> Properties { get; set; }

        public object FindPropValue(string name) {
            return Properties
                .SingleOrDefault(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                ?.Value;
        }
    }

    public class TsjProperty {
        public string Name { get; set; }
        public string Type { get; set; }
        public object Value { get; set; }
    }

    public class TmjMap {
        public int CompressionLevel { get; set; }
        public int Height { get; set; }
        public bool Infinite { get; set; }
        public List<TmjLayer> Layers { get; set; }
        public List<TmjProperty> Properties { get; set; }
        public int NextLayerId { get; set; }
        public int NextObjectId { get; set; }
        public string Orientation { get; set; }
        public string RenderOrder { get; set; }
        public string TiledVersion { get; set; }
        public int TileHeight { get; set; }
        public List<TmjTilesetInfo> Tilesets { get; set; }
        public int TileWidth { get; set; }
        public string Type { get; set; }
        public string Version { get; set; }
        public int Width { get; set; }
    }

    public enum TmjLayerType {
        TileLayer,
        ObjectGroup,
    }

    public class TmjObjectInfo {
        public int Height { get; set; }
        public int Id { get; set; }
        public int Gid { get; set; }
        public string Name { get; set; }
        public bool Point { get; set; }
        public int Rotation { get; set; }
        public string Type { get; set; }
        public bool Visible { get; set; }
        public int Width { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class TmjLayer {
        public List<int> Data { get; set; }
        public List<TmjObjectInfo> Objects { get; set; }
        public int Height { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public double Opacity { get; set; }
        public List<TmjProperty> Properties { get; set; }
        public TmjLayerType Type { get; set; }
        public bool Visible { get; set; }
        public int Width { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class TmjProperty {
        public string Name { get; set; }
        public string Type { get; set; }
        public object Value { get; set; }
    }

    public class TmjTilesetInfo {
        public int FirstGid { get; set; }
        public string Source { get; set; }
    }



    public class TileMap {

        private class MergedLayer {
            public TmjLayer Tiles { get; set; }
            public TmjLayer Objects { get; set; }
        }

        private List<Texture2D> _textures = new();
        private List<TsjTileset> _tilesets = new();
        private TmjMap _map;
        private List<MergedLayer> _layers = new();
        private Dictionary<int, (Rectangle SrcRect, Texture2D Tex, TsjTile Tile)> _tiles = new();
        private Dictionary<string, TileObjectFlags> _objFlags = new(StringComparer.InvariantCultureIgnoreCase);

        public int LayerCount => _layers.Count;
        public int TileWidth => _map.TileWidth;
        public int TileHeight => _map.TileHeight;

        public TileMap(SGame sgame, string which) {
            _map = sgame.LoadJson<TmjMap>("ffmap", "cave1.tmj");

            foreach (var sourceTileset in _map.Tilesets) {
                var tileset = sgame.LoadJson<TsjTileset>("ffmap", sourceTileset.Source);
                _tilesets.Add(tileset);
                _textures.Add(sgame.LoadTex("ffmap", tileset.Image));
                int index = sourceTileset.FirstGid;
                foreach (int ty in Enumerable.Range(0, tileset.ImageHeight / tileset.TileHeight)) {
                    foreach (int tx in Enumerable.Range(0, tileset.ImageWidth / tileset.TileWidth)) {
                        var srcTile = tileset.Tiles
                            .SingleOrDefault(t => t.Id == (index - sourceTileset.FirstGid));
                        _tiles[index] = (
                            new Rectangle(tx * tileset.TileWidth, ty * tileset.TileHeight, tileset.TileWidth, tileset.TileHeight),
                            _textures.Last(),
                            srcTile
                        );
                        index++;
                    }
                }
            }

            foreach(var layer in _map.Layers) {
                switch (layer.Type) {
                    case TmjLayerType.TileLayer:
                        _layers.Add(new MergedLayer { Tiles = layer });
                        break;
                    case TmjLayerType.ObjectGroup:
                        _layers[_layers.Count - 1].Objects = layer;
                        break;
                }
            }
        }

        public void ChangeObject(string objName, TileObjectFlags toSet, TileObjectFlags toClear) {
            if (!_objFlags.TryGetValue(objName, out var flags))
                flags = TileObjectFlags.None;
            flags |= toSet;
            flags &= ~toClear;
            _objFlags[objName] = flags;
        }

        public bool GetWalkableTile(IVector2 pos, int level, out TileProperties props, out int? newLevel) {
            newLevel = null; //TODO
            var layer = _layers[level].Tiles;

            int index = pos.Y * layer.Width + pos.X;
            int tile = layer.Data[index];
            if (_tiles.TryGetValue(tile, out var t) && (t.Tile != null)) {
                var walk = t.Tile.FindPropValue("Sylph.Walk");
                if (int.TryParse(walk?.ToString() ?? "", out int v) && (v == 1)) {
                    props = TileProperties.Walkable;
                    return true;
                }
            }

            props = default;
            return false;
        }

        public IRect GetObjectBounds(string name) {
            var bounds = IRect.Empty;
            foreach (var layer in _layers) {
                if (layer.Objects != null) {
                    foreach(var obj in layer.Objects.Objects.Where(o => o.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))) {
                        var objBounds = new IRect(
                            (layer.Objects.X + (int)obj.X) / _map.TileWidth,
                            (layer.Objects.Y + (int)obj.Y) / _map.TileHeight,
                            obj.Width / _map.TileWidth,
                            obj.Height / _map.TileHeight
                        );
                        if (obj.Gid != 0) //Tiles which go up from origin, not down
                            objBounds.Offset(0, -1);
                        bounds = bounds.Union(objBounds);
                    }
                }
            }
            return bounds;
        }

        public bool GetPoint(string name, out IVector2 pos, out int layer) {
            foreach(int L in Enumerable.Range(0, _layers.Count)) {
                if (_layers[L].Objects != null) {
                    var pt = _layers[L].Objects
                        .Objects
                        .SingleOrDefault(o => o.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                    if (pt != null) {
                        layer = L;
                        pos = new IVector2(
                            (_layers[L].Objects.X + (int)pt.X) / _map.TileWidth,
                            (_layers[L].Objects.Y + (int)pt.Y) / _map.TileHeight
                        );
                        return true;
                    }
                }
            }

            pos = IVector2.Zero;
            layer = -1;
            return false;
        }

        public void RenderLayer(SpriteBatch spriteBatch, int which, Layer depth) {
            var layer = _layers[which];
            int index = 0;
            int ox = 0, oy = 0;

            void TryDrawTile(int tile, int xOffset, int yOffset, TileObjectFlags flags) {
                if (_tiles.TryGetValue(tile, out var tiledata)) {
                    var src = tiledata.SrcRect;
                    if (flags.HasFlag(TileObjectFlags.ShiftLeft))
                        src.Offset(-src.Width, 0);
                    else if (flags.HasFlag(TileObjectFlags.ShiftRight))
                        src.Offset(src.Width, 0);

                    spriteBatch.Draw(
                        tiledata.Tex,
                        new Rectangle(xOffset, yOffset, _map.TileWidth, _map.TileHeight),
                        src,
                        Color.White,
                        0, Vector2.Zero, SpriteEffects.None,
                        depth
                    );
                }
            }

            foreach (int tile in layer.Tiles.Data) {
                if (tile != 0)
                    TryDrawTile(tile, ox + layer.Tiles.X, oy + layer.Tiles.Y, TileObjectFlags.None);
                index++;
                if ((index % layer.Tiles.Width) == 0) {
                    ox = 0;
                    oy += _map.TileHeight;
                } else
                    ox += _map.TileWidth;
            }

            if (layer.Objects != null) {
                foreach (var tobj in layer.Objects.Objects) {
                    if (!_objFlags.TryGetValue(tobj.Name, out var flags))
                        flags = TileObjectFlags.None;

                    if (flags.HasFlag(TileObjectFlags.Hidden))
                        continue;

                    if (tobj.Gid != 0) {
                        TryDrawTile(
                            tobj.Gid, 
                            layer.Objects.X + (int)tobj.X, 
                            layer.Objects.Y + (int)tobj.Y - _map.TileHeight,
                            flags
                        );
                    }
                }
            }

        }
    }
}
