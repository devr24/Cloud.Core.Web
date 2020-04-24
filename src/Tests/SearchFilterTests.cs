using Cloud.Core.Testing;
using Cloud.Core.Web.Filters;
using FluentAssertions;
using Xunit;

namespace Cloud.Core.Web.Tests
{
    [IsUnit]
    public class SearchFilterTests
    {
        /// <summary>Ensure the page number is set as expected.</summary>
        [Fact]
        public void Test_SearchFilter_SetBasePageNumber()
        {
            // Arrange
            var baseFilter = new FilterBase();

            // Act/Assert
            Assert.Equal(1, baseFilter.PageNumber);
            baseFilter.PageNumber = 0;
            Assert.Equal(1, baseFilter.PageNumber);
            baseFilter.PageNumber = 1;
            Assert.Equal(1, baseFilter.PageNumber);
        }

        /// <summary>Ensure the default page number.</summary>
        [Fact]
        public void Test_SearchFilter_DefaultPageNumberOnSearchResultIsOne()
        {
            // Arrange
            var searchResult = new SearchResult<string>();
            
            // Act/Assert
            Assert.Equal(1, searchResult.PageNumber);
        }

        /// <summary>
        /// Ensure GetFromToRecordCounts returns the correct page result summary information.
        /// </summary>
        /// <param name="totalPages">The total pages.</param>
        /// <param name="recordCount">The record count.</param>
        [Theory]
        [InlineData(1, 86)]
        [InlineData(10, 100)]
        public void Test_SearchFilter_GetFromToRecordCounts(int totalPages, int recordCount)
        {
            // Arrange
            var searchResult = new SearchResult<string>();

            // Act
            searchResult.PageRecordsFrom.Should().Be(0);
            searchResult.TotalPages = 1;
            searchResult.PageRecordsFrom.Should().Be(1);
            searchResult.PageSize = 10;
            searchResult.PageNumber = 0;
            searchResult.PageNumber = 1;
            searchResult.RecordCount = 100;
            searchResult.TotalPages = 10;
            searchResult.PageSize = 100;
            searchResult.PageNumber = 1;
            searchResult.TotalPages = totalPages;
            searchResult.RecordCount = recordCount;

            // Assert
            searchResult.PageRecordsFrom.Should().Be(1);
            searchResult.PageRecordsTo.Should().Be(recordCount);
            Assert.Equal(1, searchResult.PageRecordsFrom);
            Assert.Equal(recordCount, searchResult.PageRecordsTo);
        }
    }
}
