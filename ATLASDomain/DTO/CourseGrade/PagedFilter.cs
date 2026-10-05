namespace ATLASDomain.DTO
{
    /// <summary>
    /// Paging parameters shared by every list endpoint (MED-12).
    /// </summary>
    /// <remarks>
    /// The list endpoints used to materialise the whole table, so one request
    /// could return every student or every staff member. Deriving the filter
    /// DTOs from this type gives them a bounded page size by default, and the
    /// repositories clamp whatever the client sends to
    /// <see cref="MaxPageSize"/>.
    /// </remarks>
    public class PagedFilter
    {
        /// <summary>
        /// Upper bound on the number of rows one page may return.
        /// </summary>
        public const int MaxPageSize = 100;

        /// <summary>
        /// Number of the page to return, starting at 1.
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Number of rows per page.
        /// </summary>
        public int PageSize { get; set; } = 25;

        /// <summary>
        /// Normalizes the requested paging values so a client cannot ask for a
        /// page below 1, or a page larger than <see cref="MaxPageSize"/>.
        /// </summary>
        /// <returns>The page number and page size to use.</returns>
        public (int PageNumber, int PageSize) Normalize()
            => (Math.Max(1, PageNumber), Math.Clamp(PageSize, 1, MaxPageSize));
    }

    /// <summary>
    /// A single page of results plus the counters a client needs to page through
    /// the rest.
    /// </summary>
    /// <typeparam name="T">Type of the rows in the page.</typeparam>
    public class PagedResponse<T>
    {
        /// <summary>
        /// The rows on this page.
        /// </summary>
        public List<T> Items { get; set; } = new();

        /// <summary>
        /// Page that was returned.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Number of rows per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of matching rows across all pages.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Number of pages available.
        /// </summary>
        public int TotalPages => PageSize <= 0
            ? 0
            : (int)Math.Ceiling((double)TotalCount / PageSize);

        /// <summary>
        /// Whether a page follows this one.
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;

        /// <summary>
        /// Whether a page precedes this one.
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;
    }
}
