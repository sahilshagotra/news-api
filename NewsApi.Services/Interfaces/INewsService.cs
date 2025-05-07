using NewsApi.Models;

namespace NewsApi.Services.Interfaces
{
    public interface INewsService
    {
        Task<PagedResult<Story>> GetNewestStories(int start, int limit, string? query);
    }
}

