using Microsoft.AspNetCore.Mvc;
using NewsApi.Services.Interfaces;
namespace NewsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly INewsService _newsService;

        public NewsController(INewsService newsService)
        {
            _newsService = newsService;
        }

        /// <summary>
        /// Gets the latest news stories.
        /// </summary>
        /// <param name="page">The page number of the results.</param>
        /// <param name="pageSize">The number of stories per page.</param>
        /// <param name="query">An optional search query to filter stories by title.</param>
        /// <returns>Returns a list of news stories, or a 404 Not Found if no stories are found.</returns>
        /// <response code="200">Returns a list of news stories</response>
        /// <response code="404">No stories found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("newest")]
        public async Task<IActionResult> GetNewestStories([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? query = null)
        {

            try
            {
                var stories = await _newsService.GetNewestStories(page, pageSize, query);
                if (stories == null || !stories.Items.Any())
                {
                    return NotFound();
                }
                return Ok(stories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
