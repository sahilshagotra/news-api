using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.Protected;
using System.Net;
using NewsApi.Models;
using NewsApi.Services.Services;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace NewsApi.TestProject.Tests;

public class NewsServiceTests
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;
    private readonly IOptions<AppSettings> _options;

    public NewsServiceTests()
    {
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
               "SendAsync",
               ItExpr.IsAny<HttpRequestMessage>(),
               ItExpr.IsAny<CancellationToken>())
           .ReturnsAsync((HttpRequestMessage request, CancellationToken cancellationToken) =>
           {
               // Determine response based on request URL
               if (request.RequestUri!.AbsolutePath.EndsWith("newstories.json"))
               {
                   return new HttpResponseMessage
                   {
                       StatusCode = HttpStatusCode.OK,
                       Content = new StringContent(JsonSerializer.Serialize(new List<int> { 123 }))
                   };
               }

               if (request.RequestUri.AbsolutePath.Contains("/item/123.json"))
               {
                   return new HttpResponseMessage
                   {
                       StatusCode = HttpStatusCode.OK,
                       Content = new StringContent(JsonSerializer.Serialize(new Story
                       {
                           Id = 123,
                           Title = "Test Story",
                           Url = "http://example.com"
                       }))
                   };
               }

               return new HttpResponseMessage
               {
                   StatusCode = HttpStatusCode.NotFound
               };
           });

        _httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://hacker-news.firebaseio.com/v0/")
        };

        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _options = Options.Create(new AppSettings { NewsBaseUrl = "https://hacker-news.firebaseio.com/v0/" });
    }

    private static Mock<HttpMessageHandler> SetupHttpMock(string storyIdsUrl, string storyIdsContent, params (string url, string content)[] storyResponses)
    {
        var mockHandler = new Mock<HttpMessageHandler>();

        // Setup story ID list endpoint
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.AbsolutePath.EndsWith(storyIdsUrl)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(storyIdsContent)
            });

        // Setup individual story endpoints
        foreach (var (url, content) in storyResponses)
        {
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.AbsolutePath.Contains(url)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(content)
                });
        }

        return mockHandler;
    }
    private static NewsService CreateService(Mock<HttpMessageHandler> handlerMock)
    {
        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://hacker-news.firebaseio.com/v0/")
        };
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var options = Options.Create(new AppSettings { NewsBaseUrl = "https://hacker-news.firebaseio.com/v0/" });

        return new NewsService(httpClient, memoryCache, options);
    }


    [Fact]
    public async Task GetNewestStories_ReturnsCachedResult()
    {
        // Arrange
        var service = new NewsService(_httpClient, _memoryCache, _options);

        // Act
        var result = await service.GetNewestStories(1, 10, null);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<PagedResult<Story>>(result);
        Assert.Single(result.Items);
        Assert.Equal(123, result.Items.First().Id);
        Assert.Equal(1, result.CurrentPage);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task GetNewestStories_ReturnsEmpty_WhenNoStoryIdsAvailable()
    {
        var handlerMock = SetupHttpMock("newstories.json", "[]");
        var service = CreateService(handlerMock);

        var result = await service.GetNewestStories(1, 10, null);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task GetNewestStories_HandlesInvalidStoryGracefully()
    {
        var handlerMock = SetupHttpMock(
            "newstories.json", "[123, 124]",
            ("item/123.json", JsonSerializer.Serialize(new Story { Id = 123, Title = "Valid", Url = "http://valid.com" })),
            ("item/124.json", "null") // Invalid
        );

        var service = CreateService(handlerMock);
        var result = await service.GetNewestStories(1, 10, null);

        Assert.Single(result.Items);
        Assert.Equal(123, result.Items.First().Id);
    }

    [Fact]
    public async Task GetNewestStories_HandlesPartialData()
    {
        var handlerMock = SetupHttpMock(
            "newstories.json", "[123, 124, 125]",
            ("item/123.json", "{\"id\":123,\"title\":\"One\",\"url\":\"http://1.com\"}"),
            ("item/124.json", "{\"id\":124,\"title\":\"Two\"}"),  // No URL
            ("item/125.json", "{\"id\":125,\"title\":\"Three\",\"url\":\"http://3.com\"}")
        );

        var service = CreateService(handlerMock);
        var result = await service.GetNewestStories(1, 10, null);

        Assert.Equal(2, result.Items.Count()); // Only stories with URLs
    }

    [Fact]
    public async Task GetNewestStories_ThrowsException_WhenHttpFails()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("API down"));

        var service = CreateService(handlerMock);

        await Assert.ThrowsAsync<HttpRequestException>(() => service.GetNewestStories(1, 10, null));
    }

    [Fact]
    public async Task GetNewestStories_UsesCache_WhenAvailable()
    {
        _memoryCache.Set("StoryIds", new List<int> { 123 });
        _memoryCache.Set("Story_123", new Story { Id = 123, Title = "Cached", Url = "http://cached.com" });

        var service = new NewsService(_httpClient, _memoryCache, _options);
        var result = await service.GetNewestStories(1, 10, null);

        Assert.Single(result.Items);
        Assert.Equal("Cached", result.Items.First().Title);
    }
}



