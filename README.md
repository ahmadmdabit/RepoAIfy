# RepoAIfy

## Overview

`RepoAIfy` is a powerful .NET 9 solution designed to streamline the process of analyzing and documenting codebases. It provides two primary interfaces:

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

## Getting Started & User Manual

This guide focuses on the primary WPF application (`RepoAIfyApp`).

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

### Application Usage (Step-by-Step)

Upon launching, you will see the main window. Follow these steps to generate your markdown file.

**Step 1: Select a Source Directory**
*   Click the **Browse...** button next to the "Source Directory" field.
*   An explorer window will open. Navigate to and select the root folder of the codebase you want to analyze.
*   Once selected, the file tree view below will automatically populate with the directories and files from that location.

**Step 2: Load Configuration**
*   The application automatically loads the `options.json` file located in its directory by default.
*   The fields for extensions, excluded directories, chunk size, and output directory will be filled with the values from this file.
*   You can optionally click the **Browse...** button next to "Options File" to load a different configuration.

**Step 3: Refine File Selection**
This is the most powerful feature of the UI. You have two ways to refine which files are included:
*   **Interactive Tree View:**
    *   Use the checkboxes next to each file and folder to manually include or exclude them from the output.
    *   Checking or unchecking a folder will apply the same state to all of its children.
*   **Live Filter Text Boxes:**
    *   Modify the comma-separated list in the **"Included Extensions"** text box. The tree view will update automatically after you stop typing to show only files with matching extensions.
    *   Modify the comma-separated glob patterns in the **"Excluded Directories"** text box. The tree will update to hide files and folders matching these patterns.

**Step 4: Configure Output Settings**
*   **Max Chunk Size (KB):** Adjust the maximum size for each output markdown file.
*   **Output Directory:** Specify the folder where the generated files will be saved. This path is relative to the application's executable directory.

**Step 5: Generate the Output**
*   Click the large **Generate** button.
*   The UI will become disabled during processing, and you will see detailed logs appear in the "Logs" panel in real-time.

**Step 6: Review the Results**
*   The **Status Bar** at the bottom will update from "Processing..." to "Processing Complete." or an error message.
*   Navigate to the specified output directory (e.g., `src/RepoAIfyApp/bin/Debug/net9.0-windows/ai-output`) to find your generated `.md` file(s).

<details>
<summary><b>Advanced: Using the Console Application</b></summary>

For automation and scripting, you can use the `RepoAIfy` console application.

#### Build the Console App
From the solution's `src` directory, run:
```bash
dotnet build RepoAIfy
```
This places the executable in `RepoAIfy/bin/Debug/net9.0/`.

#### Run the Console App
To run the application, use the `dotnet run` command from the `src` directory. You must provide the path to your source directory and the `options.json` file.

```bash
dotnet run --project RepoAIfy -- --source "./YourSourceDirectory" --options "./options.json"
```

**Replace:**
*   `"./YourSourceDirectory"` with the relative path to the directory you want to process.
*   `"./options.json"` with the relative path to your configuration file.

The output files will be created in the `ai-output` directory (or as configured in your options file).

</details>

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

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for a detailed history of changes to this project.

## License

This project is licensed under the [MIT License](LICENSE).