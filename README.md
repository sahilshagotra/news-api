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
