using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

using RepoAIfyApp.Helpers;
using RepoAIfyApp.Models;
using RepoAIfyApp.Services;

using RepoAIfyLib.Models;
using RepoAIfyLib.Services;

using Serilog;

namespace RepoAIfyApp.ViewModels;

public enum AppState { Idle, PopulatingTree, Generating }

public class MainWindowViewModel : ViewModelBase
{
    // Backing Fields
    private string sourceDirectory = string.Empty;

    private string optionsFile = string.Empty;
    private string includedExtensions = string.Empty;
    private string excludedDirectories = string.Empty;
    private string maxChunkSizeKb = "128";
    private string maxFileSizeMb = "16";
    private string outputDirectory = "./ai-output";
    private string logOutput = string.Empty;
    private string statusText = "Ready";
    private bool isUiEnabled = true;
    private bool showFileSizes = false;
    private ObservableCollection<FileSystemNode> rootNodes = [];

    // Add near the other UI-Bound Properties
    public ObservableCollection<GeneratedFileViewModel> GeneratedFiles { get; } = [];

    private GeneratedFileViewModel? selectedGeneratedFile;
    public GeneratedFileViewModel? SelectedGeneratedFile
    {
        get => selectedGeneratedFile;
        set => SetField(ref selectedGeneratedFile, value);
    }

    private int selectedTabIndex = 0;
    public int SelectedTabIndex
    {
        get => selectedTabIndex;
        set => SetField(ref selectedTabIndex, value);
    }

    // UI-Bound Properties
    public string SourceDirectory { get => sourceDirectory; set => SetField(ref sourceDirectory, value); }

    public string OptionsFile { get => optionsFile; set => SetField(ref optionsFile, value); }
    public string IncludedExtensions
    { get => includedExtensions; set { if (SetField(ref includedExtensions, value)) { DebouncePopulateTreeView(); } } }
    public string ExcludedDirectories
    { get => excludedDirectories; set { if (SetField(ref excludedDirectories, value)) { DebouncePopulateTreeView(); } } }
    public string MaxChunkSizeKb { get => maxChunkSizeKb; set => SetField(ref maxChunkSizeKb, value); }
    public string MaxFileSizeMb
    { get => maxFileSizeMb; set { if (SetField(ref maxFileSizeMb, value)) { DebouncePopulateTreeView(); } } }
    public string OutputDirectory { get => outputDirectory; set => SetField(ref outputDirectory, value); }
    public string LogOutput { get => logOutput; set => SetField(ref logOutput, value); }
    public string StatusText { get => statusText; set => SetField(ref statusText, value); }
    public bool IsUiEnabled { get => isUiEnabled; set => SetField(ref isUiEnabled, value); }
    public bool ShowFileSizes
    { get => showFileSizes; set { if (SetField(ref showFileSizes, value)) { DebouncePopulateTreeView(); } } }
    public ObservableCollection<FileSystemNode> RootNodes { get => rootNodes; set => SetField(ref rootNodes, value); }

    // Commands
    public ICommand BrowseSourceCommand { get; }

    public ICommand BrowseOptionsCommand { get; }
    public ICommand GenerateCommand { get; }
    public ICommand CancelGenerateCommand { get; }

    // Services & Helpers
    private readonly IDialogService dialogService;
    private readonly ConverterRunnerService converterRunner;
    private readonly OptionsLoaderService optionsLoader;
    private readonly TreeViewDataService treeViewDataService;
    private CancellationTokenSource? _cts;
    private AppState _appState;

    public MainWindowViewModel(
        IDialogService dialogService,
        UILogRelayService logRelay,
        OptionsLoaderService optionsLoader,
        TreeViewDataService treeViewDataService,
        ConverterRunnerService converterRunner)
    {
        this.dialogService = dialogService;
        this.optionsLoader = optionsLoader;
        this.treeViewDataService = treeViewDataService;
        this.converterRunner = converterRunner;

        BrowseSourceCommand = new RelayCommand(ExecuteBrowseSource);
        BrowseOptionsCommand = new AsyncRelayCommand(async (obj) => await ExecuteBrowseOptions(obj));
        GenerateCommand = new AsyncRelayCommand(async (obj) => await ExecuteGenerate(obj), 
            _ => !string.IsNullOrWhiteSpace(SourceDirectory) && !string.IsNullOrWhiteSpace(OptionsFile) && _appState == AppState.Idle);
        CancelGenerateCommand = new RelayCommand(ExecuteCancelGenerate, _ => _appState != AppState.Idle);

        // Subscribe to log messages from the relay.
        logRelay.LogMessagePublished += OnLogMessageReceived;

        _ = LoadDefaultOptions(); // Fire-and-forget is acceptable for initialization.
    }

    private void OnLogMessageReceived(string message)
    {
        // We are now responsible for dispatching to the UI thread.
        // This happens AFTER the ViewModel is constructed, so the Dispatcher is safe to use.
        Application.Current.Dispatcher.BeginInvoke(() => LogOutput += message + Environment.NewLine);
    }

    private void ExecuteCancelGenerate(object? obj)
    {
        _cts?.Cancel();
    }

    // Command Implementations & Logic
    private void ExecuteBrowseSource(object? obj)
    {
        // Use the service
        var folder = dialogService.ShowFolderBrowserDialog();
        if (!string.IsNullOrWhiteSpace(folder))
        {
            SourceDirectory = folder;
            _ = DebouncePopulateTreeView();
        }
    }

    private async Task ExecuteBrowseOptions(object? obj)
    {
        // Use the service
        var file = dialogService.ShowFileBrowserDialog();
        if (!string.IsNullOrWhiteSpace(file))
        {
            OptionsFile = file;
            await LoadOptions(OptionsFile);
        }
    }

    private async Task ExecuteGenerate(object? obj)
    {
        _cts = new CancellationTokenSource();
        _appState = AppState.Generating;
        IsUiEnabled = false;
        LogOutput = string.Empty;
        GeneratedFiles.Clear();
        SelectedTabIndex = 0;
        StatusText = "Processing...";
        CommandManager.InvalidateRequerySuggested();

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
                    IncludedExtensions = [.. IncludedExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)],
                    ExcludedDirectories = [.. ExcludedDirectories.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)],
                    MaxFileSizeMb = int.TryParse(MaxFileSizeMb, out int maxFileSize) ? maxFileSize : 16
                },
                Chunking = new Chunking { MaxChunkSizeKb = int.TryParse(MaxChunkSizeKb, out int size) ? size : 128 },
                Output = new Output { OutputDirectory = OutputDirectory }
            };

            string outputDir = options.Output.OutputDirectory;

            await Task.Run(async () =>
            {
                await converterRunner.Run(sourceDirectoryInfo, options, includedFiles, _cts.Token);
            });

            StatusText = "Loading generated files...";
            var generatedFilePaths = Directory.GetFiles(outputDir, "*.md");
            foreach (var filePath in generatedFilePaths.Order())
            {
                _cts.Token.ThrowIfCancellationRequested();
                var fileContent = await File.ReadAllTextAsync(filePath, _cts.Token);
                GeneratedFiles.Add(new GeneratedFileViewModel
                {
                    FileName = Path.GetFileName(filePath),
                    Content = fileContent
                });
            }

            if (GeneratedFiles.Any())
            {
                SelectedGeneratedFile = GeneratedFiles.First();
                SelectedTabIndex = 1;
            }

            StatusText = "Processing Complete.";
        }
        catch (OperationCanceledException)
        {
            StatusText = "Processing Canceled.";
            Log.Warning("The generation process was canceled by the user.");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "An unexpected error occurred during processing.");
            StatusText = "Error: An unexpected error occurred.";
        }
        finally
        {
            _appState = AppState.Idle;
            IsUiEnabled = true;
            _cts.Dispose();
            _cts = null;
            CommandManager.InvalidateRequerySuggested();
        }
    }

    private async Task PopulateTreeView(string path, bool showFileSizes, CancellationToken token)
    {
        var included = IncludedExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        var excluded = ExcludedDirectories.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        try
        {
            var fileSystemTree = await Task.Run(() => 
                treeViewDataService.GetFileSystemTree(path, included, excluded, showFileSizes, token), token);

            token.ThrowIfCancellationRequested();

            var rootNode = ConvertToFileSystemNode(fileSystemTree, null);
            RootNodes = [rootNode];
        }
        catch (OperationCanceledException)
        {
            Log.Information("Tree view population canceled.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to populate tree view.");
        }
    }

    private async Task DebouncePopulateTreeView()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        _appState = AppState.PopulatingTree;
        StatusText = "Loading file tree...";
        CommandManager.InvalidateRequerySuggested();

        try
        {
            await Task.Delay(500, token);
            if (!string.IsNullOrWhiteSpace(SourceDirectory))
            {
                await PopulateTreeView(SourceDirectory, ShowFileSizes, token);
            }
        }
        catch (OperationCanceledException) 
        {
            // This is expected if a new request comes in, just ignore it.
        }
        finally
        {
            if (!token.IsCancellationRequested)
            {
                _appState = AppState.Idle;
                StatusText = "Ready";
                CommandManager.InvalidateRequerySuggested();
            }
        }
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
        var options = await optionsLoader.LoadOptions(new FileInfo(optionsFilePath));
        if (options != null)
        {
            IncludedExtensions = string.Join(", ", options.FileFilter.IncludedExtensions);
            ExcludedDirectories = string.Join(", ", options.FileFilter.ExcludedDirectories);
            MaxFileSizeMb = options.FileFilter.MaxFileSizeMb.ToString();
            MaxChunkSizeKb = options.Chunking.MaxChunkSizeKb.ToString();
            OutputDirectory = options.Output.OutputDirectory;
            Log.Information($"Options loaded from {optionsFilePath}");
        }
    }

    // Helper method from original code-behind
    private FileSystemNode ConvertToFileSystemNode(RepoAIfyLib.Models.FileSystemTree treeNode, FileSystemNode? parent)
    {
        var node = new FileSystemNode { Name = treeNode.Name, Path = treeNode.Path, IsDirectory = treeNode.IsDirectory, Parent = parent, FileSize = treeNode.FileSize };
        foreach (var child in treeNode.Children) { node.Children.Add(ConvertToFileSystemNode(child, node)); }
        return node;
    }
}