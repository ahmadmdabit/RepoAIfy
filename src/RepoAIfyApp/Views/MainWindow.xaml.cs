using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

using RepoAIfyApp.ViewModels;

namespace RepoAIfyApp.Views;

public partial class MainWindow : Window
{
    // The ViewModel is now injected by the DI container
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        // Set the DataContext to the ViewModel provided by DI
        DataContext = viewModel;
    }

    private readonly Regex regex = new("[^0-9]+", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);

    private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
    {
        e.Handled = regex.IsMatch(e.Text);
    }
}