using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SylphGame.Field {

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

    public class TmjLayer {
        public List<int> Data { get; set; }
        public int Height { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public double Opacity { get; set; }
        public List<TmjProperty> Properties { get; set; }
        public string Type { get; set; }
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
        private List<Texture2D> _textures = new();
        private List<TsjTileset> _tilesets = new();
        private TmjMap _map;
        private Dictionary<int, (Rectangle SrcRect, Texture2D Tex)> _tiles = new();

        public int LayerCount => _map.Layers.Count;
        public int TileWidth => _map.TileWidth;
        public int TileHeight => _map.TileHeight;

        public TileMap(SGame sgame, string which) {
            _map = sgame.LoadJson<TmjMap>("ffmap", "cave1.tmj");

            foreach(var sourceTileset in _map.Tilesets) {
                var tileset = sgame.LoadJson<TsjTileset>("ffmap", sourceTileset.Source);
                _tilesets.Add(tileset);
                _textures.Add(sgame.LoadTex("ffmap", tileset.Image));
                int index = sourceTileset.FirstGid;
                foreach(int ty in Enumerable.Range(0, tileset.ImageHeight / tileset.TileHeight)) {
                    foreach (int tx in Enumerable.Range(0, tileset.ImageWidth / tileset.TileWidth)) {
                        _tiles[index] = (
                            new Rectangle(tx * tileset.TileWidth, ty * tileset.TileHeight, tileset.TileWidth, tileset.TileHeight),
                            _textures.Last()
                        );
                        index++;
                    }
                }
            }
        }

        public void RenderLayer(SpriteBatch spriteBatch, int which, Layer depth) {
            var layer = _map.Layers[which];
                int index = 0;
                int ox = 0, oy = 0;
            foreach (int tile in layer.Data) {
                if ((tile != 0) && _tiles.TryGetValue(tile, out var tileData)) {
                    spriteBatch.Draw(
                        tileData.Tex,
                        new Rectangle(ox + layer.X, oy + layer.Y, _map.TileWidth, _map.TileHeight),
                        tileData.SrcRect,
                        Color.White,
                        0, Vector2.Zero, SpriteEffects.None,
                        depth
                    );
                }
                index++;
                if ((index % layer.Width) == 0) {
                    ox = 0;
                    oy += _map.TileHeight;
                } else
                    ox += _map.TileWidth;
            }
        }
    }
}
