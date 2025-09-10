using Microsoft.Extensions.Logging;

using RepoAIfyLib.Models;

namespace RepoAIfyLib.Services;

public interface IConverterRunnerService
{
    Task Run(DirectoryInfo sourceDirectory, Options options, CancellationToken cancellationToken = default);
    Task Run(DirectoryInfo sourceDirectory, Options options, IEnumerable<string> filesToInclude, CancellationToken cancellationToken = default);
}

public class ConverterRunnerService : IConverterRunnerService
{
    private readonly ILogger<ConverterRunnerService> logger;
    private readonly FileProcessorService fileProcessor;
    private readonly Func<int, MarkdownGeneratorService> markdownGeneratorFactory;

    public ConverterRunnerService(ILogger<ConverterRunnerService> logger, FileProcessorService fileProcessor, Func<int, MarkdownGeneratorService> markdownGeneratorFactory)
    {
        this.logger = logger;
        this.fileProcessor = fileProcessor;
        this.markdownGeneratorFactory = markdownGeneratorFactory;
    }

    public async Task Run(DirectoryInfo sourceDirectory, Options options, CancellationToken cancellationToken = default)
    {
        if (options == null)
        {
            logger.LogError("Options object cannot be null.");
            return;
        }

        var (filteredFiles, allRelativeDirectories) = fileProcessor.GetFilteredFiles(
            sourceDirectory,
            options.FileFilter.IncludedExtensions,
            options.FileFilter.ExcludedDirectories
        );

        await ProcessFiles(sourceDirectory, options, filteredFiles, allRelativeDirectories, cancellationToken);
    }

    public async Task Run(DirectoryInfo sourceDirectory, Options options, IEnumerable<string> filesToInclude, CancellationToken cancellationToken = default)
    {
        if (options == null)
        {
            logger.LogError("Options object cannot be null.");
            return;
        }

        var filteredFiles = filesToInclude.Select(filePath =>
        {
            var fileInfo = new FileInfo(filePath);
            var relativePath = Path.GetRelativePath(sourceDirectory.FullName, fileInfo.FullName);
            return new FileProcessorService.FileInfoDetails(fileInfo, relativePath);
        }).ToList();

        var allRelativeDirectories = filteredFiles
            .Select(f => Path.GetDirectoryName(f.RelativePath))
            .Where(d => !string.IsNullOrEmpty(d))
            .Distinct()
            .Order()
            .ToList();

        await ProcessFiles(sourceDirectory, options, filteredFiles, allRelativeDirectories!, cancellationToken);
    }

    private async Task ProcessFiles(DirectoryInfo sourceDirectory, Options options, List<FileProcessorService.FileInfoDetails> filteredFiles, List<string> allRelativeDirectories, CancellationToken cancellationToken)
    {
        var markdownGenerator = markdownGeneratorFactory(options.Chunking.MaxChunkSizeKb);
        var outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), options.Output.OutputDirectory ?? Constants.DefaultOutputDirectory);
        Directory.CreateDirectory(outputDirectory);

        logger.LogInformation("Processing files from: {SourceDirectory}", sourceDirectory.FullName);
        logger.LogInformation("Output will be written to: {OutputDirectory}", outputDirectory);

        if (filteredFiles.Count == 0)
        {
            logger.LogWarning("No files matched the criteria. No markdown content was generated.");
            return;
        }

        var repositoryOverview = markdownGenerator.GenerateRepositoryStructureOverview(
            filteredFiles.Select(f => f.RelativePath),
            allRelativeDirectories
        );

        var baseOutputFileName = sourceDirectory.Name.Replace(' ', '-') + ".md";
        var chunkedOutputFiles = new List<string>();

        var chunkCount = 1;
        // Pass the repositoryOverview directly into the generator method
        await foreach (var chunkContent in markdownGenerator.GenerateMarkdown(filteredFiles, sourceDirectory, repositoryOverview, options, cancellationToken).WithCancellation(cancellationToken))
        {
            var outputFileName = chunkCount == 1 ? baseOutputFileName : $"{sourceDirectory.Name.Replace(' ', '-')}_{chunkCount}.md";
            var outputFilePath = Path.Combine(outputDirectory, outputFileName);
            await File.WriteAllTextAsync(outputFilePath, chunkContent, cancellationToken);
            logger.LogInformation("Successfully generated markdown chunk: {OutputFilePath}", outputFilePath);
            chunkedOutputFiles.Add(outputFilePath);
            chunkCount++;
        }
    }
}