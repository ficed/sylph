using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame {
    public struct IVector2 : IEquatable<IVector2> {

        public static readonly IVector2 Zero = new IVector2();

        public int X {  get; set; } 
        public int Y { get; set; }

        public IVector2(int x, int y) {
            X = x;
            Y = y;
        }

        public bool Equals(IVector2 other) {
            return (X == other.X) && (Y == other.Y);
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

        public override int GetHashCode() {
            return X.GetHashCode() ^ Y.GetHashCode();
        }
    }
}
