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

            var fileProcessor = new FileProcessor(options, sourceDirectory);
            var includedExtensions = new HashSet<string>(options.FileFilter.IncludedExtensions, StringComparer.OrdinalIgnoreCase);
            var (filteredFiles, allRelativeDirectories) = fileProcessor.GetFilteredFiles(sourceDirectory, includedExtensions);

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
            await foreach (var chunkContent in markdownGenerator.GenerateMarkdown(filteredFiles, sourceDirectory))
            {
                var outputFileName = chunkCount == 1 ? baseOutputFileName : $"{sourceDirectory.Name.Replace(' ', '-')}_{chunkCount}.md";
                var outputFilePath = Path.Combine(outputDirectory, outputFileName);
                await File.WriteAllTextAsync(outputFilePath, chunkContent);
                logger.LogInformation($"Successfully generated markdown chunk: {outputFilePath}");
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
                    logger.LogInformation($"Successfully inserted repository overview into {chunkedOutputFiles.First()}");
                }
                else
                {
                    logger.LogWarning($"Warning: Could not find repository overview marker in {chunkedOutputFiles.First()}. Overview not inserted.");
                }
            }
        }
    }
}