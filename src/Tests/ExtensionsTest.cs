using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cloud.Core.Testing;
using Cloud.Core.Web.Filters;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Xunit;

namespace Cloud.Core.Web.Tests
{
    [IsUnit]
    public class ExtensionsTest
    {
        [Fact]
        public void Test_AssemblyName()
        {
            Assembly test = null;
            var result = test.GetAssemblyName();
            result.Should().Be(null);

            test = Assembly.GetExecutingAssembly();
            result = test.GetAssemblyName();
            result.Should().NotBe(null);
        }

        [Fact]
        public void Test_AddSwaggerWithVersioning()
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

        [Fact]
        public void Test_UseSwaggerWithVersion()
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

        [Fact]
        public void Test_UseLocalizationMiddleware()
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

        [Fact]
        public void Test_UseLocalizationMiddleware_NotUsingDefaultCulture()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            services.AddLocalization();
            IApplicationBuilder app = new ApplicationBuilder(services.BuildServiceProvider());

            // Act
            Assert.Throws<ArgumentException>(() => app.UseLocalization(new[] { "fr" }));
        }

        [Fact]
        public void Test_ActionDescription()
        {
            // Arrange
            ActionDescriptor description = null;
            description.GetApiVersion();

            var desc = new ActionDescriptor { Properties = new Dictionary<object, object>() };
            desc.GetApiVersion();
        }

        [Fact]
        public void Test_PerformSearchReturnsAscending()
        {
            var object1 = new QueryableTestObject("c name", 1, "typeone");
            var object2 = new QueryableTestObject("a name", 2, "typeone");
            var object3 = new QueryableTestObject("b name", 3, "typetwo");

            var nameFilter = new SearchFilter<NameFilter>();

            nameFilter.FilterData = new NameFilter("b name");

            var queryObject = new List<QueryableTestObject>() { object1, object2, object3 }.AsQueryable();

            var result = queryObject.PerformSearch(nameFilter).GetAwaiter().GetResult();

            result.FilterResults.Should().BeInAscendingOrder(x => x.Name);
        }

        [Fact]
        public void Test_PerformSearchReturnsDescending()
        {
            var object1 = new QueryableTestObject("c name", 1, "typeone");
            var object2 = new QueryableTestObject("a name", 2, "typeone");
            var object3 = new QueryableTestObject("b name", 3, "typetwo");

            var nameFilter = new SearchFilter<NameFilter>();

            nameFilter.FilterData = new NameFilter("b name");
            nameFilter.Ascending = false;

            var queryObject = new List<QueryableTestObject>() { object1, object2, object3 }.AsQueryable();

            var result = queryObject.PerformSearch(nameFilter).GetAwaiter().GetResult();

            result.FilterResults.Should().BeInDescendingOrder(x => x.Name);
        }

        [Fact]
        public void Test_PerformSearchSortsByInternalDefaultFieldWhenFieldNameNotSpecified()
        {
            var object1 = new QueryableTestObject("c name", 1, "c");
            var object2 = new QueryableTestObject("a name", 2, "b");
            var object3 = new QueryableTestObject("b name", 3, "a");

            var nameFilter = new SearchFilter<InternalNameTypeFilter>();

            nameFilter.Ascending = false;

            var queryObject = new List<QueryableTestObject>() { object1, object2, object3 }.AsQueryable();

            var result = queryObject.PerformSearch(nameFilter).GetAwaiter().GetResult();

            result.FilterResults.Should().BeInDescendingOrder(x => x.Name);
        }

        [Fact]
        public void Test_PerformSearchSortsByInternalDefaultFieldWhenFieldNameDoesNotExist()
        {
            var object1 = new QueryableTestObject("c name", 1, "c");
            var object2 = new QueryableTestObject("a name", 2, "b");
            var object3 = new QueryableTestObject("b name", 3, "a");

            var nameFilter = new SearchFilter<InternalNameTypeFilter>();
            nameFilter.SortBy = "Number";
            nameFilter.Ascending = false;

            var queryObject = new List<QueryableTestObject>() { object1, object2, object3 }.AsQueryable();

            var result = queryObject.PerformSearch(nameFilter).GetAwaiter().GetResult();

            result.FilterResults.Should().BeInDescendingOrder(x => x.Name);
        }

        [Fact]
        public void Test_PerformSearchCanPage50Files()
        {
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

            var result = queryObject.PerformSearch(nameFilter).GetAwaiter().GetResult();
             
            result.FilterResults.Should().HaveCount(50);
        }

        [Fact]
        public void Test_PerformSearchCanInterpretWhatPageItsOn()
        {
            var listObject = new List<QueryableTestObject>();

            for (int i = 0; i <= 105; i++)
            {
                listObject.Add(new QueryableTestObject($"object {i}", i, "TestType"));
            }

            var queryObject = listObject.AsQueryable();

            var nameFilter = new SearchFilter<NameFilter>();
            nameFilter.PageSize = 10;
            nameFilter.PageNumber = 2;

            var result = queryObject.PerformSearch(nameFilter).GetAwaiter().GetResult();

            result.FilterResults.Should().HaveCount(10);
            result.FilterResults[0].Name.Should().Match("object 12");
        }

        [Fact]
        public void Test_PerformSearchCanReturnAllFiles()
        {
            var listObject = new List<QueryableTestObject>();

            for (int i = 0; i <= 105; i++)
            {
                listObject.Add(new QueryableTestObject($"object {i}", i, "TestType"));
            }

            var queryObject = listObject.AsQueryable();

            var nameFilter = new SearchFilter<NameFilter>();
            nameFilter.PageSize = 0;

            var result = queryObject.PerformSearch(nameFilter).GetAwaiter().GetResult();

            result.FilterResults.Should().HaveCount(106);
        }

        [Fact]
        public void Test_PerformSearchCanSortByAField()
        {
            var object1 = new QueryableTestObject("c name", 1, "c typeone");
            var object2 = new QueryableTestObject("a name", 2, "a typetwo");
            var object3 = new QueryableTestObject("b name", 3, "b typethree");

            var nameFilter = new SearchFilter<NameTypeFilter>();

            nameFilter.SortBy = "Type";

            var queryObject = new List<QueryableTestObject>() { object1, object2, object3 }.AsQueryable();

            var result = queryObject.PerformSearch(nameFilter).GetAwaiter().GetResult();

            result.FilterResults.Should().BeInAscendingOrder(x => x.Type);
        }

        [Fact]
        public void Test_PerformSearchCanSortByASecondaryFieldDefaultsToAscending()
        {
            var object1 = new QueryableTestObject("c name", 1, "c typeone");
            var object2 = new QueryableTestObject("f name", 2, "a typetwo");
            var object3 = new QueryableTestObject("a name", 2, "a typetwo");
            var object4 = new QueryableTestObject("a name", 3, "b typethree");

            var nameFilter = new SearchFilter<NameTypeFilter>();

            nameFilter.SortBy = "Type";
            nameFilter.SecondarySortBy = "Name";

            var queryObject = new List<QueryableTestObject>() { object1, object2, object3, object4 }.AsQueryable();

            var result = queryObject.PerformSearch(nameFilter).GetAwaiter().GetResult();

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

        [Fact]
        public void Test_PerformSearchCanSortByASecondaryFieldSetToDescending()
        {
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

            var result = queryObject.PerformSearch(nameFilter).GetAwaiter().GetResult();

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

        [Fact]
        public void Test_PerformSearchDefaultsToFirstPropertyIfSortFieldDoesntExistOnFilter()
        {
            var object1 = new QueryableTestObject("c name", 1, "c typeone");
            var object2 = new QueryableTestObject("a name", 2, "a typetwo");
            var object3 = new QueryableTestObject("b name", 3, "b typethree");

            var nameFilter = new SearchFilter<NameTypeFilter>();

            nameFilter.SortBy = "Number";

            var queryObject = new List<QueryableTestObject>() { object1, object2, object3 }.AsQueryable();

            var result = queryObject.PerformSearch(nameFilter).GetAwaiter().GetResult();

            result.FilterResults.Should().BeInAscendingOrder(x => x.Name);
        }

        [Fact]
        public void Test_PerformSearchCanEditMaxPageSize()
        {
            var listObject = new List<QueryableTestObject>();

            for (int i = 0; i <= 160; i++)
            {
                listObject.Add(new QueryableTestObject($"object {i}", i, "TestType"));
            }

            var queryObject = listObject.AsQueryable();

            var nameFilter = new SearchFilter<NameFilter>();
            nameFilter.PageSize = 150;

            var result = queryObject.PerformSearch(nameFilter, 150).GetAwaiter().GetResult();

            result.FilterResults.Should().HaveCount(150);
        }

        [Fact]
        public void Test_SortByFieldReturnsAscendingQuery()
        {
            var object1 = new QueryableTestObject("c name", 1, "c typeone");
            var object2 = new QueryableTestObject("a name", 2, "a typetwo");
            var object3 = new QueryableTestObject("b name", 3, "b typethree");

            var queryObject = new List<QueryableTestObject>() { object1, object2, object3 }.AsQueryable();

            var result = queryObject.OrderByField("Name", true);

            var queriedResult = result.ToList();

            result.Should().BeInAscendingOrder(x => x.Name);
        }

        [Fact]
        public void Test_SortByFieldReturnsDescendingQuery()
        {
            var object1 = new QueryableTestObject("c name", 1, "c typeone");
            var object2 = new QueryableTestObject("a name", 2, "a typetwo");
            var object3 = new QueryableTestObject("b name", 3, "b typethree");

            var queryObject = new List<QueryableTestObject>() { object1, object2, object3 }.AsQueryable();

            var result = queryObject.OrderByField("Name", false);

            var queriedResult = result.ToList();

            result.Should().BeInDescendingOrder(x => x.Name);
        }

        [Fact]
        public void Test_ThenByFieldReturnsAscendingQuery()
        {
            var object1 = new QueryableTestObject("c name", 1, "c typeone");
            var object2 = new QueryableTestObject("f name", 2, "a typetwo");
            var object3 = new QueryableTestObject("a name", 2, "a typetwo");
            var object4 = new QueryableTestObject("a name", 3, "b typethree");

            var queryObject = new List<QueryableTestObject>() { object1, object2, object3, object4 }.AsQueryable();

            queryObject = queryObject.OrderByField("Type", true).ThenByField("Name", true);

            var queriedResult = queryObject.ToList();

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

        [Fact]
        public void Test_ThenByFieldReturnsDescendingQuery()
        {
            var object1 = new QueryableTestObject("c name", 1, "c typeone");
            var object2 = new QueryableTestObject("f name", 2, "a typetwo");
            var object3 = new QueryableTestObject("a name", 2, "a typetwo");
            var object4 = new QueryableTestObject("a name", 3, "b typethree");

            var queryObject = new List<QueryableTestObject>() { object1, object2, object3, object4 }.AsQueryable();

            queryObject = queryObject.OrderByField("Type", false).ThenByField("Name", false);

            var queriedResult = queryObject.ToList();

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

        [Fact]
        public void Test_SortByFieldInternalField()
        {
            var object1 = new QueryableTestObjectWithInternalFields("c name", 1, "c typeone");
            var object2 = new QueryableTestObjectWithInternalFields("a name", 2, "a typetwo");
            var object3 = new QueryableTestObjectWithInternalFields("b name", 3, "b typethree");

            var queryObject = new List<QueryableTestObjectWithInternalFields>() { object1, object2, object3 }.AsQueryable();

            var result = queryObject.OrderByField("Number", true);

            var queriedResult = result.ToList();

            result.Should().BeInAscendingOrder(x => x.Number);
        }

        [Fact]
        public void Test_OrderByWithDirectionReturnsAscendingQuery()
        {
            var object1 = new QueryableTestObject("c name", 1, "c typeone");
            var object2 = new QueryableTestObject("a name", 2, "a typetwo");
            var object3 = new QueryableTestObject("b name", 3, "b typethree");

            var queryObject = new List<QueryableTestObject>() { object1, object2, object3 }.AsQueryable();

            var result = queryObject.OrderByWithDirection(x => x.Name, true);

            var queriedResult = result.ToList();

            result.Should().BeInAscendingOrder(x => x.Name);
        }

        [Fact]
        public void Test_OrderByWithDirectionReturnsDescendingQuery()
        {
            var object1 = new QueryableTestObject("c name", 1, "c typeone");
            var object2 = new QueryableTestObject("a name", 2, "a typetwo");
            var object3 = new QueryableTestObject("b name", 3, "b typethree");

            var queryObject = new List<QueryableTestObject>() { object1, object2, object3 }.AsQueryable();

            var result = queryObject.OrderByWithDirection(x => x.Name, false);

            var queriedResult = result.ToList();

            result.Should().BeInDescendingOrder(x => x.Name);
        }

        [Theory]
        [InlineData(10, true)]
        [InlineData(0, false)]
        [InlineData(99, false)]
        [InlineData(98, true)]
        public void Test_CalculatesHasNextPageCorrectly(int pageSize, bool hasNextPage)
        {
            var listObject = new List<QueryableTestObject>();

            for (int i = 0; i <= 98; i++)
            {
                listObject.Add(new QueryableTestObject($"object {i}", i, "TestType"));
            }

            var queryObject = listObject.AsQueryable();

            var nameFilter = new SearchFilter<NameFilter>();
            nameFilter.PageSize = pageSize;

            var result = queryObject.PerformSearch(nameFilter).GetAwaiter().GetResult();

            result.HasNextPage.Should().Be(hasNextPage);
        }

        [Theory]
        [InlineData(10, 10)]
        [InlineData(0, 1)]
        [InlineData(99, 1)]
        [InlineData(98, 2)]
        public void Test_CalculatesTotalPagesCorrectly(int pageSize, int totalPages)
        {
            var listObject = new List<QueryableTestObject>();

            for (int i = 0; i <= 98; i++)
            {
                listObject.Add(new QueryableTestObject($"object {i}", i, "TestType"));
            }

            var queryObject = listObject.AsQueryable();

            var nameFilter = new SearchFilter<NameFilter>();
            nameFilter.PageSize = pageSize;

            var result = queryObject.PerformSearch(nameFilter).GetAwaiter().GetResult();

            result.TotalPages.Should().Be(totalPages);
        }

        [Theory]
        [InlineData(0, 0, 1, 10, 100, 101)]
        [InlineData(1, 0, 1, 10, 100, 101)]
        [InlineData(2, 102, 103, 104, 105, 11)]
        public void Test_EnsureCorrectFilesAreReturnedOnAPage(int pageNumber, int matchOne, int matchTwo, int matchThree, int matchFour, int matchFive)
        {
            var listObject = new List<QueryableTestObject>();

            for (int i = 0; i <= 105; i++)
            {
                listObject.Add(new QueryableTestObject($"object {i}", i, "TestType"));
            }

            var queryObject = listObject.AsQueryable();

            var nameFilter = new SearchFilter<NameFilter>();
            nameFilter.PageSize = 5;
            nameFilter.PageNumber = pageNumber;

            var result = queryObject.PerformSearch(nameFilter).GetAwaiter().GetResult();

            result.FilterResults.Should().HaveCount(5);
            result.FilterResults[0].Name.Should().Match($"object {matchOne}");
            result.FilterResults[1].Name.Should().Match($"object {matchTwo}");
            result.FilterResults[2].Name.Should().Match($"object {matchThree}");
            result.FilterResults[3].Name.Should().Match($"object {matchFour}");
            result.FilterResults[4].Name.Should().Match($"object {matchFive}");
        }

        [Fact]
        public void Test_EnsureNoFilesReturnedWhenPageNumberGreaterThanNumberOfPagesIsGiven()
        {
            var listObject = new List<QueryableTestObject>();

            for (int i = 0; i <= 105; i++)
            {
                listObject.Add(new QueryableTestObject($"object {i}", i, "TestType"));
            }

            var queryObject = listObject.AsQueryable();

            var nameFilter = new SearchFilter<NameFilter>();
            nameFilter.PageSize = 10;
            nameFilter.PageNumber = 25;

            var result = queryObject.PerformSearch(nameFilter).GetAwaiter().GetResult();

            result.FilterResults.Should().HaveCount(0);
        }

    }

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
}
