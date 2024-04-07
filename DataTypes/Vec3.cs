using System.Numerics;

namespace Minecraft.DataTypes;

public struct Vec3<T> where T : INumber<T>
{
    public T X { readonly get; set; }
    public T Y { readonly get; set; }
    public T Z { readonly get; set; }

    public Vec3(T xyz)
        => X = Y = Z = xyz;

    public Vec3(T x, T y, T z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static Vec3<T> Zero => new(T.Zero);
    public static Vec3<T> One => new(T.One);

    public static Vec3<T> operator +(Vec3<T> left, T right) => new(left.X + right, left.Y + right, left.Z + right);
    public static Vec3<T> operator +(Vec3<T> left, Vec3<T> right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

    public static Vec3<T> operator -(Vec3<T> left, T right) => new(left.X - right, left.Y - right, left.Z - right);
    public static Vec3<T> operator -(Vec3<T> left, Vec3<T> right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

    public static Vec3<T> operator *(Vec3<T> left, T right) => new(left.X * right, left.Y * right, left.Z * right);
    public static Vec3<T> operator *(Vec3<T> left, Vec3<T> right) => new(left.X * right.X, left.Y * right.Y, left.Z * right.Z);

    public static Vec3<T> operator /(Vec3<T> left, T right) => new(left.X / right, left.Y / right, left.Z / right);
    public static Vec3<T> operator /(Vec3<T> left, Vec3<T> right) => new(left.X / right.X, left.Y / right.Y, left.Z / right.Z);

    public static Vec3<T> operator %(Vec3<T> left, T right) => new(left.X % right, left.Y % right, left.Z % right);
    public static Vec3<T> operator %(Vec3<T> left, Vec3<T> right) => new(left.X % right.X, left.Y % right.Y, left.Z % right.Z);

    public static bool operator ==(Vec3<T> self, Vec3<T> other)
        => self.Equals(other);

    public static bool operator !=(Vec3<T> self, Vec3<T> other)
        => !(self == other);

    public static bool operator <(Vec3<T> self, Vec3<T> other)
        => Compare(self, other) < 0;

    public static bool operator >(Vec3<T> self, Vec3<T> other)
        => Compare(self, other) > 0;

    static int Compare(Vec3<T> self, Vec3<T> other)
    {
        if (self.X < other.X)
            return -1;
        if (self.X > other.X)
            return 1;

        if (self.Y < other.Y)
            return -1;
        if (self.Y > other.Y)
            return 1;

        if (self.Z < other.Z)
            return -1;
        if (self.Z > other.Z)
            return 1;

        return 0;
    }

    public override readonly bool Equals(object obj)
        => obj is Vec3<T> other && Equals(other);

    public override readonly int GetHashCode()
        => HashCode.Combine(X, Y, Z);

    public readonly bool Equals(Vec3<T> other)
    {
        return X == other.X
            && Y == other.Y
            && Z == other.Y;
    }

    public override readonly string ToString()
        => $"Vec3[{typeof(T)}]{{ X = {X}, Y = {Y}, Z = {Z} }}";
}
