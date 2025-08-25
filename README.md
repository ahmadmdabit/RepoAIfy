# RepoAIfy

## Overview

`RepoAIfy` is a .NET 9 console application designed to streamline the process of documenting codebases or specific project directories. It reads files from a designated source directory, intelligently filters them based on a user-defined `options.json` configuration, and then compiles their content into one or more well-structured Markdown files. This tool is ideal for generating quick documentation, creating context files for AI models, or simply consolidating relevant source code for review.

## Features

*   **Configurable File Filtering:** Include or exclude files based on their extensions and directory paths using glob patterns defined in `options.json`, ensuring only relevant files are processed.
*   **Smart Chunking:** Automatically splits the output Markdown content into multiple files based on a configurable maximum chunk size, improving manageability for large outputs and preventing excessively large single files.
*   **Dynamic Repository Overview:** Generates a structured overview of the processed repository (files and directories) and inserts it into the first output chunk, enhancing navigability and providing immediate context. This includes a hierarchical representation of the repository structure.
*   **Command-Line Interface (CLI):** Easy-to-use command-line arguments for specifying the source directory and configuration file, making the tool simple to integrate into scripts and workflows.
*   **Markdown Output:** Generates clean Markdown file(s), with each processed file's content enclosed in a code block and clearly identified by its relative path, adhering to Markdown best practices for headings and metadata presentation.
*   **Cross-Platform:** Built with .NET 9, ensuring compatibility across various operating systems.

## Getting Started

### Prerequisites

Before you begin, ensure you have the following installed:

*   [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Build the Application

To build the `RepoAIfy` application, navigate to the root of the `src` directory (where `src.sln` is located) and execute the following command:

```bash
dotnet build D:\engamd89-dev\dotnet\RepoAIfy\src\RepoAIfy
```

**Note:** The example above uses an absolute path for clarity within this CLI context. For improved portability, you might use a relative path like `dotnet build RepoAIfy` if running from the solution directory.

This will compile the project and place the executable in the `bin/Debug/net9.0/` (or `bin/Release/net9.0/`) subdirectory within the `RepoAIfy` project folder.

### Run the Application

To run the application, use the `dotnet run` command. You need to provide the absolute path to your source directory and the `options.json` file.

```bash
dotnet run --project D:\engamd89-dev\dotnet\RepoAIfy\src\RepoAIfy -- --source "D:\engamd89-dev\dotnet\RepoAIfy\src\YourSourceDirectory" --options "D:\engamd89-dev\dotnet\RepoAIfy\src\options.json"
```

**Replace:**
*   `"D:\engamd89-dev\dotnet\RepoAIfy\src\YourSourceDirectory"` with the absolute path to the directory containing the files you want to process.
*   `"D:\engamd89-dev\dotnet\RepoAIfy\src\options.json"` with the absolute path to your configuration file.

**Note:** The example above uses absolute paths for clarity within this CLI context. For improved portability, you might use relative paths (e.g., `--project RepoAIfy`) if running from the solution directory.

By default, the output Markdown file(s) (e.g., `[Name of Your Source Directory].md`, `[Name of Your Source Directory]_2.md`, etc.) will be created in an `ai-output` directory relative to where you run the command, as specified in `options.json`. The `[Name of Your Source Directory]` part of the filename is derived directly from the name of the directory provided to the `--source` argument.

## Configuration (`options.json`)

The behavior of `RepoAIfy` is controlled by the `options.json` file. This file allows you to define file filtering rules, chunking behavior, and output location. A detailed explanation of the configuration options can be found in `user-manual.md`.

```json
{
  "FileFilter": {
    "IncludedExtensions": [
      ".cs",
      ".vb",
      ".fs",
      ".csproj",
      ".sln",
      ".props",
      ".targets",
      ".json",
      ".config",
      ".md"
    ],
    "ExcludedDirectories": [
      "**/bin/**",
      "**/obj/**",
      "**/node_modules/**",
      "**/packages/**",
      "**/.git/**",
      "**/.vs/**",
      "**/TestResults/**"
    ]
  },
  "Chunking": {
    "MaxChunkSizeKb": 128
  },
  "Output": {
    "OutputDirectory": "./ai-output"
  }
}
```

## License

This project is licensed under the [MIT License](LICENSE).
