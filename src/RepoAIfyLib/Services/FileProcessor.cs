using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace RepoAIfyLib.Services;

public class FileProcessor
{
    public record FileInfoDetails(FileInfo File, string RelativePath);

    public (List<FileInfoDetails> FilteredFiles, List<string> AllRelativeDirectories) GetFilteredFiles(
        DirectoryInfo sourceDirectory,
        IEnumerable<string> includedExtensions,
        IEnumerable<string> excludedDirectoryPatterns)
    {
        var matcher = new Matcher(StringComparison.OrdinalIgnoreCase);
        
        // Add patterns to include files with the specified extensions from any directory.
        matcher.AddIncludePatterns(includedExtensions.Select(ext => $"**/*{ext}"));
        
        // Add patterns to exclude directories.
        matcher.AddExcludePatterns(excludedDirectoryPatterns);

        // Execute the matcher to get a list of relative file paths that match.
        var result = matcher.Execute(new DirectoryInfoWrapper(sourceDirectory));

        var filteredFiles = result.Files.Select(match =>
        {
            var fullPath = Path.Combine(sourceDirectory.FullName, match.Path);
            var fileInfo = new FileInfo(fullPath);
            // Ensure the relative path uses forward slashes for consistency.
            var relativePath = match.Path.Replace('\\', '/');
            return new FileInfoDetails(fileInfo, relativePath);
        }).ToList();

        // Derive the set of unique parent directories from the list of filtered files.
        var allRelativeDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var fileDetail in filteredFiles)
        {
            var currentDir = Path.GetDirectoryName(fileDetail.RelativePath);
            while (!string.IsNullOrEmpty(currentDir) && currentDir != ".")
            {
                allRelativeDirectories.Add(currentDir.Replace('\\', '/'));
                currentDir = Path.GetDirectoryName(currentDir);
            }
        }
        // Add the root directory itself.
        if (filteredFiles.Any())
        {
            allRelativeDirectories.Add(".");
        }

        return (filteredFiles, allRelativeDirectories.OrderBy(x => x).ToList());
    }
}
