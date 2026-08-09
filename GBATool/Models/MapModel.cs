using GBATool.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Windows;

namespace GBATool.Models;

public class Tile
{
    public bool FlipHorizontal { get; set; }
    public bool FlipVertical { get; set; }
    public byte PaletteIndex { get; set; }
    public Point TileSetOrigin { get; set; } = default;
    public string TileSetID { get; set; } = string.Empty;
    public string MapID { get; init; } = string.Empty;
    public int CellIndex { get; init; }

    public bool IsEmpty()
    {
        return (TileSetOrigin.X == 0 && TileSetOrigin.Y == 0) || string.IsNullOrEmpty(TileSetID);
    }

    public void Clean()
    {
        TileSetOrigin = default;
        TileSetID = string.Empty;
    }
}

public class MapModel : AFileModel
{
    private const string _extensionKey = "extensionMaps";

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

    [JsonIgnore]
    public const int RegularTileMin = 32 * 32;
    [JsonIgnore]
    public const int RegularTileMax = RegularTileMin * 4;
    [JsonIgnore]
    public const int AffineTileMax = RegularTileMax * 2;

    public MapType MapType { get; set; } = MapType.Regular;
    public Priority Priority { get; set; } = Priority.Highest;
    public BckgrRegularSize BckgrRegularSize { get; set; } = BckgrRegularSize.Regular32x32;
    public BckgrAffineSize BckgrAffineSize { get; set; } = BckgrAffineSize.Affine16x16;
    public Dictionary<string, Tile[]> Tiles { get; set; } = [];
    public List<Tile> AffineTiles { get; set; } = [];
    public bool EnableMosaic { get; set; }
    public bool AffineWrapping { get; set; }
    public string[] PaletteIDs { get; set; } = [.. Enumerable.Repeat(string.Empty, 16)];
    public ScreenBaseBlock ScreenBaseBlock { get; set; } = ScreenBaseBlock.Block0;
    public CharacterBaseBlock CharacterBaseBlock { get; set; } = CharacterBaseBlock.Block0;
    public string BankID { get; set; } = string.Empty;

    public void InsertNewTiles()
    {
        for (int i = 0; i < 4; i++)
        {
            string mapID = Guid.NewGuid().ToString();

            List<Tile> tiles = [];

            for (int j = 0; j < RegularTileMin; j++)
            {
                tiles.Add(new Tile() { MapID = mapID, CellIndex = j });
            }

            Tiles.Add(mapID, [.. tiles]);
        }
    }

    public List<Tile> GetTilesFromRegularBackground(List<(string, int)> indices)
    {
        List<Tile> listOfTiles = [];

        foreach ((string mapID, int index) in indices)
        {
            if (Tiles.TryGetValue(mapID, out Tile[]? tiles))
            {
                if (tiles != null && index >= 0 && index < RegularTileMin)
                {
                    listOfTiles.Add(tiles[index]);
                }
            }
        }

        return listOfTiles;
    }
}
