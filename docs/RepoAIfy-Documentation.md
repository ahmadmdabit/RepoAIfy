# RepoAIfy Documentation

## Project Overview

RepoAIfy is a powerful .NET 9 solution designed to streamline the process of analyzing and documenting codebases. It provides two primary interfaces:

1.  **A user-friendly WPF Desktop Application (`RepoAIfyApp`)** for interactive file selection, real-time filtering, and visual feedback.
2.  **A Command-Line Interface (`RepoAIfy`)** for scripting and automated workflows.

The core logic reads files from a source directory, intelligently filters them based on a user-defined `options.json` configuration, and then compiles their content into one or more well-structured Markdown files. This tool is ideal for creating comprehensive context files for AI models, generating quick documentation, or consolidating source code for review.

## Features

*   **Dual Interface:** Choose between an intuitive WPF GUI for visual interaction or a powerful CLI for automation.
*   **Interactive File Tree:** The WPF app displays your source directory in a tree view, allowing you to visually include or exclude specific files and folders with checkboxes.
*   **Real-Time Filtering:** Dynamically filter the file tree in the UI by included extensions or excluded directory patterns. The view updates automatically as you type.
*   **Configurable File Filtering:** Use glob patterns in `options.json` to define robust rules for including files by extension and excluding directories (e.g., `bin`, `obj`, `.git`).
*   **Smart Chunking:** Automatically splits the output into multiple Markdown files based on a configurable maximum size, making outputs manageable for large repositories.
*   **Dynamic Repository Overview:** Generates a structured, hierarchical overview of the processed files and directories and inserts it into the first output chunk for immediate context.
*   **Live Log Output:** The WPF application provides a dedicated log panel that displays real-time processing status and errors.
*   **Markdown Preview:** The WPF application now includes a built-in Markdown previewer that renders the generated files in real-time.
*   **Cross-Platform Core:** The core logic is built with .NET 9, with the console app being fully cross-platform. The WPF application is for Windows.

## Project Structure

The solution is architected with a clean separation of concerns:

*   `RepoAIfyLib`: A .NET class library containing all the core business logic (file processing, filtering, Markdown generation). It has no dependency on any UI framework.
*   `RepoAIfyApp`: The primary WPF desktop application. It provides the graphical user interface and consumes `RepoAIfyLib`.
*   `RepoAIfy`: The original console application, ideal for scripting and automation. It also consumes `RepoAIfyLib`.

## Getting Started

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

The original console application is still available for command-line use.

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
*   **MVVM Pattern:** Used in the WPF application with ViewModels and data binding

### Code Structure
*   **`RepoAIfyLib` (Core Logic):** A class library containing the core business logic. It is responsible for file processing, filtering, and Markdown generation. It has no dependency on any UI framework.
*   **`RepoAIfyApp` (WPF):** The main user interface project. It contains all UI elements (Views), and ViewModels, adhering to the MVVM pattern. It uses a custom `ViewModelSink` for logging.
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

## Key Classes and Components

### Core Library (RepoAIfyLib)

- **ConverterRunner**: Main processing class that orchestrates file processing and markdown generation
- **OptionsLoader**: Loads and validates configuration from options.json
- **FileProcessor**: Filters files based on inclusion/exclusion rules
- **MarkdownGenerator**: Converts files to markdown format with chunking support
- **TreeViewDataService**: Provides file system data for the WPF tree view

### WPF Application (RepoAIfyApp)

- **MainWindow**: Main application window with XAML layout
- **MainWindowViewModel**: ViewModel implementing MVVM pattern with data binding
- **FileSystemNode**: Represents a node in the file tree view with checkbox support
- **AsyncRelayCommand**: Implementation of ICommand for safe async command handling
- **GeneratedFileViewModel**: New ViewModel class representing a generated Markdown file

### Console Application (RepoAIfy)

- **Program.cs**: Entry point with command-line argument parsing using System.CommandLine

## Recent Improvements

The RepoAIfy solution has been significantly enhanced with a comprehensive set of improvements across three phases. See [CHANGELOG.md](CHANGELOG.md) for a detailed history of changes.

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

### Additional Major Architectural Improvements ✅ COMPLETED
1. **IDialogService Pattern**: Implemented proper abstraction of UI-specific functionality behind interfaces ✅ DONE
2. **UILogRelayService**: Added thread-safe log message relaying to the UI ✅ DONE
3. **Enhanced Logging Architecture**: Improved overall logging structure and configuration ✅ DONE
4. **Code Quality Improvements**: Removed unnecessary dependencies and improved code organization ✅ DONE
5. **Markdown Preview Feature**: Implemented integrated Markdown preview functionality in WPF application ✅ DONE

## Configuration (`options.json`)

The behavior of RepoAIfy is controlled by an `options.json` file with the following structure:

```json
{
  "FileFilter": {
    "IncludedExtensions": [
      ".cs", ".vb", ".fs", ".csproj", ".sln", ".props", ".targets", 
      ".json", ".config", ".md"
    ],
    "ExcludedDirectories": [
      "**/bin/**", "**/obj/**", "**/node_modules/**", "**/packages/**",
      "**/.git/**", "**/.vs/**", "**/TestResults/**"
    ],
    "MaxFileSizeMb": 16
  },
  "Chunking": {
    "MaxChunkSizeKb": 128
  },
  "Output": {
    "OutputDirectory": "./ai-output"
  }
}
```

*   **`IncludedExtensions`**: An array of file extensions (including the dot) to include in the processing.
*   **`ExcludedDirectories`**: An array of glob patterns for directories to exclude. `**/` is a wildcard for any directory level.
*   **`MaxFileSizeMb`**: The maximum size in megabytes for each input file. Files larger than this limit will be skipped to prevent memory issues. Default is 16 MB.
*   **`MaxChunkSizeKb`**: The maximum size in kilobytes for each output markdown file.
*   **`OutputDirectory`**: The relative path where the output files will be saved.

## Markdown Preview Feature

The WPF application now includes a built-in Markdown preview feature that enhances the user experience by allowing users to view the generated Markdown files directly within the application.

### Feature Overview

After processing is complete, the application automatically:
1. Reads the generated `.md` files from the output directory
2. Creates a new tab inside the "Markdown Output" tab for each file
3. Switches to the "Markdown Output" tab to show the rendered content
4. Allows users to switch between different generated files using tabs
5. Allows users to switch back to the "Logs" tab to view the processing logs

### Implementation Details

The feature was implemented by:
1. Adding the `Markdig.Wpf` NuGet package for Markdown rendering
2. Creating a `GeneratedFileViewModel` class to represent generated files
3. Modifying the `MainWindowViewModel` to load and manage generated files
4. Updating the `MainWindow.xaml` with a tabbed interface for logs and Markdown preview

### Technical Architecture

- **Data Binding**: The tabbed interface uses data binding to dynamically create tabs for each generated file
- **Markdown Viewer**: The `MarkdownViewer` control from the `Markdig.Wpf` library renders the Markdown content
- **UI/UX**: Clean tabbed interface that separates logs from Markdown preview with automatic switching when generation is complete

## License

This project is licensed under the [MIT License](LICENSE).