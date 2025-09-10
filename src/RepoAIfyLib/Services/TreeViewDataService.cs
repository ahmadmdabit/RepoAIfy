using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.Extensions.Logging;

using RepoAIfyLib.Models;

namespace RepoAIfyLib.Services
{
    public class TreeViewDataService
    {
        private readonly ILogger<TreeViewDataService> _logger;

        public TreeViewDataService(ILogger<TreeViewDataService> logger)
        {
            _logger = logger;
        }

        public FileSystemTree GetFileSystemTree(string rootPath, List<string> includedExtensions, List<string> excludedDirectories)
        {
            var root = new FileSystemTree(Path.GetFileName(rootPath), rootPath, true);
            var directoryNodes = new Dictionary<string, FileSystemTree>(System.StringComparer.OrdinalIgnoreCase)
            {
                { "", root } // The root node corresponds to an empty relative path
            };

            try
            {
                var matcher = new Matcher(System.StringComparison.OrdinalIgnoreCase);
                // Add patterns to include files with the specified extensions.
                matcher.AddIncludePatterns(includedExtensions.Select(ext => $"**/*{ext}"));
                // Add patterns to exclude directories.
                matcher.AddExcludePatterns(excludedDirectories);

                // Use the high-level, correct Execute method to get all matching files.
                var result = matcher.Execute(new DirectoryInfoWrapper(new DirectoryInfo(rootPath)));

                foreach (var fileMatch in result.Files)
                {
                    // The path from the matcher uses forward slashes, which is great for consistency.
                    string relativePath = fileMatch.Path;
                    var pathSegments = relativePath.Split('/');

                    FileSystemTree parentNode = root;

                    // Create directory nodes as needed for the file's path.
                    for (int i = 0; i < pathSegments.Length - 1; i++)
                    {
                        string currentPath = string.Join("/", pathSegments.Take(i + 1));
                        string segmentName = pathSegments[i];

                        if (!directoryNodes.TryGetValue(currentPath, out var childDirNode))
                        {
                            string fullDirPath = Path.Combine(rootPath, currentPath);
                            childDirNode = new FileSystemTree(segmentName, fullDirPath, true);
                            parentNode.Children.Add(childDirNode);
                            directoryNodes[currentPath] = childDirNode;
                        }
                        parentNode = childDirNode;
                    }

                    // Add the file node.
                    string fileName = pathSegments.Last();
                    string fullFilePath = Path.Combine(rootPath, relativePath);
                    var fileNode = new FileSystemTree(fileName, fullFilePath, false);
                    parentNode.Children.Add(fileNode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not fully build the file system tree for {Path}", rootPath);
            }

            return root;
        }
    }
}