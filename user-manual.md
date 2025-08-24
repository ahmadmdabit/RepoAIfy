# User Manual: FileToMarkdownConverter

This manual provides detailed instructions on how to use the `FileToMarkdownConverter` application, including its configuration options and expected behavior.

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

`FileToMarkdownConverter` is a command-line utility designed to convert the contents of multiple source files within a directory into a single Markdown file. It's particularly useful for:

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
    dotnet build D:\engamd89-dev\dotnet\dotnet-utils\FileToMarkdownConverter
    ```

    This will compile the project and create the executable in the `bin/Debug/net9.0/` (or `bin/Release/net9.0/`) folder within the `FileToMarkdownConverter` project directory.

## 3. Usage

To run the `FileToMarkdownConverter`, you need to specify two main arguments:

*   `--source <SOURCE_DIRECTORY_PATH>`: The absolute path to the directory containing the files you want to process.
*   `--options <OPTIONS_FILE_PATH>`: The absolute path to your `options.json` configuration file.

### Command Syntax

Navigate to the `dotnet-utils` directory in your terminal and use the following command structure:

```bash
dotnet run --project D:\engamd89-dev\dotnet\dotnet-utils\FileToMarkdownConverter -- --source "D:\engamd89-dev\dotnet\dotnet-utils\YourSourceDirectory" --options "D:\engamd89-dev\dotnet\dotnet-utils\options.json"
```

**Important:**
*   Replace `"D:\engamd89-dev\dotnet\dotnet-utils\YourSourceDirectory"` with the actual absolute path to your source directory.
*   Replace `"D:\engamd89-dev\dotnet\dotnet-utils\options.json"` with the actual absolute path to your `options.json` file.

## 4. Configuration (`options.json`)

The `options.json` file dictates how `FileToMarkdownConverter` operates. Below is a breakdown of its structure and available settings.

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
    "Format": "markdown",
    "OutputDirectory": "./ai-output"
  }
}
```

### FileFilter

This section controls which files are included or excluded from the processing.

*   `IncludedExtensions` (array of strings): A list of file extensions (e.g., `.cs`, `.json`) that the converter should process. Only files with these extensions will be included. The comparison is case-insensitive.
*   `ExcludedDirectories` (array of strings): A list of glob-like patterns for directories that should be excluded from the search. The current implementation supports basic patterns:
    *   `**/directoryName/**`: Excludes any directory named `directoryName` at any depth.
    *   `directoryName/`: Excludes a directory named `directoryName` (relative to the source directory).
    *   `directoryName`: Excludes a directory with an exact name match.

### Chunking

This section is currently defined but not fully implemented in the provided code. It is intended for future enhancements related to breaking large files into smaller chunks.

*   `MaxChunkSizeKb` (integer): The maximum size (in kilobytes) a file chunk should be. (Currently not used).

### Output

This section defines the format and location of the generated output.

*   `Format` (string): The desired output format. Currently, only `"markdown"` is supported.
*   `OutputDirectory` (string): The path where the output Markdown file will be saved. This path is relative to the directory from which you run the `dotnet run` command. If the directory does not exist, it will be created.

## 5. Examples

### Example 1: Basic Conversion

To convert all `.cs` and `.json` files from a source directory named `MyProject` (located at `D:\engamd89-dev\dotnet\dotnet-utils\MyProject`) into a markdown file, using the default `options.json`:

```bash
dotnet run --project D:\engamd89-dev\dotnet\dotnet-utils\FileToMarkdownConverter -- --source "D:\engamd89-dev\dotnet\dotnet-utils\MyProject" --options "D:\engamd89-dev\dotnet\dotnet-utils\options.json"
```

This will create `output.md` in `D:\engamd89-dev\dotnet\dotnet-utils\ai-output`.

### Example 2: Custom Output Directory

If you modify `options.json` to set `"OutputDirectory": "./docs/generated"`, the output file will be saved in `D:\engamd89-dev\dotnet\dotnet-utils\docs\generated`.

## 6. Troubleshooting

*   **"Error: Source directory ... does not exist."**: Ensure the path provided to `--source` is an absolute and correct path to an existing directory.
*   **"Error: Options file ... does not exist."**: Ensure the path provided to `--options` is an absolute and correct path to your `options.json` file.
*   **"Error: Could not deserialize options.json."**: Check your `options.json` file for syntax errors (e.g., missing commas, unclosed brackets) and ensure it matches the expected structure.
*   **No output file generated**: Verify that your `IncludedExtensions` in `options.json` match the files in your source directory and that no directories are inadvertently excluded by `ExcludedDirectories` patterns.
*   **Build Errors**: If you encounter build errors, ensure you have the correct .NET 9 SDK installed and that the `System.CommandLine` package is correctly referenced in your `.csproj` file (version `2.0.0-beta4.22272.1`).
