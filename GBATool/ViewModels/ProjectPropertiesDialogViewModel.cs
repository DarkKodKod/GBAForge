using ArchitectureLibrary.Model;
using ArchitectureLibrary.Signals;
using ArchitectureLibrary.ViewModel;
using GBATool.Commands.FileSystem;
using GBATool.Commands.Utils;
using GBATool.Enums;
using GBATool.Models;
using GBATool.Signals;
using System.Windows;
using System.Windows.Controls;

namespace GBATool.ViewModels;

public class ProjectPropertiesDialogViewModel : ViewModel
{
    #region Commands
    public BrowseFolderCommand BrowseFolderCommand { get; } = new();
    public DispatchSignalCommand<CloseDialogSignal> CloseDialogCommand { get; } = new();
    #endregion

    private bool _changed = false;
    private SpritePattern _selectedSpriteFormat = SpritePattern.Format1D;
    private OutputFormat _selectedOutputFormatHeader = OutputFormat.None;
    private OutputFormat _selectedOutputFormatPalettes = OutputFormat.None;
    private OutputFormat _selectedOutputFormatCharacters = OutputFormat.None;
    private OutputFormat _selectedOutputFormatMaps = OutputFormat.None;
    private OutputFormat _selectedOutputFormatScreenBlock = OutputFormat.None;
    private string _projectTitle = string.Empty;
    private int _softwareVersion = 0;
    private string _projectInitials = string.Empty;
    private string _developerId = string.Empty;
    private string _folderSourcePath = string.Empty;
    private string _folderAssetsPath = string.Empty;
    private string _folderHeadersPath = string.Empty;
    private string _folderCPPsPath = string.Empty;
    private Visibility _visibilityCPPs = Visibility.Visible;
    private Visibility _visibilityHeaders = Visibility.Visible;
    private Visibility _visibilitySourceCode = Visibility.Visible;
    private Visibility _visibilityAssets = Visibility.Visible;
    private Visibility _visibilityPaths = Visibility.Visible;

    #region get/set
    public Visibility VisibilityPaths
    {
        get => _visibilityPaths;
        set
        {
            _visibilityPaths = value;
            OnPropertyChanged(nameof(VisibilityPaths));
        }
    }

    public Visibility VisibilityCPPs
    {
        get => _visibilityCPPs;
        set
        {
            _visibilityCPPs = value;
            OnPropertyChanged(nameof(VisibilityCPPs));
        }
    }

    public Visibility VisibilityHeaders
    {
        get => _visibilityHeaders;
        set
        {
            _visibilityHeaders = value;
            OnPropertyChanged(nameof(VisibilityHeaders));
        }
    }

    public Visibility VisibilityAssets
    {
        get => _visibilityAssets;
        set
        {
            _visibilityAssets = value;
            OnPropertyChanged(nameof(VisibilityAssets));
        }
    }

    public Visibility VisibilitySourceCode
    {
        get => _visibilitySourceCode;
        set
        {
            _visibilitySourceCode = value;
            OnPropertyChanged(nameof(VisibilitySourceCode));
        }
    }

    public string FolderSourcePath
    {
        get => _folderSourcePath;
        set
        {
            _folderSourcePath = value;
            OnPropertyChanged(nameof(FolderSourcePath));
        }
    }

    public string FolderAssetsPath
    {
        get => _folderAssetsPath;
        set
        {
            _folderAssetsPath = value;
            OnPropertyChanged(nameof(FolderAssetsPath));
        }
    }

    public string FolderHeadersPath
    {
        get => _folderHeadersPath;
        set
        {
            _folderHeadersPath = value;
            OnPropertyChanged(nameof(FolderHeadersPath));
        }
    }

    public string FolderCPPsPaths
    {
        get => _folderCPPsPath;
        set
        {
            _folderCPPsPath = value;
            OnPropertyChanged(nameof(FolderCPPsPaths));
        }
    }

    public OutputFormat SelectedOutputFormatHeader
    {
        get { return _selectedOutputFormatHeader; }
        set
        {
            _selectedOutputFormatHeader = value;
            OnPropertyChanged(nameof(SelectedOutputFormatHeader));

            UpdateVisibility();

            _changed = true;
        }
    }

    public OutputFormat SelectedOutputFormatPalettes
    {
        get { return _selectedOutputFormatPalettes; }
        set
        {
            _selectedOutputFormatPalettes = value;
            OnPropertyChanged(nameof(SelectedOutputFormatPalettes));

            UpdateVisibility();

            _changed = true;
        }
    }

    public OutputFormat SelectedOutputFormatMaps
    {
        get { return _selectedOutputFormatMaps; }
        set
        {
            _selectedOutputFormatMaps = value;
            OnPropertyChanged(nameof(SelectedOutputFormatMaps));

            UpdateVisibility();

            _changed = true;
        }
    }

    public OutputFormat SelectedOutputFormatCharacters
    {
        get { return _selectedOutputFormatCharacters; }
        set
        {
            _selectedOutputFormatCharacters = value;
            OnPropertyChanged(nameof(SelectedOutputFormatCharacters));

            UpdateVisibility();

            _changed = true;
        }
    }

    public OutputFormat SelectedOutputFormatScreenBlock
    {
        get { return _selectedOutputFormatScreenBlock; }
        set
        {
            _selectedOutputFormatScreenBlock = value;
            OnPropertyChanged(nameof(SelectedOutputFormatScreenBlock));

            UpdateVisibility();

            _changed = true;
        }
    }

    public SpritePattern SelectedSpriteFormat
    {
        get { return _selectedSpriteFormat; }
        set
        {
            _selectedSpriteFormat = value;
            OnPropertyChanged(nameof(SelectedSpriteFormat));

            _changed = true;
        }
    }

    public string ProjectTitle
    {
        get { return _projectTitle; }
        set
        {
            _projectTitle = value;
            OnPropertyChanged(nameof(ProjectTitle));

            _changed = true;
        }
    }

    public int SoftwareVersion
    {
        get { return _softwareVersion; }
        set
        {
            _softwareVersion = value;
            OnPropertyChanged(nameof(SoftwareVersion));

            _changed = true;
        }
    }

    public string ProjectInitials
    {
        get { return _projectInitials; }
        set
        {
            _projectInitials = value;
            OnPropertyChanged(nameof(ProjectInitials));

            _changed = true;
        }
    }

    public string DeveloperId
    {
        get { return _developerId; }
        set
        {
            _developerId = value;
            OnPropertyChanged(nameof(DeveloperId));

            _changed = true;
        }
    }
    #endregion

    public ProjectPropertiesDialogViewModel()
    {
        ReadProjectData();

        #region Signals
        SignalManager.Get<BrowseFolderSuccessSignal>().Listener += BrowseFolderSuccess;
        SignalManager.Get<CloseDialogSignal>().Listener += OnCloseDialog;
        #endregion

        ProjectModel project = ModelManager.Get<ProjectModel>();

        FolderSourcePath = project.Build.GeneratedSourcePath;
        FolderAssetsPath = project.Build.GeneratedAssetsPath;
        FolderHeadersPath = project.Build.GeneratedHeadersPath;
        FolderCPPsPaths = project.Build.GeneratedCPPsPath;

        _changed = false;

        UpdateVisibility();
    }

    private void OnCloseDialog()
    {
        if (_changed)
        {
            // Save all changes
            ProjectModel project = ModelManager.Get<ProjectModel>();

            if (project.SpritePatternFormat != SelectedSpriteFormat)
            {
                project.SpritePatternFormat = SelectedSpriteFormat;

                SignalManager.Get<ReloadBankImageSignal>().Dispatch();

                BankModel.MetaDataCache.Clear();
            }

            project.Build.OutputFormatHeader = SelectedOutputFormatHeader;
            project.Build.OutputFormatPalettes = SelectedOutputFormatPalettes;
            project.Build.OutputFormatCharacters = SelectedOutputFormatCharacters;
            project.Build.OutputFormatScreenBlock = SelectedOutputFormatScreenBlock;
            project.Build.OutputFormatMaps = SelectedOutputFormatMaps;

            project.ProjectTitle = ProjectTitle;
            project.SoftwareVersion = (byte)SoftwareVersion;
            project.ProjectInitials = ProjectInitials;
            project.DeveloperId = DeveloperId;

            project.Save();
        }

        #region Signals
        SignalManager.Get<CloseDialogSignal>().Listener -= OnCloseDialog;
        SignalManager.Get<BrowseFolderSuccessSignal>().Listener -= BrowseFolderSuccess;
        #endregion
    }

    private void BrowseFolderSuccess(Control owner, string folderPath)
    {
        ProjectModel project = ModelManager.Get<ProjectModel>();

        if (owner.Name == "btnSourcePath")
        {
            if (project.Build.GeneratedSourcePath != folderPath)
            {
                FolderSourcePath = folderPath;

                project.Build.GeneratedSourcePath = folderPath;
            }
        }
        else if (owner.Name == "btnAssetsPath")
        {
            if (project.Build.GeneratedAssetsPath != folderPath)
            {
                FolderAssetsPath = folderPath;

                project.Build.GeneratedAssetsPath = folderPath;
            }
        }
        else if (owner.Name == "btnHeadersPath")
        {
            if (project.Build.GeneratedHeadersPath != folderPath)
            {
                FolderHeadersPath = folderPath;

                project.Build.GeneratedHeadersPath = folderPath;
            }
        }
        else if (owner.Name == "btnCppPath")
        {
            if (project.Build.GeneratedCPPsPath != folderPath)
            {
                FolderCPPsPaths = folderPath;

                project.Build.GeneratedCPPsPath = folderPath;
            }
        }

        project.Save();
    }

    private void UpdateVisibility()
    {
        bool shouldShowCPPPahts = SelectedOutputFormatPalettes == OutputFormat.Butano ||
            SelectedOutputFormatCharacters == OutputFormat.Butano ||
            SelectedOutputFormatScreenBlock == OutputFormat.Butano;
        bool shouldShowAssetPaths = SelectedOutputFormatScreenBlock == OutputFormat.Binary;
        bool shouldShowSourceCodePaths =
            SelectedOutputFormatHeader == OutputFormat.Fasmarm ||
            SelectedOutputFormatCharacters == OutputFormat.Fasmarm ||
            SelectedOutputFormatPalettes == OutputFormat.Fasmarm ||
            SelectedOutputFormatMaps == OutputFormat.Fasmarm ||
            SelectedOutputFormatScreenBlock == OutputFormat.Binary;

        VisibilityCPPs = shouldShowCPPPahts ? Visibility.Visible : Visibility.Collapsed;
        VisibilityHeaders = shouldShowCPPPahts ? Visibility.Visible : Visibility.Collapsed;
        VisibilitySourceCode = shouldShowSourceCodePaths ? Visibility.Visible : Visibility.Collapsed;
        VisibilityAssets = shouldShowAssetPaths ? Visibility.Visible : Visibility.Collapsed;

        VisibilityPaths = shouldShowCPPPahts || shouldShowAssetPaths || shouldShowSourceCodePaths ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ReadProjectData()
    {
        ProjectModel project = ModelManager.Get<ProjectModel>();

        SelectedSpriteFormat = project.SpritePatternFormat;
        ProjectTitle = project.ProjectTitle;
        SoftwareVersion = project.SoftwareVersion;
        ProjectInitials = project.ProjectInitials;
        DeveloperId = project.DeveloperId;

        SelectedOutputFormatHeader = project.Build.OutputFormatHeader;
        SelectedOutputFormatPalettes = project.Build.OutputFormatPalettes;
        SelectedOutputFormatCharacters = project.Build.OutputFormatCharacters;
        SelectedOutputFormatScreenBlock = project.Build.OutputFormatScreenBlock;
        SelectedOutputFormatMaps = project.Build.OutputFormatMaps;
    }
}
