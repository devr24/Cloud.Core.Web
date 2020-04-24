using System;
using System.Collections.Generic;

namespace Cloud.Core.Web.Filters
{
    /// <summary>
    /// 
    /// </summary>
    public class FilterBase
    {
        private int _pageNumber = 1;
        /// <summary>
        /// Page Size (defaults to 100)
        /// </summary>
        public int PageSize { get; set; } = 100;
        /// <summary>
        /// Page Number
        /// </summary>
        public int PageNumber { get => _pageNumber; set => _pageNumber = value == 0 ? 1 : value; }
        /// <summary>
        /// Sort By
        /// </summary>
        public string SortBy { get; set; }
        /// <summary>
        /// Ascending
        /// </summary>
        public bool Ascending { get; set; } = true;
        /// <summary>
        /// Secondary Sort By 
        /// This is the name of the second field used to sort the search results
        /// </summary>
        public string SecondarySortBy { get; set; }
        /// <summary>
        /// SecondaryAscending
        /// </summary>
        public bool SecondaryAscending { get; set; } = true;
    }

    /// <summary>
    /// Filter Base
    /// </summary>
    public class SearchFilter<T> : FilterBase where T : class, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchFilter{T}"/> class.
        /// </summary>
        public SearchFilter()
        {
            FilterData = (T)Activator.CreateInstance(typeof(T));
        }

        /// <summary>
        /// Gets or sets the filter data.
        /// This data is the data that will be searched on.
        /// </summary>
        /// <value>
        /// The filter data.
        /// </value>
        public T FilterData { get; set; }
    }

    /// <summary>
    /// PaginationInfo
    /// </summary>
    public class SearchResult<T> : FilterBase
    {
        /// <summary>
        /// Gets or sets the filtered results.
        /// </summary>
        /// <value>
        /// The results.
        /// </value>
        public IList<T> FilterResults { get; set; }
        /// <summary>
        /// Record Count
        /// </summary>
        public int RecordCount { get; set; }
        /// <summary>
        /// Page Record from
        /// </summary>
        public int PageRecordsFrom => (TotalPages == 0 ? 0 : TotalPages == 1 ? 1 : (PageSize * (PageNumber - 1)) + 1);
        /// <summary>
        /// Page Record To
        /// </summary>
        public int PageRecordsTo => GetRecordsTo();
        /// <summary>
        /// Total Page
        /// </summary>
        public int TotalPages { get; set; }
        /// <summary>True if has another page of results, false if not</summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchResult{T}"/> class.
        /// </summary>
        public SearchResult()
        {
            FilterResults = (List<T>)Activator.CreateInstance(typeof(List<T>));
        }

        /// <summary>
        /// private method to work out totals and page sizes.
        /// </summary>
        /// <returns>
        /// Returns which record number is the highest to be retrieved based on the TotalPages, PageNumber and PageSize
        /// </returns>
        private int GetRecordsTo()
        {
            if (TotalPages == 1 || PageNumber == TotalPages)
                return RecordCount;

            return PageSize * PageNumber;
        }
    }
}
