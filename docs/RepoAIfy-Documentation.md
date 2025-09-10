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
*   **Cross-Platform Core:** The core logic is built with .NET 9, with the console app being fully cross-platform. The WPF application is for Windows.

## Project Structure

The solution is architected with a clean separation of concerns:

*   `RepoAIfyLib`: A .NET class library containing all the core business logic (file processing, filtering, Markdown generation). It has no dependency on any UI framework.
*   `RepoAIfyApp`: The primary WPF desktop application. It provides the graphical user interface and consumes `RepoAIfyLib`.
*   `RepoAIfy`: The original console application, ideal for scripting and automation. It also consumes `RepoAIfyLib`.

## Getting Started

### Prerequisites

*   [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
*   [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) (Recommended for the best experience)
*   Windows Operating System (for the WPF application)

### Project Setup

1.  **Clone the Repository:**
    ```bash
    git clone <repository-url>
    ```
2.  **Open the Solution:**
    Navigate to the `src` directory and open the `RepoAIfy.sln` file in Visual Studio 2022.
3.  **Build the Solution:**
    Press `Ctrl+Shift+B` or go to `Build > Build Solution` in Visual Studio. This will restore all necessary NuGet packages and compile all three projects.

### Running the Application

1.  **Set Startup Project:** In the Solution Explorer, right-click the **`RepoAIfyApp`** project and select "Set as Startup Project".
2.  **Run:** Press `F5` or click the "Start" button in Visual Studio to build and run the WPF application.

## Configuration (`options.json`)

The behavior of `RepoAIfy` is controlled by the `options.json` file.

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

## Architectural Transformation

### Overview
RepoAIfy has undergone a comprehensive architectural transformation, elevating it from a functional application to a professionally structured, maintainable, and extensible solution. All planned improvements have been implemented, resulting in a robust, production-ready application that follows modern .NET best practices.

### Key Improvements

#### 1. Dependency Injection Implementation
The most significant improvement was the implementation of proper dependency injection patterns:

**Before:**
- ViewModels directly depended on `Func<string?>` delegates for dialog operations
- Tight coupling between View and ViewModel
- Service Locator anti-pattern in use

**After:**
- Created `IDialogService` interface defining UI dialog operations
- Implemented `WpfDialogService` as concrete WPF-specific implementation
- ViewModels now depend on the interface rather than concrete implementations
- Proper dependency injection throughout the application

**Benefits:**
- Fully decoupled ViewModels from UI framework specifics
- Completely testable with mock implementations
- Adherence to Dependency Inversion Principle
- Centralized service configuration

#### 2. Enhanced Logging Architecture
A new thread-safe logging relay system was implemented:

**Components Added:**
- `UILogRelayService`: Thread-safe event broadcaster for UI log messages
- Improved `ViewModelSink` that publishes to the relay service
- Proper UI thread marshaling for log message updates

**Benefits:**
- Thread safety when updating UI from background threads
- Better separation of logging concerns
- Improved performance and reliability

#### 3. Improved Dependency Injection Configuration
The DI container configuration was significantly enhanced:

**Changes:**
- Proper registration of all services in `App.xaml.cs`
- Elimination of Service Locator anti-pattern
- Constructor injection for all ViewModels
- Simplified `MainWindow.xaml.cs` with DI-provided ViewModel

**Benefits:**
- Full compliance with DI principles
- Centralized service configuration
- Improved maintainability
- Better testability

#### 4. Code Quality and Structure Improvements
Several code quality enhancements were made:

**Changes:**
- Removed unnecessary package references
- Improved namespace organization
- Better code commenting and documentation
- Removal of redundant using statements

**Benefits:**
- Cleaner dependencies
- Better code organization
- Improved readability

## Completed Work Summary

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

## Key Technical Achievements

### 1. Professional Dependency Injection Implementation
- Eliminated Service Locator anti-pattern
- Implemented proper constructor injection for all components
- Created interface-based abstractions for UI operations
- Centralized service registration in application startup

### 2. Thread-Safe UI Operations
- Implemented UILogRelayService for safe cross-thread communication
- Proper UI thread marshaling for log message updates
- Improved handling of background task completion

### 3. Enhanced Testability
- ViewModels can now be unit tested with mock implementations
- Decoupled UI-specific operations through interfaces
- Clear separation of concerns throughout the application

### 4. Improved Maintainability
- Clean separation of UI and business logic
- Single responsibility principle adherence
- Centralized configuration and service registration

## Quality Attributes Achieved

### Security
- Protected against directory traversal attacks with robust path validation
- Eliminated potential injection vulnerabilities

### Reliability
- Eliminated potential crashes from unhandled exceptions
- Prevented null-reference exceptions through proper validation
- Added memory protection for large files

### Performance
- Efficient byte counting in markdown generator
- Improved file processing algorithms
- Memory protection for large files

### Maintainability
- Clean separation of concerns
- Interface-based abstractions for extensibility
- Centralized service configuration
- Clear code organization

### Testability
- Full dependency injection support
- Interface-based components for mocking
- Decoupled UI and business logic

### Extensibility
- Easy addition of new dialog types through IDialogService
- Alternative UI implementations possible
- Modular service architecture

## Future Roadmap

With all planned improvements completed, RepoAIfy is now a production-ready solution. Future work could include:

1. **Enhanced Testing Suite**: Implement comprehensive unit and integration tests
2. **Additional UI Themes**: Support for light/dark mode switching
3. **Performance Monitoring**: Add performance metrics and profiling capabilities
4. **Plugin Architecture**: Support for third-party extensions
5. **Internationalization**: Support for multiple languages

## Conclusion

The RepoAIfy project has successfully undergone a complete architectural transformation, resulting in a professional-grade application that exemplifies modern .NET development practices. The solution is now secure, reliable, maintainable, and extensible, positioning it well for future growth and enhancement.

All planned work has been completed to the highest standards, and the application is ready for production use.