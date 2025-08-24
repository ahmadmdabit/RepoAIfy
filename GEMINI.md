# Project: FileToMarkdownConverter

## Project Overview

This project is a .NET 9 console application named `FileToMarkdownConverter`. Its primary purpose is to process files from a specified source directory, apply filtering rules defined in an `options.json` configuration file, and then consolidate the content of the filtered files into a single markdown (`.md`) output file.

Key features include:
*   **Command-Line Argument Parsing:** Utilizes `System.CommandLine` for robust command-line argument handling, accepting a source directory path and an `options.json` file path.
*   **Configuration:** Reads file filtering and output settings from a JSON configuration file (`options.json`).
*   **File Filtering:** Filters files based on included file extensions (e.g., `.cs`, `.json`, `.md`) and excluded directory patterns (e.g., `**/bin/**`, `**/obj/**`).
*   **Markdown Output:** Generates a markdown file where each processed file's content is presented within a code block, preceded by a heading indicating its relative path.

## Building and Running

### Prerequisites
*   .NET 9 SDK

### Build
To build the `FileToMarkdownConverter` application, navigate to the root of the `dotnet-utils` directory and run the following command:

```bash
dotnet build D:\engamd89-dev\dotnet\dotnet-utils\FileToMarkdownConverter
```

### Run
To run the application, use the following command from the `dotnet-utils` directory. Remember to replace `"D:\engamd89-dev\dotnet\dotnet-utils\YourSourceDirectory"` with the actual absolute path to the source directory you wish to process.

```bash
dotnet run --project D:\engamd89-dev\dotnet\dotnet-utils\FileToMarkdownConverter -- --source "D:\engamd89-dev\dotnet\dotnet-utils\YourSourceDirectory" --options "D:\engamd89-dev\dotnet\dotnet-utils\options.json"
```

The output markdown file (`output.md`) will be generated in the directory specified by the `OutputDirectory` setting in `options.json` (default is `./ai-output` relative to the current working directory).

## Development Conventions

*   **Language:** C#
*   **Framework:** .NET 9
*   **Command-Line Interface Library:** `System.CommandLine` (prerelease version `2.0.0-beta4.22272.1`)
*   **JSON Handling:** `System.Text.Json` for serialization and deserialization of configuration.
*   **File System Operations:** Standard .NET `System.IO` classes for directory and file manipulation.
*   **Code Structure:** The application logic is primarily contained within `Program.cs`, with configuration models defined in `Options.cs`.
*   **Error Handling:** Basic error handling for file system operations and JSON deserialization is in place.
*   **Glob Matching:** A simplified glob matching logic is implemented for `ExcludedDirectories`. For more complex scenarios, a dedicated glob library might be considered.
