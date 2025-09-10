using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

using Microsoft.Extensions.DependencyInjection;

using RepoAIfyLib;
using RepoAIfyLib.Services;

using Serilog;

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

    // Add near the other UI-Bound Properties
    public ObservableCollection<GeneratedFileViewModel> GeneratedFiles { get; } = new();

    private GeneratedFileViewModel? _selectedGeneratedFile;
    public GeneratedFileViewModel? SelectedGeneratedFile
    {
        get => _selectedGeneratedFile;
        set => SetField(ref _selectedGeneratedFile, value);
    }

    private int _selectedTabIndex = 0;
    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set => SetField(ref _selectedTabIndex, value);
    }

    // UI-Bound Properties
    public string SourceDirectory { get => _sourceDirectory; set => SetField(ref _sourceDirectory, value); }

    public string OptionsFile { get => _optionsFile; set => SetField(ref _optionsFile, value); }
    public string IncludedExtensions
    { get => _includedExtensions; set { if (SetField(ref _includedExtensions, value)) { DebouncePopulateTreeView(); } } }
    public string ExcludedDirectories
    { get => _excludedDirectories; set { if (SetField(ref _excludedDirectories, value)) { DebouncePopulateTreeView(); } } }
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
    private readonly IDialogService _dialogService; // Depend on the interface

    private readonly OptionsLoader _optionsLoader;
    private readonly TreeViewDataService _treeViewDataService;
    private CancellationTokenSource? _filterCts;

    public MainWindowViewModel(
        IDialogService dialogService,
        UILogRelayService logRelay,
        OptionsLoader optionsLoader,
        TreeViewDataService treeViewDataService)
    {
        _dialogService = dialogService;
        _optionsLoader = optionsLoader;
        _treeViewDataService = treeViewDataService;

        BrowseSourceCommand = new RelayCommand(ExecuteBrowseSource);
        BrowseOptionsCommand = new AsyncRelayCommand(async (obj) => await ExecuteBrowseOptions(obj));
        GenerateCommand = new AsyncRelayCommand(async (obj) => await ExecuteGenerate(), _ => !string.IsNullOrWhiteSpace(SourceDirectory) && !string.IsNullOrWhiteSpace(OptionsFile));

        // Subscribe to log messages from the relay.
        logRelay.LogMessagePublished += OnLogMessageReceived;

        _ = LoadDefaultOptions(); // Fire-and-forget is acceptable for initialization.
    }

    private void OnLogMessageReceived(string message)
    {
        // We are now responsible for dispatching to the UI thread.
        // This happens AFTER the ViewModel is constructed, so the Dispatcher is safe to use.
        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            LogOutput += message + Environment.NewLine;
        });
    }

    // Command Implementations & Logic
    private void ExecuteBrowseSource(object? obj)
    {
        // Use the service
        var folder = _dialogService.ShowFolderBrowserDialog();
        if (!string.IsNullOrWhiteSpace(folder))
        {
            SourceDirectory = folder;
            PopulateTreeView(SourceDirectory);
        }
    }

    private async Task ExecuteBrowseOptions(object? obj)
    {
        // Use the service
        var file = _dialogService.ShowFileBrowserDialog();
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
        GeneratedFiles.Clear(); // Clear previous results
        SelectedTabIndex = 0; // Switch back to the Logs tab
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

            string outputDir = options.Output.OutputDirectory; // Capture the output directory path

            await Task.Run(async () =>
            {
                // Get services from the DI container through the application
                var services = ((App)System.Windows.Application.Current).ServiceProvider;
                var converterRunner = services.GetRequiredService<ConverterRunner>();
                await converterRunner.Run(sourceDirectoryInfo, options, includedFiles);
            });

            // --- START: New logic to load results ---
            StatusText = "Loading generated files...";
            var generatedFilePaths = Directory.GetFiles(outputDir, "*.md");
            foreach (var filePath in generatedFilePaths.OrderBy(f => f))
            {
                var fileContent = await File.ReadAllTextAsync(filePath);
                GeneratedFiles.Add(new GeneratedFileViewModel
                {
                    FileName = Path.GetFileName(filePath),
                    Content = fileContent
                });
            }

            if (GeneratedFiles.Any())
            {
                SelectedGeneratedFile = GeneratedFiles.First(); // Select the first file
                SelectedTabIndex = 1; // Switch to the Markdown Preview tab
            }
            // --- END: New logic ---

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

        var fileSystemTree = _treeViewDataService.GetFileSystemTree(path, included, excluded);

        var rootNode = ConvertToFileSystemNode(fileSystemTree, null);
        RootNodes = new ObservableCollection<FileSystemNode> { rootNode };
    }

    private async Task DebouncePopulateTreeView()
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

    private async Task LoadDefaultOptions()
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
        var options = await _optionsLoader.LoadOptions(new FileInfo(optionsFilePath));
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
        return node;
    }
}