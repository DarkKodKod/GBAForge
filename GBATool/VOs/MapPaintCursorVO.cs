using System.Windows;
using System.Windows.Controls;

namespace GBATool.VOs;

public record MapPaintCursorVO(Image Image, string TilesetID, Point[] Rects);