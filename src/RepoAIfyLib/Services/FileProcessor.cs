using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace RepoAIfyLib.Services;

public class FileProcessor
{
    private readonly Matcher _matcher;
    private readonly DirectoryInfo _baseDirectory;

    public record FileInfoDetails(FileInfo File, string RelativePath);


    public FileProcessor(Options options, DirectoryInfo baseDirectory)
    {
        _matcher = new Matcher(StringComparison.OrdinalIgnoreCase);
        foreach (var pattern in options.FileFilter.ExcludedDirectories)
        {
            _matcher.AddInclude(pattern);
        }
        _baseDirectory = baseDirectory;
    }

    public (List<FileInfoDetails> FilteredFiles, List<string> AllRelativeDirectories) GetFilteredFiles(DirectoryInfo sourceDirectory, HashSet<string> includedExtensions)
    {
        var filteredFiles = new List<FileInfoDetails>();
        var allRelativeDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!sourceDirectory.Exists)
        {
            Console.Error.WriteLine($"Error: Source directory '{sourceDirectory.FullName}' does not exist.");
            return (filteredFiles, allRelativeDirectories.ToList());
        }

        foreach (var file in sourceDirectory.EnumerateFiles("*", SearchOption.AllDirectories))
        {
            var fileExtension = file.Extension;

            // Check if extension is included
            if (!includedExtensions.Contains(fileExtension))
            {
                continue;
            }

            // Get the relative path for glob matching
            var relativeFilePath = Path.GetRelativePath(_baseDirectory.FullName, file.FullName);

            // Check if directory is excluded using the glob matcher
            var fileMatch = _matcher.Match(relativeFilePath);
            if (fileMatch.HasMatches)
            {
                continue;
            }

            filteredFiles.Add(new FileInfoDetails(file, relativeFilePath));

            // Add all parent directories of the current file to the set of all relative directories
            var currentDir = Path.GetDirectoryName(relativeFilePath);
            while (!string.IsNullOrEmpty(currentDir) && currentDir != ".")
            {
                allRelativeDirectories.Add(currentDir.Replace('\\', '/'));
                currentDir = Path.GetDirectoryName(currentDir);
            }
        }

        // Add the root source directory itself
        allRelativeDirectories.Add(".");

        return (filteredFiles, allRelativeDirectories.OrderBy(x => x).ToList());
    }
}