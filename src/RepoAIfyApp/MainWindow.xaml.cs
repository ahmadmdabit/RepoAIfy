using System.Windows;
using Microsoft.Win32;
using Serilog;

namespace RepoAIfyApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var viewModel = new MainWindowViewModel(ShowFolderBrowserDialog, ShowFileBrowserDialog);
            DataContext = viewModel;

            // Reconfigure Serilog to sink to the ViewModel property
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("RepoAIfyApp.log", rollingInterval: RollingInterval.Day)
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
