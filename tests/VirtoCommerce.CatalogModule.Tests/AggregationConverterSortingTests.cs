using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Search.BrowseFilters;
using VirtoCommerce.CatalogModule.Data.Search.Indexing;
using VirtoCommerce.SearchModule.Core.Model;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    [Trait("Category", "CI")]
    public class AggregationConverterSortingTests
    {
        private const string FieldName = "Color";

        private static readonly IList<AggregationResponseValue> _termResponseValues =
        [
            new AggregationResponseValue { Id = "Banana", Count = 5 },
            new AggregationResponseValue { Id = "Apple", Count = 30 },
            new AggregationResponseValue { Id = "Cherry", Count = 15 },
        ];

        [Fact]
        public async Task SortAggregationItems_NameAscending_ItemsSortedByValueAscending()
        {
            // Arrange
            var converter = GetAggregationConverter(ModuleConstants.TermValuesSortingTypeNameAscending);
            var criteria = new ProductIndexedSearchCriteria();
            var responses = BuildAggregationResponses();

            // Act
            var aggregations = await converter.ConvertAggregationsAsync(responses, criteria);

            // Assert
            Assert.Single(aggregations);
            var items = aggregations[0].Items;
            Assert.Equal(3, items.Length);
            Assert.Equal("Apple", items[0].Value);
            Assert.Equal("Banana", items[1].Value);
            Assert.Equal("Cherry", items[2].Value);
        }

        [Fact]
        public async Task SortAggregationItems_NameDescending_ItemsSortedByValueDescending()
        {
            // Arrange
            var converter = GetAggregationConverter(ModuleConstants.TermValuesSortingTypeNameDescending);
            var criteria = new ProductIndexedSearchCriteria();
            var responses = BuildAggregationResponses();

            // Act
            var aggregations = await converter.ConvertAggregationsAsync(responses, criteria);

            // Assert
            Assert.Single(aggregations);
            var items = aggregations[0].Items;
            Assert.Equal(3, items.Length);
            Assert.Equal("Cherry", items[0].Value);
            Assert.Equal("Banana", items[1].Value);
            Assert.Equal("Apple", items[2].Value);
        }

        [Fact]
        public async Task SortAggregationItems_Score_ItemsRetainSearchEngineOrder()
        {
            // Arrange
            // Responses arrive pre-sorted by score (count desc): Apple=30, Cherry=15, Banana=5
            var converter = GetAggregationConverter(ModuleConstants.TermValuesSortingTypeScore);
            var criteria = new ProductIndexedSearchCriteria();
            var responses = new List<AggregationResponse>
            {
                new AggregationResponse
                {
                    Id = FieldName,
                    Values =
                    [
                        new AggregationResponseValue { Id = "Apple", Count = 30 },
                        new AggregationResponseValue { Id = "Cherry", Count = 15 },
                        new AggregationResponseValue { Id = "Banana", Count = 5 },
                    ],
                },
            };

            // Act
            var aggregations = await converter.ConvertAggregationsAsync(responses, criteria);

            // Assert
            Assert.Single(aggregations);
            var items = aggregations[0].Items;
            Assert.Equal(3, items.Length);
            Assert.Equal("Apple", items[0].Value);
            Assert.Equal("Cherry", items[1].Value);
            Assert.Equal("Banana", items[2].Value);
        }

        private static IList<AggregationResponse> BuildAggregationResponses()
        {
            return
            [
                new AggregationResponse
                {
                    Id = FieldName,
                    Values = [.. _termResponseValues],
                },
            ];
        }

        private static IAggregationConverter GetAggregationConverter(string termValuesSortingType)
        {
            var browseFilters = new List<IBrowseFilter>
            {
                new AttributeFilter
                {
                    Key = FieldName,
                    TermValuesSortingType = termValuesSortingType,
                },
            };

            var browseFilterServiceMock = new Mock<IBrowseFilterService>();
            browseFilterServiceMock
                .Setup(x => x.GetBrowseFiltersAsync(It.IsAny<ProductIndexedSearchCriteria>()))
                .ReturnsAsync(browseFilters);

            var propertyServiceMock = new Mock<IPropertyService>();
            propertyServiceMock
                .Setup(x => x.GetAllCatalogPropertiesAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<Property>());

            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new List<Category>());

            return new AggregationConverter(browseFilterServiceMock.Object, propertyServiceMock.Object, null, categoryServiceMock.Object, null, null);
        }
    }
}
