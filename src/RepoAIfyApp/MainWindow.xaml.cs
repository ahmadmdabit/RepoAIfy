using System.Windows;

namespace RepoAIfyApp
{
    public partial class MainWindow : Window
    {
        // The ViewModel is now injected by the DI container
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            // Set the DataContext to the ViewModel provided by DI
            DataContext = viewModel;
        }
    }
}