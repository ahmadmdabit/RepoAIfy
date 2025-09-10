# Changelog

All notable changes to the RepoAIfy project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- MaxFileSizeMb configuration option to prevent memory issues with large files
- Continuation headers for markdown chunks to provide context in multi-part outputs
- Enhanced security with robust path validation to prevent directory traversal attacks
- Dependency injection using Microsoft.Extensions.Hosting in both WPF and Console applications
- Structured logging with ILogger throughout the application
- AsyncRelayCommand for safe async command handling in WPF
- IDialogService pattern for proper abstraction of UI-specific functionality
- UILogRelayService for thread-safe log message relaying to the UI
- Markdown preview feature with tabbed interface in WPF application
- GeneratedFileViewModel to represent generated Markdown files
- Markdig.Wpf NuGet package for Markdown rendering

### Changed
- Improved file filtering logic with proper glob pattern matching
- Enhanced markdown chunking with efficient byte counting
- Refactored all service classes to use constructor injection
- Updated platform targeting consistency across all projects
- Improved log file locations to use user-specific directories
- Enhanced error handling with structured logging instead of Console.WriteLine
- Implemented proper DI pattern for MainWindowViewModel with IDialogService abstraction
- Improved logging architecture with thread-safe UI log relay
- Replaced simple log TextBox with tabbed interface (Logs and Markdown Output)
- Removed unnecessary package references for cleaner dependencies

### Fixed
- Critical path traversal security vulnerability in OptionsLoader
- File filtering logic bug in TreeViewDataService
- Potential unhandled exceptions from async void methods
- Null-reference exceptions from nullable properties in FileSystemTree
- Inefficient byte counting in markdown generator
- Platform targeting inconsistencies causing build warnings
- Dependency injection conflict with MainWindowViewModel requiring instance methods from MainWindow
- Thread safety issues with UI log message handling

### Removed
- Direct service instantiation in favor of dependency injection
- Console.WriteLine calls in favor of structured logging
- Service Locator anti-pattern in favor of proper DI