using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NewsApi.Models;
using NewsApi.Services.Interfaces;
using Newtonsoft.Json;



namespace NewsApi.Services.Services
{
    public class NewsService : INewsService
    {

        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly AppSettings _appSettings;

        public NewsService(HttpClient httpClient, IMemoryCache memoryCache, IOptions<AppSettings> options)
        {
            _httpClient = httpClient;
            _cache = memoryCache;
            _appSettings = options.Value;
        }

        /// <summary>
        /// Fetches the newest stories from the Hacker News API.
        /// </summary>
        /// <param name="start">The page number of the results (1-based index).</param>
        /// <param name="limit">The number of stories per page.</param>
        /// <param name="query">An optional search query to filter stories by title.</param>
        /// <returns>Returns a list of stories, or an empty list if no stories are found.</returns>
        /// <remarks>
        /// This method first checks the cache for a list of story IDs.
        /// If the list is not in the cache, it fetches the story IDs from the News API and caches them.
        /// Each individual story is also cached for later use.
        /// </remarks>
        public async Task<PagedResult<Story>> GetNewestStories(int start, int limit, string? query)
        {
            if (start < 1) start = 1;
            if (limit < 1) limit = 10;

            int topStoriesLimit = 200;

            var startIndex = (start - 1) * limit;
            var baseUrl = _appSettings.NewsBaseUrl;

            //Fetch story ID'sfrom cache or API
            if (!_cache.TryGetValue("StoryIds", out List<int>? storyIds) || storyIds == null)
            {
                var cacheresponse = await _httpClient.GetStringAsync($"{baseUrl}/newstories.json?print=pretty");
                storyIds = JsonConvert.DeserializeObject<List<int>>(cacheresponse);

                if (storyIds == null || storyIds.Count == 0)
                    return new PagedResult<Story>(new List<Story>(), 0, start, limit);

                _cache.Set("StoryIds", storyIds, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });
            }

            storyIds = storyIds.Take(topStoriesLimit).ToList();

            var allStories = new List<Story>();

            foreach (var storyId in storyIds)
            {

                if (!_cache.TryGetValue($"Story_{storyId}", out Story? cachedStory))
                {
                    var storyResponse = await _httpClient.GetStringAsync($"{baseUrl}/item/{storyId}.json?print=pretty");
                    var story = JsonConvert.DeserializeObject<Story>(storyResponse);

                    // Only add the story if it has a valid URL
                    if (story != null && !string.IsNullOrEmpty(story.Url))
                    {
                        allStories.Add(story);

                        // Cache the story for later use
                        _cache.Set($"Story_{storyId}", story, new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                        });
                    }
                }
                else
                {
                    // Add cached story if it has a valid URL
                    if (cachedStory != null && !string.IsNullOrEmpty(cachedStory.Url))
                    {
                        allStories.Add(cachedStory);
                    }
                }
            }
            // Apply Search query if present
            if (!string.IsNullOrWhiteSpace(query))
            {
                allStories = allStories
                    .Where(s => s.Title != null && s.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            //Pagination
            var pagedStories = allStories.Skip((start - 1) * limit).Take(limit).ToList();
            return new PagedResult<Story>(pagedStories, allStories.Count, start, limit);
        }
    }
}
