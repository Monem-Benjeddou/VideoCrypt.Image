
using System.Collections.Generic;

namespace VideoCrypt.Image.Dashboard.Models
{
    public class PaginatedList<T>
    {
        public List<T> Items { get; set; }
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
        public string SearchQuery { get; set; }
        public bool HasNextPage => PageIndex < TotalPages;
        public bool HasPreviousPage => PageIndex > 1;
    }
}