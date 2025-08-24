using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace FileToMarkdownConverter.Services;

public class FileProcessor
{
    private readonly Matcher _matcher;
    private readonly DirectoryInfo _baseDirectory;

    public FileProcessor(Options options, DirectoryInfo baseDirectory)
    {
        _matcher = new Matcher(StringComparison.OrdinalIgnoreCase);
        foreach (var pattern in options.FileFilter.ExcludedDirectories)
        {
            _matcher.AddInclude(pattern);
        }
        _baseDirectory = baseDirectory;
    }

    public IEnumerable<FileInfo> GetFilteredFiles(DirectoryInfo sourceDirectory, HashSet<string> includedExtensions)
    {
        if (!sourceDirectory.Exists)
        {
            Console.Error.WriteLine($"Error: Source directory '{sourceDirectory.FullName}' does not exist.");
            yield break;
        }

        var directoryInfoWrapper = new DirectoryInfoWrapper(sourceDirectory);

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

            yield return file;
        }
    }
}