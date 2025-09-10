using Microsoft.Extensions.Logging;
using System.Text.Json;

using RepoAIfyLib;

namespace RepoAIfyLib.Services;

public class OptionsLoader
{
    private readonly ILogger<OptionsLoader> _logger;

    public OptionsLoader(ILogger<OptionsLoader> logger)
    {
        _logger = logger;
    }

    public async Task<Options?> LoadOptions(FileInfo optionsFile)
    {
        if (!optionsFile.Exists)
        {
            _logger.LogError("Options file '{FilePath}' does not exist.", optionsFile.FullName);
            return null;
        }

        try
        {
            var jsonString = await File.ReadAllTextAsync(optionsFile.FullName);
            var options = JsonSerializer.Deserialize<Options>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (options == null)
            {
                _logger.LogError("Error: Could not deserialize options.json. Check its content and format.");
                return null;
            }

            // Basic path validation for OutputDirectory
            if (!string.IsNullOrEmpty(options.Output.OutputDirectory))
            {
                string sandboxRoot = Path.GetFullPath(Directory.GetCurrentDirectory());
                string resolvedOutputPath = Path.GetFullPath(Path.Combine(sandboxRoot, options.Output.OutputDirectory));

                if (!resolvedOutputPath.StartsWith(sandboxRoot, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Security Warning: Output directory '{OutputDirectory}' resolves outside the application sandbox. Path traversal detected. Using default output directory.", options.Output.OutputDirectory);
                    options.Output.OutputDirectory = Constants.DefaultOutputDirectory;
                }
                else
                {
                    // The path is safe, so we can use the resolved, absolute path.
                    options.Output.OutputDirectory = resolvedOutputPath;
                }
            }

            return options;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading or deserializing options.json");
            return null;
        }
    }
}