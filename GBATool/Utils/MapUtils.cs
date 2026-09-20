using GBATool.Enums;
using GBATool.FileSystem;
using GBATool.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GBATool.Utils;

public static class MapUtils
{
    private readonly static ConcurrentDictionary<string, WriteableBitmap> _frameBitmapCache = [];

    public const int CellSize = 8;
    public const int RegularMapMinimumSize = 32;
    public const int AffineMapMinimumSize = 16;

    public static int GetRegularMapSizeWidth(BckgrRegularSize size)
    {
        return size switch
        {
            BckgrRegularSize.Wide or BckgrRegularSize.Big => RegularMapMinimumSize * 2,
            BckgrRegularSize.Small or BckgrRegularSize.Tall or _ => RegularMapMinimumSize
        };
    }

    public static int GetRegularMapSizeHeight(BckgrRegularSize size)
    {
        return size switch
        {
            BckgrRegularSize.Big or BckgrRegularSize.Tall => RegularMapMinimumSize * 2,
            BckgrRegularSize.Small or BckgrRegularSize.Wide or _ => RegularMapMinimumSize,
        };
    }

    public static int GetRegularMapSizeWidthInPixels(BckgrRegularSize size)
    {
        return GetRegularMapSizeWidth(size) * CellSize;
    }

    public static int GetRegularMapSizeHeightInPixels(BckgrRegularSize size)
    {
        return GetRegularMapSizeHeight(size) * CellSize;
    }

    public static int GetAffineMapSize(BckgrAffineSize size)
    {
        return size switch
        {
            BckgrAffineSize.Affine128x128 => AffineMapMinimumSize * 8,
            BckgrAffineSize.Affine64x64 => AffineMapMinimumSize * 4,
            BckgrAffineSize.Affine32x32 => AffineMapMinimumSize * 2,
            BckgrAffineSize.Affine16x16 or _ => AffineMapMinimumSize
        };
    }

    public static int GetAffineMapSizeInPixels(BckgrAffineSize size)
    {
        return GetAffineMapSize(size) * CellSize;
    }

    public static List<int> GetCellsIndicesFromRect(Rect rect, MapModel model)
    {
        static int ClosestMultiple(int number)
        {
            int remainder = number % CellSize;
            int closestMultiple = number - remainder;

            return closestMultiple;
        }

        List<int> indices = [];

        int pointX = (int)rect.Left;
        int pointY = (int)rect.Top;
        int endPointX = (int)rect.Right;
        int endPointY = (int)rect.Bottom;

        int maxSize;
        int sizeHeightInPixels;

        if (model.MapType == MapType.Regular)
        {
            int size = GetRegularMapSizeWidth(model.BckgrRegularSize);

            maxSize = size * size;
            sizeHeightInPixels = GetRegularMapSizeHeightInPixels(model.BckgrRegularSize);
        }
        else
        {
            int size = GetAffineMapSize(model.BckgrAffineSize);

            maxSize = size * size;
            sizeHeightInPixels = GetAffineMapSizeInPixels(model.BckgrAffineSize);
        }

        bool canContinue = true;
        while (canContinue)
        {
            int cellIndex = GetCellIndexFromPoint(new Point(pointX, pointY), model);

            if (cellIndex >= CellSize * maxSize)
            {
                break;
            }

            indices.Add(cellIndex);

            pointX = ClosestMultiple(pointX);

            pointX += CellSize;

            if (pointX >= endPointX)
            {
                pointX = (int)rect.Left;

                pointY = ClosestMultiple(pointY);

                pointY += CellSize;
            }

            if (pointY >= endPointY || pointY >= (sizeHeightInPixels * CellSize))
            {
                canContinue = false;
            }
        }

        return indices;
    }

    public static int GetCellIndexFromPoint(Point point, MapModel model)
    {
        int sizeWidth;

        if (model.MapType == MapType.Regular)
        {
            sizeWidth = GetRegularMapSizeWidth(model.BckgrRegularSize);
        }
        else
        {
            sizeWidth = GetAffineMapSize(model.BckgrAffineSize);
        }

        int cellIndex = ((int)point.X / CellSize) + ((int)point.Y / CellSize * sizeWidth);

        return cellIndex;
    }

    public static Point GetCellPointFromIndex(int cellIndex, MapModel model)
    {
        int sizeWidth;
        int sizeHeight;

        if (model.MapType == MapType.Regular)
        {
            sizeWidth = GetRegularMapSizeWidth(model.BckgrRegularSize);
            sizeHeight = GetRegularMapSizeHeight(model.BckgrRegularSize);
        }
        else
        {
            sizeWidth = GetAffineMapSize(model.BckgrAffineSize);
            sizeHeight = sizeWidth;
        }

        int x = (cellIndex % sizeWidth) * CellSize;
        int y = (cellIndex * CellSize - x) / sizeHeight;

        return new Point(x, y);
    }

    public static void InvalidateImageFromCache(string mapID)
    {
        if (_frameBitmapCache.ContainsKey(mapID))
        {
            _frameBitmapCache.TryRemove(mapID, out WriteableBitmap? _);
        }
    }

    public static WriteableBitmap? GetFrameImageFromCache(MapModel mapModel)
    {
        if (!_frameBitmapCache.TryGetValue(mapModel.MapID, out WriteableBitmap? sourceBitmap))
        {
            sourceBitmap = CreateMap(mapModel);

            if (sourceBitmap == null)
            {
                return null;
            }

            sourceBitmap.Freeze();

            _frameBitmapCache.TryAdd(mapModel.MapID, sourceBitmap);
        }

        return sourceBitmap;
    }

    private static WriteableBitmap CreateMap(MapModel model)
    {
        int matrixSizeWidth;
        int matrixSizeHeight;

        if (model.MapType == MapType.Regular)
        {
            matrixSizeWidth = GetRegularMapSizeWidth(model.BckgrRegularSize);
            matrixSizeHeight = GetRegularMapSizeHeight(model.BckgrRegularSize);
        }
        else
        {
            matrixSizeWidth = GetAffineMapSize(model.BckgrAffineSize);
            matrixSizeHeight = matrixSizeWidth;
        }

        WriteableBitmap mapBitmap = BitmapFactory.New(matrixSizeWidth * CellSize, matrixSizeHeight * CellSize);

        using (mapBitmap.GetBitmapContext())
        {
            foreach (Tile tile in model.RegularMapTiles)
            {
                if (tile.IsEmpty())
                {
                    continue;
                }

                TileSetModel? tileSetModel = ProjectFiles.GetModel<TileSetModel>(tile.TileSetID);

                if (tileSetModel == null)
                {
                    continue;
                }

                (_, WriteableBitmap? tileSetBitmap) = TileSetUtils.GetSourceBitmapFromCache(tileSetModel);

                if (tileSetBitmap == null)
                {
                    continue;
                }

                WriteableBitmap sourceBitmap = tileSetBitmap.CloneCurrentValue();

                WriteableBitmap cropped = sourceBitmap.Crop((int)tile.TileSetOrigin.X, (int)tile.TileSetOrigin.Y, CellSize, CellSize);

                int x = (tile.CellIndex % matrixSizeWidth) * CellSize;
                int y = (tile.CellIndex / matrixSizeHeight) * CellSize;

                Util.CopyBitmapImageToWriteableBitmap(ref mapBitmap, x, y, cropped);
            }
        }

        return mapBitmap;
    }
}
