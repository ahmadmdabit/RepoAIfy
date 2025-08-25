using System.CommandLine;
using System.Text;
using System.CommandLine.Invocation;
using RepoAIfy.Services;

namespace RepoAIfy;

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
                Console.Error.WriteLine("Error: Source directory and options file are required.");
                return;
            }

            await RunConverter(sourceDirectory, optionsFile);
        });

        return await rootCommand.InvokeAsync(args);
    }

    static async Task RunConverter(DirectoryInfo sourceDirectory, FileInfo optionsFile)
    {
        var optionsLoader = new OptionsLoader();
        var options = await optionsLoader.LoadOptions(optionsFile);

        if (options == null)
        {
            return;
        }

        var fileProcessor = new FileProcessor(options, sourceDirectory);
        var markdownGenerator = new MarkdownGenerator(options.Chunking.MaxChunkSizeKb);

        var outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), options.Output.OutputDirectory ?? Constants.DefaultOutputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var includedExtensions = new HashSet<string>(options.FileFilter.IncludedExtensions, StringComparer.OrdinalIgnoreCase);

        Console.WriteLine($"Processing files from: {sourceDirectory.FullName}");
        Console.WriteLine($"Output will be written to: {outputDirectory}");

        var (filteredFiles, allRelativeDirectories) = fileProcessor.GetFilteredFiles(sourceDirectory, includedExtensions);

        if (!filteredFiles.Any())
        {
            Console.WriteLine("No files matched the criteria. No markdown content was generated.");
            return;
        }

        var repositoryOverview = markdownGenerator.GenerateRepositoryStructureOverview(
            filteredFiles.Select(f => f.RelativePath),
            allRelativeDirectories
        );

        var baseOutputFileName = sourceDirectory.Name.Replace(' ', '-') + ".md";
        var chunkedOutputFiles = new List<string>();

        var chunkCount = 1;
        await foreach (var chunkContent in markdownGenerator.GenerateMarkdown(filteredFiles, sourceDirectory))
        {
            var outputFileName = chunkCount == 1 ? baseOutputFileName : $"{sourceDirectory.Name.Replace(' ', '-')}_{chunkCount}.md";
            var outputFilePath = Path.Combine(outputDirectory, outputFileName);
            await File.WriteAllTextAsync(outputFilePath, chunkContent);
            Console.WriteLine($"Successfully generated markdown chunk: {outputFilePath}");
            chunkedOutputFiles.Add(outputFilePath);
            chunkCount++;
        }

        // Insert the repository overview into the first chunk
        if (chunkedOutputFiles.Any())
        {
            var firstChunkContent = await File.ReadAllTextAsync(chunkedOutputFiles.First());
            var overviewMarker = "## Repository Overview"; // Now a proper Markdown heading
            var insertIndex = firstChunkContent.IndexOf(overviewMarker);

            if (insertIndex != -1)
            {
                var updatedContent = new StringBuilder(firstChunkContent);
                // Insert after the heading and its immediate newline
                updatedContent.Insert(insertIndex + overviewMarker.Length + Environment.NewLine.Length, Environment.NewLine + repositoryOverview);
                await File.WriteAllTextAsync(chunkedOutputFiles.First(), updatedContent.ToString());
                Console.WriteLine($"Successfully inserted repository overview into {chunkedOutputFiles.First()}");
            }
            else
            {
                Console.Error.WriteLine($"Warning: Could not find repository overview marker in {chunkedOutputFiles.First()}. Overview not inserted.");
            }
        }
    }
}
