using System.Text;

namespace RepoAIfy.Services;

public class MarkdownGenerator
{
    private readonly int _maxChunkSizeKb;
    private const int BytesPerKb = 1024;

    public MarkdownGenerator(int maxChunkSizeKb)
    {
        _maxChunkSizeKb = maxChunkSizeKb;
    }

    public async IAsyncEnumerable<string> GenerateMarkdown(IEnumerable<(FileInfo File, string RelativePath)> files, DirectoryInfo baseDirectory)
    {
        var currentChunkContent = new StringBuilder();
        var chunkCount = 1;

        // Generate header for the first chunk
        var headerContent = GenerateHeader(baseDirectory.Name);
        var headerSize = Encoding.UTF8.GetByteCount(headerContent);

        if (_maxChunkSizeKb > 0 && (double)headerSize / BytesPerKb > _maxChunkSizeKb)
        {
            Console.Error.WriteLine($"Warning: The generated header alone ({(double)headerSize / BytesPerKb:F2}KB) exceeds the MaxChunkSizeKb ({_maxChunkSizeKb}KB). Header will be in its own chunk.");
            yield return headerContent;
        }
        else
        {
            currentChunkContent.AppendLine(headerContent);
        }

        foreach (var fileTuple in files)
        {
            var file = fileTuple.File;
            var relativePath = fileTuple.RelativePath;
            var fileExtension = file.Extension;
            var fileExtensionWithoutDot = string.IsNullOrEmpty(fileExtension) ? "" : fileExtension.Substring(1);

            string fileContent;
            try
            {
                fileContent = await File.ReadAllTextAsync(file.FullName);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Warning: Could not read file '{file.FullName}': {ex.Message}");
                fileContent = $"Error reading file: {ex.Message}";
            }

            // Estimate the size of the current file's markdown representation
            var fileMarkdownSize = GetEstimatedMarkdownSize(relativePath, fileExtension, fileContent);

            // Check if adding this file would exceed the max chunk size
            if (_maxChunkSizeKb > 0 && (currentChunkContent.Length + fileMarkdownSize) / BytesPerKb > _maxChunkSizeKb)
            {
                yield return currentChunkContent.ToString();
                currentChunkContent.Clear();
                chunkCount++;
            }

            currentChunkContent.AppendLine($"\n### File: `{relativePath}`"); // Use proper Markdown heading
            currentChunkContent.AppendLine($"*   **Full Path:** `{file.FullName}`");
            currentChunkContent.AppendLine($"*   **Extension:** `{fileExtension}`");
            currentChunkContent.AppendLine();
            currentChunkContent.AppendLine($"``` {fileExtensionWithoutDot}");
            currentChunkContent.AppendLine(fileContent);
            currentChunkContent.AppendLine("```");
            currentChunkContent.AppendLine(Constants.FileEndDelimiter); // Added explicit file end marker
            currentChunkContent.AppendLine(); // Add a blank line after each file entry
        }

        if (currentChunkContent.Length > 0)
        {
            yield return currentChunkContent.ToString();
        }
    }

    private string GenerateHeader(string sourceDirectoryName)
    {
        var header = new StringBuilder();
        header.AppendLine($"# Repository Analysis: {sourceDirectoryName}");
        header.AppendLine();
        header.AppendLine("This document provides a consolidated view of the source code from the specified repository, filtered and chunked for AI analysis. Each file's content is presented within a clearly delimited section, including its full path, relative path, and extension.");
        header.AppendLine();
        header.AppendLine("## Repository Overview"); // Changed to proper Markdown heading
        header.AppendLine();
        return header.ToString();
    }

    public string GenerateRepositoryStructureOverview(IEnumerable<string> relativeFilePaths, IEnumerable<string> allRelativeDirectoryPaths)
    {
        var overview = new StringBuilder();
        overview.AppendLine("### Repository Structure"); // Sub-heading for the structure
        overview.AppendLine();

        var allPaths = new HashSet<string>(allRelativeDirectoryPaths.Where(d => d != "."), StringComparer.OrdinalIgnoreCase);
        foreach (var filePath in relativeFilePaths)
        {
            allPaths.Add(filePath);
        }

        var sortedPaths = allPaths.OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();

        var lastPathSegments = new string[0];

        foreach (var path in sortedPaths)
        {
            var pathSegments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var commonSegments = 0;

            for (int i = 0; i < Math.Min(pathSegments.Length, lastPathSegments.Length); i++)
            {
                if (pathSegments[i].Equals(lastPathSegments[i], StringComparison.OrdinalIgnoreCase))
                {
                    commonSegments++;
                }
                else
                {
                    break;
                }
            }

            // Close previous levels
            for (int i = lastPathSegments.Length - 1; i >= commonSegments; i--)
            {
                // No explicit closing needed for markdown list items, just adjust indent
            }

            // Open new levels
            for (int i = commonSegments; i < pathSegments.Length; i++)
            {
                var indent = new string(' ', i * 2);
                if (path.EndsWith(pathSegments[i], StringComparison.OrdinalIgnoreCase) && relativeFilePaths.Contains(path))
                {
                    // It's a file
                    overview.AppendLine($"{indent}- {pathSegments[i]}");
                }
                else
                {
                    // It's a directory
                    overview.AppendLine($"{indent}- **{pathSegments[i]}**/");
                }
            }
            lastPathSegments = pathSegments;
        }
        overview.AppendLine();
        return overview.ToString();
    }

    private int GetEstimatedMarkdownSize(string relativePath, string fileExtension, string fileContent)
    {
        // Rough estimation of markdown overhead (delimiters, metadata lines, code block fences)
        var overhead = $"\n### File: `{relativePath}`".Length + // File heading
                       $"*   **Full Path:** `{260}`".Length + // Max path length estimate
                       $"*   **Extension:** `{fileExtension}`".Length + 
                       $"``` {fileExtension.Substring(1)}```".Length + // Code fences
                       Constants.FileEndDelimiter.Length + // Explicit file end marker
                       (8 * 10); // Newline characters and some buffer

        return Encoding.UTF8.GetByteCount(fileContent) + overhead;
    }
}
