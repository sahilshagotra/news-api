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
        /// Gets the latestnews stories.
        /// </summary>
        /// <param name="page">The page number ofthe results.</param>
        /// <param name="pageSize">The number of stories per page.</param>
        /// <returns>Returns a list of news stories.</returns>
        [HttpGet("newest")]
        public async Task<IActionResult> GetNewestStories([FromQuery] int page = 1, [FromQuery] int pageSize = 10,[FromQuery] string? query = null)
        {

            try
            {
                var stories = await _newsService.GetNewestStories(page, pageSize, query);
                if (stories == null || !stories.Any())
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
