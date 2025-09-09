# Project: RepoAIfy

## Project Overview

This project is a .NET 9 tool named `RepoAIfy` that helps you consolidate source code into markdown files, which is ideal for generating documentation or creating context for AI models. It consists of two main components:

1.  **A WPF GUI Application (`RepoAIfyApp`):** A user-friendly interface that allows you to visually select a source directory, manage configuration options, and see a live preview of the file structure before processing.
2.  **A Core Library (`RepoAIfyLib`):** The underlying library that handles all the core logic, including file filtering, chunking, and markdown generation.

Key features include:
*   **WPF GUI:** An intuitive interface for managing the entire process.
*   **Interactive File Tree:** A tree view with checkboxes lets you precisely select which files and folders to include.
*   **Dynamic Filtering:** The file tree automatically updates as you modify the inclusion/exclusion rules.
*   **Configuration:** Reads file filtering and output settings from a JSON configuration file (`options.json`), and allows you to modify them in the UI.
*   **Smart Chunking:** Automatically splits the output Markdown content into multiple files based on a configurable maximum chunk size.
*   **Markdown Output:** Generates markdown file(s) where each processed file's content is presented within a code block, preceded by a heading indicating its relative path.

## Building and Running

### Prerequisites
*   .NET 9 SDK

### WPF Application

To build and run the `RepoAIfy` WPF application, navigate to the root of the project and run the following command:

```bash
dotnet run --project src/RepoAIfyApp
```

The application will start, and you can use the UI to select your source directory and options.

### Console Application

The original console application is still available for command-line use.

**Build**

```bash
dotnet build src/RepoAIfy
```

**Run**

Remember to replace the placeholder paths with the actual absolute paths to your source directory and `options.json` file.

```bash
dotnet run --project src/RepoAIfy -- --source "D:\engamd89-dev\dotnet\RepoAIfy\src\YourSourceDirectory" --options "D:\engamd89-dev\dotnet\RepoAIfy\src\options.json"
```

The output markdown file(s) will be generated in the directory specified by the `OutputDirectory` setting in `options.json`.

## Development Conventions

*   **Language:** C#
*   **Framework:** .NET 9
*   **UI Framework:** WPF
*   **Command-Line Interface Library:** `System.CommandLine`
*   **JSON Handling:** `System.Text.Json`
*   **File System Operations:** Standard .NET `System.IO` classes.
*   **Glob Matching:** `Microsoft.Extensions.FileSystemGlobbing`.
*   **Logging:** `Serilog` is used for logging to both a file and the UI.
*   **Code Structure:** The application is divided into three projects:
    *   `RepoAIfyApp`: The WPF application.
    *   `RepoAIfyLib`: The core logic library.
    *   `RepoAIfy`: The original console application.
