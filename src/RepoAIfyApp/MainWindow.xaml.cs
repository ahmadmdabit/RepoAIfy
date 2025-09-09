using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Win32;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.FileSystemGlobbing;


using Serilog;
using Serilog.Extensions.Logging;

using RepoAIfyLib;

namespace RepoAIfyApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Reconfigure Serilog to include the TextBoxSink after UI components are initialized
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("RepoAIfyApp.log", rollingInterval: RollingInterval.Day)
                .WriteTo.Sink(new TextBoxSink(LogTextBox, null))
                .CreateLogger();

            BrowseSourceButton.Click += BrowseSourceButton_Click;
            BrowseOptionsButton.Click += BrowseOptionsButton_Click;
            GenerateButton.Click += GenerateButton_Click;

            IncludedExtensionsTextBox.TextChanged += FilterTextBox_TextChanged;
            ExcludedDirectoriesTextBox.TextChanged += FilterTextBox_TextChanged;


            LoadDefaultOptions();
        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(SourceDirectoryTextBox.Text))
            {
                PopulateTreeView(SourceDirectoryTextBox.Text);
            }
        }

        private void LoadDefaultOptions()
        {
            var defaultOptionsPath = System.IO.Path.Combine(AppContext.BaseDirectory, "options.json");
            if (File.Exists(defaultOptionsPath))
            {
                OptionsFileTextBox.Text = defaultOptionsPath;
                LoadOptions(defaultOptionsPath);
            }
            else
            {
                Log.Warning("Default options.json not found. Please select one manually.");
            }
        }

        private void BrowseSourceButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog()
            {
                Title = "Select Source Directory"
            };

            if (dialog.ShowDialog() == true)
            {
                SourceDirectoryTextBox.Text = dialog.FolderName;
                PopulateTreeView(dialog.FolderName);
            }
        }

        private void PopulateTreeView(string path)
        {
            var includedExtensions = new HashSet<string>(IncludedExtensionsTextBox.Text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries), StringComparer.OrdinalIgnoreCase);
            var excludedDirectories = ExcludedDirectoriesTextBox.Text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

            var matcher = new Matcher(StringComparison.OrdinalIgnoreCase);
            matcher.AddIncludePatterns(excludedDirectories);

            var rootNode = new FileSystemNode { Name = System.IO.Path.GetFileName(path), Path = path, IsDirectory = true };
            PopulateChildren(rootNode, includedExtensions, matcher, path);
            FileTreeView.ItemsSource = new ObservableCollection<FileSystemNode> { rootNode };
        }

                private void PopulateChildren(FileSystemNode parentNode, HashSet<string> includedExtensions, Matcher matcher, string basePath)
        {
            if (parentNode.Path == null) return;

            try
            {
                foreach (var directory in Directory.GetDirectories(parentNode.Path))
                {
                    var relativePath = System.IO.Path.GetRelativePath(basePath, directory);
                    // Append a directory separator to correctly match directory patterns like '**/bin/**'
                    if (matcher.Match(relativePath + System.IO.Path.DirectorySeparatorChar).HasMatches) continue;

                    var childNode = new FileSystemNode { Name = System.IO.Path.GetFileName(directory), Path = directory, IsDirectory = true, Parent = parentNode };
                    parentNode.Children.Add(childNode);
                    PopulateChildren(childNode, includedExtensions, matcher, basePath);
                }

                foreach (var file in Directory.GetFiles(parentNode.Path))
                {
                    var relativePath = System.IO.Path.GetRelativePath(basePath, file);
                    if (matcher.Match(relativePath).HasMatches) continue;

                    if (!includedExtensions.Contains(System.IO.Path.GetExtension(file))) continue;

                    var childNode = new FileSystemNode { Name = System.IO.Path.GetFileName(file), Path = file, IsDirectory = false, Parent = parentNode };
                    parentNode.Children.Add(childNode);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Ignore folders that can't be accessed
            }
        }


        private void BrowseOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                Title = "Select options.json file",
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                OptionsFileTextBox.Text = dialog.FileName;
                LoadOptions(dialog.FileName);
            }
        }

        private async void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            SetUiState(false);
            LogTextBox.Clear();
            StatusTextBlock.Text = "Processing...";

            try
            {
                var sourceDirectoryPath = SourceDirectoryTextBox.Text;
                var optionsFilePath = OptionsFileTextBox.Text;

                if (string.IsNullOrWhiteSpace(sourceDirectoryPath) || string.IsNullOrWhiteSpace(optionsFilePath))
                {
                    Log.Error("Source directory and options file paths cannot be empty.");
                    StatusTextBlock.Text = "Error: Missing input.";
                    return;
                }

                var sourceDirectory = new DirectoryInfo(sourceDirectoryPath);
                var optionsFile = new FileInfo(optionsFilePath);

                if (!sourceDirectory.Exists)
                {
                    Log.Error($"Source directory not found: {sourceDirectoryPath}");
                    StatusTextBlock.Text = "Error: Source directory not found.";
                    return;
                }

                if (!optionsFile.Exists)
                {
                    Log.Error($"Options file not found: {optionsFilePath}");
                    StatusTextBlock.Text = "Error: Options file not found.";
                    return;
                }

                var includedFiles = GetCheckedFiles();
                if (!includedFiles.Any())
                {
                    Log.Error("No files selected in the tree view.");
                    StatusTextBlock.Text = "Error: No files selected.";
                    return;
                }


                // Construct Options object from UI fields
                var options = new RepoAIfyLib.Options
                {
                    FileFilter = new RepoAIfyLib.FileFilter
                    {
                        IncludedExtensions = IncludedExtensionsTextBox.Text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
                        ExcludedDirectories = ExcludedDirectoriesTextBox.Text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                    },
                    Chunking = new RepoAIfyLib.Chunking
                    {
                        MaxChunkSizeKb = int.TryParse(MaxChunkSizeKbTextBox.Text, out int chunkSize) ? chunkSize : 128
                    },
                    Output = new RepoAIfyLib.Output
                    {
                        OutputDirectory = OutputDirectoryTextBox.Text
                    }
                };

                await Task.Run(async () =>
                {
                    var loggerFactory = new SerilogLoggerFactory(Log.Logger);
                    var logger = loggerFactory.CreateLogger<ConverterRunner>();
                    var converterRunner = new ConverterRunner(logger);
                    await converterRunner.Run(sourceDirectory, options, includedFiles);
                });

                StatusTextBlock.Text = "Processing Complete.";
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "An unexpected error occurred during processing.");
                StatusTextBlock.Text = "Error: An unexpected error occurred.";
            }
            finally
            {
                SetUiState(true);
            }
        }

        private List<string> GetCheckedFiles()
        {
            var checkedFiles = new List<string>();
            var rootNode = FileTreeView.ItemsSource.Cast<FileSystemNode>().FirstOrDefault();
            if (rootNode != null)
            {
                GetCheckedFilesRecursive(rootNode, checkedFiles);
            }
            return checkedFiles;
        }

        private void GetCheckedFilesRecursive(FileSystemNode node, List<string> checkedFiles)
        {
            if (node.IsChecked == true && !node.IsDirectory && node.Path != null)
            {
                checkedFiles.Add(node.Path);
            }

            foreach (var child in node.Children)
            {
                GetCheckedFilesRecursive(child, checkedFiles);
            }
        }


        private void SetUiState(bool isEnabled)
        {
            SourceDirectoryTextBox.IsEnabled = isEnabled;
            OptionsFileTextBox.IsEnabled = isEnabled;
            BrowseSourceButton.IsEnabled = isEnabled;
            BrowseOptionsButton.IsEnabled = isEnabled;
            GenerateButton.IsEnabled = isEnabled;

            IncludedExtensionsTextBox.IsEnabled = isEnabled;
            ExcludedDirectoriesTextBox.IsEnabled = isEnabled;
            MaxChunkSizeKbTextBox.IsEnabled = isEnabled;
            OutputDirectoryTextBox.IsEnabled = isEnabled;
        }

        private async void LoadOptions(string optionsFilePath)
        {
            try
            {
                var optionsLoader = new RepoAIfyLib.Services.OptionsLoader();
                var options = await optionsLoader.LoadOptions(new FileInfo(optionsFilePath));

                if (options != null)
                {
                    IncludedExtensionsTextBox.Text = string.Join(", ", options.FileFilter.IncludedExtensions);
                    ExcludedDirectoriesTextBox.Text = string.Join(", ", options.FileFilter.ExcludedDirectories);
                    MaxChunkSizeKbTextBox.Text = options.Chunking.MaxChunkSizeKb.ToString();
                    OutputDirectoryTextBox.Text = options.Output.OutputDirectory;
                    Log.Information($"Options loaded from {optionsFilePath}");
                }
                else
                {
                    Log.Warning($"Could not load options from {optionsFilePath}. Using default values.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error loading options from {optionsFilePath}");
            }
        }
    }
}
