# News API

This is a simple REST API for fetching the latest news stories from a popular news source, with filtering and caching mechanisms. It allows clients to get the most recent stories and filter them based on a search query.

## Features

- **Get Latest Stories**: Fetches the most recent news stories.
- **Search Filtering**: Allows filtering of news stories by title or keyword.
- **Caching**: Uses caching to optimize repeated requests and reduce load times.
- **Pagination**: Supports pagination for fetching results in chunks.

## Technologies Used

- **ASP.NET Core**: For building the API.
- **MoQ**: For unit testing with mocking dependencies.
- **xUnit**: For writing unit tests.
- **MemoryCache**: For in-memory caching of news stories.
- **HttpClient**: For making HTTP requests to external APIs.
- **Swagger**:  For generating API documentation.

## Folder Structure

Here's the basic folder structure for the API project:
Root
│
├── NewsApi/ # API Project
│ ├── Controllers/ # API Controllers
│ │ └── NewsController.cs # The controller for the news endpoints
│ ├── Program.cs # Entry point for the API
│ ├── Startup.cs # Configuration for the API
│ ├── appsettings.json # Configuration for the API project
│ ├── NewsApi.csproj # Project file for NewsApi (API)
│
├── NewsApi.Models/ # Model Project
│ ├── Story.cs # Model for a news story
│ ├── AppSettings.cs # Configuration model for settings
│ └── NewsApi.Models.csproj # Project file for the Models
│
├── NewsApi.Services/ # Service Project
│ ├── Interfaces/ # Service interfaces
│ │ └── INewsService.cs # Interface for news-related functionality
│ ├── Services/ # Service implementation
│ │ └── NewsService.cs # Implements the INewsService logic
│ └── NewsApi.Services.csproj # Project file for the Services
│
├── NewsApi.Tests/ # Unit Testing Project
│ ├── NewsControllerTests.cs # Unit tests for NewsController
│ └── NewsServiceTests.cs # Unit tests for NewsService
│ └── NewsApi.Tests.csproj # Project file for the testing project
│
└── NewsApi.sln # Solution file containing all the projects

## Setup Instructions

### Prerequisites

- .NET SDK 8.0 or higher.
- Visual Studio 2022 or later (or VS Code with the C# extension).
- Git for version control.

### Clone the Repository

```bash
git clone

dotnet restore

dotnet build

dotnet run

### Running unit test
dotnet test
