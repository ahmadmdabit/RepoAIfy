namespace RepoAIfyLib.Models;

public class FileSystemTree
{
    public string Name { get; set; }
    public string Path { get; set; }
    public bool IsDirectory { get; set; }
    public long? FileSize { get; set; }
    public List<FileSystemTree> Children { get; set; }

    public FileSystemTree(string name, string path, bool isDirectory)
    {
        Name = name;
        Path = path;
        IsDirectory = isDirectory;
        Children = [];
    }
}
