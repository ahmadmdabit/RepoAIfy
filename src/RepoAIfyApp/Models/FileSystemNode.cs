using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RepoAIfyApp.Models;

public class FileSystemNode : INotifyPropertyChanged
{
    private bool? isChecked = true;
    private bool isExpanded;

    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public bool IsDirectory { get; set; }
    public long? FileSize { get; set; }

    public string DisplayName
    {
        get
        {
            if (FileSize.HasValue)
            {
                return $"{Name} ({FormatBytes(FileSize.Value)})";
            }
            return Name;
        }
    }

    public ObservableCollection<FileSystemNode> Children { get; set; }
    public FileSystemNode? Parent { get; set; }

    public FileSystemNode()
    {
        Children = [];
    }

    public bool? IsChecked
    {
        get { return isChecked; }
        set { SetIsChecked(value, true, true); }
    }

    public bool IsExpanded
    {
        get { return isExpanded; }
        set
        {
            if (isExpanded != value)
            {
                isExpanded = value;
                OnPropertyChanged();
            }
        }
    }

    void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
    {
        if (value == isChecked)
            return;

        isChecked = value;

        if (updateChildren && isChecked.HasValue)
        {
            foreach (var child in Children)
            {
                child.SetIsChecked(isChecked, true, false);
            }
        }

        if (updateParent && Parent != null)
        {
            Parent.VerifyCheckState();
        }

        OnPropertyChanged(nameof(IsChecked));
    }

    void VerifyCheckState()
    {
        bool? state = null;
        for (int i = 0; i < Children.Count; ++i)
        {
            bool? current = Children[i].IsChecked;
            if (i == 0)
            {
                state = current;
            }
            else if (state != current)
            {
                state = null;
                break;
            }
        }
        SetIsChecked(state, false, true);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public IEnumerable<string> GetCheckedFilePaths()
    {
        var checkedFiles = new List<string>();
        GetCheckedFilesRecursive(this, checkedFiles);
        return checkedFiles;
    }

    private void GetCheckedFilesRecursive(FileSystemNode node, List<string> checkedFiles)
    {
        if (node.IsChecked == true && !node.IsDirectory && node.Path != null)
        {
            checkedFiles.Add(node.Path);
        }

        foreach (var child in node.Children)
        {
            GetCheckedFilesRecursive(child, checkedFiles);
        }
    }

    private static string FormatBytes(long bytes)
    {
        string[] suffixes = ["B", "KB", "MB", "GB", "TB"];
        int i = 0;
        double dblSByte = bytes;
        while (dblSByte >= 1024 && i < suffixes.Length - 1)
        {
            dblSByte /= 1024;
            i++;
        }
        return $"{dblSByte:0.##} {suffixes[i]}";
    }
}
