using Microsoft.AspNetCore.Mvc;
using Moq;
using NewsApi.Controllers;
using NewsApi.Models;
using NewsApi.Services.Interfaces;

namespace NewsApi.TestProject.Tests;

public class NewsControllerTests
{
    private readonly Mock<INewsService> _newsServiceMock;
    private readonly NewsController _controller;


    public NewsControllerTests()
    {
        _newsServiceMock = new Mock<INewsService>();
        _controller = new NewsController(_newsServiceMock.Object);
    }

    [Fact]
    public async Task GetNewestStories_ReturnsOkResult_WhenStoriesExist()
    {
        // Arrange
        var pagedResult = new PagedResult<Story>
        {
            Items = new List<Story>
            {
                new Story { Id = 1, Title = "Test Story", Url = "http://example.com" }
            },
            TotalCount = 1,
            CurrentPage = 1,
            PageSize = 10
        };

        _newsServiceMock.Setup(service => service.GetNewestStories(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                        .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetNewestStories(1, 10, "query");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedStories = Assert.IsType<PagedResult<Story>>(okResult.Value);
        Assert.Single(returnedStories.Items);
        Assert.Equal("Test Story", returnedStories.Items.First().Title);
    }

    [Fact]
    public async Task GetNewestStories_ReturnsNotFoundResult_WhenNoStoriesExist()
    {
        var emptyResult = new PagedResult<Story>
        {
            Items = new List<Story>(),
            TotalCount = 0,
            CurrentPage = 1,
            PageSize = 10
        };
        // Arrange
        _newsServiceMock.Setup(service => service.GetNewestStories(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(emptyResult);

        // Act
        var result = await _controller.GetNewestStories(1, 10, "query");

        // Assert
        var notFoundResult = Assert.IsType<NotFoundResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task GetNewestStories_Returns500_WhenExceptionThrown()
    {
        // Arrange
        _newsServiceMock.Setup(service => service.GetNewestStories(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).ThrowsAsync(new Exception("Something went wrong"));

        // Act
        var result = await _controller.GetNewestStories(1, 10, "query");

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        Assert.Contains("Something went wrong", objectResult.Value?.ToString());
    }

    [Fact]
    public async Task GetNewestStories_ReturnsOk_WhenQueryIsNull()
    {
        var pagedResult = new PagedResult<Story>(
            new List<Story> { new Story { Id = 1, Title = "Test", Url = "http://test.com" } },
            1, 1, 10);

        _newsServiceMock.Setup(s => s.GetNewestStories(1, 10, null))
                        .ReturnsAsync(pagedResult);

        var result = await _controller.GetNewestStories(1, 10, null);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsType<PagedResult<Story>>(okResult.Value);
        Assert.Single(data.Items);
    }

    [Fact]
    public async Task GetNewestStories_ReturnsNotFound_WhenPagedResultHasZeroItems()
    {
        var pagedResult = new PagedResult<Story>(new List<Story>(), 0, 1, 10);

        _newsServiceMock.Setup(s => s.GetNewestStories(1, 10, null))
                        .ReturnsAsync(pagedResult);

        var result = await _controller.GetNewestStories(1, 10, null);

        Assert.IsType<NotFoundResult>(result);
    }  
    
    [Fact]
    public async Task GetNewestStories_ReturnsNotFound_WhenQueryFiltersOutAllStories()
    {
        var pagedResult = new PagedResult<Story>(new List<Story>(), 0, 1, 10);

        _newsServiceMock.Setup(s => s.GetNewestStories(1, 10, "no-match"))
                        .ReturnsAsync(pagedResult);

        var result = await _controller.GetNewestStories(1, 10, "no-match");

        Assert.IsType<NotFoundResult>(result);
    }


}

