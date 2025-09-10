using Microsoft.Win32;

namespace RepoAIfyApp.Services;

public class WpfDialogService : IDialogService
{
    public string? ShowFolderBrowserDialog()
    {
        var dialog = new OpenFolderDialog { Title = "Select Source Directory" };
        return dialog.ShowDialog() == true ? dialog.FolderName : null;
    }

    public string? ShowFileBrowserDialog()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select options.json file",
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}