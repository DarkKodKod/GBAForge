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
    public const int RegularMapSizeWidth = 32;
    public const int AffineMapSizeWidth = 16;
    public const int RegularMapSizeInPixels = RegularMapSizeWidth * CellSize;
    public const int AffineMapSizeInPixels = AffineMapSizeWidth * CellSize;

    public static List<int> GetCellsIndicesFromRect(Rect rect, BckgrRegularSize size)
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

        int maxSize = size switch
        {
            BckgrRegularSize.Small => RegularMapSizeWidth * RegularMapSizeWidth,
            _ => 0,
        };

        bool canContinue = true;
        while (canContinue)
        {
            int cellIndex = GetCellIndexFromPoint(new Point(pointX, pointY), size);

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

            if (pointY >= endPointY || pointY >= RegularMapSizeInPixels)
            {
                canContinue = false;
            }
        }

        return indices;
    }

    public static int GetCellIndexFromPoint(Point point, BckgrRegularSize size)
    {
        int sizeMatrix = size switch
        {
            BckgrRegularSize.Small => RegularMapSizeWidth,
            _ => 0,
        };

        int cellIndex = ((int)point.X / CellSize) + ((int)point.Y / CellSize * sizeMatrix);

        return cellIndex;
    }

    public static Point GetCellPointFromIndex(int cellIndex)
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
        int matrixSize = model.BckgrRegularSize switch
        {
            BckgrRegularSize.Small => RegularMapSizeInPixels,
            _ => 0
        };

        WriteableBitmap mapBitmap = BitmapFactory.New(matrixSize, matrixSize);

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

                int x = (tile.CellIndex % RegularMapSizeWidth) * CellSize;
                int y = (tile.CellIndex / RegularMapSizeWidth) * CellSize;

                Util.CopyBitmapImageToWriteableBitmap(ref mapBitmap, x, y, cropped);
            }
        }

        return mapBitmap;
    }

    public static void FloodFill(int[,] matrix, int startX, int startY, int newValue)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        // Obtener el valor original de la celda donde se hizo click
        int targetValue = matrix[startX, startY];

        // Si el valor objetivo es igual al nuevo valor, no hace falta hacer nada
        if (targetValue == newValue)
        {
            return;
        }

        // Cola para almacenar las posiciones a procesar (X, Y)
        Queue<(int, int)> queue = [];

        // Agregar la celda inicial a la cola
        queue.Enqueue((startX, startY));

        // Direcciones para moverse: Arriba, Abajo, Izquierda, Derecha
        int[] rowOffsets = { -1, 1, 0, 0 };
        int[] colOffsets = { 0, 0, -1, 1 };

        while (queue.Count > 0)
        {
            (int currX, int currY) = queue.Dequeue();

            // Si la celda actual tiene el valor buscado, la cambiamos
            if (matrix[currX, currY] == targetValue)
            {
                matrix[currX, currY] = newValue;

                // Revisar los 4 vecinos adyacentes
                for (int i = 0; i < 4; i++)
                {
                    int newX = currX + rowOffsets[i];
                    int newY = currY + colOffsets[i];

                    // Validar que el vecino esté dentro de los límites de la matriz
                    if (newX >= 0 && newX < rows && newY >= 0 && newY < cols)
                    {
                        // Solo encolar si el vecino tiene el valor original que queremos reemplazar
                        if (matrix[newX, newY] == targetValue)
                        {
                            queue.Enqueue((newX, newY));
                        }
                    }
                }
            }
        }
    }
}
