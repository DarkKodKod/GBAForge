using ArchitectureLibrary.Signals;
using ArchitectureLibrary.Utils;
using GBATool.Enums;
using GBATool.Signals;
using GBATool.Utils;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GBATool.Models;

public class TileSetModel : AFileModel
{
    public static readonly int ImageScale = 4;

    private const string _extensionKey = "extensionTileSets";

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

    public string ImagePath { get; set; } = string.Empty;
    public int ImageWidth { get; set; } = 0;
    public int ImageHeight { get; set; } = 0;
    public List<SpriteModel> Sprites { get; set; } = [];

    public TileSetModel()
    {
        SignalManager.Get<ProjectItemLoadedSignal>().Listener += OnProjectItemLoaded;
    }

    private void OnProjectItemLoaded(string id)
    {
        if (id != GUID)
        {
            return;
        }

        _ = TileSetUtils.GetSourceBitmapFromCache(this);
    }

    public static WriteableBitmap? LoadBitmap(TileSetModel tileSetModel, bool forceRedraw = false)
    {
        (_, WriteableBitmap? bitmap) = TileSetUtils.GetSourceBitmapFromCache(tileSetModel, forceRedraw);

        return bitmap;
    }

    public bool RemoveSprite(string spriteID)
    {
        if (spriteID != null)
        {
            if (Sprites.RemoveAll((item) => item.ID == spriteID) > 0)
            {
                return true;
            }
        }

        return false;
    }

    public bool RenameAliasSprite(string? spriteID, string newName)
    {
        if (string.IsNullOrEmpty(spriteID))
            return false;

        SpriteModel? find = Sprites.Find((item) => item.ID == spriteID);

        if (string.IsNullOrEmpty(find?.ID))
        {
            return false;
        }

        find.Alias = newName;

        return true;
    }

    public bool StoreNewSprite(string spriteID, int posX, int posY, SpriteShape shape, SpriteSize size, string tileSetID, int width, int height)
    {
        // if the size and shape is known then there is no need to store the values
        if (shape != SpriteShape.Custom && size != SpriteSize.Custom)
        {
            width = 0;
            height = 0;
        }

        SpriteModel newSprite = new()
        {
            ID = spriteID,
            Shape = shape,
            Size = size,
            PosX = posX,
            PosY = posY,
            TileSetID = tileSetID,
            Alias = spriteID,
            Width = width,
            Height = height,
        };

        SpriteModel? find = Sprites.Find((item) => item == newSprite);

        if (string.IsNullOrEmpty(find?.ID))
        {
            Sprites.Add(newSprite);

            return true;
        }

        return false;
    }

    public bool GetSpriteIndex(Rectangle<int> rect, out int spriteIndex)
    {
        spriteIndex = -1;

        // TODO:
//        for (int i = 0; i < Sprites.Count; i++)
//        {
//            SpriteModel sprite = Sprites[i];
//
//            if (sprite.ID == spriteID)
//            {
//                spriteIndex = i;
//                return true;
//            }
//        }

        return false;
    }
}
