namespace System.Linq
{
    using Collections.Generic;
    using Expressions;
    using Reflection;
    using Threading.Tasks;
    using Cloud.Core.Web.Filters;

    /// <summary>
    /// Queryable extensions class.
    /// </summary>
    public static class QueryableExtensions
    {
        private const BindingFlags AllPropertiesFlag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        /// <summary>
        /// Creates a dynamic order by (sort) linq statement using the passed in fieldname.  If field name
        /// is not found, the sort will default to the first property of the type.
        /// </summary>
        /// <typeparam name="T">Type of queryable that is being built.</typeparam>
        /// <param name="query">The query object to add the sort to.</param>
        /// <param name="sortField">The sort field to order by.</param>
        /// <param name="ascending">If set to <c>true</c> then order the data ascending, otherwise descending.</param>
        /// <returns>Modified IQueryable with sorting.</returns>
        public static IOrderedQueryable<T> OrderByField<T>(this IQueryable<T> query, string sortField, bool ascending)
        { 
            // Parameter expression.
            var param = Expression.Parameter(typeof(T), "p");

            // List of properties of this type.
            var properties = typeof(T).GetProperties(AllPropertiesFlag);
            
            // Find property that matches the sort field name.
            var prop = properties.FirstOrDefault(p => p.Name == sortField);
            // If property is not found, then default to first property for sorting.
            if (prop == null)
                prop = properties[0];
            
            // Property for lamda expression.
            var expProp = Expression.Property(param, prop);
            var expLamda = Expression.Lambda(expProp, param);
            
            // Set ascending / descending in query.
            string method = ascending ? "OrderBy" : "OrderByDescending";
            Type[] types = new Type[] { query.ElementType, expLamda.Body.Type };
            
            // Create the query and return.
            var mce = Expression.Call(typeof(Queryable), method, types, query.Expression, expLamda);
            return query.Provider.CreateQuery<T>(mce) as IOrderedQueryable<T>;
        }

        /// <summary>
        /// Creates a dynamic then by (sort) linq statement using the passed in fieldname. If field name
        /// is not found, the sort will default to the first property of the type.
        /// </summary>
        /// <typeparam name="T">Type of queryable that is being built.</typeparam>
        /// <param name="query">The query object to add the sort to.</param>
        /// <param name="sortField">The sort field to order by.</param>
        /// <param name="ascending">If set to <c>true</c> then order the data ascending, otherwise descending.</param>
        /// <returns>Modified IQueryable with sorting.</returns>
        public static IOrderedQueryable<T> ThenByField<T>(this IOrderedQueryable<T> query, string sortField, bool ascending)
        {
            // Parameter expression.
            var param = Expression.Parameter(typeof(T), "p");
            
            // List of properties of this type.
            var properties = typeof(T).GetProperties(AllPropertiesFlag);
            
            // Find property that matches the sort field name.
            var prop = properties.FirstOrDefault(p => p.Name == sortField);
            
            // If property is not found, then default to first property for sorting.
            if (prop == null)
                prop = properties[0];
            
            // Property for lamda expression.
            var expProp = Expression.Property(param, prop);
            var expLamda = Expression.Lambda(expProp, param);
            
            // Set ascending / descending in query.
            string method = ascending ? "ThenBy" : "ThenByDescending";
            Type[] types = new Type[] { query.ElementType, expLamda.Body.Type };
            
            // Create the query and return.
            var mce = Expression.Call(typeof(Queryable), method, types, query.Expression, expLamda);
            return query.Provider.CreateQuery<T>(mce) as IOrderedQueryable<T>;
        }

        /// <summary>
        /// OrderBy With Direction
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TKey">The type of the result.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <param name="ascending">if set to <c>true</c> [ascending].</param>
        /// <returns>
        /// Returns an IEnumberable which is ordered by a given key in ascending or descending order
        /// </returns>
        public static IEnumerable<TSource> OrderByWithDirection<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, bool ascending)
        {
            return !ascending ? source.OrderByDescending(keySelector) : source.OrderBy(keySelector);
        }

        /// <summary>
        /// OrderByWithDirection
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <param name="ascending">if set to <c>true</c> [ascending].</param>
        /// <returns>
        /// Returns an IEnumberable which is ordered by a given key in ascending or descending order
        /// </returns>
        public static IEnumerable<TSource> OrderByWithDirection<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, bool ascending)
        {
            return !ascending ? source.OrderByDescending(keySelector) : source.OrderBy(keySelector);
        }

        /// <summary>
        /// Performs the search on the IQueryable and returns the results as a SearchResult.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <param name="filter">The filter options.</param>
        /// <param name="maxAllowedPageSize">Maximum size of page that can be returned.</param>
        /// <returns>
        ///   <see cref="SearchResult{T}" /> result.
        /// </returns>
        public static async Task<SearchResult<TResult>> PerformSearch<TSource, TResult>(this IQueryable<TResult> query, SearchFilter<TSource> filter, int maxAllowedPageSize = 100) where TSource : class, new()
        {
            // If sort by is not set, then sort using the default id property..
            if (string.IsNullOrEmpty(filter.SortBy))
                filter.SortBy = typeof(TSource).GetProperties(AllPropertiesFlag)[0].Name;
            else
            {
                // Set default property if the passed prop does not exist.
                var propertyMatch = (typeof(TSource).GetProperties(AllPropertiesFlag)
                    .Where(x => x.Name.ToLowerInvariant() == filter.SortBy.ToLowerInvariant())).FirstOrDefault();
                filter.SortBy = propertyMatch != null ? propertyMatch.Name : typeof(TSource).GetProperties(AllPropertiesFlag)[0].Name;
            }

            // If secondary sort by is not set or it is the same as the primary sort then do not performn secondary sort
            if (string.IsNullOrEmpty(filter.SecondarySortBy) || filter.SecondarySortBy.ToLowerInvariant() == filter.SortBy.ToLowerInvariant())
                query = query.OrderByField(filter.SortBy, filter.Ascending);
            else
            {
                var secondPropertyMatch =  (typeof(TSource).GetProperties(AllPropertiesFlag)
                    .Where(x => x.Name.ToLowerInvariant() == filter.SecondarySortBy.ToLowerInvariant())).FirstOrDefault();

                if(secondPropertyMatch != null)
                {
                    filter.SecondarySortBy = secondPropertyMatch.Name;
                    query = query.OrderByField(filter.SortBy, filter.Ascending).ThenByField(filter.SecondarySortBy, filter.SecondaryAscending);
                }
                else
                {
                    query = query.OrderByField(filter.SortBy, filter.Ascending);
                }
            }

            // Work out the total number of records that will be returned before paging is applied.
            var totalRecords = query.Count();
                
            // Don't allow page size to be greater than 100.
            if (filter.PageSize > maxAllowedPageSize)
            {
                filter.PageSize = maxAllowedPageSize;
            }

            // PageSize of 0 indicates all records.
            if (filter.PageSize == 0)
            {
                filter.PageSize = totalRecords;
            }

            query = query.Skip(filter.PageSize * (filter.PageNumber == 1 ? 0 : filter.PageNumber - 1)).Take(filter.PageSize);

            // Perform select on the table and map the results.
            var searchResults = await Task.FromResult(query.ToList());

            //Calculate Total Pages
            int totalPages = (totalRecords == 0 ? 0 : Convert.ToInt32(Math.Ceiling(((decimal)totalRecords / filter.PageSize))));

            // Build response object.
            return new SearchResult<TResult>
            {
                FilterResults = searchResults,
                RecordCount = totalRecords,
                SortBy = filter.SortBy,
                Ascending = filter.Ascending,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalPages = totalPages,
                HasNextPage = totalPages > filter.PageNumber,
                SecondarySortBy = filter.SecondarySortBy,
                SecondaryAscending = filter.SecondaryAscending
            };
        }
    }
}
