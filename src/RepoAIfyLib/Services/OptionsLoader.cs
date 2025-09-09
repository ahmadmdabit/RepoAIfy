using System.Text.Json;

using RepoAIfyLib;

namespace RepoAIfyLib.Services;

public class OptionsLoader
{
    public async Task<Options?> LoadOptions(FileInfo optionsFile)
    {
        if (!optionsFile.Exists)
        {
            Console.Error.WriteLine($"Error: Options file '{optionsFile.FullName}' does not exist.");
            return null;
        }

        try
        {
            var jsonString = await File.ReadAllTextAsync(optionsFile.FullName);
            var options = JsonSerializer.Deserialize<Options>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (options == null)
            {
                Console.Error.WriteLine("Error: Could not deserialize options.json. Check its content and format.");
                return null;
            }

            // Basic path validation for OutputDirectory
            if (!string.IsNullOrEmpty(options.Output.OutputDirectory))
            {
                var normalizedOutputPath = Path.GetFullPath(options.Output.OutputDirectory);
                // Prevent path traversal by ensuring the output directory is not outside the current working directory
                // This is a basic check; more robust validation might be needed depending on security requirements.
                if (!normalizedOutputPath.StartsWith(Directory.GetCurrentDirectory(), StringComparison.OrdinalIgnoreCase))
                {
                    Console.Error.WriteLine($"Security Warning: Output directory '{options.Output.OutputDirectory}' attempts to write outside the current working directory. Using default output directory.");
                    options.Output.OutputDirectory = Constants.DefaultOutputDirectory;
                }
            }

            return options;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error reading or deserializing options.json: {ex.Message}");
            return null;
        }
    }
}
