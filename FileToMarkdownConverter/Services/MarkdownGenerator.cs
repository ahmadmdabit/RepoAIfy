using System.Text;

namespace FileToMarkdownConverter.Services;

public class MarkdownGenerator
{
    private readonly int _maxChunkSizeKb;
    private const int BytesPerKb = 1024;

    public MarkdownGenerator(int maxChunkSizeKb)
    {
        _maxChunkSizeKb = maxChunkSizeKb;
    }

    public async IAsyncEnumerable<string> GenerateMarkdown(IEnumerable<FileInfo> files, DirectoryInfo baseDirectory)
    {
        var currentChunkContent = new StringBuilder();
        var chunkCount = 1;

        foreach (var file in files)
        {
            var relativePath = Path.GetRelativePath(baseDirectory.FullName, file.FullName);
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

            currentChunkContent.AppendLine(Constants.FileStartDelimiter);
            currentChunkContent.AppendLine($"File Path: {file.FullName}");
            currentChunkContent.AppendLine($"Relative Path: {relativePath}");
            currentChunkContent.AppendLine($"Extension: {fileExtension}");
            currentChunkContent.AppendLine();
            currentChunkContent.AppendLine($"``` {fileExtensionWithoutDot}");
            currentChunkContent.AppendLine(fileContent);
            currentChunkContent.AppendLine("```");
            currentChunkContent.AppendLine(Constants.FileEndDelimiter);
            currentChunkContent.AppendLine();
        }

        if (currentChunkContent.Length > 0)
        {
            yield return currentChunkContent.ToString();
        }
    }

    private int GetEstimatedMarkdownSize(string relativePath, string fileExtension, string fileContent)
    {
        // Rough estimation of markdown overhead (delimiters, metadata lines, code block fences)
        var overhead = Constants.FileStartDelimiter.Length + Constants.FileEndDelimiter.Length + // Delimiters
                       $"File Path: ".Length + 260 + // Max path length estimate
                       $"Relative Path: ".Length + relativePath.Length + 
                       $"Extension: ".Length + fileExtension.Length + 
                       $"``` {fileExtension.Substring(1)}".Length + "```".Length + // Code fences
                       (8 * 10); // Newline characters and some buffer

        return Encoding.UTF8.GetByteCount(fileContent) + overhead;
    }
}