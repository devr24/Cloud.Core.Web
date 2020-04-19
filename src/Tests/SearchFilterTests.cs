using System;
using System.Collections.Generic;
using System.Text;
using Cloud.Core.Testing;
using Cloud.Core.Web.Filters;
using FluentAssertions;
using Xunit;

namespace Cloud.Core.Web.Tests
{
    [IsUnit]
    public class SearchFilterTests
    {
        [Fact]
        public void SetBasePageNumber()
        {
            var baseFilter = new FilterBase();
            Assert.Equal(1, baseFilter.PageNumber);
            baseFilter.PageNumber = 0;
            Assert.Equal(1, baseFilter.PageNumber);
            baseFilter.PageNumber = 1;
            Assert.Equal(1, baseFilter.PageNumber);
        }

        [Fact]
        public void DefaultPageNumberOnSearchResultIsOne()
        {
            var searchResult = new SearchResult<string>();
            
            Assert.Equal(1, searchResult.PageNumber);
        }

        [Theory]
        [InlineData(1, 86)]
        [InlineData(10, 100)]
        public void GetFromToRecordCounts(int totalPages, int recordCount)
        {
            var searchResult = new SearchResult<string>();


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
            searchResult.PageRecordsFrom.Should().Be(1);
            searchResult.PageRecordsTo.Should().Be(recordCount);
            Assert.Equal(1, searchResult.PageRecordsFrom);
            Assert.Equal(recordCount, searchResult.PageRecordsTo);
        }
    }
}
