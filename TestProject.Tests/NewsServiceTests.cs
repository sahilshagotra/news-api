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

    [Fact]
    public async Task GetNewestStories_ReturnsCachedResult()
    {
        // Arrange
        var service = new NewsService(_httpClient, _memoryCache, _options);

        // Act
        var result = await service.GetNewestStories(1, 10, null);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(123, result.First().Id);
    }

}



