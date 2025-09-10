// RepoAIfyApp/GeneratedFileViewModel.cs
namespace RepoAIfyApp
{
    public class GeneratedFileViewModel : ViewModelBase
    {
        private string _fileName = string.Empty;
        public string FileName
        {
            get => _fileName;
            set => SetField(ref _fileName, value);
        }

        private string _content = string.Empty;
        public string Content
        {
            get => _content;
            set => SetField(ref _content, value);
        }
    }
}