using ArchitectureLibrary.Model;
using ArchitectureLibrary.Signals;
using GBATool.Enums;
using GBATool.Signals;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;
using Tomlyn;

namespace GBATool.Models;

public class ProjectModel : ISingletonModel
{
    public class BuildConfig
    {
        public string GeneratedSourcePath { get; set; } = string.Empty;
        public string GeneratedAssetsPath { get; set; } = string.Empty;
        public string GeneratedHeadersPath { get; set; } = string.Empty;
        public string GeneratedCPPsPath { get; set; } = string.Empty;
        public OutputFormat OutputFormatHeader { get; set; } = OutputFormat.Fasmarm;
        public OutputFormat OutputFormatPalettes { get; set; } = OutputFormat.Fasmarm;
        public OutputFormat OutputFormatMaps { get; set; } = OutputFormat.None;
        public OutputFormat OutputFormatCharacters { get; set; } = OutputFormat.Fasmarm;
        public OutputFormat OutputFormatScreenBlock { get; set; } = OutputFormat.Binary;
        public OutputFormat OutputFormatBackground { get; set; } = OutputFormat.None;
        public bool OutputRemoveTimeFromTimestamp { get; set; }

        public BuildConfig()
        {
            Reset();
        }

        public void Reset()
        {
            GeneratedSourcePath = string.Empty;
            GeneratedAssetsPath = string.Empty;
            GeneratedHeadersPath = string.Empty;
            GeneratedCPPsPath = string.Empty;
            OutputFormatHeader = OutputFormat.Fasmarm;
            OutputFormatPalettes = OutputFormat.Fasmarm;
            OutputFormatMaps = OutputFormat.None;
            OutputFormatCharacters = OutputFormat.Fasmarm;
            OutputFormatScreenBlock = OutputFormat.Binary;
            OutputFormatBackground = OutputFormat.None;
            OutputRemoveTimeFromTimestamp = false;
        }
    }

    public int Version { get; set; } = 0;
    public string Name { get; set; } = string.Empty;
    public string ProjectTitle { get; set; } = string.Empty;
    public byte SoftwareVersion { get; set; } = 0;
    public string ProjectInitials { get; set; } = string.Empty;
    public string DeveloperId { get; set; } = string.Empty;

    [JsonIgnore] public string ProjectFilePath { get; set; } = string.Empty;
    [JsonIgnore] public string ProjectPath { get; set; } = string.Empty;
    public BuildConfig Build { get; set; } = new();
    public SpritePattern SpritePatternFormat { get; set; } = SpritePattern.Format1D;

    public ProjectModel()
    {
    }

    public void Reset()
    {
        ProjectFilePath = string.Empty;
        ProjectPath = string.Empty;
        Name = string.Empty;
        ProjectTitle = string.Empty;
        ProjectInitials = string.Empty;
        DeveloperId = string.Empty;
        SoftwareVersion = 0;
        SpritePatternFormat = SpritePattern.Format1D;

        Build.Reset();
    }

    public bool IsOpen()
    {
        return !string.IsNullOrEmpty(ProjectFilePath) && !string.IsNullOrEmpty(ProjectPath);
    }

    public void Copy(ProjectModel? copy)
    {
        if (copy == null)
        {
            return;
        }

        Name = copy.Name;
        Version = copy.Version;
        SpritePatternFormat = copy.SpritePatternFormat;
        ProjectTitle = copy.ProjectTitle;
        ProjectInitials = copy.ProjectInitials;
        SoftwareVersion = copy.SoftwareVersion;
        DeveloperId = copy.DeveloperId;

        Build.GeneratedSourcePath = copy.Build.GeneratedSourcePath;
        Build.GeneratedAssetsPath = copy.Build.GeneratedAssetsPath;
        Build.OutputFormatHeader = copy.Build.OutputFormatHeader;
        Build.OutputFormatPalettes = copy.Build.OutputFormatPalettes;
        Build.OutputFormatMaps = copy.Build.OutputFormatMaps;
        Build.OutputFormatCharacters = copy.Build.OutputFormatCharacters;
        Build.OutputFormatScreenBlock = copy.Build.OutputFormatScreenBlock;
        Build.OutputFormatBackground = copy.Build.OutputFormatBackground;
        Build.GeneratedHeadersPath = copy.Build.GeneratedHeadersPath;
        Build.GeneratedCPPsPath = copy.Build.GeneratedCPPsPath;
        Build.OutputRemoveTimeFromTimestamp = copy.Build.OutputRemoveTimeFromTimestamp;
    }

    public void Load(string path, string filePath)
    {
        ProjectPath = path;
        ProjectFilePath = filePath;

        TomlSerializerOptions options = new();
        string fileContent = File.ReadAllText(ProjectFilePath, Encoding.UTF8);
        Copy(TomlSerializer.Deserialize<ProjectModel>(fileContent, options));
    }

    public void Save(string path)
    {
        ProjectFilePath = path;

        Save();
    }

    public void Save()
    {
        if (string.IsNullOrEmpty(ProjectFilePath))
        {
            return;
        }

        TomlSerializerOptions options = new();
        File.WriteAllText(ProjectFilePath, TomlSerializer.Serialize(this, options));

        SignalManager.Get<ProjectConfigurationSavedSignal>().Dispatch();
    }
}
