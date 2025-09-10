# 📚 RepoAIfy Documentation

## ✨ Project Overview

RepoAIfy is a powerful .NET 9 solution designed to streamline the process of analyzing and documenting codebases. It provides two primary interfaces:

1.  **A user-friendly WPF Desktop Application (`RepoAIfyApp`)** for interactive file selection, real-time filtering, and visual feedback.
2.  **A Command-Line Interface (`RepoAIfy`)** for scripting and automated workflows.

The core logic reads files from a source directory, intelligently filters them based on a user-defined `options.json` configuration, and then compiles their content into one or more well-structured Markdown files. This tool is ideal for creating comprehensive context files for AI models, generating quick documentation, or consolidating source code for review.

## 🌟 Features

*   **Dual Interface:** Choose between an intuitive WPF GUI for visual interaction or a powerful CLI for automation.
*   **Interactive File Tree:** The WPF app displays your source directory in a tree view, allowing you to visually include or exclude specific files and folders with checkboxes.
*   **Optional File Size Display:** Toggle the visibility of file sizes directly in the tree view to make more informed decisions about which files to include.
*   **Real-Time & Cancellable Filtering:** Dynamically filter the file tree by included extensions or excluded directory patterns. The view updates automatically, and you can cancel the refresh at any time if it's slow.
*   **Unified & Contextual Cancellation:** A single "Cancel" button intelligently stops whichever long-running task is active, whether it's populating the file tree or generating the markdown output.
*   **Configurable File Filtering:** Use glob patterns in `options.json` to define robust rules for including files by extension and excluding directories (e.g., `bin`, `obj`, `.git`).
*   **Smart Chunking:** Automatically splits the output into multiple Markdown files based on a configurable maximum chunk size, making outputs manageable for large repositories.
*   **Live Log Output:** The WPF application provides a dedicated log panel that displays real-time processing status and errors.
*   **Markdown Preview:** The WPF application includes a built-in Markdown previewer that renders the generated files in real-time after processing is complete.
*   **Cross-Platform Core:** The core logic is built with .NET 9, with the console app being fully cross-platform. The WPF application is for Windows.

## 🏗️ Project Structure

The solution is architected with a clean separation of concerns:

*   `RepoAIfyLib`: A .NET class library containing all the core business logic (file processing, filtering, Markdown generation). It has no dependency on any UI framework.
*   `RepoAIfyApp`: The primary WPF desktop application, architected using the Model-View-ViewModel (MVVM) pattern. It contains separate folders for `Views`, `ViewModels`, `Models`, `Services`, and `Helpers`.
*   `RepoAIfy`: The console application, ideal for scripting and automation. It also consumes `RepoAIfyLib`.

## ▶️ Getting Started

### 📋 Prerequisites
*   .NET 9 SDK
*   Visual Studio 2022 (Recommended for the best experience)
*   Windows Operating System (for the WPF application)

### 🖥️ WPF Application

### 💻 Console Application

The console application is available for command-line use.

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

## 🧑‍💻 Development Conventions

### 📐 Architecture and Design
*   **Clean Architecture:** The solution is structured to separate concerns, with UI, business logic, and data access kept in distinct projects.
*   **SOLID Principles:** Code is written following SOLID principles to ensure it is maintainable, scalable, and testable.
*   **Dependency Injection (DI):** Key services are managed via dependency injection using Microsoft.Extensions.Hosting.
*   **MVVM Pattern:** The WPF application (`RepoAIfyApp`) strictly follows the MVVM pattern. The project is organized into `Views`, `ViewModels`, `Models`, `Services`, and `Helpers` directories to maintain a clean separation of concerns.
*   **Unified Cancellation:** Long-running operations (tree view population, file generation) are fully cancellable using a unified `CancellationTokenSource` managed in the `MainWindowViewModel`. An `AppState` enum tracks the current application state (`Idle`, `PopulatingTree`, `Generating`) to provide contextual cancellation.

### 📁 Code Structure
*   **`RepoAIfyLib` (Core Logic):** A class library containing the core business logic. It is responsible for file processing, filtering, and Markdown generation. Key classes end with the `Service` suffix (e.g., `ConverterRunnerService`).
*   **`RepoAIfyApp` (WPF):** The main user interface project, organized into `Views`, `ViewModels`, `Models`, `Services`, and `Helpers` folders.
*   **`RepoAIfy` (Console):** The command-line interface for the tool.

### 🏷️ Naming Conventions
*   Follows standard Microsoft C# Naming Conventions.
*   Service classes are post-fixed with `Service` (e.g., `OptionsLoaderService`).

### 🧪 Testing Strategy
*   **Unit Testing:** (Future) xUnit will be used for unit testing the core logic in `RepoAIfyLib`.
*   **Integration Testing:** (Future) The console application can be used for integration testing the end-to-end file processing workflow.

### 📜 Commit Conventions
*   Commits should follow the Conventional Commits specification (e.g., `feat:`, `fix:`, `docs:`, `refactor:`).
*   Commit messages should be clear and concise, explaining the "what" and the "why" of the change.

## 🧩 Key Classes and Components

### 📦 Core Library (`RepoAIfyLib`)

- **Services/ConverterRunnerService.cs**: Main processing class that orchestrates file processing and markdown generation.
- **Services/OptionsLoaderService.cs**: Loads and validates configuration from options.json.
- **Services/FileProcessorService.cs**: Filters files based on inclusion/exclusion rules.
- **Services/MarkdownGeneratorService.cs**: Converts files to markdown format with chunking support.
- **Services/TreeViewDataService.cs**: Provides file system data for the WPF tree view.

### 🖥️ WPF Application (`RepoAIfyApp`)

- **Views/MainWindow.xaml**: Main application window with XAML layout.
- **ViewModels/MainWindowViewModel.cs**: ViewModel implementing MVVM pattern with data binding. Manages application state (`AppState`) and cancellation.
- **Models/FileSystemNode.cs**: Represents a node in the file tree view with checkbox support and a `DisplayName` for showing file sizes.
- **Helpers/AsyncRelayCommand.cs**: Implementation of ICommand for safe async command handling.
- **Services/IDialogService.cs**: Interface for abstracting UI-specific dialog functionality.

### 💻 Console Application (`RepoAIfy`)

- **Program.cs**: Entry point with command-line argument parsing using System.CommandLine.

## 📈 Recent Improvements

The RepoAIfy solution has been significantly enhanced with a comprehensive set of improvements. See [CHANGELOG.md](CHANGELOG.md) for a detailed history of changes.

### 🚀 Architectural Refactoring and Feature Enhancements (Latest)
- **Architectural Refactoring**: Reorganized the `RepoAIfyApp` project into a standard MVVM structure (`Views`, `ViewModels`, `Models`, etc.) to improve maintainability and separation of concerns.
- **Feature: Optional File Size Display**: Added a checkbox to the UI to allow users to see the size of each file in the tree view.
- **Feature: Unified & Contextual Cancellation**: Implemented a single "Cancel" button that can stop any long-running task, whether it is populating the file tree or generating markdown files. This is managed by a state machine (`AppState`) in the main view model.
- **UI/UX Enhancements**: Added input validation to numeric fields to prevent errors.

###  foundational Improvements
*A summary of previous improvements establishing the baseline for the current architecture.*
1. **Security & Correctness**: Fixed path traversal, corrected glob pattern matching, and eliminated `async void` methods.
2. **Robustness**: Added `MaxFileSizeMb` to prevent memory issues and enforced null safety in models.
3. **Best Practices**: Centralized DI and logging, improved markdown chunking, and implemented the `IDialogService` pattern for better abstraction.

## ⚙️ Configuration (`options.json`)

The behavior of `RepoAIfy` is controlled by an `options.json` file with the following structure:

```json
{
  "FileFilter": {
    "IncludedExtensions": [
      ".cs",
      ".csproj",
      ".sln",
      ".json",
      ".md",
      ".xaml",
      ".xaml.cs"
    ],
    "ExcludedDirectories": [
      "**/bin/",
      "**/obj/",
      "**/.vs/",
      "**/.git/"
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

*   **`IncludedExtensions`**: An array of file extensions to include.
*   **`ExcludedDirectories`**: An array of glob patterns for directories to exclude. **Note:** Patterns must end with a `/` to correctly match directories.
*   **`MaxFileSizeMb`**: The maximum size in megabytes for any single file to be processed. Files larger than this are skipped to prevent high memory usage.
*   **`MaxChunkSizeKb`**: The maximum size in kilobytes for each output markdown file.
*   **`OutputDirectory`**: The relative path where the output files will be saved.

## 📄 License

This project is licensed under the [MIT License](LICENSE).