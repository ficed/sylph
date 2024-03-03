using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame {
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
}
