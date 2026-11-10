using ArchitectureLibrary.Utils;
using GBATool.Enums;
using GBATool.FileSystem;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;

namespace GBATool.Models;

public class CharacterCollision
{
    public string ID { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public int PosX { get; set; }
    public int PosY { get; set; }
    public Color Color { get; set; }
    public int Mask { get; set; }
    public int CustomMask { get; set; }
}

public class CharacterSprite
{
    public string ID { get; set; } = string.Empty;
    public Point Position { get; set; }
    public bool FlipHorizontal { get; set; }
    public bool FlipVertical { get; set; }
    public string SpriteID { get; set; } = string.Empty;
    public string TileSetID { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public bool EnableMosaic { get; set; }
    public GraphicMode GraphicMode { get; set; }
    public ObjectMode ObjectMode { get; set; }
}

public class FrameModel
{
    public string ID { get; set; } = string.Empty;
    public Dictionary<string, CharacterSprite> Tiles { get; set; } = [];
    public string BankID { get; set; } = string.Empty;
    public Dictionary<string, CharacterCollision> CollisionInfo { get; set; } = [];
    public bool IsHeldFrame { get; set; } = false;
}

public class CharacterAnimation
{
    public string ID { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public float Speed { get; set; }
    public int Repeat { get; set; } = 1;
    public bool Looping { get; set; } = true;
    public Dictionary<string, FrameModel> Frames { get; set; } = [];

    /// <summary>
    /// Returns the bounding fox for a frame
    /// </summary>
    public Rectangle<double> GetFrameBoundingBox(string frameID)
    {
        if (string.IsNullOrEmpty(frameID))
        {
            return Rectangle<double>.Empty;
        }

        double minX = double.MaxValue;
        double minY = double.MaxValue;
        double maxX = double.MinValue;
        double maxY = double.MinValue;

        foreach (KeyValuePair<string, CharacterSprite> item in Frames[frameID].Tiles)
        {
            double tileX = item.Value.Position.X;
            double tileY = item.Value.Position.Y;

            if (tileX < minX) minX = tileX;
            if (tileY < minY) minY = tileY;
            if (tileX > maxX) maxX = tileX + item.Value.Width;
            if (tileY > maxY) maxY = tileY + item.Value.Height;
        }

        return new Rectangle<double>(minX, minY, maxX - minX, maxY - minY);
    }
}

public class CharacterModel : AFileModel
{
    private const string _extensionKey = "extensionCharacters";

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

    public Point RelativeOrigin { get; set; }
    public int VerticalAxis { get; set; }
    public int SpriteBase { get; set; }
    public string PaletteID { get; set; } = string.Empty;
    public int PaletteIndex { get; set; }
    public Priority Priority { get; set; } = Priority.Highest;
    public Dictionary<string, CharacterAnimation> Animations { get; set; } = [];

    public bool CleanUp()
    {
        bool somethingRemoved = false;

        foreach (KeyValuePair<string, CharacterAnimation> animation in Animations)
        {
            foreach (KeyValuePair<string, FrameModel> frame in animation.Value.Frames)
            {
                FrameModel frameModel = frame.Value;

                BankModel? bankModel = ProjectFiles.GetModel<BankModel>(frameModel.BankID);

                if (bankModel == null)
                {
                    continue;
                }

                List<string> idsToRemove = [];

                foreach (KeyValuePair<string, CharacterSprite> tile in frameModel.Tiles)
                {
                    CharacterSprite sprite = tile.Value;

                    int index = 0;

                    if (!bankModel.GetTileIndex(sprite.SpriteID, ref index))
                    {
                        // this sprite does not exist in the bank anymore
                        idsToRemove.Add(tile.Key);
                    }
                }

                foreach (string id in idsToRemove)
                {
                    _ = frameModel.Tiles.Remove(id);
                }

                somethingRemoved |= idsToRemove.Count > 0;
            }
        }

        return somethingRemoved;
    }
}
