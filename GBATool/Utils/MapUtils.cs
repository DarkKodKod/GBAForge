using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GBATool.Utils;

public static class MapUtils
{
    private readonly static ConcurrentDictionary<string, WriteableBitmap> _frameBitmapCache = [];

    public readonly static int CellSize = 8;
    public readonly static int MapSizeWidth = 32;

    public static List<int> GetCellsIndicesFromRect(Rect rect)
    {
        List<int> indices = [];

        double pointX = rect.Left;
        double pointY = rect.Top;
        double endPointX = rect.Right;
        double endPointY = rect.Bottom;

        bool canContinue = true;
        while (canContinue)
        {
            int index = GetCellIndexFromPoint(new Point(pointX, pointY));

            indices.Add(index);

            pointX += CellSize;

            if (pointX > endPointX)
            {
                pointX = rect.Left;
                pointY += CellSize;
            }

            if (pointY > endPointY)
            {
                canContinue = false;
            }
        }

        return indices;
    }

    public static int GetCellIndexFromPoint(Point point)
    {
        int cellIndex = ((int)point.X / CellSize) + ((int)point.Y / CellSize * MapSizeWidth);

        return cellIndex;
    }

    public static Point GetCellPointFromIndex(int cellIndex, int mapIndex)
    {
        int x = (cellIndex % MapSizeWidth) * CellSize;
        int y = (cellIndex * CellSize - x) / MapSizeWidth;

        return new Point(x, y);
    }

    public static void InvalidateImageFromCache(string mapID)
    {
        if (_frameBitmapCache.ContainsKey(mapID))
        {
            _frameBitmapCache.TryRemove(mapID, out WriteableBitmap? _);
        }
    }
}
