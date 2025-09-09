using System.CommandLine;
using System.CommandLine.Invocation;

using Microsoft.Extensions.Logging;

using RepoAIfyLib;

using Serilog;

namespace RepoAIfy;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("RepoAIfy.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddSerilog();
            });

            var logger = loggerFactory.CreateLogger<ConverterRunner>();

            var converterRunner = new ConverterRunner(logger);

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
                    Log.Error("Error: Source directory and options file are required.");
                    context.ExitCode = 1;
                    return;
                }

                await converterRunner.Run(sourceDirectory, optionsFile);
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