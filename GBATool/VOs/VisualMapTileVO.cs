using System;
using System.Windows;

namespace GBATool.VOs;

public record VisualMapTileVO
{
    public string TileSetID { get; init; } = string.Empty;
    public Point Point { get; init; }

    public bool IsEmpty() { return this == Empty; }

    public static readonly VisualMapTileVO Empty = new();

    public override int GetHashCode() => HashCode.Combine(TileSetID);
}
