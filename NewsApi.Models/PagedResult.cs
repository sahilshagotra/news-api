using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsApi.Models
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>(); // The collection of items
        public int TotalCount { get; set; }  =0;      // Total number of items
        public int CurrentPage { get; set; }  =1;     // Current page number
        public int PageSize { get; set; }   =10;       // Number of items per page
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);  // Total number of pages

        public PagedResult() { }
        
        // Constructor to initialize the PagedResult
        public PagedResult(IEnumerable<T> items, int totalCount, int currentPage, int pageSize)
        {
            Items = items ?? throw new ArgumentNullException(nameof(items));
            TotalCount = totalCount;
            CurrentPage = currentPage;
            PageSize = pageSize;
        }
    }
}
