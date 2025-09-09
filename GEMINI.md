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

### Architecture and Design
*   **Clean Architecture:** The solution is structured to separate concerns, with UI, business logic, and data access kept in distinct projects.
*   **SOLID Principles:** Code is written following SOLID principles to ensure it is maintainable, scalable, and testable.
*   **Dependency Injection (DI):** Key services are managed via dependency injection, particularly in the WPF application's composition root.

### Code Structure
*   **`RepoAIfyApp` (WPF):** The main user interface project. It contains all UI elements (Views), and ViewModels, adhering to the MVVM pattern. It uses a custom `ViewModelSink` for logging.
*   **`RepoAIfyLib` (Core Logic):** A class library containing the core business logic. It is responsible for file processing, filtering, and Markdown generation. It has no dependency on any UI framework.
*   **`RepoAIfy` (Console):** The original command-line interface for the tool.

### Naming Conventions
*   Follows standard Microsoft C# Naming Conventions (e.g., `PascalCase` for classes and methods, `camelCase` for local variables).
*   UI element names in XAML are post-fixed with their type (e.g., `SourceDirectoryTextBox`, `GenerateButton`).

### Testing Strategy
*   **Unit Testing:** (Future) xUnit will be used for unit testing the core logic in `RepoAIfyLib`.
*   **Integration Testing:** (Future) The console application can be used for integration testing the end-to-end file processing workflow.

### Commit Conventions
*   Commits should follow the Conventional Commits specification (e.g., `feat:`, `fix:`, `docs:`, `refactor:`).
*   Commit messages should be clear and concise, explaining the "what" and the "why" of the change.
