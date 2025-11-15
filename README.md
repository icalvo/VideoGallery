# VideoGallery

A comprehensive video library management system built with .NET, featuring both a command-line interface and a modern web application for organizing, tagging, and tracking your video collection.

## Features

- **Video Management**: Add, update, and organize videos with metadata
- **Tagging System**: Flexible tagging with categories (actors, composition, etc.)
- **Watch Tracking**: Track viewing history and statistics
- **Search & Filter**: Advanced filtering and listing capabilities
- **Interactive Shell**: Command-line interface with interactive mode
- **Web Interface**: Modern web UI for browsing and managing your collection
- **Calendar View**: Visualize your viewing patterns
- **Plugin System**: Extensible architecture for custom tag validation
- **Multiple Storage Options**: Support for filesystem and Dropbox storage
- **Video Downloading**: Integrated yt-dlp support for downloading videos
- **Database Backed**: PostgreSQL database for reliable data storage

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later
- [PostgreSQL](https://www.postgresql.org/) database server
- (Optional) [yt-dlp](https://github.com/yt-dlp/yt-dlp) for video downloading functionality

## Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd VideoGallery
   ```

2. **Set up the database**
   
   Create a PostgreSQL database for the application.

3. **Configure user secrets**
   
   Navigate to the CommandLine project directory:
   ```bash
   cd src/CommandLine
   ```
   
   Set your connection string and storage configuration:
   ```bash
   dotnet user-secrets set "ConnectionStrings:Db" "Host=localhost;Database=videogallery;Username=your_user;Password=your_password"
   dotnet user-secrets set "Storage:Folder" "/path/to/your/video/folder"
   dotnet user-secrets set "Storage:Type" "FileSystem"
   ```

4. **Initialize the database**
   ```bash
   dotnet run --project src/CommandLine/CommandLine.csproj init
   ```

5. **Build the solution**
   ```bash
   dotnet build src/VideoGallery.sln
   ```

## Usage

### Command Line Interface

The CLI tool (`vd`) provides several commands for managing your video library:

**Interactive Shell Mode** (default when run without arguments):
```bash
dotnet run --project src/CommandLine/CommandLine.csproj
```

Available commands in shell mode:
- `list` - List and filter videos
- `details` - Show detailed information about a video
- `open` - Open a video for viewing
- `update` - Update video metadata
- `watch` - Mark a video as watched
- `unwatch` - Remove watch history

**Direct Command Execution**:
```bash
# Add a new video
dotnet run --project src/CommandLine/CommandLine.csproj add

# List all videos
dotnet run --project src/CommandLine/CommandLine.csproj list

# Recalculate calculated tags
dotnet run --project src/CommandLine/CommandLine.csproj calctags

# View watch calendar
dotnet run --project src/CommandLine/CommandLine.csproj calendar

# Register a "no video" event
dotnet run --project src/CommandLine/CommandLine.csproj novideo
```

### Web Interface

Run the web application using the provided PowerShell script:
```powershell
.\Run.ps1
```

Or manually:
```bash
dotnet run --project src/WebSite/Website.csproj --launch-profile https
```

The web interface provides:
- Video grid with thumbnail view
- Detailed video information pages
- Advanced filtering and sorting
- Calendar view of watching activity
- Authentication support (OpenID Connect)

## Configuration

### Storage Types

The application supports multiple storage backends:

- **FileSystem**: Store videos on local or network filesystem
- **Dropbox**: Store videos in Dropbox (requires additional configuration)

Configure via user secrets:
```bash
dotnet user-secrets set "Storage:Type" "FileSystem"  # or "Dropbox"
dotnet user-secrets set "Storage:Folder" "/path/to/videos"
```

### Web Application Settings

The web application can be configured via `appsettings.json`, `appsettings.Development.json`, or `appsettings.Production.json` in the `src/WebSite` directory.

## Project Structure

```
VideoGallery/
├── src/
│   ├── Abstractions/          # Core interfaces and abstractions
│   ├── Library/               # Core business logic and data models
│   │   ├── Infrastructure/    # Storage implementations (Dropbox, FileSystem)
│   │   ├── Migrations/        # Entity Framework migrations
│   │   └── Parsing/           # Query parsing logic
│   ├── CommandLine/           # CLI application
│   │   ├── Listing/           # List and filter commands
│   │   └── Utils/             # CLI utilities
│   ├── WebSite/              # ASP.NET Core web application
│   │   ├── Auth/             # Authentication configuration
│   │   ├── Calendar/         # Calendar view
│   │   ├── VideoDetail/      # Video detail pages
│   │   └── VideoGrid/        # Video grid view
│   └── Tests/                # Unit tests
├── Run.ps1                   # Quick start script for web app
└── README.md                 # This file
```

## Technology Stack

- **Framework**: .NET 10.0
- **Database**: PostgreSQL with Entity Framework Core
- **Web Framework**: ASP.NET Core with Razor Pages
- **CLI Framework**: Custom shell with Spectre.Console
- **Authentication**: OpenID Connect
- **Storage**: FileSystem / Dropbox API
- **Video Download**: yt-dlp via CliWrap
- **Testing**: xUnit

## Key Dependencies

- Npgsql.EntityFrameworkCore.PostgreSQL - PostgreSQL provider
- Spectre.Console - Beautiful console applications
- Dropbox.Api - Dropbox integration
- CliWrap - Process execution wrapper
- Microsoft.AspNetCore.Authentication.OpenIdConnect - Authentication
- Duende.AccessTokenManagement - Token management

## Development

### Running Tests

```bash
dotnet test src/Tests/Tests.csproj
```

### Creating Migrations

```bash
cd src/Library
dotnet ef migrations add MigrationName --startup-project ../CommandLine/CommandLine.csproj
```

### Plugin Development

The application supports custom tag validation plugins. Create a class implementing `ITagValidation` and load it via the plugin system.

## License

See [LICENSE.md]

## Contributing

## Support

