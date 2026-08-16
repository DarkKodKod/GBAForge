using ArchitectureLibrary.Model;
using GBATool.Enums;
using GBATool.FileSystem;
using GBATool.Utils;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Windows;

namespace GBATool.Models;

using TileBlocks = (int width, int height, int numberOfTiles);

public record SpriteRef
{
    public string? SpriteID { get; set; }
    public string? TileSetID { get; set; }
}

public class BankModel : AFileModel
{
    public static ConcurrentDictionary<string, BankImageMetaData> MetaDataCache { get; } = [];

    private const string _extensionKey = "extensionBanks";

    [JsonIgnore]
    public override string FileExtension
    {
        get
        {
            if (string.IsNullOrEmpty(_fileExtension))
            {
                _fileExtension = (string)Application.Current.FindResource(_extensionKey);
            }

            return _fileExtension;
        }
    }

    public BitsPerPixel BitsPerPixel { get; set; } = BitsPerPixel.f4bpp;
    public bool IsBackground { get; set; }
    public int TransparentColor { get; set; } = 0;
    public List<SpriteRef> Sprites { get; set; } = [];

    [JsonIgnore]
    public bool IsFull { get; private set; }

    public bool GetTileIndex(string spriteID, ref int index)
    {
        ProjectModel projectModel = ModelManager.Get<ProjectModel>();

        bool is1DPattern = IsBackground || (projectModel.SpritePatternFormat == SpritePattern.Format1D);

        int tilesHeight = 1;
        int acuWidth = 0;
        int acuHeight = 0;

        foreach (SpriteRef sprite in Sprites)
        {
            if (string.IsNullOrEmpty(sprite.TileSetID))
            {
                continue;
            }

            if (sprite.SpriteID == spriteID)
            {
                return true;
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

            if (is1DPattern)
            {
                // iterate over all the sprites and it counts how many tiles each sprite has
                // and when the spriteID is found, it returns the amount of tiles accumulated
                // until that very sprite

                index += SpriteUtils.Count8x8Tiles(sm.Shape, sm.Size);
            }
            // 2D pattern
            else
            {
                int width = 0;
                int height = 0;
                SpriteUtils.ConvertToWidthHeight(sm.Shape, sm.Size, ref width, ref height);

                width /= BankUtils.SizeOfCellInPixels;
                height /= BankUtils.SizeOfCellInPixels;

                calculate:
                if (acuWidth + width <= BankUtils.MaxTextureCellsWidth)
                {
                    acuWidth += width;

                    if (acuHeight < height)
                    {
                        acuHeight = height;
                    }
                }
                else
                {
                    tilesHeight += acuHeight;
                    acuWidth = 0;
                    acuHeight = 0;

                    goto calculate;
                }

                index = acuWidth * tilesHeight;
            }
        }

        return false;
    }

    public (bool, string) RegisterSprite(SpriteModel sprite)
    {
        SpriteRef? find = Sprites.Find((spriteRef) => spriteRef.SpriteID == sprite.ID);

        if (!string.IsNullOrEmpty(find?.SpriteID))
        {
            return (false, "This sprite is already in the bank");
        }

        Sprites.Add(new SpriteRef() { SpriteID = sprite.ID, TileSetID = sprite.TileSetID });

        return (true, "");
    }

    public bool RemoveSprite(SpriteModel sprite)
    {
        SpriteRef? find = Sprites.Find((spriteRef) => spriteRef.SpriteID == sprite.ID);

        if (string.IsNullOrEmpty(find?.SpriteID))
        {
            return false;
        }

        return Sprites.Remove(new SpriteRef() { SpriteID = sprite.ID, TileSetID = sprite.TileSetID });
    }

    public void ReplaceSpriteList(List<SpriteRef> list)
    {
        Sprites = list;
    }

    public void CleanUpDeletedSprites()
    {
        List<SpriteRef> idsToRemove = [];

        foreach (SpriteRef spriteRef in Sprites)
        {
            if (spriteRef == null ||
                string.IsNullOrEmpty(spriteRef.SpriteID) ||
                string.IsNullOrEmpty(spriteRef.TileSetID))
                continue;

            TileSetModel? tileSetModel = ProjectFiles.GetModel<TileSetModel>(spriteRef.TileSetID);

            if (tileSetModel == null)
            {
                idsToRemove.Add(spriteRef);
                continue;
            }

            SpriteModel? sprite = tileSetModel.Sprites.Find((item) => item.ID == spriteRef.SpriteID);

            if (string.IsNullOrEmpty(sprite?.ID))
            {
                idsToRemove.Add(spriteRef);
            }
        }

        foreach (SpriteRef spriteToRemove in idsToRemove)
        {
            Sprites.Remove(spriteToRemove);
        }
    }

    public TileBlocks GetBoundingBoxSize()
    {
        ProjectModel projectModel = ModelManager.Get<ProjectModel>();

        bool is1DImage = IsBackground || projectModel.SpritePatternFormat == SpritePattern.Format1D;

        int tilesWidth = 1;
        int tilesHeight = 1;
        int countTiles = 0;
        int acuWidth = 0;
        int acuHeight = 0;

        foreach (SpriteRef spriteRef in Sprites)
        {
            if (spriteRef == null ||
                string.IsNullOrEmpty(spriteRef.SpriteID) ||
                string.IsNullOrEmpty(spriteRef.TileSetID))
                continue;

            TileSetModel? tileSetModel = ProjectFiles.GetModel<TileSetModel>(spriteRef.TileSetID);

            if (tileSetModel == null)
                continue;

            SpriteModel? sprite = tileSetModel.Sprites.Find((item) => item.ID == spriteRef.SpriteID);

            if (string.IsNullOrEmpty(sprite?.ID))
                continue;

            if (is1DImage)
            {
                if (sprite.Shape == SpriteShape.Custom || sprite.Size == SpriteSize.Custom)
                {
                    countTiles += (sprite.Width / 8) * (sprite.Height / 8);
                }
                else
                {
                    countTiles += SpriteUtils.Count8x8Tiles(sprite.Shape, sprite.Size);
                }
            }
            else
            {
                int width = 0;
                int height = 0;

                if (sprite.Shape == SpriteShape.Custom || sprite.Size == SpriteSize.Custom)
                {
                    width = sprite.Width;
                    height = sprite.Height;
                }
                else
                {
                    SpriteUtils.ConvertToWidthHeight(sprite.Shape, sprite.Size, ref width, ref height);
                }

                width /= BankUtils.SizeOfCellInPixels;
                height /= BankUtils.SizeOfCellInPixels;

                calculate:
                if (acuWidth + width <= BankUtils.MaxTextureCellsWidth)
                {
                    acuWidth += width;

                    if (acuHeight < height)
                    {
                        acuHeight = height;
                    }
                    if (acuWidth > tilesWidth)
                    {
                        tilesWidth = acuWidth;
                    }
                }
                else
                {
                    tilesHeight += acuHeight;
                    acuWidth = 0;
                    acuHeight = 0;

                    goto calculate;
                }
            }
        }

        if (acuHeight > 0)
        {
            tilesHeight += acuHeight;
        }

        if (is1DImage)
        {
            // 1D is a continous array of tiles and if this array is bigger than the max texture size
            // then it will add in height as much it is required to fill it with all the tiles

            tilesWidth = countTiles < BankUtils.MaxTextureCellsWidth ? countTiles : BankUtils.MaxTextureCellsWidth;

            tilesHeight = (int)Util.BankRowsRound(countTiles / (double)BankUtils.MaxTextureCellsWidth);
        }
        else
        {
            countTiles = tilesWidth * tilesHeight;
        }

        return (tilesWidth, tilesHeight, countTiles);
    }
}
