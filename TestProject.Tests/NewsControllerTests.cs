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
        var stories = new List<Story>
            {
                new Story { Id = 1, Title = "Test Story", Url = "http://example.com" }
            };
        string query = "Searchable";
        _newsServiceMock.Setup(service => service.GetNewestStories(It.IsAny<int>(), It.IsAny<int>(), query))
                        .ReturnsAsync(stories);

        // Act
        var result = await _controller.GetNewestStories(1, 10, query);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedStories = Assert.IsAssignableFrom<IEnumerable<Story>>(okResult.Value);
        Assert.Single(returnedStories);
        Assert.Equal("Test Story", returnedStories.First().Title);
    }

    [Fact]
    public async Task GetNewestStories_ReturnsNotFoundResult_WhenNoStoriesExist()
    {
        // Arrange
        string query = "Searchable";
        _newsServiceMock.Setup(service => service.GetNewestStories(It.IsAny<int>(), It.IsAny<int>(), query)).ReturnsAsync(new List<Story>());

        // Act
        var result = await _controller.GetNewestStories(1, 10, query);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task GetNewestStories_Returns500_WhenExceptionThrown()
    {
        // Arrange
        string query = "Searchable";
        _newsServiceMock.Setup(service => service.GetNewestStories(It.IsAny<int>(), It.IsAny<int>(), query)).ThrowsAsync(new Exception("Something went wrong"));

        // Act
        var result = await _controller.GetNewestStories(1, 10, query);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        Assert.Contains("Something went wrong", objectResult.Value?.ToString());
    }

}

