using System.Text;

using Microsoft.Extensions.Logging;

using RepoAIfyLib.Services;

namespace RepoAIfyLib
{
    public class ConverterRunner
    {
        private readonly ILogger<ConverterRunner> logger;

        public ConverterRunner(ILogger<ConverterRunner> logger)
        {
            this.logger = logger;
        }

        public async Task Run(DirectoryInfo sourceDirectory, Options options)
        {
            if (options == null)
            {
                logger.LogError("Options object cannot be null.");
                return;
            }

            var fileProcessor = new FileProcessor();
            var (filteredFiles, allRelativeDirectories) = fileProcessor.GetFilteredFiles(
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
                logger.LogError("Options object cannot be null.");
                return;
            }

            var filteredFiles = filesToInclude.Select(filePath => {
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
            var markdownGenerator = new MarkdownGenerator(options.Chunking.MaxChunkSizeKb);
            var outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), options.Output.OutputDirectory ?? Constants.DefaultOutputDirectory);
            Directory.CreateDirectory(outputDirectory);

            logger.LogInformation($"Processing files from: {sourceDirectory.FullName}");
            logger.LogInformation($"Output will be written to: {outputDirectory}");

            if (!filteredFiles.Any())
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
            await foreach (var chunkContent in markdownGenerator.GenerateMarkdown(filteredFiles, sourceDirectory, repositoryOverview))
            {
                var outputFileName = chunkCount == 1 ? baseOutputFileName : $"{sourceDirectory.Name.Replace(' ', '-')}_{chunkCount}.md";
                var outputFilePath = Path.Combine(outputDirectory, outputFileName);
                await File.WriteAllTextAsync(outputFilePath, chunkContent);
                logger.LogInformation($"Successfully generated markdown chunk: {outputFilePath}");
                chunkedOutputFiles.Add(outputFilePath);
                chunkCount++;
            }
        }
    }
}