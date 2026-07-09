using System.Collections.Generic;
using System.Linq;
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
    public class AggregationConverterSortingTests
    {
        private const string PropertyId = "color-property-id";
        private const string PropertyFieldName = "Color";

        private static readonly IList<AggregationResponseValue> _termResponseValues =
        [
            new AggregationResponseValue { Id = "Black", Count = 5 },
            new AggregationResponseValue { Id = "Azure", Count = 30 },
            new AggregationResponseValue { Id = "Crimson", Count = 15 },
        ];

        private static readonly Property ColorProperty = new() { Id = PropertyId, Name = PropertyFieldName, DisplayNames = [] };

        private static readonly List<PropertyDictionaryItem> DictinaryItems =
        [
            new PropertyDictionaryItem { PropertyId = PropertyId, Alias = "Azure", SortOrder = 100, LocalizedValues = [] },
            new PropertyDictionaryItem { PropertyId = PropertyId, Alias = "Black", SortOrder = 50, LocalizedValues = [] },
            new PropertyDictionaryItem { PropertyId = PropertyId, Alias = "Crimson", SortOrder = 10, LocalizedValues = [] },
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
            Assert.Equal("Azure", items[0].Value);
            Assert.Equal("Black", items[1].Value);
            Assert.Equal("Crimson", items[2].Value);
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
            Assert.Equal("Crimson", items[0].Value);
            Assert.Equal("Black", items[1].Value);
            Assert.Equal("Azure", items[2].Value);
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
                    Id = PropertyFieldName,
                    Values =
                    [
                        new AggregationResponseValue { Id = "Azure", Count = 30 },
                        new AggregationResponseValue { Id = "Crimson", Count = 15 },
                        new AggregationResponseValue { Id = "Black", Count = 5 },
                    ],
                },
            };

            // Act
            var aggregations = await converter.ConvertAggregationsAsync(responses, criteria);

            // Assert
            Assert.Single(aggregations);
            var items = aggregations[0].Items;
            Assert.Equal(3, items.Length);
            Assert.Equal("Azure", items[0].Value);
            Assert.Equal("Crimson", items[1].Value);
            Assert.Equal("Black", items[2].Value);
        }

        [Fact]
        public async Task SortAggregationItems_Priority_ItemsSortedBySortOrderDescending()
        {
            // Arrange
            var converter = GetAggregationConverter(ModuleConstants.TermValuesSortingTypePriority);
            var criteria = new ProductIndexedSearchCriteria();
            var responses = BuildAggregationResponses();

            // Act
            var aggregations = await converter.ConvertAggregationsAsync(responses, criteria);

            // Assert
            Assert.Single(aggregations);
            var items = aggregations[0].Items;
            Assert.Equal(3, items.Length);
            Assert.Equal("Azure", items[0].Value);
            Assert.Equal("Black", items[1].Value);
            Assert.Equal("Crimson", items[2].Value);
        }

        [Fact]
        public async Task SortAggregationItems_NumericAscending_ItemsSortedNumerically()
        {
            // Arrange: string-alphabetical order would be 100,109,110,12,56 — numeric must give 12,56,100,109,110
            var converter = GetAggregationConverterWithValues(
                ModuleConstants.TermValuesSortingTypeNumericAscending,
                ["100", "109", "110", "12", "56"]);
            var criteria = new ProductIndexedSearchCriteria();
            var responses = BuildAggregationResponsesFromIds(["100", "109", "110", "12", "56"]);

            // Act
            var aggregations = await converter.ConvertAggregationsAsync(responses, criteria);

            // Assert
            var items = aggregations[0].Items;
            Assert.Equal(5, items.Length);
            Assert.Equal("12", items[0].Value);
            Assert.Equal("56", items[1].Value);
            Assert.Equal("100", items[2].Value);
            Assert.Equal("109", items[3].Value);
            Assert.Equal("110", items[4].Value);
        }

        [Fact]
        public async Task SortAggregationItems_NumericDescending_ItemsSortedNumerically()
        {
            // Arrange
            var converter = GetAggregationConverterWithValues(
                ModuleConstants.TermValuesSortingTypeNumericDescending,
                ["100", "109", "110", "12", "56"]);
            var criteria = new ProductIndexedSearchCriteria();
            var responses = BuildAggregationResponsesFromIds(["100", "109", "110", "12", "56"]);

            // Act
            var aggregations = await converter.ConvertAggregationsAsync(responses, criteria);

            // Assert
            var items = aggregations[0].Items;
            Assert.Equal(5, items.Length);
            Assert.Equal("110", items[0].Value);
            Assert.Equal("109", items[1].Value);
            Assert.Equal("100", items[2].Value);
            Assert.Equal("56", items[3].Value);
            Assert.Equal("12", items[4].Value);
        }

        [Fact]
        public async Task SortAggregationItems_NumericAscending_DecimalValues()
        {
            // Arrange
            var converter = GetAggregationConverterWithValues(
                ModuleConstants.TermValuesSortingTypeNumericAscending,
                ["10.2", "1.5", "12"]);
            var criteria = new ProductIndexedSearchCriteria();
            var responses = BuildAggregationResponsesFromIds(["10.2", "1.5", "12"]);

            // Act
            var aggregations = await converter.ConvertAggregationsAsync(responses, criteria);

            // Assert
            var items = aggregations[0].Items;
            Assert.Equal(3, items.Length);
            Assert.Equal("1.5", items[0].Value);
            Assert.Equal("10.2", items[1].Value);
            Assert.Equal("12", items[2].Value);
        }

        [Fact]
        public async Task SortAggregationItems_NumericAscending_NonNumericValuesGoToEnd()
        {
            // Arrange
            var converter = GetAggregationConverterWithValues(
                ModuleConstants.TermValuesSortingTypeNumericAscending,
                ["abc", "12", "5", "xyz"]);
            var criteria = new ProductIndexedSearchCriteria();
            var responses = BuildAggregationResponsesFromIds(["abc", "12", "5", "xyz"]);

            // Act
            var aggregations = await converter.ConvertAggregationsAsync(responses, criteria);

            // Assert
            var items = aggregations[0].Items;
            Assert.Equal(4, items.Length);
            Assert.Equal("5", items[0].Value);
            Assert.Equal("12", items[1].Value);
            // non-numeric values come last (alphabetical among themselves)
            Assert.Equal("abc", items[2].Value);
            Assert.Equal("xyz", items[3].Value);
        }

        [Fact]
        public async Task SortAggregationItems_NumericDescending_NonNumericValuesGoToEnd()
        {
            // Arrange: non-numeric values must go to the end even in descending mode
            var converter = GetAggregationConverterWithValues(
                ModuleConstants.TermValuesSortingTypeNumericDescending,
                ["abc", "12", "5", "xyz"]);
            var criteria = new ProductIndexedSearchCriteria();
            var responses = BuildAggregationResponsesFromIds(["abc", "12", "5", "xyz"]);

            // Act
            var aggregations = await converter.ConvertAggregationsAsync(responses, criteria);

            // Assert
            var items = aggregations[0].Items;
            Assert.Equal(4, items.Length);
            Assert.Equal("12", items[0].Value);
            Assert.Equal("5", items[1].Value);
            // non-numeric values come last (alphabetical among themselves)
            Assert.Equal("abc", items[2].Value);
            Assert.Equal("xyz", items[3].Value);
        }

        private static IList<AggregationResponse> BuildAggregationResponsesFromIds(IList<string> ids)
        {
            return
            [
                new AggregationResponse
                {
                    Id = PropertyFieldName,
                    Values = [.. ids.Select(id => new AggregationResponseValue { Id = id, Count = 1 })],
                },
            ];
        }

        private static AggregationConverter GetAggregationConverterWithValues(string termValuesSortingType, IList<string> valueIds)
        {
            var browseFilters = new List<IBrowseFilter>
            {
                new AttributeFilter
                {
                    Key = PropertyFieldName,
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
                .ReturnsAsync(new List<Property> { ColorProperty });

            var propDictSearchServiceMock = new Mock<IPropertyDictionaryItemSearchService>();
            propDictSearchServiceMock
                .Setup(x => x.SearchAsync(It.IsAny<PropertyDictionaryItemSearchCriteria>(), It.IsAny<bool>()))
                .ReturnsAsync(new PropertyDictionaryItemSearchResult { Results = [], TotalCount = 0 });

            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new List<Category>());

            return new AggregationConverter(browseFilterServiceMock.Object, propertyServiceMock.Object, propDictSearchServiceMock.Object, categoryServiceMock.Object, null, null);
        }

        private static IList<AggregationResponse> BuildAggregationResponses()
        {
            return
            [
                new AggregationResponse
                {
                    Id = PropertyFieldName,
                    Values = [.. _termResponseValues],
                },
            ];
        }

        private static AggregationConverter GetAggregationConverter(string termValuesSortingType)
        {
            var browseFilters = new List<IBrowseFilter>
            {
                new AttributeFilter
                {
                    Key = PropertyFieldName,
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
                .ReturnsAsync(new List<Property> { ColorProperty });

            var propDictSearchServiceMock = new Mock<IPropertyDictionaryItemSearchService>();
            propDictSearchServiceMock
                .Setup(x => x.SearchAsync(It.IsAny<PropertyDictionaryItemSearchCriteria>(), It.IsAny<bool>()))
                .ReturnsAsync(new PropertyDictionaryItemSearchResult { Results = DictinaryItems, TotalCount = DictinaryItems.Count });

            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new List<Category>());

            return new AggregationConverter(browseFilterServiceMock.Object, propertyServiceMock.Object, propDictSearchServiceMock.Object, categoryServiceMock.Object, null, null);
        }
    }
}
