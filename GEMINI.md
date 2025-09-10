# Project: RepoAIfy

## Project Overview

This project is a .NET 9 tool named `RepoAIfy` that helps you consolidate source code into markdown files, which is ideal for generating documentation or creating context for AI models. It consists of three main components:

1.  **A WPF GUI Application (`RepoAIfyApp`):** A user-friendly interface that allows you to visually select a source directory, manage configuration options, and see a live preview of the file structure before processing.
2.  **A Core Library (`RepoAIfyLib`):** The underlying library that handles all the core logic, including file filtering, chunking, and markdown generation.
3.  **A Console Application (`RepoAIfy`):** A command-line interface for scripting and automation.

Key features include:
*   **WPF GUI:** An intuitive interface for managing the entire process.
*   **Interactive File Tree:** A tree view with checkboxes lets you precisely select which files and folders to include.
*   **Dynamic Filtering:** The file tree automatically updates as you modify the inclusion/exclusion rules.
*   **Configuration:** Reads file filtering and output settings from a JSON configuration file (`options.json`), and allows you to modify them in the UI.
*   **Smart Chunking:** Automatically splits the output Markdown content into multiple files based on a configurable maximum chunk size.
*   **Markdown Output:** Generates markdown file(s) where each processed file's content is presented within a code block, preceded by a heading indicating its relative path.
*   **Large File Protection:** Automatically skips files larger than a configurable size limit to prevent memory issues.
*   **Enhanced Security:** Improved path validation prevents directory traversal attacks.
*   **Modern Architecture:** Full dependency injection and structured logging throughout the application.

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
*   **MVVM Pattern:** Used in the WPF application with ViewModels and data binding.

### Code Structure
*   **`RepoAIfyLib` (Core Logic):** A class library containing the core business logic. It is responsible for file processing, filtering, and Markdown generation. It has no dependency on any UI framework.
*   **`RepoAIfyApp` (WPF):** The main user interface project. It contains all UI elements (Views), and ViewModels, adhering to the MVVM pattern. It uses a custom `ViewModelSink` for logging.
*   **`RepoAIfy` (Console):** The command-line interface for the tool.

### Naming Conventions
*   Follows standard Microsoft C# Naming Conventions (e.g., `PascalCase` for classes and methods, `camelCase` for local variables).
*   UI element names in XAML are post-fixed with their type (e.g., `SourceDirectoryTextBox`, `GenerateButton`).

### Testing Strategy
*   **Unit Testing:** (Future) xUnit will be used for unit testing the core logic in `RepoAIfyLib`.
*   **Integration Testing:** (Future) The console application can be used for integration testing the end-to-end file processing workflow.

### Commit Conventions
*   Commits should follow the Conventional Commits specification (e.g., `feat:`, `fix:`, `docs:`, `refactor:`).
*   Commit messages should be clear and concise, explaining the "what" and the "why" of the change.

## Recent Improvements

The RepoAIfy solution has been significantly enhanced with a comprehensive set of improvements across multiple phases. See [CHANGELOG.md](CHANGELOG.md) for a detailed history of changes and [docs/RepoAIfy-Documentation.md](docs/RepoAIfy-Documentation.md) for complete documentation.

### Phase 1: Critical Security, Correctness, and Reliability Fixes ✅ COMPLETED
1. **Security Enhancement**: Fixed critical path traversal vulnerability in OptionsLoader with robust path sandboxing ✅ DONE
2. **Correctness Fix**: Corrected file filtering logic in TreeViewDataService with proper glob pattern matching ✅ DONE
3. **Reliability Improvement**: Eliminated async void methods to prevent unhandled exceptions with AsyncRelayCommand ✅ DONE

### Phase 2: High-Impact Robustness and Performance ✅ COMPLETED
1. **Memory Protection**: Added MaxFileSizeMb configuration option (default: 16 MB) to prevent high memory usage with large files ✅ DONE
2. **Null Safety**: Prevented null-reference exceptions by enforcing non-nullable properties in FileSystemTree model ✅ DONE

### Phase 3: Architectural Refinement and Best Practices ✅ COMPLETED
1. **Dependency Injection**: Centralized logging and dependency injection using Microsoft.Extensions.Hosting in both WPF and Console applications ✅ DONE
2. **Improved Chunking**: Enhanced markdown chunking logic with continuation headers and efficient byte counting ✅ DONE
3. **Polish**: Addressed minor issues including platform targeting consistency and improved log file locations ✅ DONE

### Additional Architectural Improvements ✅ COMPLETED
1. **IDialogService Pattern**: Implemented proper abstraction of UI-specific functionality behind interfaces ✅ DONE
2. **UILogRelayService**: Added thread-safe log message relaying to the UI ✅ DONE
3. **Enhanced Logging Architecture**: Improved overall logging structure and configuration ✅ DONE
4. **Code Quality Improvements**: Removed unnecessary dependencies and improved code organization ✅ DONE

These changes transform RepoAIfy into a secure, correct, robust, and architecturally sound solution that follows modern .NET best practices.