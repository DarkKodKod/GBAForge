using System.Windows.Controls;

namespace GBATool.VOs;

public record MapPaintCursorVO(Image Image, string BankID, VisualMapTileVO[,] VisualMapTiles);