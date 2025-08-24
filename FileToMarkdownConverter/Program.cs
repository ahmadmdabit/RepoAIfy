using System.CommandLine;
using System.Text;
using System.CommandLine.Invocation;
using FileToMarkdownConverter.Services;

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

        var filteredFiles = fileProcessor.GetFilteredFiles(sourceDirectory, includedExtensions);
        
        var chunkCount = 1;
        await foreach (var chunkContent in markdownGenerator.GenerateMarkdown(filteredFiles, sourceDirectory))
        {
            var outputFileName = chunkCount == 1 ? Constants.OutputFileName : $"output_{chunkCount}.md";
            var outputFilePath = Path.Combine(outputDirectory, outputFileName);
            await File.WriteAllTextAsync(outputFilePath, chunkContent);
            Console.WriteLine($"Successfully generated markdown chunk: {outputFilePath}");
            chunkCount++;
        }

        if (chunkCount == 1) // No chunks were generated, meaning no files were processed or content was empty
        {
            Console.WriteLine("No markdown content was generated. Check source directory, file filters, and file contents.");
        }
    }
}
