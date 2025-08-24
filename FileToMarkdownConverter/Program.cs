using System.CommandLine;
using System.Text.Json;
using System.Text;
using System.CommandLine.Invocation;

namespace FileToMarkdownConverter;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var sourceDirectoryOption = new Option<DirectoryInfo>(
            name: "--source",
            description: "The source directory to read files from."
        ) { IsRequired = true };

        var optionsFileOption = new Option<FileInfo>(
            name: "--options",
            description: "The path to the options.json file."
        ) { IsRequired = true };

        var rootCommand = new RootCommand("A .NET console app to convert source files to markdown.")
        {
            sourceDirectoryOption,
            optionsFileOption
        };

        rootCommand.SetHandler(async (InvocationContext context) =>
        {
            var sourceDirectory = context.ParseResult.GetValueForOption(sourceDirectoryOption);
            var optionsFile = context.ParseResult.GetValueForOption(optionsFileOption);

            if (sourceDirectory == null || optionsFile == null)
            {
                Console.WriteLine("Error: Source directory and options file are required.");
                return;
            }

            await ProcessFiles(sourceDirectory, optionsFile);
        });

        return await rootCommand.InvokeAsync(args);
    }

    static async Task ProcessFiles(DirectoryInfo sourceDirectory, FileInfo optionsFile)
    {
        if (!sourceDirectory.Exists)
        {
            Console.WriteLine($"Error: Source directory '{sourceDirectory.FullName}' does not exist.");
            return;
        }

        if (!optionsFile.Exists)
        {
            Console.WriteLine($"Error: Options file '{optionsFile.FullName}' does not exist.");
            return;
        }

        Options? options;
        try
        {
            var jsonString = await File.ReadAllTextAsync(optionsFile.FullName);
            options = JsonSerializer.Deserialize<Options>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (options == null)
            {
                Console.WriteLine("Error: Could not deserialize options.json.");
                return;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading or deserializing options.json: {ex.Message}");
            return;
        }

        var outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), options.Output.OutputDirectory);
        Directory.CreateDirectory(outputDirectory);
        var outputFilePath = Path.Combine(outputDirectory, "output.md");

        var includedExtensions = new HashSet<string>(options.FileFilter.IncludedExtensions, StringComparer.OrdinalIgnoreCase);
        var excludedDirectories = options.FileFilter.ExcludedDirectories;

        var markdownContent = new StringBuilder();

        foreach (var file in sourceDirectory.EnumerateFiles("*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceDirectory.FullName, file.FullName);
            var fileExtension = file.Extension;

            // Check if extension is included
            if (!includedExtensions.Contains(fileExtension))
            {
                continue;
            }

            // Check if directory is excluded
            bool isExcluded = false;
            foreach (var excludedPattern in excludedDirectories)
            {
                // Simple glob matching for now. For more complex scenarios, a dedicated glob library would be better.
                // This assumes patterns like "**/bin/**" or "obj/"
                if (IsPathExcluded(file.FullName, sourceDirectory.FullName, excludedPattern))
                {
                    isExcluded = true;
                    break;
                }
            }

            if (isExcluded)
            {
                continue;
            }

            markdownContent.AppendLine($"## File: {relativePath}");
            markdownContent.AppendLine("```");
            try
            {
                markdownContent.AppendLine(await File.ReadAllTextAsync(file.FullName));
            }
            catch (Exception ex)
            {
                markdownContent.AppendLine($"Error reading file: {ex.Message}");
            }
            markdownContent.AppendLine("```");
            markdownContent.AppendLine();
        }

        await File.WriteAllTextAsync(outputFilePath, markdownContent.ToString());
        Console.WriteLine($"Successfully generated markdown file: {outputFilePath}");
    }

    private static bool IsPathExcluded(string filePath, string baseDirectory, string excludedPattern)
    {
        // Normalize paths for comparison
        var normalizedFilePath = filePath.Replace('\\', '/').ToLowerInvariant();
        var normalizedBaseDirectory = baseDirectory.Replace('\\', '/').ToLowerInvariant();
        var normalizedExcludedPattern = excludedPattern.Replace('\\', '/').ToLowerInvariant();

        // Handle patterns like "**/bin/**"
        if (normalizedExcludedPattern.StartsWith("**/" ) && normalizedExcludedPattern.EndsWith("/**"))
        {
            var dirName = normalizedExcludedPattern.Substring(3, normalizedExcludedPattern.Length - 6);
            return normalizedFilePath.Contains($"/{dirName}/");
        }
        // Handle patterns like "obj/"
        else if (normalizedExcludedPattern.EndsWith("/"))
        {
            var dirName = normalizedExcludedPattern.TrimEnd('/');
            return normalizedFilePath.Contains($"/{dirName}/");
        }
        // Handle patterns like ".git" (exact directory name match)
        else
        {
            var directoryName = Path.GetFileName(Path.GetDirectoryName(filePath));
            return normalizedFilePath.Contains($"/{normalizedExcludedPattern}/");
        }
    }
}
