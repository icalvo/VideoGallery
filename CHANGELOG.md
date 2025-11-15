# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Initial implementation of VideoGallery application
- Command-line interface (CLI) with interactive shell mode
- Web application with Razor Pages
- PostgreSQL database backend with Entity Framework Core
- Video management features (add, update, list, filter)
- Tag system with categories and flexible tagging
- Watch tracking functionality with date tracking
- Calendar view for visualizing viewing patterns
- Plugin system for extensible tag validation
- Multi-storage backend support (FileSystem and Dropbox)
- Video download capability using yt-dlp integration
- OpenID Connect authentication for web interface
- Advanced filtering and search capabilities
- Video detail pages with metadata display
- Grid view for browsing video collection
- Calculated tags functionality
- "No video" event registration
- Support for video sequences and duration tracking
- Year-over-year statistics tracking

### Changed
- N/A

### Deprecated
- N/A

### Removed
- N/A

### Fixed
- N/A

### Security
- Implemented OpenID Connect authentication
- User secrets management for sensitive configuration
- Token management with Duende.AccessTokenManagement

## [0.1.0] - 2025-11-15

### Added
- Initial project structure with solution and projects
- Core abstractions layer with interfaces
- Library project with domain models
- CommandLine project for CLI operations
- Website project for web interface
- Tests project with xUnit
- Entity Framework migrations for database schema
- Basic video entity with properties: filename, duration, sequences, comments
- Tag entity with category support
- Watch entity for tracking viewing history
- Video context for database operations
- Interactive shell with contextual commands
- Spectre.Console integration for beautiful CLI output
- Configuration via user secrets and appsettings
- Launch configuration for web application
- PowerShell run script for quick startup

[unreleased]: https://github.com/yourusername/VideoGallery/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/yourusername/VideoGallery/releases/tag/v0.1.0

