# User Manual: RepoAIfy

This manual provides detailed instructions on how to use the `RepoAIfy` application, including its configuration options and expected behavior.

## Table of Contents
1.  [Introduction](#1-introduction)
2.  [Installation](#2-installation)
3.  [Usage](#3-usage)
4.  [Configuration (`options.json`)](#4-configuration-optionsjson)
    *   [FileFilter](#filefilter)
    *   [Chunking](#chunking)
    *   [Output](#output)
5.  [Examples](#5-examples)
6.  [Troubleshooting](#6-troubleshooting)

## 1. Introduction

`RepoAIfy` is a command-line utility designed to convert the contents of multiple source files within a directory into one or more Markdown files. It's particularly useful for:

*   Generating documentation from source code.
*   Creating a consolidated view of a project's files for review.
*   Preparing input context for AI models or other automated analysis tools.

## 2. Installation

### Prerequisites

Ensure you have the [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) installed on your system.

### Building the Application

1.  Clone the repository or navigate to the `dotnet-utils` directory.
2.  Open your terminal or command prompt.
3.  Execute the following command to build the application:

    ```bash
    dotnet build D:\engamd89-dev\dotnet\dotnet-utils\RepoAIfy
    ```

    This will compile the project and create the executable in the `bin/Debug/net9.0/` (or `bin/Release/net9.0/`) folder within the `RepoAIfy` project directory.

## 3. Usage

To run the `RepoAIfy`, you need to specify two main arguments:

*   `--source <SOURCE_DIRECTORY_PATH>`: The absolute path to the directory containing the files you want to process.
*   `--options <OPTIONS_FILE_PATH>`: The absolute path to your `options.json` configuration file.

### Command Syntax

Navigate to the `dotnet-utils` directory in your terminal and use the following command structure:

```bash
dotnet run --project D:\engamd89-dev\dotnet\dotnet-utils\RepoAIfy -- --source "D:\engamd89-dev\dotnet\dotnet-utils\YourSourceDirectory" --options "D:\engamd89-dev\dotnet\dotnet-utils\options.json"
```

**Important:**
*   Replace `"D:\engamd89-dev\dotnet\dotnet-utils\YourSourceDirectory"` with the actual absolute path to your source directory.
*   Replace `"D:\engamd89-dev\dotnet\dotnet-utils\options.json"` with the actual absolute path to your `options.json` file.

**Security Warning:** Always ensure that the `--source` directory and the `OutputDirectory` specified in `options.json` point to trusted locations. Providing untrusted paths could lead to unintended file access or modification on your system.

## 4. Configuration (`options.json`)

The `options.json` file dictates how `RepoAIfy` operates. Below is a breakdown of its structure and available settings.

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

### FileFilter

This section controls which files are included or excluded from the processing.

*   `IncludedExtensions` (array of strings): A list of file extensions (e.g., `.cs`, `.json`) that the converter should process. Only files with these extensions will be included. The comparison is case-insensitive.
*   `ExcludedDirectories` (array of strings): A list of glob-like patterns for directories that should be excluded from the search. The current implementation uses `Microsoft.Extensions.FileSystemGlobbing` for robust pattern matching.

### Chunking

This section defines how the output Markdown content is split into multiple files.

*   `MaxChunkSizeKb` (integer): The maximum desired size (in kilobytes) for each output Markdown file. If the total content (including markdown formatting overhead) of a single file exceeds this size, that file will be placed in its own chunk, and that chunk will exceed the `MaxChunkSizeKb`. Otherwise, if adding a file would cause the current chunk to exceed the limit, a new chunk will be started. A value of `0` or a very large number effectively disables chunking, resulting in a single output file.

### Output

This section defines the location of the generated output.

*   `OutputDirectory` (string): The path where the output Markdown file(s) will be saved. This path is relative to the directory from which you run the `dotnet run` command. If the directory does not exist, it will be created.

## 5. Examples

### Example 1: Basic Conversion (with potential chunking)

To convert all `.cs` and `.json` files from a source directory named `MyProject` (located at `D:\engamd89-dev\dotnet\dotnet-utils\MyProject`) into markdown file(s), using the default `options.json`:

```bash
dotnet run --project D:\engamd89-dev\dotnet\dotnet-utils\RepoAIfy -- --source "D:\engamd89-dev\dotnet\dotnet-utils\MyProject" --options "D:\engamd89-dev\dotnet\dotnet-utils\options.json"
```

This will create `MyProject.md` (and potentially `MyProject_2.md`, `MyProject_3.md`, etc., if chunking is active) in `D:\engamd89-dev\dotnet\dotnet-utils\ai-output`.

### Example 2: Custom Output Directory and Chunk Size

If you modify `options.json` to set `"OutputDirectory": "./docs/generated"` and `"MaxChunkSizeKb": 512`, the output files will be saved in `D:\engamd89-dev\dotnet\dotnet-utils\docs\generated`, with each file not exceeding approximately 512KB (unless a single file itself is larger).

## 6. Troubleshooting

*   **"Error: Source directory ... does not exist."**: Ensure the path provided to `--source` is an absolute and correct path to an existing directory.
*   **"Error: Options file ... does not exist."**: Ensure the path provided to `--options` is an absolute and correct path to your `options.json` file.
*   **"Error: Could not deserialize options.json."**: Check your `options.json` file for syntax errors (e.g., missing commas, unclosed brackets) and ensure it matches the expected structure.
*   **No output file(s) generated**: Verify that your `IncludedExtensions` in `options.json` match the files in your source directory and that no directories are inadvertently excluded by `ExcludedDirectories` patterns. Also, check if the source directory is empty.
*   **Output file is unexpectedly split**: This is likely due to the `MaxChunkSizeKb` setting in `options.json`. Adjust this value if you prefer larger or smaller chunks.
*   **Output chunk exceeds `MaxChunkSizeKb`**: This can happen if a single source file's content (after markdown formatting) is larger than the specified `MaxChunkSizeKb`. The application will issue a warning to `Console.Error` in such cases.
*   **Build Errors**: If you encounter build errors, ensure you have the correct .NET 9 SDK installed and that the `System.CommandLine` and `Microsoft.Extensions.FileSystemGlobbing` packages are correctly referenced in your `.csproj` file.