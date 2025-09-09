// RepoAIfyApp/MainWindowViewModel.cs
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using RepoAIfyLib;
using RepoAIfyLib.Services;
using Serilog;
using Serilog.Extensions.Logging;

namespace RepoAIfyApp;

public class MainWindowViewModel : ViewModelBase
{
    // Backing Fields
    private string _sourceDirectory = string.Empty;
    private string _optionsFile = string.Empty;
    private string _includedExtensions = string.Empty;
    private string _excludedDirectories = string.Empty;
    private string _maxChunkSizeKb = "128";
    private string _outputDirectory = "./ai-output";
    private string _logOutput = string.Empty;
    private string _statusText = "Ready";
    private bool _isUiEnabled = true;
    private ObservableCollection<FileSystemNode> _rootNodes = new();

    // UI-Bound Properties
    public string SourceDirectory { get => _sourceDirectory; set => SetField(ref _sourceDirectory, value); }
    public string OptionsFile { get => _optionsFile; set => SetField(ref _optionsFile, value); }
    public string IncludedExtensions { get => _includedExtensions; set { if (SetField(ref _includedExtensions, value)) { DebouncePopulateTreeView(); } } }
    public string ExcludedDirectories { get => _excludedDirectories; set { if (SetField(ref _excludedDirectories, value)) { DebouncePopulateTreeView(); } } }
    public string MaxChunkSizeKb { get => _maxChunkSizeKb; set => SetField(ref _maxChunkSizeKb, value); }
    public string OutputDirectory { get => _outputDirectory; set => SetField(ref _outputDirectory, value); }
    public string LogOutput { get => _logOutput; set => SetField(ref _logOutput, value); }
    public string StatusText { get => _statusText; set => SetField(ref _statusText, value); }
    public bool IsUiEnabled { get => _isUiEnabled; set => SetField(ref _isUiEnabled, value); }
    public ObservableCollection<FileSystemNode> RootNodes { get => _rootNodes; set => SetField(ref _rootNodes, value); }

    // Commands
    public ICommand BrowseSourceCommand { get; }
    public ICommand BrowseOptionsCommand { get; }
    public ICommand GenerateCommand { get; }

    // Services & Helpers
    private readonly Func<string?> _browseForFolder;
    private readonly Func<string?> _browseForJsonFile;
    private CancellationTokenSource? _filterCts;

    public MainWindowViewModel(Func<string?> browseForFolder, Func<string?> browseForJsonFile)
    {
        _browseForFolder = browseForFolder;
        _browseForJsonFile = browseForJsonFile;

        BrowseSourceCommand = new RelayCommand(ExecuteBrowseSource);
        BrowseOptionsCommand = new RelayCommand(ExecuteBrowseOptions);
        GenerateCommand = new RelayCommand(async _ => await ExecuteGenerate(), _ => !string.IsNullOrWhiteSpace(SourceDirectory) && !string.IsNullOrWhiteSpace(OptionsFile));

        LoadDefaultOptions();
    }

    // Command Implementations & Logic
    private void ExecuteBrowseSource(object? obj)
    {
        var folder = _browseForFolder();
        if (!string.IsNullOrWhiteSpace(folder))
        {
            SourceDirectory = folder;
            PopulateTreeView(SourceDirectory);
        }
    }

    private async void ExecuteBrowseOptions(object? obj)
    {
        var file = _browseForJsonFile();
        if (!string.IsNullOrWhiteSpace(file))
        {
            OptionsFile = file;
            await LoadOptions(OptionsFile);
        }
    }

    private async Task ExecuteGenerate()
    {
        IsUiEnabled = false;
        LogOutput = string.Empty;
        StatusText = "Processing...";

        try
        {
            var sourceDirectoryInfo = new DirectoryInfo(SourceDirectory);
            var optionsFileInfo = new FileInfo(OptionsFile);

            if (!sourceDirectoryInfo.Exists) { /* Error handling... */ return; }
            if (!optionsFileInfo.Exists) { /* Error handling... */ return; }

            var rootNode = RootNodes.FirstOrDefault();
            var includedFiles = rootNode?.GetCheckedFilePaths() ?? Enumerable.Empty<string>();
            if (!includedFiles.Any())
            {
                Log.Error("No files selected in the tree view.");
                StatusText = "Error: No files selected.";
                return;
            }

            var options = new Options
            {
                FileFilter = new FileFilter
                {
                    IncludedExtensions = IncludedExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
                    ExcludedDirectories = ExcludedDirectories.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                },
                Chunking = new Chunking { MaxChunkSizeKb = int.TryParse(MaxChunkSizeKb, out int size) ? size : 128 },
                Output = new Output { OutputDirectory = OutputDirectory }
            };

            await Task.Run(async () =>
            {
                var loggerFactory = new SerilogLoggerFactory(Log.Logger);
                var logger = loggerFactory.CreateLogger<ConverterRunner>();
                var converterRunner = new ConverterRunner(logger);
                await converterRunner.Run(sourceDirectoryInfo, options, includedFiles);
            });

            StatusText = "Processing Complete.";
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "An unexpected error occurred during processing.");
            StatusText = "Error: An unexpected error occurred.";
        }
        finally
        {
            IsUiEnabled = true;
        }
    }

    private void PopulateTreeView(string path)
    {
        var included = IncludedExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        var excluded = ExcludedDirectories.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        var dataService = new TreeViewDataService();
        var fileSystemTree = dataService.GetFileSystemTree(path, included, excluded);

        var rootNode = ConvertToFileSystemNode(fileSystemTree, null);
        RootNodes = new ObservableCollection<FileSystemNode> { rootNode };
    }
    
    private async void DebouncePopulateTreeView()
    {
        _filterCts?.Cancel();
        _filterCts = new CancellationTokenSource();
        try
        {
            await Task.Delay(500, _filterCts.Token);
            if (!string.IsNullOrWhiteSpace(SourceDirectory))
            {
                PopulateTreeView(SourceDirectory);
            }
        }
        catch (TaskCanceledException) { /* Ignore */ }
    }

    private async void LoadDefaultOptions()
    {
        var defaultOptionsPath = Path.Combine(AppContext.BaseDirectory, "options.json");
        if (File.Exists(defaultOptionsPath))
        {
            OptionsFile = defaultOptionsPath;
            await LoadOptions(defaultOptionsPath);
        }
    }

    private async Task LoadOptions(string optionsFilePath)
    {
        var optionsLoader = new OptionsLoader();
        var options = await optionsLoader.LoadOptions(new FileInfo(optionsFilePath));
        if (options != null)
        {
            IncludedExtensions = string.Join(", ", options.FileFilter.IncludedExtensions);
            ExcludedDirectories = string.Join(", ", options.FileFilter.ExcludedDirectories);
            MaxChunkSizeKb = options.Chunking.MaxChunkSizeKb.ToString();
            OutputDirectory = options.Output.OutputDirectory;
            Log.Information($"Options loaded from {optionsFilePath}");
        }
    }

    // Helper method from original code-behind
    private FileSystemNode ConvertToFileSystemNode(RepoAIfyLib.Models.FileSystemTree treeNode, FileSystemNode? parent)
    {
        var node = new FileSystemNode { Name = treeNode.Name, Path = treeNode.Path, IsDirectory = treeNode.IsDirectory, Parent = parent };
        foreach (var child in treeNode.Children) { node.Children.Add(ConvertToFileSystemNode(child, node)); }
        return node;    }
}
