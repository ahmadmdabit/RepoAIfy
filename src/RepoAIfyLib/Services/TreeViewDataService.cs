using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.FileSystemGlobbing;
using RepoAIfyLib.Models;

namespace RepoAIfyLib.Services
{
    public class TreeViewDataService
    {
        public FileSystemTree GetFileSystemTree(string rootPath, List<string> includedExtensions, List<string> excludedDirectories)
        {
            var includedExtensionsSet = new HashSet<string>(includedExtensions, System.StringComparer.OrdinalIgnoreCase);
            
            var matcher = new Matcher(System.StringComparison.OrdinalIgnoreCase);
            matcher.AddIncludePatterns(excludedDirectories);

            var root = new FileSystemTree { Name = Path.GetFileName(rootPath), Path = rootPath, IsDirectory = true };
            PopulateChildren(root, includedExtensionsSet, matcher, rootPath);
            return root;
        }

        private void PopulateChildren(FileSystemTree parentNode, HashSet<string> includedExtensions, Matcher matcher, string basePath)
        {
            if (parentNode.Path == null) return;

            try
            {
                foreach (var directory in Directory.GetDirectories(parentNode.Path))
                {
                    var relativePath = Path.GetRelativePath(basePath, directory);
                    if (matcher.Match(relativePath + Path.DirectorySeparatorChar).HasMatches) continue;

                    var childNode = new FileSystemTree { Name = Path.GetFileName(directory), Path = directory, IsDirectory = true };
                    parentNode.Children.Add(childNode);
                    PopulateChildren(childNode, includedExtensions, matcher, basePath);
                }

                foreach (var file in Directory.GetFiles(parentNode.Path))
                {
                    var relativePath = Path.GetRelativePath(basePath, file);
                    if (matcher.Match(relativePath).HasMatches) continue;

                    if (!includedExtensions.Contains(Path.GetExtension(file))) continue;

                    var childNode = new FileSystemTree { Name = Path.GetFileName(file), Path = file, IsDirectory = false };
                    parentNode.Children.Add(childNode);
                }
            }
            catch (System.UnauthorizedAccessException)
            {
                // Ignore
            }
        }
    }
}
