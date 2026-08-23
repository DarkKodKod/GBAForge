using ArchitectureLibrary.Model;
using GBATool.Enums;
using GBATool.FileSystem;
using GBATool.Models;
using GBATool.VOs;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GBATool.Utils;

public class SpriteInfo
{
    public BitmapSource? BitmapSource { get; set; }
    public int OffsetX { get; set; } = 0;
    public int OffsetY { get; set; } = 0;
}

public class TileInfo
{
    public int TileIndex { get; set; } = 0;
    public string SpriteID { get; set; } = string.Empty;
    public string TilesetID { get; set; } = string.Empty;
    public string BitmapHash { get; set; } = string.Empty;
    public Point OriginInTileset { get; set; } = new Point(0, 0);
}

public class BankImageMetaData
{
    /// <value>
    /// Property <c>image</c> represents the bank image.
    /// </value>
    public WriteableBitmap? Image { get; set; }
    /// <value>
    /// Property <c>IndividualTileInfo</c> Is the list of each sprites, its tilesetID and what is its index in the bank.
    /// <para/>
    /// The Tuple represent this values: (tile index, spriteID, tilesetID, 8x8 pixels hash)
    /// <para/>
    /// </value>
    public List<TileInfo> IndividualTileInfo { get; set; } = [];
    /// <value>
    /// Property <c>UniqueTileSet</c> List of tileset used by the bank, it is called unique tileset because each tileset ID appears only once.
    /// </value>
    public List<string> UniqueTileSet { get; set; } = [];
    /// <value>
    /// Property <c>BankSprites</c> The list of spritemodels that are part of the bank.
    /// </value>
    public List<SpriteModel> BankSprites { get; set; } = [];
    /// <value>
    /// Property <c>Sprites</c> Is the list of the individual sprite information for each of the SpriteModels, it is indexed by the SpriteModel ID.
    /// </value>
    public Dictionary<string, SpriteInfo> Sprites { get; set; } = [];
}

public static class BankUtils
{
    public const int SizeOfCellInPixels = 8;
    public const int MaxTextureCellsWidth = 32;

    public static BankImageMetaData CreateImage(BankModel bankModel, bool foce2DView, int canvasWidth, int canvasHeight)
    {
        BankImageMetaData metaData = new();

        WriteableBitmap bankBitmap = BitmapFactory.New(canvasWidth, canvasHeight);

        ProjectModel projectModel = ModelManager.Get<ProjectModel>();

        int index = 0;

        bool is1DImage = !foce2DView && (bankModel.IsBackground || (projectModel.SpritePatternFormat == SpritePattern.Format1D));

        int widthNextPosition = 0;
        int heightNextPosition = 0;
        int keepHeightPosition = 0;

        foreach (SpriteRef spriteRef in bankModel.Sprites)
        {
            if (string.IsNullOrEmpty(spriteRef.SpriteID) || string.IsNullOrEmpty(spriteRef.TileSetID))
            {
                continue;
            }

            TileSetModel? tileSetModel = ProjectFiles.GetModel<TileSetModel>(spriteRef.TileSetID);

            if (tileSetModel == null)
            {
                continue;
            }

            WriteableBitmap? sourceBitmapCached = TileSetUtils.GetSourceBitmapFromCacheWithMetaData(tileSetModel, ref metaData);

            if (sourceBitmapCached == null)
            {
                continue;
            }

            WriteableBitmap sourceBitmap = sourceBitmapCached.CloneCurrentValue();

            SpriteModel? sprite = tileSetModel.Sprites.Find((item) => item.ID == spriteRef.SpriteID);

            if (string.IsNullOrEmpty(sprite?.ID))
            {
                continue;
            }

            metaData.BankSprites.Add(sprite);

            int width = 0;
            int height = 0;

            SpriteUtils.ConvertToWidthHeight(sprite.Shape, sprite.Size, ref width, ref height);

            if (width == 0 || height == 0)
            {
                width = sprite.Width;
                height = sprite.Height;
            }

            if (widthNextPosition + width > canvasWidth)
            {
                widthNextPosition = 0;
                heightNextPosition += keepHeightPosition;
                keepHeightPosition = 0;

                // In case the next sprite is going to be place outside of the destinated bank size.
                if (heightNextPosition + height > canvasHeight)
                {
                    break;
                }
            }

            int posX = sprite.PosX;
            int posY = sprite.PosY;

            // Keep the sprite as a separated image in a cache
            metaData.Sprites.Add(
                sprite.ID,
                new()
                {
                    BitmapSource = sourceBitmap.Crop(posX, posY, width, height),
                    OffsetX = widthNextPosition,
                    OffsetY = heightNextPosition
                });

            if (is1DImage)
            {
                for (int j = 0; j < (height / SizeOfCellInPixels); ++j)
                {
                    for (int i = 0; i < (width / SizeOfCellInPixels); ++i)
                    {
                        WriteableBitmap cropped = sourceBitmap.Crop(posX, posY, SizeOfCellInPixels, SizeOfCellInPixels);

                        int destX = index % MaxTextureCellsWidth * SizeOfCellInPixels;
                        int destY = index / MaxTextureCellsWidth * SizeOfCellInPixels;

                        Util.CopyBitmapImageToWriteableBitmap(ref bankBitmap, destX, destY, cropped);

                        metaData.IndividualTileInfo.Add(
                            new()
                            {
                                TileIndex = index,
                                SpriteID = sprite.ID,
                                TilesetID = sprite.TileSetID,
                                BitmapHash = HashTile(cropped),
                                OriginInTileset = new Point(posX, posY)
                            });

                        posX += SizeOfCellInPixels;

                        index++;
                    }

                    posX = sprite.PosX;
                    posY += SizeOfCellInPixels;
                }
            }
            else
            {
                index = (MaxTextureCellsWidth * (heightNextPosition / SizeOfCellInPixels)) + (widthNextPosition / SizeOfCellInPixels);

                // 2D
                for (int j = 0; j < (height / SizeOfCellInPixels); ++j)
                {
                    for (int i = 0; i < (width / SizeOfCellInPixels); ++i)
                    {
                        WriteableBitmap cropped = sourceBitmap.Crop(posX, posY, SizeOfCellInPixels, SizeOfCellInPixels);

                        int destX = i * SizeOfCellInPixels;
                        int destY = j * SizeOfCellInPixels;

                        Util.CopyBitmapImageToWriteableBitmap(ref bankBitmap, destX + widthNextPosition, destY + heightNextPosition, cropped);

                        metaData.IndividualTileInfo.Add(
                            new()
                            {
                                TileIndex = index,
                                SpriteID = sprite.ID,
                                TilesetID = sprite.TileSetID,
                                BitmapHash = HashTile(cropped),
                                OriginInTileset = new Point(posX, posY)
                            });

                        posX += SizeOfCellInPixels;

                        index++;
                    }

                    index += MaxTextureCellsWidth - (width / SizeOfCellInPixels);

                    posX = sprite.PosX;
                    posY += SizeOfCellInPixels;
                }

                widthNextPosition += width;

                if (height > keepHeightPosition)
                {
                    keepHeightPosition = height;
                }
            }
        }

        metaData.Image = bankBitmap;

        return metaData;
    }

    public static string HashTile(WriteableBitmap bitmapSource)
    {
        int stride = (bitmapSource.PixelWidth * bitmapSource.Format.BitsPerPixel + 7) / 8;

        int size = stride * bitmapSource.PixelHeight;

        byte[] pixels = new byte[size];

        bitmapSource.CopyPixels(pixels, stride, 0);

        byte[] hashBytes = SHA256.HashData(pixels);

        return Convert.ToHexString(hashBytes);
    }

    public static void GenerateTemporalPalette(BankModel bankModel, ref List<Color> palette)
    {
        List<SpriteModel> spriteModels = [];

        foreach (SpriteRef sprite in bankModel.Sprites)
        {
            if (string.IsNullOrEmpty(sprite.TileSetID))
            {
                continue;
            }

            TileSetModel? tileSetModel = ProjectFiles.GetModel<TileSetModel>(sprite.TileSetID);

            if (tileSetModel == null)
            {
                continue;
            }

            SpriteModel? sm = tileSetModel.Sprites.Find(x => x.ID == sprite.SpriteID);

            if (sm == null)
            {
                continue;
            }

            spriteModels.Add(sm);
        }

        palette = PaletteUtils.GeneratePaletteColorList(
            spriteModels,
            PaletteUtils.GetColorFromInt(bankModel.TransparentColor),
            bankModel.BitsPerPixel);
    }

    public static bool GetPaletteIfExistInCharacters(BankModel bankModel, ref List<Color> palette)
    {
        List<FileModelVO> models = ProjectFiles.GetModels<CharacterModel>();

        foreach (FileModelVO fileModel in models)
        {
            if (fileModel.Model is not CharacterModel character)
            {
                continue;
            }

            if (string.IsNullOrEmpty(character.PaletteID))
            {
                continue;
            }

            foreach (KeyValuePair<string, CharacterAnimation> animation in character.Animations)
            {
                foreach (KeyValuePair<string, FrameModel> frame in animation.Value.Frames)
                {
                    if (frame.Value.BankID == bankModel.GUID)
                    {
                        PaletteModel? paletteModel = ProjectFiles.GetModel<PaletteModel>(character.PaletteID);

                        if (paletteModel != null)
                        {
                            if (paletteModel.LinkedPalettes.Count > 0)
                            {
                                foreach (string id in paletteModel.LinkedPalettes)
                                {
                                    PaletteModel? model = ProjectFiles.GetModel<PaletteModel>(id);

                                    model?.GetColors(ref palette);
                                }
                            }
                            else
                            {
                                paletteModel.GetColors(ref palette);
                            }

                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }
}
