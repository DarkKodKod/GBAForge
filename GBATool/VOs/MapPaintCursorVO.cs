using System.Windows.Media;

namespace GBATool.VOs;

public record MapPaintCursorVO(ImageSource Image, string BankID, VisualMapTileVO[,] VisualMapTiles);