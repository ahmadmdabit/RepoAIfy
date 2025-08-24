# FileToMarkdownConverter

## Overview

`FileToMarkdownConverter` is a .NET 9 console application designed to streamline the process of documenting codebases or specific project directories. It reads files from a designated source directory, intelligently filters them based on a user-defined `options.json` configuration, and then compiles their content into a single, well-structured Markdown file. This tool is ideal for generating quick documentation, creating context files for AI models, or simply consolidating relevant source code for review.

## Features

*   **Configurable File Filtering:** Include or exclude files based on their extensions and directory paths using glob patterns defined in `options.json`.
*   **Command-Line Interface (CLI):** Easy-to-use command-line arguments for specifying the source directory and configuration file.
*   **Markdown Output:** Generates a clean Markdown file, with each processed file's content enclosed in a code block and clearly identified by its relative path.
*   **Cross-Platform:** Built with .NET 9, ensuring compatibility across various operating systems.

## Getting Started

### Prerequisites

Before you begin, ensure you have the following installed:

*   [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Build the Application

To build the `FileToMarkdownConverter` application, navigate to the root of the `dotnet-utils` directory (where `dotnet-utils.sln` is located) and execute the following command:

```bash
dotnet build D:\engamd89-dev\dotnet\dotnet-utils\FileToMarkdownConverter
```

This will compile the project and place the executable in the `bin/Debug/net9.0/` (or `bin/Release/net9.0/`) subdirectory within the `FileToMarkdownConverter` project folder.

### Run the Application

To run the application, use the `dotnet run` command. You need to provide the absolute path to your source directory and the `options.json` file.

```bash
dotnet run --project D:\engamd89-dev\dotnet\dotnet-utils\FileToMarkdownConverter -- --source "D:\engamd89-dev\dotnet\dotnet-utils\YourSourceDirectory" --options "D:\engamd89-dev\dotnet\dotnet-utils\options.json"
```

**Replace:**
*   `"D:\engamd89-dev\dotnet\dotnet-utils\YourSourceDirectory"` with the absolute path to the directory containing the files you want to process.
*   `"D:\engamd89-dev\dotnet\dotnet-utils\options.json"` with the absolute path to your configuration file.

By default, the output Markdown file (`output.md`) will be created in an `ai-output` directory relative to where you run the command, as specified in `options.json`.

## Configuration (`options.json`)

The behavior of `FileToMarkdownConverter` is controlled by the `options.json` file. This file allows you to define which files to include or exclude and where to save the output. A detailed explanation of the configuration options can be found in `user-manual.md`.

## Contributing

Contributions are welcome! Please refer to `CONTRIBUTING.md` (TODO: Create this file) for guidelines on how to contribute to this project.

## License

This project is licensed under the [MIT License](LICENSE.md) (TODO: Create this file).
