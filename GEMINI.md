# Project: RepoAIfy

## Project Overview

This project is a .NET 9 console application named `RepoAIfy`. Its primary purpose is to process files from a specified source directory, apply filtering rules defined in an `options.json` configuration file, and then consolidate the content of the filtered files into one or more markdown (`.md`) output files. This tool is ideal for generating quick documentation, creating context files for AI models, or simply consolidating relevant source code for review.

Key features include:
*   **Command-Line Argument Parsing:** Utilizes `System.CommandLine` for robust command-line argument handling, accepting a source directory path and an `options.json` file path.
*   **Configuration:** Reads file filtering and output settings from a JSON configuration file (`options.json`).
*   **File Filtering:** Filters files based on included file extensions (e.g., `.cs`, `.json`, `.md`) and excluded directory patterns (e.g., `**/bin/**`, `**/obj/**`).
*   **Smart Chunking:** Automatically splits the output Markdown content into multiple files based on a configurable maximum chunk size, improving manageability for large outputs.
*   **Markdown Output:** Generates markdown file(s) where each processed file's content is presented within a code block, preceded by a heading indicating its relative path.

## Building and Running

### Prerequisites
*   .NET 9 SDK

### Build
To build the `RepoAIfy` application, navigate to the root of the `src` directory and run the following command:

```bash
dotnet build D:\engamd89-dev\dotnet\RepoAIfy\src\RepoAIfy
```

### Run
To run the application, use the following command from the `src` directory. Remember to replace `"D:\engamd89-dev\dotnet\RepoAIfy\src\YourSourceDirectory"` with the actual absolute path to the source directory you wish to process.

```bash
dotnet run --project D:\engamd89-dev\dotnet\RepoAIfy\src\RepoAIfy -- --source "D:\engamd89-dev\dotnet\RepoAIfy\src\YourSourceDirectory" --options "D:\engamd89-dev\dotnet\RepoAIfy\src\options.json"
```

The output markdown file(s) (`output.md`, `output_2.md`, etc.) will be generated in the directory specified by the `OutputDirectory` setting in `options.json` (default is `./ai-output` relative to the current working directory).

## Development Conventions

*   **Language:** C#
*   **Framework:** .NET 9
*   **Command-Line Interface Library:** `System.CommandLine` (prerelease version `2.0.0-beta4.22272.1`)
*   **JSON Handling:** `System.Text.Json` for serialization and deserialization of configuration.
*   **File System Operations:** Standard .NET `System.IO` classes for directory and file manipulation.
*   **Code Structure:** The application logic is primarily orchestrated by `Program.cs`, with configuration models defined in `Options.cs` and core functionalities decoupled into `Services` (e.g., `OptionsLoader`, `FileProcessor`, `MarkdownGenerator`).
*   **Error Handling:** Enhanced error handling for file system operations and JSON deserialization is in place, with messages directed to `Console.Error`.
*   **Glob Matching:** Robust glob matching for `ExcludedDirectories` is implemented using `Microsoft.Extensions.FileSystemGlobbing`.