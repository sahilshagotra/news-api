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

        public async Task<List<Story>> GetNewestStories(int start, int limit, string query)
        {
            if (start < 1) start = 1;
            if (limit < 1) limit = 10;

            int topStoriesLimit = 200;

            var startIndex = (start - 1) * limit;
            var baseUrl = _appSettings.NewsBaseUrl;

            if (!_cache.TryGetValue("StoryIds", out List<int>? storyIds) || storyIds == null)
            {
                var cacheresponse = await _httpClient.GetStringAsync($"{baseUrl}/newstories.json?print=pretty");
                storyIds = JsonConvert.DeserializeObject<List<int>>(cacheresponse);

                if (storyIds == null || storyIds.Count == 0)
                    return new List<Story>();

                _cache.Set("StoryIds", storyIds, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });
            }

            storyIds = storyIds.Take(topStoriesLimit).ToList();

            var allStories = new List<Story>();

            //if (startIndex >= storyIds.Count)
            //    return new List<Story>();

            //var stories = new List<Story>();

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
            if (!string.IsNullOrWhiteSpace(query))
    {
        allStories = allStories
            .Where(s => s.Title != null && s.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

            return allStories.Skip((start-1)*limit).Take(limit).ToList();
        }
    }
}
