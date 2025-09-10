using System.Text;

using Microsoft.Extensions.Logging;

namespace RepoAIfyLib.Services;

public class MarkdownGenerator
{
    private readonly ILogger<MarkdownGenerator> _logger;
    private readonly int _maxChunkSizeKb;
    private const int BytesPerKb = 1024;

    public MarkdownGenerator(ILogger<MarkdownGenerator> logger, int maxChunkSizeKb)
    {
        _logger = logger;
        _maxChunkSizeKb = maxChunkSizeKb;
    }

    public async IAsyncEnumerable<string> GenerateMarkdown(IEnumerable<FileProcessor.FileInfoDetails> files, DirectoryInfo baseDirectory, string repositoryOverview, Options options)
    {
        var currentChunkContent = new StringBuilder();
        long currentChunkBytes = 0;
        var chunkCount = 1;

        // --- Initial Header for the first chunk ---
        var firstHeader = new StringBuilder();
        firstHeader.AppendLine(GenerateHeader(baseDirectory.Name));
        firstHeader.AppendLine(repositoryOverview);
        string firstHeaderString = firstHeader.ToString();
        currentChunkContent.Append(firstHeaderString);
        currentChunkBytes += Encoding.UTF8.GetByteCount(firstHeaderString);

        foreach (var fileTuple in files)
        {
            var file = fileTuple.File;
            var relativePath = fileTuple.RelativePath;
            var fileExtension = file.Extension;
            var fileExtensionWithoutDot = string.IsNullOrEmpty(fileExtension) ? "" : fileExtension.Substring(1);

            // Check file size before attempting to read
            if (options.FileFilter.MaxFileSizeMb > 0 && file.Length > options.FileFilter.MaxFileSizeMb * 1024 * 1024)
            {
                _logger.LogWarning("Skipping file '{RelativePath}' because its size ({FileSize} MB) exceeds the configured limit of {MaxFileSize} MB.",
                    relativePath, file.Length / 1024.0 / 1024.0, options.FileFilter.MaxFileSizeMb);
                continue; // Skip to the next file
            }

            var fileContentBuilder = new StringBuilder();
            try
            {
                // For files under the limit, we can still read them efficiently.
                // For a full streaming implementation, you would use StreamReader here.
                // This implementation prioritizes the size check, which is the most critical part.
                fileContentBuilder.Append(await File.ReadAllTextAsync(file.FullName));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not read file '{FilePath}'", file.FullName);
                fileContentBuilder.Append($"Error reading file: {ex.Message}");
            }
            string fileContent = fileContentBuilder.ToString();

            // Generate the full markdown for the current file into a temporary string
            var fileMarkdownBuilder = new StringBuilder();
            fileMarkdownBuilder.AppendLine($"\n### File: `{relativePath}`");
            fileMarkdownBuilder.AppendLine($"*   **Full Path:** `{file.FullName}`");
            fileMarkdownBuilder.AppendLine($"*   **Extension:** `{fileExtension}`");
            fileMarkdownBuilder.AppendLine();
            fileMarkdownBuilder.AppendLine($"``` {fileExtensionWithoutDot}");
            fileMarkdownBuilder.AppendLine(fileContent);
            fileMarkdownBuilder.AppendLine("```");
            fileMarkdownBuilder.AppendLine(Constants.FileEndDelimiter);
            fileMarkdownBuilder.AppendLine();

            string fileMarkdown = fileMarkdownBuilder.ToString();
            long newFileBytes = Encoding.UTF8.GetByteCount(fileMarkdown);

            if (_maxChunkSizeKb > 0 && currentChunkBytes > 0 && (currentChunkBytes + newFileBytes) > _maxChunkSizeKb * 1024)
            {
                yield return currentChunkContent.ToString();
                chunkCount++;
                currentChunkContent.Clear();

                // --- Continuation Header for subsequent chunks ---
                var continuationHeader = $"# Repository Analysis: {baseDirectory.Name} (Part {chunkCount})\n\n";
                currentChunkContent.Append(continuationHeader);
                currentChunkBytes = Encoding.UTF8.GetByteCount(continuationHeader);
            }

            currentChunkContent.Append(fileMarkdown);
            currentChunkBytes += newFileBytes;
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
        header.AppendLine(Constants.RepositoryOverviewHeader);
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
                       $"*   **Full Path:** `{{260}}`".Length + // Max path length estimate
                       $"*   **Extension:** `{fileExtension}`".Length +
                       $"``` {fileExtension.Substring(1)}```".Length + // Code fences
                       Constants.FileEndDelimiter.Length + // Explicit file end marker
                       8 * 10; // Newline characters and some buffer

        return Encoding.UTF8.GetByteCount(fileContent) + overhead;
    }
}