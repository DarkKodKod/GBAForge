using GBATool.FileSystem;
using GBATool.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GBATool.Utils;

public static class MapUtils
{
    private readonly static ConcurrentDictionary<string, WriteableBitmap> _frameBitmapCache = [];

    public readonly static int CellSize = 8;
    public readonly static int RegularMapSizeWidth = 32;
    public readonly static int AffineMapSizeWidth = 16;
    public readonly static int RegularMapSizeInPixels = RegularMapSizeWidth * CellSize;
    public readonly static int AffineMapSizeInPixels = AffineMapSizeWidth * CellSize;

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
            (int _, int cellIndex) = GetCellIndexFromPoint(new Point(pointX, pointY));

            if (cellIndex >= 1024)
            {
                break;
            }

            indices.Add(cellIndex);

            pointX += CellSize;

            if (pointX > endPointX)
            {
                pointX = rect.Left;
                pointY += CellSize;
            }

            if (pointY > endPointY || pointY >= RegularMapSizeInPixels)
            {
                canContinue = false;
            }
        }

        return indices;
    }

    public static (int mapIndex, int cellIndex) GetCellIndexFromPoint(Point point)
    {
        // TODO: calculate the map index based on the input point
        int mapIndex = 0;

        int cellIndex = ((int)point.X / CellSize) + ((int)point.Y / CellSize * RegularMapSizeWidth);

        return (mapIndex, cellIndex);
    }

    public static Point GetCellPointFromIndex(int cellIndex, int mapIndex)
    {
        int x = (cellIndex % RegularMapSizeWidth) * CellSize;
        int y = (cellIndex * CellSize - x) / RegularMapSizeWidth;

        return new Point(x, y);
    }

    public static void InvalidateImageFromCache(string mapID)
    {
        if (_frameBitmapCache.ContainsKey(mapID))
        {
            _frameBitmapCache.TryRemove(mapID, out WriteableBitmap? _);
        }
    }

    public static WriteableBitmap? GetFrameImageFromCache(MapModel mapModel, string mapID)
    {
        if (!_frameBitmapCache.TryGetValue(mapID, out WriteableBitmap? sourceBitmap))
        {
            sourceBitmap = CreateMap(mapModel, mapID);

            if (sourceBitmap == null)
            {
                return null;
            }

            sourceBitmap.Freeze();

            _frameBitmapCache.TryAdd(mapID, sourceBitmap);
        }

        return sourceBitmap;
    }

    private static WriteableBitmap? CreateMap(MapModel model, string mapID, bool createNew = true)
    {
        WriteableBitmap? mapBitmap = null;

        if (createNew)
        {
            mapBitmap = BitmapFactory.New(RegularMapSizeInPixels, RegularMapSizeInPixels);
        }

        if (mapBitmap == null) 
        {
            return null;
        }

        using (mapBitmap.GetBitmapContext())
        {
            RegularMap? regularMap = model.RegularMap.SingleOrDefault(rm => rm.MapID == mapID);

            if (regularMap == null)
            {
                return null;
            }

            foreach (Tile tile in regularMap.Tiles)
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
        
                int destX = (int)Math.Floor(tile.TileSetOrigin.X / CellSize) * CellSize;
                int destY = (int)Math.Floor(tile.TileSetOrigin.Y / CellSize) * CellSize;
        
                Util.CopyBitmapImageToWriteableBitmap(ref mapBitmap, destX, destY, cropped);
            }
        }

        return mapBitmap;
    }
}
