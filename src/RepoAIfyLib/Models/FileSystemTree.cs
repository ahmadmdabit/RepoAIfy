namespace RepoAIfyLib.Models
{
    public class FileSystemTree
    {
        public string Name { get; set; } // Changed from string?
        public string Path { get; set; } // Changed from string?
        public bool IsDirectory { get; set; }
        public List<FileSystemTree> Children { get; set; }

        public FileSystemTree(string name, string path, bool isDirectory)
        {
            Name = name;
            Path = path;
            IsDirectory = isDirectory;
            Children = new List<FileSystemTree>();
        }
    }
}
