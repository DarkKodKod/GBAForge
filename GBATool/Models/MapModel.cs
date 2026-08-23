using GBATool.Enums;
using GBATool.Utils;
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
    public string BankID { get; set; } = string.Empty;
    public int CellIndex { get; init; }

    public bool IsEmpty()
    {
        return TileSetOrigin.X == 0 &&
            TileSetOrigin.Y == 0 &&
            string.IsNullOrEmpty(TileSetID) &&
            string.IsNullOrEmpty(BankID);
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
    public const int RegularTileMinSize = MapUtils.RegularMapSizeWidth * MapUtils.RegularMapSizeWidth;
    [JsonIgnore]
    public const int NumberOfBackgrounds = 4;
    [JsonIgnore]
    public const int RegularTileMaxSize = RegularTileMinSize * NumberOfBackgrounds;
    [JsonIgnore]
    public const int AffineTileMaxSize = RegularTileMaxSize * 2;

    public string MapID { get; private set; } = string.Empty;
    public MapType MapType { get; set; } = MapType.Regular;
    public Priority Priority { get; set; } = Priority.Highest;
    public BckgrRegularSize BckgrRegularSize { get; set; } = BckgrRegularSize.Small;
    public BckgrAffineSize BckgrAffineSize { get; set; } = BckgrAffineSize.Affine16x16;
    public List<Tile> RegularMapTiles { get; set; } = [];
    public List<Tile> AffineMapTiles { get; set; } = [];
    public bool EnableMosaic { get; set; }
    public bool AffineWrapping { get; set; }
    public string[] PaletteIDs { get; set; } = [.. Enumerable.Repeat(string.Empty, 16)];
    public ScreenBaseBlock ScreenBaseBlock { get; set; } = ScreenBaseBlock.Block0;
    public CharacterBaseBlock CharacterBaseBlock { get; set; } = CharacterBaseBlock.Block0;
    public string BankID { get; set; } = string.Empty;

    public void CreateNewRegularMap()
    {
        MapID = Guid.NewGuid().ToString();

        List<Tile> tiles = [];

        int totalSizeOfFourBackgrounds = RegularTileMinSize * NumberOfBackgrounds;

        for (int j = 0; j < totalSizeOfFourBackgrounds; j++)
        {
            tiles.Add(new() { CellIndex = j });
        }

        RegularMapTiles = [.. tiles];
    }
}
