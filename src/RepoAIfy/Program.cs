using System.CommandLine;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using RepoAIfyLib.Services;

using Serilog;

namespace RepoAIfy;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Configure Serilog
        builder.Services.AddSerilog(config =>
        {
            config
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
                .WriteTo.Debug() // Add the Debug sink
                .WriteTo.Console()
                .WriteTo.File(
                    Path.Combine(
                        AppContext.BaseDirectory,
                        "Logs",
                        "RepoAIfy-.log"
                    ),
                    rollingInterval: RollingInterval.Day
                );
        });

        // Register services
        builder.Services.AddTransient<OptionsLoaderService>();
        builder.Services.AddTransient<FileProcessorService>();
        builder.Services.AddTransient<ConverterRunnerService>();
        builder.Services.AddTransient<TreeViewDataService>();
        builder.Services.AddTransient<MarkdownGeneratorService>();
        builder.Services.AddTransient<Func<int, MarkdownGeneratorService>>(provider =>
            (maxChunkSizeKb) => new MarkdownGeneratorService(provider.GetRequiredService<ILogger<MarkdownGeneratorService>>(), maxChunkSizeKb));

        var host = builder.Build();

        try
        {
            var logger = host.Services.GetRequiredService<ILogger<Program>>();

            var sourceDirectoryOption = new Option<DirectoryInfo>(
                name: "--source",
                description: "The source directory to read files from."
            )
            { IsRequired = true };

            var optionsFileOption = new Option<FileInfo>(
                name: "--options",
                description: "The path to the options.json file."
            )
            { IsRequired = true };

            var rootCommand = new RootCommand("A .NET console app to convert source files to markdown.")
            {
                sourceDirectoryOption,
                optionsFileOption
            };

            rootCommand.SetHandler(async context =>
            {
                var sourceDirectory = context.ParseResult.GetValueForOption(sourceDirectoryOption);
                var optionsFile = context.ParseResult.GetValueForOption(optionsFileOption);

                if (sourceDirectory == null || optionsFile == null)
                {
                    logger.LogError("Error: Source directory and options file are required.");
                    context.ExitCode = 1;
                    return;
                }

                var optionsLoader = host.Services.GetRequiredService<OptionsLoaderService>();
                var options = await optionsLoader.LoadOptions(optionsFile);

                if (options == null)
                {
                    logger.LogError("Error: Could not load options from {OptionsFile}", optionsFile.FullName);
                    context.ExitCode = 1;
                    return;
                }

                var converterRunner = host.Services.GetRequiredService<ConverterRunnerService>();
                await converterRunner.Run(sourceDirectory, options);
            });

            return await rootCommand.InvokeAsync(args);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly.");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}