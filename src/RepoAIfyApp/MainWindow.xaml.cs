using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Serilog;

namespace RepoAIfyApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Get services from the DI container
            var services = ((App)Application.Current).ServiceProvider;
            var optionsLoader = services.GetRequiredService<RepoAIfyLib.Services.OptionsLoader>();
            var treeViewDataService = services.GetRequiredService<RepoAIfyLib.Services.TreeViewDataService>();
            
            var viewModel = new MainWindowViewModel(ShowFolderBrowserDialog, ShowFileBrowserDialog, optionsLoader, treeViewDataService);
            DataContext = viewModel;

            // Reconfigure Serilog to sink to the ViewModel property
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "RepoAIfy",
                        "RepoAIfyApp-.log"
                    ),
                    rollingInterval: RollingInterval.Day
                )
                .WriteTo.Sink(new ViewModelSink(s => Dispatcher.Invoke(() => viewModel.LogOutput += s + Environment.NewLine)))
                .CreateLogger();
        }

        private string? ShowFolderBrowserDialog()
        {
            var dialog = new OpenFolderDialog { Title = "Select Source Directory" };
            return dialog.ShowDialog() == true ? dialog.FolderName : null;
        }

        private string? ShowFileBrowserDialog()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select options.json file",
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
            };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }
    }
}