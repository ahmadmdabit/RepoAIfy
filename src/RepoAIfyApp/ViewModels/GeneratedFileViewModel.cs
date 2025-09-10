using RepoAIfyApp.Helpers;

namespace RepoAIfyApp.ViewModels;

public class GeneratedFileViewModel : ViewModelBase
{
    private string fileName = string.Empty;
    public string FileName
    {
        get => fileName;
        set => SetField(ref fileName, value);
    }

    private string content = string.Empty;
    public string Content
    {
        get => content;
        set => SetField(ref content, value);
    }
}