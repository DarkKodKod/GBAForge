using GBATool.Enums;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GBATool.Utils.Image;

using TileBlocks = (int width, int height, int numberOfTiles);

// Information taken from: https://sneslab.net/wiki/Graphics_Format
public static class ImageProcessing
{
    public static byte[]? ConvertToXbpp(BitsPerPixel bpp, WriteableBitmap bitmap, in TileBlocks cellsCount, in List<Color> palette, ref List<string> warnings)
    {
        int bitsPerPixel = (int)bpp;
        const int bitPlaneSizeInBytes = 8; // 8x8 bits in a tile

        int bufferSize = (bitsPerPixel * bitPlaneSizeInBytes) * cellsCount.numberOfTiles;

        byte[] bytes = new byte[bufferSize];
        Array.Fill<byte>(bytes, 0);

        int pixelIndex = 0;
        int currentX = 0;
        int currentY = 0;
        int countingTiles = 0;

        for (int j = 0; j < cellsCount.height; ++j)
        {
            for (int i = 0; i < cellsCount.width; ++i)
            {
                if (countingTiles >= cellsCount.numberOfTiles)
                {
                    goto done;
                }

                // read pixels in the 8x8 quadrant
                for (int y = currentY; y < currentY + 8; ++y)
                {
                    for (int x = currentX; x < currentX + 8; ++x)
                    {
                        Color color = bitmap.GetPixel(x, y);
                        color.A = 255;

                        int colorIndex = 0;
                        bool colorFoundInPalette = false;
                        foreach (Color paletteColor in palette)
                        {
                            if (paletteColor.Equals(color))
                            {
                                switch (bpp)
                                {
                                    case BitsPerPixel.f4bpp:
                                        Set4BitAccordingToIndex(pixelIndex, colorIndex, ref bytes);
                                        break;
                                    case BitsPerPixel.f8bpp:
                                        Set8BitAccordingToIndex(pixelIndex, colorIndex, ref bytes);
                                        break;
                                    case BitsPerPixel.f1bpp:
                                        Set1BitAccordingToIndex(pixelIndex, colorIndex, ref bytes);
                                        break;
                                }

                                colorFoundInPalette = true;
                                break;
                            }

                            colorIndex++;
                        }

                        if (!colorFoundInPalette)
                        {
                            warnings.Add("Color in the bank is not found in the given palette");
                        }

                        pixelIndex++;
                    }
                }

                currentX += 8;
                countingTiles++;
            }

            currentX = 0;
            currentY += 8;
        }

        done:
        return bytes;
    }

    private static void Set4BitAccordingToIndex(int pixelIndex, int colorIndex, ref byte[] output)
    {
        int outputIndex = (pixelIndex * 4) / 8;

        byte b = (byte)colorIndex;

        Math.DivRem(pixelIndex, 2, out int reminder);

        byte store = (byte)(reminder == 0 ? b : b << 4);

        output[outputIndex] |= store;
    }

    private static void Set8BitAccordingToIndex(int pixelIndex, int colorIndex, ref byte[] output)
    {
        output[pixelIndex] = (byte)colorIndex;
    }

    private static void Set1BitAccordingToIndex(int pixelIndex, int colorIndex, ref byte[] output)
    {
        int outputIndex = pixelIndex / 8;

        byte store = (byte)(colorIndex << (pixelIndex % 8));

        output[outputIndex] |= store;
    }
}
