﻿using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SylphGame.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SylphGame {

    public struct IRect : IEquatable<IRect> {

        public static readonly IRect Empty = new();

        public int Top { get; set; }
        public int Left { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public int Right => Left + Width;
        public int Bottom => Top + Height;
        public bool IsEmpty => (Width == 0) && (Height == 0);

        public IRect(int left, int top, int width, int height) {
            Top = top;
            Left = left;
            Width = width;
            Height = height;
        }

        public static IRect FromCoords(int left, int top, int right, int bottom) {
            return new IRect(
                left,
                top,
                right - left,
                bottom - top
            );
        }

        public void Offset(int offsetX, int offsetY) {
            Left += offsetX;
            Top += offsetY;
        }
        public void Offset(IVector2 offset) {
            Left += offset.X;
            Top += offset.Y;
        }
        public void Expand(int sizeX, int sizeY) {
            Width += sizeX;
            Height += sizeY;
        }

        public IRect Union(IRect other) {
            if (IsEmpty)
                return other;
            else if (other.IsEmpty)
                return this;
            else
                return FromCoords(
                    Math.Min(Left, other.Left),
                    Math.Min(Top, other.Top),
                    Math.Max(Right, other.Right),
                    Math.Max(Bottom, other.Bottom)
                );
        }

        public IRect Intersect(IRect other) {
            if (Overlaps(other)) {
                return FromCoords(
                    Math.Max(Left, other.Left),
                    Math.Max(Top, other.Top),
                    Math.Min(Right, other.Right),
                    Math.Min(Bottom, other.Bottom)
                );
            } else
                return Empty;
        }


        public bool Equals(IRect other) {
            return (Top == other.Top) &&
                (Left == other.Left) &&
                (Width == other.Width) &&
                Height == other.Height;
        }

        public override bool Equals([NotNullWhen(true)] object obj) {
            if (obj is IRect other)
                return Equals(other);
            else
                return base.Equals(obj);
        }

        public override int GetHashCode() {
            return Top ^ Left ^ Width ^ Height;
        }

        public bool Overlaps(IRect other) {
            if (Bottom <= other.Top) return false;
            if (Right <= other.Left) return false;
            if (Top >= other.Bottom) return false;
            if (Left >= other.Right) return false; 
            return true;
        }
    }

    public struct IVector2 : IEquatable<IVector2> {

        public static readonly IVector2 Zero = new IVector2();
        public static readonly IVector2 UnitX = new IVector2(1, 0);
        public static readonly IVector2 UnitY = new IVector2(0, 1);
        public static readonly IVector2 One = new IVector2(1, 1);

        public int X {  get; set; } 
        public int Y { get; set; }

        public IVector2 Direction => new IVector2(Math.Sign(X), Math.Sign(Y));
        public float Length => (float)Math.Sqrt(X * X + Y * Y);

        public IVector2(int x, int y) {
            X = x;
            Y = y;
        }

        public bool Equals(IVector2 other) {
            return (X == other.X) && (Y == other.Y);
        }

        public override int GetHashCode() {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public override bool Equals([NotNullWhen(true)] object obj) {
            if (obj is IVector2 otherPos)
                return Equals(otherPos);
            else
                return base.Equals(obj);
        }

        public static bool operator ==(IVector2 left, IVector2 right) {
            return left.Equals(right);
        }

        public static bool operator !=(IVector2 left, IVector2 right) {
            return !(left == right);
        }

        public static IVector2 operator+(IVector2 left, IVector2 right) {
            return new IVector2(left.X + right.X, left.Y + right.Y);
        }
        public static IVector2 operator -(IVector2 left, IVector2 right) {
            return new IVector2(left.X - right.X, left.Y - right.Y);
        }
        public static IVector2 operator -(IVector2 unary) {
            return new IVector2(-unary.X, -unary.Y);
        }

        public static IVector2 FromFacing(Facing f) {
            switch (f) {
                case Facing.N:
                    return -UnitY;
                case Facing.S:
                    return UnitY;
                    case Facing.E:
                    return UnitX;
                case Facing.W:
                    return -UnitX;
                default:
                    throw new NotImplementedException();
            }
        }

    }

    public static class Util {

        private static JsonSerializer _serializer = new JsonSerializer {
            Converters = {
                new Newtonsoft.Json.Converters.StringEnumConverter()
            }
        };

        public static void SaveJson(object o, Stream s) {
            using (var streamWriter = new StreamWriter(s))
            using (var jsonWriter = new JsonTextWriter(streamWriter)) {
                _serializer.Serialize(jsonWriter, o);
            }
        }

        public static T LoadJson<T>(Stream s) {
            using (var streamReader = new StreamReader(s))
            using (var jsonReader = new JsonTextReader(streamReader)) {
                return _serializer.Deserialize<T>(jsonReader);
            }
        }

        public static object LoadJson(Stream s, Type t) {
            using (var streamReader = new StreamReader(s))
            using (var jsonReader = new JsonTextReader(streamReader)) {
                return _serializer.Deserialize(jsonReader, t);
            }
        }

        public static Color WithAlpha(this Color c, byte alpha) {
            c.A = alpha;
            return c;
        }
        public static Color WithAlpha(this Color c, float alpha) => c.WithAlpha((byte)(alpha * 255));

        public static float DrawText(this DynamicSpriteFont font, SpriteBatch batch, string text, Vector2 position, 
            Color color, float rotation = 0f, Vector2 origin = default(Vector2), Vector2? scale = null, 
            float layerDepth = 0f, float characterSpacing = 0f, float lineSpacing = 0f, 
            TextStyle textStyle = TextStyle.None, FontSystemEffect effect = FontSystemEffect.None, 
            int effectAmount = 0, TextAlign textAlign = TextAlign.Left) {

            if (textAlign != TextAlign.Left) {
                float width = font.MeasureString(text, scale).X;
                switch (textAlign) {
                    case TextAlign.Center:
                        position.X -= (int)(width * 0.5f);
                        break;
                    case TextAlign.Right:
                        position.X -= (int)width;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            return font.DrawText(
                batch, text, position, color, rotation, origin, scale, layerDepth, characterSpacing, lineSpacing,
                textStyle, effect, effectAmount
            );
        }
    }
}
