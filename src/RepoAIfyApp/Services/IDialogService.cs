namespace RepoAIfyApp.Services;

public interface IDialogService
{
    string? ShowFolderBrowserDialog();
    string? ShowFileBrowserDialog();
}