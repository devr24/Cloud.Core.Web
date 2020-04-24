using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cloud.Core.Testing;
using Cloud.Core.Web.Filters;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Xunit;

namespace Cloud.Core.Web.Tests
{
    [IsUnit]
    public class ExtensionsTests
    {
        /// <summary>Verify assembly name is gathered correctly.</summary>
        [Fact]
        public void Test_AssemblyExtensions_GetName()
        {
            // Arrange/Act
            Assembly testAssembly = null;
            var assemblyNameBefore = testAssembly.GetAssemblyName();
            testAssembly = Assembly.GetExecutingAssembly();
            var assemblyNameAfter = testAssembly.GetAssemblyName();

            // Assert
            assemblyNameBefore.Should().Be(null); // not got assembly so name is null
            assemblyNameAfter.Should().NotBe(null); // got assembly so name is set
        }

        /// <summary>Verifies Swagger and Versioning services are added using the extension method.</summary>
        [Fact]
        public void Test_ServiceCollection_AddSwaggerWithVersioning()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            var initialCount = services.Count;

            // Act
            services.AddSwaggerWithVersions(new[] { 1.0, 2.0, 2.1 });
            var updatedCount = services.Count;
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            updatedCount.Should().BeGreaterThan(initialCount);
            services.Any(x => x.ServiceType == typeof(Swashbuckle.AspNetCore.Swagger.ISwaggerProvider)).Should().BeTrue();
            services.Any(x => x.ServiceType == typeof(Microsoft.AspNetCore.Mvc.ReportApiVersionsAttribute)).Should().BeTrue();
        }

        /// <summary>Ensure swagger has been added to the application using the extension method.</summary>
        [Fact]
        public void Test_ApplicationBuilder_UseSwaggerWithVersion()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            services.AddSwaggerWithVersions(new[] { 1.0, 2 });
            IApplicationBuilder app = new ApplicationBuilder(services.BuildServiceProvider());

            // Act
            app.UseSwaggerWithVersion(new[] { 1.0, 2 }, null, (c) => c.RoutePrefix = "");

            // Assert
            services.Any(x => x.ServiceType == typeof(Swashbuckle.AspNetCore.Swagger.ISwaggerProvider)).Should().BeTrue();
            services.Any(x => x.ServiceType == typeof(Microsoft.AspNetCore.Mvc.ReportApiVersionsAttribute)).Should().BeTrue();
        }

        /// <summary>Ensure localisation has been added to the application using extension method.</summary>
        [Fact]
        public void Test_ApplicationBuilder_UseLocalizationMiddleware()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            services.AddLocalization();
            IApplicationBuilder app = new ApplicationBuilder(services.BuildServiceProvider());

            // Act
            app.UseLocalization(new[] { "en", "fr" });

            // Assert
            services.Any(x => x.ServiceType == typeof(IStringLocalizerFactory)).Should().BeTrue();
        }

        /// <summary>Ensure error when an unsupported culture is specified.</summary>
        [Fact]
        public void Test_ApplicationBuilder_UseLocalizationMiddleware_NotUsingDefaultCulture()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            IApplicationBuilder app = new ApplicationBuilder(services.BuildServiceProvider());

            // Act
            services.AddLocalization();

            // Assert
            Assert.Throws<ArgumentException>(() => app.UseLocalization(new[] { "fr" }));
        }

        /// <summary>Verify the Api version is gathered as expected.</summary>
        [Fact]
        public void Test_ActionDescription_GetApiVersion()
        {
            // Arrange
            ActionDescriptor description = null;
            var desc = new ActionDescriptor {  Properties = new Dictionary<object, object>()
                {
                    { typeof(ApiVersionModel), new ApiVersionModel(new ApiVersion(1,0)) }
                }
            };

            // Act
            var nullActionResult = description.GetApiVersion();
            var setActionResult = desc.GetApiVersion();

            // Assert
            nullActionResult.Should().BeNull();
            setActionResult.Should().NotBeNull();
            setActionResult.DeclaredApiVersions[0].MajorVersion.Should().Be(1);
            setActionResult.DeclaredApiVersions[0].MinorVersion.Should().Be(0);
        }

        /// <summary>Ensure queryable returns the correct results in ascending order.</summary>
        [Fact]
        public void Test_Queryable_PerformSearchReturnsAscending()
        {
            // Arrange
            var object1 = new QueryableTestObject("c name", 1, "typeone");
            var object2 = new QueryableTestObject("a name", 2, "typeone");
            var object3 = new QueryableTestObject("b name", 3, "typetwo");
            var nameFilter = new SearchFilter<NameFilter>();
            nameFilter.FilterData = new NameFilter("b name");
            var queryObject = new List<QueryableTestObject>() { object1, object2, object3 }.AsQueryable();

            // Act
            var result = queryObject.PerformSearch(nameFilter).GetAwaiter().GetResult();

            // Assert
            result.FilterResults.Should().BeInAscendingOrder(x => x.Name);
        }

        /// <summary>Ensure queryable returns the correct results in descending order.</summary>
        [Fact]
        public void Test_Queryable_PerformSearchReturnsDescending()
        {
            // Arrange
            var object1 = new QueryableTestObject("c name", 1, "typeone");
            var object2 = new QueryableTestObject("a name", 2, "typeone");
            var object3 = new QueryableTestObject("b name", 3, "typetwo");
            var nameFilter = new SearchFilter<NameFilter>();
            nameFilter.FilterData = new NameFilter("b name");
            nameFilter.Ascending = false;
            var queryObject = new List<QueryableTestObject>() { object1, object2, object3 }.AsQueryable();

            // Act
            var result = queryObject.PerformSearch(nameFilter).GetAwaiter().GetResult();

            // Assert
            result.FilterResults.Should().BeInDescendingOrder(x => x.Name);
        }

        /// <summary>Ensure queryable returns the correct results in ascending order.</summary>
        [Fact]
        public void Test_Queryable_SortsByInternalDefaultFieldWhenFieldNameNotSpecified()
        {
            // Arrange
            var object1 = new QueryableTestObject("c name", 1, "c");
            var object2 = new QueryableTestObject("a name", 2, "b");
            var object3 = new QueryableTestObject("b name", 3, "a");
            var nameFilter = new SearchFilter<InternalNameTypeFilter>();
            nameFilter.Ascending = false;
            var queryObject = new List<QueryableTestObject>() { object1, object2, object3 }.AsQueryable();

            // Act
            var result = queryObject.PerformSearch(nameFilter).GetAwaiter().GetResult();

            // Assert
            result.FilterResults.Should().BeInDescendingOrder(x => x.Name);
        }

        /// <summary>Ensure default sort order works as expected.</summary>
        [Fact]
        public void Test_PerformSearch_SortsByInternalDefaultFieldWhenFieldNameDoesNotExist()
        {
            // Arrange
            var object1 = new QueryableTestObject("c name", 1, "c");
            var object2 = new QueryableTestObject("a name", 2, "b");
            var object3 = new QueryableTestObject("b name", 3, "a");
            var nameFilter = new SearchFilter<InternalNameTypeFilter>();
            nameFilter.SortBy = "Number";
            nameFilter.Ascending = false;
            var queryObject = new List<QueryableTestObject>() { object1, object2, object3 }.AsQueryable();

            // Act
            var result = queryObject.PerformSearch(nameFilter).GetAwaiter().GetResult();

            // Assert
            result.FilterResults.Should().BeInDescendingOrder(x => x.Name);
        }

        /// <summary>Ensure paging works as expected.</summary>
        [Fact]
        public void Test_PerformSearch_CanPage50Items()
        {
            // Arrange
            var listObject = new List<QueryableTestObject>();

            for (int i = 0; i <= 105; i++)
            {
                listObject.Add(new QueryableTestObject($"object {i}", i, "TestType"));
            }

            var queryObject = listObject.AsQueryable();

            var nameFilter = new SearchFilter<NameFilter>();
            nameFilter.PageSize = 50;
            nameFilter.FilterData = new NameFilter("name");
            nameFilter.FilterData.Should().NotBeNull();
            nameFilter.FilterData.Name.Should().Be("name");

            // Act
            var result = queryObject.PerformSearch(nameFilter).GetAwaiter().GetResult();

            // Assert
            result.FilterResults.Should().HaveCount(50);
        }

        /// <summary>Ensure page number reflects results set.</summary>
        [Fact]
        public void Test_PerformSearch_CanInterpretWhatPageItsOn()
        {
            // Arrange
            var listObject = new List<QueryableTestObject>();

            for (int i = 0; i <= 105; i++)
            {
                listObject.Add(new QueryableTestObject($"object {i}", i, "TestType"));
            }

            var queryObject = listObject.AsQueryable();

            var nameFilter = new SearchFilter<NameFilter>();
            nameFilter.PageSize = 10;
            nameFilter.PageNumber = 2;

            // Act
            var result = queryObject.PerformSearch(nameFilter).GetAwaiter().GetResult();

            // Assert
            result.FilterResults.Should().HaveCount(10);
            result.FilterResults[0].Name.Should().Match("object 12");
        }

        /// <summary>Ensure all items can be returned at once.</summary>
        [Fact]
        public void Test_PerformSearch_CanReturnAllItems()
        {
            // Arrange
            var listObject = new List<QueryableTestObject>();

            for (int i = 0; i <= 105; i++)
            {
                listObject.Add(new QueryableTestObject($"object {i}", i, "TestType"));
            }

            var queryObject = listObject.AsQueryable();

            var nameFilter = new SearchFilter<NameFilter>();
            nameFilter.PageSize = 0;

            // Act
            var result = queryObject.PerformSearch(nameFilter).GetAwaiter().GetResult();

            // Assert
            result.FilterResults.Should().HaveCount(106);
        }

        /// <summary>Ensure can sort by a specific field.</summary>
        [Fact]
        public void Test_PerformSearch_CanSortByAField()
        {
            // Arrange
            var object1 = new QueryableTestObject("c name", 1, "c typeone");
            var object2 = new QueryableTestObject("a name", 2, "a typetwo");
            var object3 = new QueryableTestObject("b name", 3, "b typethree");

            var nameFilter = new SearchFilter<NameTypeFilter>();

            nameFilter.SortBy = "Type";

            var queryObject = new List<QueryableTestObject>() { object1, object2, object3 }.AsQueryable();

            // Act
            var result = queryObject.PerformSearch(nameFilter).GetAwaiter().GetResult();

            // Assert
            result.FilterResults.Should().BeInAscendingOrder(x => x.Type);
        }

        /// <summary>Ensure secondary sort filter defaults when sorting ascending.</summary>
        [Fact]
        public void Test_PerformSearch_CanSortByASecondaryFieldDefaultsToAscending()
        {
            // Arrange
            var object1 = new QueryableTestObject("c name", 1, "c typeone");
            var object2 = new QueryableTestObject("f name", 2, "a typetwo");
            var object3 = new QueryableTestObject("a name", 2, "a typetwo");
            var object4 = new QueryableTestObject("a name", 3, "b typethree");

            var nameFilter = new SearchFilter<NameTypeFilter>();

            nameFilter.SortBy = "Type";
            nameFilter.SecondarySortBy = "Name";

            var queryObject = new List<QueryableTestObject>() { object1, object2, object3, object4 }.AsQueryable();

            // Act
            var result = queryObject.PerformSearch(nameFilter).GetAwaiter().GetResult();

            // Assert
            result.FilterResults[0].Name.Should().Be("a name");
            result.FilterResults[0].Type.Should().Be("a typetwo");

            result.FilterResults[1].Name.Should().Be("f name");
            result.FilterResults[1].Type.Should().Be("a typetwo");

            result.FilterResults[2].Name.Should().Be("a name");
            result.FilterResults[2].Type.Should().Be("b typethree");

            result.FilterResults[3].Name.Should().Be("c name");
            result.FilterResults[3].Type.Should().Be("c typeone");

            result.FilterResults.Should().BeInAscendingOrder(x => x.Type);
        }

        /// <summary>Ensure secondary sort filter defaults when sorting descending.</summary>
        [Fact]
        public void Test_PerformSearch_CanSortByASecondaryFieldSetToDescending()
        {
            // Arrange
            var object1 = new QueryableTestObject("c name", 1, "c typeone");
            var object2 = new QueryableTestObject("f name", 2, "a typetwo");
            var object3 = new QueryableTestObject("a name", 2, "a typetwo");
            var object4 = new QueryableTestObject("a name", 3, "b typethree");

            var nameFilter = new SearchFilter<NameTypeFilter>();

            nameFilter.SortBy = "Type";
            nameFilter.Ascending = false;
            nameFilter.SecondarySortBy = "Name";
            nameFilter.SecondaryAscending = false;

            var queryObject = new List<QueryableTestObject>() { object1, object2, object3, object4 }.AsQueryable();

            // Act
            var result = queryObject.PerformSearch(nameFilter).GetAwaiter().GetResult();

            // Assert
            result.FilterResults[0].Name.Should().Be("c name");
            result.FilterResults[0].Type.Should().Be("c typeone");

            result.FilterResults[1].Name.Should().Be("a name");
            result.FilterResults[1].Type.Should().Be("b typethree");

            result.FilterResults[2].Name.Should().Be("f name");
            result.FilterResults[2].Type.Should().Be("a typetwo");

            result.FilterResults[3].Name.Should().Be("a name");
            result.FilterResults[3].Type.Should().Be("a typetwo");

            result.FilterResults.Should().BeInDescendingOrder(x => x.Type);
        }

        /// <summary>Ensure first sort field defaults when sort field does not exist on the filter.</summary>
        [Fact]
        public void Test_PerformSearch_DefaultsToFirstPropertyIfSortFieldDoesntExistOnFilter()
        {
            // Arrange
            var object1 = new QueryableTestObject("c name", 1, "c typeone");
            var object2 = new QueryableTestObject("a name", 2, "a typetwo");
            var object3 = new QueryableTestObject("b name", 3, "b typethree");

            var nameFilter = new SearchFilter<NameTypeFilter>();

            nameFilter.SortBy = "Number";

            var queryObject = new List<QueryableTestObject>() { object1, object2, object3 }.AsQueryable();

            // Act
            var result = queryObject.PerformSearch(nameFilter).GetAwaiter().GetResult();

            // Assert
            result.FilterResults.Should().BeInAscendingOrder(x => x.Name);
        }

        /// <summary>Ensure max page size is configurable.</summary>
        [Fact]
        public void Test_PerformSearch_CanEditMaxPageSize()
        {
            // Arrange
            var listObject = new List<QueryableTestObject>();

            for (int i = 0; i <= 160; i++)
            {
                listObject.Add(new QueryableTestObject($"object {i}", i, "TestType"));
            }

            var queryObject = listObject.AsQueryable();

            var nameFilter = new SearchFilter<NameFilter>();
            nameFilter.PageSize = 150;

            // Act
            var result = queryObject.PerformSearch(nameFilter, 150).GetAwaiter().GetResult();

            // Assert
            result.FilterResults.Should().HaveCount(150);
        }

        /// <summary>Ensure query sort order is ascending as expected.</summary>
        [Fact]
        public void Test_Queryable_SortByFieldReturnsAscendingQuery()
        {
            // Arrange
            var object1 = new QueryableTestObject("c name", 1, "c typeone");
            var object2 = new QueryableTestObject("a name", 2, "a typetwo");
            var object3 = new QueryableTestObject("b name", 3, "b typethree");

            var queryObject = new List<QueryableTestObject>() { object1, object2, object3 }.AsQueryable();

            // Act
            var result = queryObject.OrderByField("Name", true);

            var queriedResult = result.ToList();

            // Assert
            result.Should().BeInAscendingOrder(x => x.Name);
        }

        /// <summary>Ensure query sort order is descending as expected.</summary>
        [Fact]
        public void Test_Queryable_SortByFieldReturnsDescendingQuery()
        {
            // Arrange
            var object1 = new QueryableTestObject("c name", 1, "c typeone");
            var object2 = new QueryableTestObject("a name", 2, "a typetwo");
            var object3 = new QueryableTestObject("b name", 3, "b typethree");

            var queryObject = new List<QueryableTestObject>() { object1, object2, object3 }.AsQueryable();

            // Act
            var result = queryObject.OrderByField("Name", false);

            var queriedResult = result.ToList();

            // Assert
            result.Should().BeInDescendingOrder(x => x.Name);
        }

        /// <summary>Ensure query sort order using ThenBy extension is in ascending order.</summary>
        [Fact]
        public void Test_Queryable_ThenByFieldReturnsAscendingQuery()
        {
            // Arrange
            var object1 = new QueryableTestObject("c name", 1, "c typeone");
            var object2 = new QueryableTestObject("f name", 2, "a typetwo");
            var object3 = new QueryableTestObject("a name", 2, "a typetwo");
            var object4 = new QueryableTestObject("a name", 3, "b typethree");

            var queryObject = new List<QueryableTestObject>() { object1, object2, object3, object4 }.AsQueryable();

            // Act
            queryObject = queryObject.OrderByField("Type", true).ThenByField("Name", true);

            var queriedResult = queryObject.ToList();

            // Assert
            queriedResult[0].Name.Should().Be("a name");
            queriedResult[0].Type.Should().Be("a typetwo");

            queriedResult[1].Name.Should().Be("f name");
            queriedResult[1].Type.Should().Be("a typetwo");

            queriedResult[2].Name.Should().Be("a name");
            queriedResult[2].Type.Should().Be("b typethree");

            queriedResult[3].Name.Should().Be("c name");
            queriedResult[3].Type.Should().Be("c typeone");

            queriedResult.Should().BeInAscendingOrder(x => x.Type);
        }

        /// <summary>Ensure query sort order using ThenBy extension is in descending order.</summary>
        [Fact]
        public void Test_Queryable_ThenByFieldReturnsDescendingQuery()
        {
            // Arrange
            var object1 = new QueryableTestObject("c name", 1, "c typeone");
            var object2 = new QueryableTestObject("f name", 2, "a typetwo");
            var object3 = new QueryableTestObject("a name", 2, "a typetwo");
            var object4 = new QueryableTestObject("a name", 3, "b typethree");

            var queryObject = new List<QueryableTestObject>() { object1, object2, object3, object4 }.AsQueryable();

            // Act
            queryObject = queryObject.OrderByField("Type", false).ThenByField("Name", false);

            var queriedResult = queryObject.ToList();

            // Assert
            queriedResult[0].Name.Should().Be("c name");
            queriedResult[0].Type.Should().Be("c typeone");

            queriedResult[1].Name.Should().Be("a name");
            queriedResult[1].Type.Should().Be("b typethree");

            queriedResult[2].Name.Should().Be("f name");
            queriedResult[2].Type.Should().Be("a typetwo");

            queriedResult[3].Name.Should().Be("a name");
            queriedResult[3].Type.Should().Be("a typetwo");

            queriedResult.Should().BeInDescendingOrder(x => x.Type);
        }

        /// <summary>Ensure the order by field returns correct sort order.</summary>
        [Fact]
        public void Test_Queryable_SortByFieldInternalField()
        {
            // Arrange
            var object1 = new QueryableTestObjectWithInternalFields("c name", 1, "c typeone");
            var object2 = new QueryableTestObjectWithInternalFields("a name", 2, "a typetwo");
            var object3 = new QueryableTestObjectWithInternalFields("b name", 3, "b typethree");

            var queryObject = new List<QueryableTestObjectWithInternalFields>() { object1, object2, object3 }.AsQueryable();

            // Act
            var result = queryObject.OrderByField("Number", true);

            var queriedResult = result.ToList();

            // Assert
            result.Should().BeInAscendingOrder(x => x.Number);
        }

        /// <summary>Ensure order with direction returns ascending results.</summary>
        [Fact]
        public void Test_Queryable_OrderByWithDirectionReturnsAscendingQuery()
        {
            // Arrange
            var object1 = new QueryableTestObject("c name", 1, "c typeone");
            var object2 = new QueryableTestObject("a name", 2, "a typetwo");
            var object3 = new QueryableTestObject("b name", 3, "b typethree");

            var queryObject = new List<QueryableTestObject>() { object1, object2, object3 }.AsQueryable();

            // Act
            var result = queryObject.OrderByWithDirection(x => x.Name, true);

            var queriedResult = result.ToList();

            // Assert
            result.Should().BeInAscendingOrder(x => x.Name);
        }

        /// <summary>Ensure order with direction returns descending results.</summary>
        [Fact]
        public void Test_Queryable_OrderByWithDirectionReturnsDescendingQuery()
        {
            // Arrange
            var object1 = new QueryableTestObject("c name", 1, "c typeone");
            var object2 = new QueryableTestObject("a name", 2, "a typetwo");
            var object3 = new QueryableTestObject("b name", 3, "b typethree");

            var queryObject = new List<QueryableTestObject>() { object1, object2, object3 }.AsQueryable();

            // Act
            var result = queryObject.OrderByWithDirection(x => x.Name, false);

            var queriedResult = result.ToList();

            // Assert
            result.Should().BeInDescendingOrder(x => x.Name);
        }

        /// <summary>
        /// Ensure has next page is returns as expected.
        /// </summary>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="hasNextPage">if set to <c>true</c> [has next page].</param>
        [Theory]
        [InlineData(10, true)]
        [InlineData(0, false)]
        [InlineData(99, false)]
        [InlineData(98, true)]
        public void Test_PerformSearch_CalculatesHasNextPageCorrectly(int pageSize, bool hasNextPage)
        {
            // Arrange
            var listObject = new List<QueryableTestObject>();

            for (int i = 0; i <= 98; i++)
            {
                listObject.Add(new QueryableTestObject($"object {i}", i, "TestType"));
            }

            var queryObject = listObject.AsQueryable();

            var nameFilter = new SearchFilter<NameFilter>();
            nameFilter.PageSize = pageSize;

            // Act
            var result = queryObject.PerformSearch(nameFilter).GetAwaiter().GetResult();

            // Assert
            result.HasNextPage.Should().Be(hasNextPage);
        }

        //// <summary>Ensure page count calculation is correct.</summary>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="totalPages">The total pages.</param>
        [Theory]
        [InlineData(10, 10)]
        [InlineData(0, 1)]
        [InlineData(99, 1)]
        [InlineData(98, 2)]
        public void Test_PerformSearch_CalculatesTotalPagesCorrectly(int pageSize, int totalPages)
        {
            // Arrange
            var listObject = new List<QueryableTestObject>();

            for (int i = 0; i <= 98; i++)
            {
                listObject.Add(new QueryableTestObject($"object {i}", i, "TestType"));
            }

            var queryObject = listObject.AsQueryable();

            var nameFilter = new SearchFilter<NameFilter>();
            nameFilter.PageSize = pageSize;

            // Act
            var result = queryObject.PerformSearch(nameFilter).GetAwaiter().GetResult();

            // Assert
            result.TotalPages.Should().Be(totalPages);
        }

        //// <summary>Ensure a page of results is returned with correct results.</summary>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="matchOne">The match one.</param>
        /// <param name="matchTwo">The match two.</param>
        /// <param name="matchThree">The match three.</param>
        /// <param name="matchFour">The match four.</param>
        /// <param name="matchFive">The match five.</param>
        [Theory]
        [InlineData(0, 0, 1, 10, 100, 101)]
        [InlineData(1, 0, 1, 10, 100, 101)]
        [InlineData(2, 102, 103, 104, 105, 11)]
        public void Test_PerformSearch_EnsureCorrectItemsReturnedOnPage(int pageNumber, int matchOne, int matchTwo, int matchThree, int matchFour, int matchFive)
        {
            // Arrange
            var listObject = new List<QueryableTestObject>();

            for (int i = 0; i <= 105; i++)
            {
                listObject.Add(new QueryableTestObject($"object {i}", i, "TestType"));
            }

            var queryObject = listObject.AsQueryable();

            var nameFilter = new SearchFilter<NameFilter>();
            nameFilter.PageSize = 5;
            nameFilter.PageNumber = pageNumber;

            // Act
            var result = queryObject.PerformSearch(nameFilter).GetAwaiter().GetResult();

            // Assert
            result.FilterResults.Should().HaveCount(5);
            result.FilterResults[0].Name.Should().Match($"object {matchOne}");
            result.FilterResults[1].Name.Should().Match($"object {matchTwo}");
            result.FilterResults[2].Name.Should().Match($"object {matchThree}");
            result.FilterResults[3].Name.Should().Match($"object {matchFour}");
            result.FilterResults[4].Name.Should().Match($"object {matchFive}");
        }

        /// <summary>Ensure count is zero when a page beyond the max limit is requested.</summary>
        [Fact]
        public void Test_PerformSearch_EnsureZeroCountWhenPageNumberGreaterThanNumberOfPagesIsGiven()
        {
            // Arrange
            var listObject = new List<QueryableTestObject>();

            for (int i = 0; i <= 105; i++)
            {
                listObject.Add(new QueryableTestObject($"object {i}", i, "TestType"));
            }

            var queryObject = listObject.AsQueryable();

            var nameFilter = new SearchFilter<NameFilter>();
            nameFilter.PageSize = 10;
            nameFilter.PageNumber = 25;

            // Act
            var result = queryObject.PerformSearch(nameFilter).GetAwaiter().GetResult();

            // Assert
            result.FilterResults.Should().HaveCount(0);
        }

    }

    #region Query Test Objects

    public class QueryableTestObject
    {

        public QueryableTestObject(string name, int number, string type)
        {
            Name = name;
            Number = number;
            Type = type;
        }

        public string Name { get; set; }

        public int Number { get; set; }

        public string Type { get; set; }
    }

    public class QueryableTestObjectWithInternalFields
    {

        public QueryableTestObjectWithInternalFields(string name, int number, string type)
        {
            Name = name;
            Number = number;
            Type = type;
        }

        internal string Name { get; set; }

        internal int Number { get; set; }

        public string Type { get; set; }
    }

    public class NameFilter
    {

        //Search Filter needs to handle a non parameterless constructor?
        public NameFilter()
        {

        }

        public NameFilter(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }

    public class NameTypeFilter
    {

        public NameTypeFilter()
        {

        }

        public string Name { get; set; }

        public string Type { get; set; }
    }

    public class InternalNameTypeFilter
    {

        public InternalNameTypeFilter()
        {

        }

        internal string Name { get; set; }

        public string Type { get; set; }
    }

    #endregion
}
