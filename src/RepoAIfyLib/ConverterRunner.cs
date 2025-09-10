using Microsoft.Extensions.Logging;

using RepoAIfyLib.Services;

namespace RepoAIfyLib
{
    public class ConverterRunner
    {
        private readonly ILogger<ConverterRunner> _logger;
        private readonly FileProcessor _fileProcessor;
        private readonly Func<int, MarkdownGenerator> _markdownGeneratorFactory;

        public ConverterRunner(ILogger<ConverterRunner> logger, FileProcessor fileProcessor, Func<int, MarkdownGenerator> markdownGeneratorFactory)
        {
            _logger = logger;
            _fileProcessor = fileProcessor;
            _markdownGeneratorFactory = markdownGeneratorFactory;
        }

        public async Task Run(DirectoryInfo sourceDirectory, Options options)
        {
            if (options == null)
            {
                _logger.LogError("Options object cannot be null.");
                return;
            }

            var (filteredFiles, allRelativeDirectories) = _fileProcessor.GetFilteredFiles(
                sourceDirectory,
                options.FileFilter.IncludedExtensions,
                options.FileFilter.ExcludedDirectories
            );

            await ProcessFiles(sourceDirectory, options, filteredFiles, allRelativeDirectories);
        }

        public async Task Run(DirectoryInfo sourceDirectory, Options options, IEnumerable<string> filesToInclude)
        {
            if (options == null)
            {
                _logger.LogError("Options object cannot be null.");
                return;
            }

            var filteredFiles = filesToInclude.Select(filePath =>
            {
                var fileInfo = new FileInfo(filePath);
                var relativePath = Path.GetRelativePath(sourceDirectory.FullName, fileInfo.FullName);
                return new FileProcessor.FileInfoDetails(fileInfo, relativePath);
            }).ToList();

            var allRelativeDirectories = filteredFiles
                .Select(f => Path.GetDirectoryName(f.RelativePath))
                .Where(d => !string.IsNullOrEmpty(d))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            await ProcessFiles(sourceDirectory, options, filteredFiles, allRelativeDirectories!);
        }

        private async Task ProcessFiles(DirectoryInfo sourceDirectory, Options options, List<FileProcessor.FileInfoDetails> filteredFiles, List<string> allRelativeDirectories)
        {
            var markdownGenerator = _markdownGeneratorFactory(options.Chunking.MaxChunkSizeKb);
            var outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), options.Output.OutputDirectory ?? Constants.DefaultOutputDirectory);
            Directory.CreateDirectory(outputDirectory);

            _logger.LogInformation("Processing files from: {SourceDirectory}", sourceDirectory.FullName);
            _logger.LogInformation("Output will be written to: {OutputDirectory}", outputDirectory);

            if (!filteredFiles.Any())
            {
                _logger.LogWarning("No files matched the criteria. No markdown content was generated.");
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
            await foreach (var chunkContent in markdownGenerator.GenerateMarkdown(filteredFiles, sourceDirectory, repositoryOverview, options))
            {
                var outputFileName = chunkCount == 1 ? baseOutputFileName : $"{sourceDirectory.Name.Replace(' ', '-')}_{chunkCount}.md";
                var outputFilePath = Path.Combine(outputDirectory, outputFileName);
                await File.WriteAllTextAsync(outputFilePath, chunkContent);
                _logger.LogInformation("Successfully generated markdown chunk: {OutputFilePath}", outputFilePath);
                chunkedOutputFiles.Add(outputFilePath);
                chunkCount++;
            }
        }
    }
}