using System;
using System.Numerics;

namespace ArchitectureLibrary.Utils;

public struct Rectangle<T> where T : struct, INumber<T>
{
    public T X { get; set; }
    public T Y { get; set; }
    public T Width { get; set; }
    public T Height { get; set; }

    public static readonly Rectangle<T> Empty = new(default, default, default, default);

    public Rectangle()
    {
    }

    public Rectangle(T x, T y, T width, T height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public static bool operator ==(Rectangle<T> r1, Rectangle<T> r2)
    {
        return r1.X == r2.X &&
               r1.Y == r2.Y &&
               r1.Width == r2.Width &&
               r1.Height == r2.Height;
    }

    public static bool operator !=(Rectangle<T> r1, Rectangle<T> r2)
    {
        return !(r1 == r2);
    }

    public static Rectangle<T> operator +(Rectangle<T> r, (T numX, T numY) shift)
    {
        ArgumentNullException.ThrowIfNull(r);

        return new Rectangle<T>(r.X + shift.numX, r.Y + shift.numY, r.Width, r.Height);
    }

    public static Rectangle<T> operator *(Rectangle<T> r, T scale)
    {
        ArgumentNullException.ThrowIfNull(r);

        return new Rectangle<T>(r.X, r.Y, r.Width * scale, r.Height * scale);
    }

    public void ScaleFromOrigin(T scalar)
    {
        X *= scalar;
        Y *= scalar;
        Width *= scalar;
        Height *= scalar;
    }

    public override readonly bool Equals(object? obj) => obj is Rectangle<T> other && this == other;
    public override readonly int GetHashCode() => HashCode.Combine(X, Y, Width, Height);
    public override readonly string ToString() => $"[X: {X}, Y: {Y}, W: {Width}, H: {Height}]";
}
