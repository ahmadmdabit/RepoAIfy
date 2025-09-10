# Project: RepoAIfy

## Project Overview

This project is a .NET 9 tool named `RepoAIfy` that helps you consolidate source code into markdown files, which is ideal for generating documentation or creating context for AI models. It consists of three main components:

1.  **A WPF GUI Application (`RepoAIfyApp`):** A user-friendly interface that allows you to visually select a source directory, manage configuration options, and see a live preview of the file structure before processing.
2.  **A Core Library (`RepoAIfyLib`):** The underlying library that handles all the core logic, including file filtering, chunking, and markdown generation.
3.  **A Console Application (`RepoAIfy`):** A command-line interface for scripting and automation.

Key features include:
*   **WPF GUI:** An intuitive interface for managing the entire process.
*   **Interactive and Cancellable File Tree:** A tree view with checkboxes lets you precisely select which files and folders to include. The tree population can be cancelled if it's slow.
*   **Optional File Size Display:** Users can toggle the visibility of file sizes in the tree view to help identify large files.
*   **Dynamic Filtering:** The file tree automatically updates as you modify the inclusion/exclusion rules.
*   **Unified Cancellation:** A single contextual "Cancel" button stops any active long-running task, whether it's populating the tree or generating files.
*   **Configuration:** Reads file filtering and output settings from a JSON configuration file (`options.json`), and allows you to modify them in the UI.
*   **Smart Chunking:** Automatically splits the output Markdown content into multiple files based on a configurable maximum chunk size.
*   **Large File Protection:** Automatically skips files larger than a configurable size limit to prevent memory issues.
*   **Enhanced Security:** Improved path validation prevents directory traversal attacks.
*   **Modern Architecture:** Full dependency injection and structured logging throughout the application, following clean MVVM principles.

## Building and Running

### Prerequisites
*   .NET 9 SDK
*   Visual Studio 2022 (Recommended for the best experience)
*   Windows Operating System (for the WPF application)

### WPF Application

To build and run the `RepoAIfy` WPF application, navigate to the root of the project and run the following command:

```bash
dotnet run --project src/RepoAIfyApp
```

The application will start, and you can use the UI to select your source directory and options.

### Console Application

The console application is available for command-line use and automation.

**Build**

```bash
dotnet build src/RepoAIfy
```

**Run**

Remember to replace the placeholder paths with the actual absolute paths to your source directory and `options.json` file.

```bash
dotnet run --project src/RepoAIfy -- --source "D:\dev\RepoAIfy\src\YourSourceDirectory" --options "D:\dev\RepoAIfy\src\options.json"
```

The output markdown file(s) will be generated in the directory specified by the `OutputDirectory` setting in `options.json`.

## Development Conventions

### Architecture and Design
*   **Clean Architecture:** The solution is structured to separate concerns, with UI, business logic, and data access kept in distinct projects.
*   **SOLID Principles:** Code is written following SOLID principles to ensure it is maintainable, scalable, and testable.
*   **Dependency Injection (DI):** Key services are managed via dependency injection using Microsoft.Extensions.Hosting.
*   **MVVM Pattern:** The WPF application (`RepoAIfyApp`) strictly follows the MVVM pattern. The project is organized into `Views`, `ViewModels`, `Models`, `Services`, and `Helpers` directories to maintain a clean separation of concerns.
*   **Unified Cancellation:** Long-running operations (tree view population, file generation) are fully cancellable using a unified `CancellationTokenSource` managed in the `MainWindowViewModel`. An `AppState` enum tracks the current application state (`Idle`, `PopulatingTree`, `Generating`) to provide contextual cancellation.

### Code Structure
*   **`RepoAIfyLib` (Core Logic):** A class library containing the core business logic. It is responsible for file processing, filtering, and Markdown generation. It has no dependency on any UI framework. Key classes end with the `Service` suffix (e.g., `ConverterRunnerService`).
*   **`RepoAIfyApp` (WPF):** The main user interface project. It contains all UI elements (`Views`), ViewModels, and UI-specific services and helpers.
*   **`RepoAIfy` (Console):** The command-line interface for the tool.

### Naming Conventions
*   Follows standard Microsoft C# Naming Conventions.
*   Service classes are post-fixed with `Service` (e.g., `OptionsLoaderService`).

### Recent Architectural Refactoring

A major refactoring was recently completed to improve the application's architecture and maintainability.
- **Project Structure:** The `RepoAIfyApp` project was reorganized into a standard MVVM structure (`Views`, `ViewModels`, `Models`, etc.).
- **Service Naming:** Core logic classes were renamed with a `Service` suffix.
- **DI Cleanup:** The dependency injection container was updated to reflect the new structure.
- **Feature Implementation:** New features like optional file size display and unified cancellation were added during this refactoring.