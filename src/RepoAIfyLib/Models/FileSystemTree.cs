namespace RepoAIfyLib.Models
{
    public class FileSystemTree
    {
        public string? Name { get; set; }
        public string? Path { get; set; }
        public bool IsDirectory { get; set; }
        public List<FileSystemTree> Children { get; set; }

        public FileSystemTree()
        {
            Children = new List<FileSystemTree>();
        }
    }
}
