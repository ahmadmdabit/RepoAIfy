using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RepoAIfyLib;
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
            config.WriteTo.Console();
            config.WriteTo.File(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "RepoAIfy",
                    "RepoAIfy-.log"
                ),
                rollingInterval: RollingInterval.Day
            );
        });

        // Register services
        builder.Services.AddTransient<OptionsLoader>();
        builder.Services.AddTransient<FileProcessor>();
        builder.Services.AddTransient<ConverterRunner>();
        builder.Services.AddTransient<TreeViewDataService>();
        builder.Services.AddTransient<MarkdownGenerator>();
        builder.Services.AddTransient<Func<int, MarkdownGenerator>>(provider => 
            (maxChunkSizeKb) => new MarkdownGenerator(provider.GetRequiredService<ILogger<MarkdownGenerator>>(), maxChunkSizeKb));

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

            rootCommand.SetHandler(async (InvocationContext context) =>
            {
                var sourceDirectory = context.ParseResult.GetValueForOption(sourceDirectoryOption);
                var optionsFile = context.ParseResult.GetValueForOption(optionsFileOption);

                if (sourceDirectory == null || optionsFile == null)
                {
                    logger.LogError("Error: Source directory and options file are required.");
                    context.ExitCode = 1;
                    return;
                }

                var optionsLoader = host.Services.GetRequiredService<OptionsLoader>();
                var options = await optionsLoader.LoadOptions(optionsFile);

                if (options == null)
                {
                    logger.LogError("Error: Could not load options from {OptionsFile}", optionsFile.FullName);
                    context.ExitCode = 1;
                    return;
                }

                var converterRunner = host.Services.GetRequiredService<ConverterRunner>();
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