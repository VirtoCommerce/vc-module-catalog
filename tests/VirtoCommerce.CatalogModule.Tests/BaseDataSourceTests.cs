using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using VirtoCommerce.CatalogModule.BulkActions.DataSources;
using VirtoCommerce.CatalogModule.BulkActions.Models;
using VirtoCommerce.CatalogModule.Core.Model.ListEntry;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{


    public class BaseDataSourceTests
    {
        [Theory]
        [InlineData(1)]
        public async Task Fetch_Should_HaveCount(int count)
        {
            // arrange
            var entries = new List<ListEntryBase> { new ListEntryBase() };
            var dataQuery = Mock.Of<DataQuery>(t => t.ListEntries == entries.ToArray());
            var searchService = Mock.Of<IInternalListEntrySearchService>();
            var dataSource = new BaseDataSource(searchService, dataQuery);

            // act
            await dataSource.FetchAsync();

            // assert
            dataSource.Items.Should().HaveCount(count);
        }

        [Fact]
        public async Task Fetch_SearchService_InvokeSearch()
        {
            // arrange
            var listEntrySearchResult = Mock.Of<ListEntrySearchResult>();
            var dataQuery = Mock.Of<DataQuery>(t => t.SearchCriteria == Mock.Of<CatalogListEntrySearchCriteria>());
            var searchService = new Mock<IInternalListEntrySearchService>();
            searchService.Setup(t => t.InnerListEntrySearchAsync(It.IsAny<CatalogListEntrySearchCriteria>())).ReturnsAsync(listEntrySearchResult);
            var dataSource = new BaseDataSource(searchService.Object, dataQuery);

            // act
            await dataSource.FetchAsync();

            // assert
            searchService.Verify(t => t.InnerListEntrySearchAsync(It.IsAny<CatalogListEntrySearchCriteria>()));
        }

        [Fact]
        public async Task Fetch_Return_ShouldBeTrue()
        {
            // arrange
            var entries = new List<ListEntryBase> { new ListEntryBase() };
            var dataQuery = Mock.Of<DataQuery>(t => t.ListEntries == entries.ToArray());
            var searchService = Mock.Of<IInternalListEntrySearchService>();
            var dataSource = new BaseDataSource(searchService, dataQuery);

            // act
            var result = await dataSource.FetchAsync();

            // assert
            result.Should().Be(true);
        }

        [Fact]
        public async Task Fetch_Result_ShouldNotBeNull()
        {
            // arrange
            var dataQuery = Mock.Of<DataQuery>();
            var searchService = Mock.Of<IInternalListEntrySearchService>();
            var dataSource = new BaseDataSource(searchService, dataQuery);

            // act
            await dataSource.FetchAsync();

            // assert
            dataSource.Items.Should().NotBeNull();
        }

        [Theory]
        [InlineData(0)]
        public async Task GetTotalCount_IfSearchCriteriaIsNull_ShouldBeEqual(int count)
        {
            // arrange
            var dataQuery = Mock.Of<DataQuery>();
            var searchService = Mock.Of<IInternalListEntrySearchService>();
            var dataSource = new BaseDataSource(searchService, dataQuery);

            // act
            var result = await dataSource.GetTotalCountAsync();

            // assert
            result.Should().Be(count);
        }

        [Theory]
        [InlineData(1)]
        public async Task GetTotalCount_IfSearchCriteriaIsNotNull_ShouldBeEqual(int count)
        {
            // arrange
            var dataQuery = Mock.Of<DataQuery>(t => t.SearchCriteria == new CatalogListEntrySearchCriteria());
            var searchService = new Mock<IInternalListEntrySearchService>();
            var listEntrySearchResult = new ListEntrySearchResult { TotalCount = 1 };
            searchService.Setup(t => t.InnerListEntrySearchAsync(It.IsAny<CatalogListEntrySearchCriteria>())).ReturnsAsync(listEntrySearchResult);

            var dataSource = new BaseDataSource(searchService.Object, dataQuery);

            // act
            var result = await dataSource.GetTotalCountAsync();

            // assert
            result.Should().Be(count);
        }

        [Theory]
        [InlineData(1)]
        public async Task GetTotalCount_IfListEntriesNotNull_ShouldBeEqual(int count)
        {
            // arrange
            var dataQuery = Mock.Of<DataQuery>(t => t.ListEntries == new[] { new ListEntryBase() });
            var searchService = new Mock<IInternalListEntrySearchService>();

            var dataSource = new BaseDataSource(searchService.Object, dataQuery);

            // act
            var result = await dataSource.GetTotalCountAsync();

            // assert
            result.Should().Be(count);
        }
    }
}
