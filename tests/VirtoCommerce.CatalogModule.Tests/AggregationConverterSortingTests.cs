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
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class AggregationConverterSortingTests
    {
        private const string PropertyId = "color-property-id";
        private const string LinkedPropertyId = "linked-color-property-id";
        private const string PropertyFieldName = "Color";
        private const string StoreId = "Store-Test";

        private static readonly IList<AggregationResponseValue> _termResponseValues =
        [
            new AggregationResponseValue { Id = "Black", Count = 5 },
            new AggregationResponseValue { Id = "Azure", Count = 30 },
            new AggregationResponseValue { Id = "Crimson", Count = 15 },
        ];

        private static readonly Property ColorProperty = new() { Id = PropertyId, Name = PropertyFieldName, DisplayNames = [] };

        /// <summary>
        /// A second property with the same name, as a virtual catalog reports for every linked catalog.
        /// </summary>
        private static readonly Property LinkedColorProperty = new() { Id = LinkedPropertyId, Name = PropertyFieldName, DisplayNames = [] };

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
        public async Task SortAggregationItems_PriorityAscending_ItemsSortedBySortOrderAscending()
        {
            // Arrange: "The lowest value will be used first" — Crimson=10, Black=50, Azure=100
            var converter = GetAggregationConverter(ModuleConstants.TermValuesSortingTypePriorityAscending);
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
        public async Task SortAggregationItems_PriorityDescending_ItemsSortedBySortOrderDescending()
        {
            // Arrange
            var converter = GetAggregationConverter(ModuleConstants.TermValuesSortingTypePriorityDescending);
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
        public async Task SortAggregationItems_LegacyPriority_ItemsSortedBySortOrderAscending()
        {
            // Arrange: stores configured before the ascending/descending split persisted a bare "Priority".
            // It has to follow the documented contract, which is the lowest value first.
            var converter = GetAggregationConverter(ModuleConstants.TermValuesSortingTypePriority);
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
        public async Task SortAggregationItems_PriorityAscending_ValuesWithoutDictionaryItemGoLast()
        {
            // Arrange: Crimson has no dictionary item, so it has no priority at all
            var converter = GetAggregationConverter(
                ModuleConstants.TermValuesSortingTypePriorityAscending,
                dictionaryItems:
                [
                    new PropertyDictionaryItem { PropertyId = PropertyId, Alias = "Azure", SortOrder = 100, LocalizedValues = [] },
                    new PropertyDictionaryItem { PropertyId = PropertyId, Alias = "Black", SortOrder = 50, LocalizedValues = [] },
                ]);
            var criteria = new ProductIndexedSearchCriteria();
            var responses = BuildAggregationResponses();

            // Act
            var aggregations = await converter.ConvertAggregationsAsync(responses, criteria);

            // Assert
            var items = aggregations[0].Items;
            Assert.Equal(3, items.Length);
            Assert.Equal("Black", items[0].Value);
            Assert.Equal("Azure", items[1].Value);
            Assert.Equal("Crimson", items[2].Value);
        }

        [Fact]
        public async Task SortAggregationItems_PriorityDescending_ValuesWithoutDictionaryItemGoLast()
        {
            // Arrange: values without a priority must go last in this direction too, never first
            var converter = GetAggregationConverter(
                ModuleConstants.TermValuesSortingTypePriorityDescending,
                dictionaryItems:
                [
                    new PropertyDictionaryItem { PropertyId = PropertyId, Alias = "Azure", SortOrder = 100, LocalizedValues = [] },
                    new PropertyDictionaryItem { PropertyId = PropertyId, Alias = "Black", SortOrder = 50, LocalizedValues = [] },
                ]);
            var criteria = new ProductIndexedSearchCriteria();
            var responses = BuildAggregationResponses();

            // Act
            var aggregations = await converter.ConvertAggregationsAsync(responses, criteria);

            // Assert
            var items = aggregations[0].Items;
            Assert.Equal(3, items.Length);
            Assert.Equal("Azure", items[0].Value);
            Assert.Equal("Black", items[1].Value);
            Assert.Equal("Crimson", items[2].Value);
        }

        [Fact]
        public async Task SortAggregationItems_PriorityAscending_EqualSortOrdersFallBackToValue()
        {
            // Arrange
            var converter = GetAggregationConverter(
                ModuleConstants.TermValuesSortingTypePriorityAscending,
                dictionaryItems:
                [
                    new PropertyDictionaryItem { PropertyId = PropertyId, Alias = "Azure", SortOrder = 10, LocalizedValues = [] },
                    new PropertyDictionaryItem { PropertyId = PropertyId, Alias = "Black", SortOrder = 10, LocalizedValues = [] },
                    new PropertyDictionaryItem { PropertyId = PropertyId, Alias = "Crimson", SortOrder = 5, LocalizedValues = [] },
                ]);
            var criteria = new ProductIndexedSearchCriteria();
            var responses = BuildAggregationResponses();

            // Act
            var aggregations = await converter.ConvertAggregationsAsync(responses, criteria);

            // Assert
            var items = aggregations[0].Items;
            Assert.Equal(3, items.Length);
            Assert.Equal("Crimson", items[0].Value);
            Assert.Equal("Azure", items[1].Value);
            Assert.Equal("Black", items[2].Value);
        }

        [Fact]
        public async Task SortAggregationItems_PriorityAscending_SameNamedPropertiesWithUnsetPriorities()
        {
            // Arrange: both same-named properties define the same aliases, but only the linked one
            // has priorities filled in. The unset zeros must not swamp the priorities that were set.
            var converter = GetAggregationConverter(
                ModuleConstants.TermValuesSortingTypePriorityAscending,
                properties: [ColorProperty, LinkedColorProperty],
                dictionaryItems:
                [
                    new PropertyDictionaryItem { PropertyId = PropertyId, Alias = "Azure", SortOrder = 0, LocalizedValues = [] },
                    new PropertyDictionaryItem { PropertyId = PropertyId, Alias = "Black", SortOrder = 0, LocalizedValues = [] },
                    new PropertyDictionaryItem { PropertyId = PropertyId, Alias = "Crimson", SortOrder = 0, LocalizedValues = [] },
                    new PropertyDictionaryItem { PropertyId = LinkedPropertyId, Alias = "Azure", SortOrder = 100, LocalizedValues = [] },
                    new PropertyDictionaryItem { PropertyId = LinkedPropertyId, Alias = "Black", SortOrder = 50, LocalizedValues = [] },
                    new PropertyDictionaryItem { PropertyId = LinkedPropertyId, Alias = "Crimson", SortOrder = 10, LocalizedValues = [] },
                ]);
            var criteria = new ProductIndexedSearchCriteria();
            var responses = BuildAggregationResponses();

            // Act
            var aggregations = await converter.ConvertAggregationsAsync(responses, criteria);

            // Assert
            var items = aggregations[0].Items;
            Assert.Equal(3, items.Length);
            Assert.Equal("Crimson", items[0].Value);
            Assert.Equal("Black", items[1].Value);
            Assert.Equal("Azure", items[2].Value);
        }

        [Fact]
        public async Task SortAggregationItems_PriorityAscending_SameNamedPropertiesFromLinkedCatalogs()
        {
            // Arrange: a virtual catalog reports several properties named "Color", one per linked catalog.
            // The dictionary items live on the linked property, not on the first one that matches the name.
            var converter = GetAggregationConverter(
                ModuleConstants.TermValuesSortingTypePriorityAscending,
                properties: [ColorProperty, LinkedColorProperty],
                dictionaryItems:
                [
                    new PropertyDictionaryItem { PropertyId = LinkedPropertyId, Alias = "Azure", SortOrder = 100, LocalizedValues = [] },
                    new PropertyDictionaryItem { PropertyId = LinkedPropertyId, Alias = "Black", SortOrder = 50, LocalizedValues = [] },
                    new PropertyDictionaryItem { PropertyId = LinkedPropertyId, Alias = "Crimson", SortOrder = 10, LocalizedValues = [] },
                ]);
            var criteria = new ProductIndexedSearchCriteria();
            var responses = BuildAggregationResponses();

            // Act
            var aggregations = await converter.ConvertAggregationsAsync(responses, criteria);

            // Assert
            var items = aggregations[0].Items;
            Assert.Equal(3, items.Length);
            Assert.Equal("Crimson", items[0].Value);
            Assert.Equal("Black", items[1].Value);
            Assert.Equal("Azure", items[2].Value);
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

        [Theory]
        [InlineData(ModuleConstants.TermValuesSortingTypePriority, "Crimson", "Black", "Azure")]
        [InlineData(ModuleConstants.TermValuesSortingTypePriorityAscending, "Crimson", "Black", "Azure")]
        [InlineData(ModuleConstants.TermValuesSortingTypePriorityDescending, "Azure", "Black", "Crimson")]
        public async Task SortAggregationItems_SortingTypeReadFromPersistedStoreConfig(
            string persistedSortingType, string first, string second, string third)
        {
            // Arrange: take the sorting type through the real persistence path instead of a hand-built
            // filter, so the whole backend mapping is covered - store setting, deserialization, converter.
            var converter = GetAggregationConverterWithPersistedConfig(persistedSortingType);
            var criteria = new ProductIndexedSearchCriteria { StoreId = StoreId };
            var responses = BuildAggregationResponses();

            // Act
            var aggregations = await converter.ConvertAggregationsAsync(responses, criteria);

            // Assert
            Assert.Single(aggregations);
            var items = aggregations[0].Items;
            Assert.Equal(3, items.Length);
            Assert.Equal(first, items[0].Value);
            Assert.Equal(second, items[1].Value);
            Assert.Equal(third, items[2].Value);
        }

        private static AggregationConverter GetAggregationConverterWithPersistedConfig(string persistedSortingType)
        {
            // The legacy XML format, which is what a store configured before the split still holds.
            var store = new Store
            {
                Id = StoreId,
                Settings =
                [
                    new ObjectSettingEntry(ModuleConstants.Settings.Search.FilteredBrowsing)
                    {
                        Value = $"""
                                 <browsing>
                                   <attribute key="{PropertyFieldName}">
                                     <termValuesSortingType>{persistedSortingType}</termValuesSortingType>
                                   </attribute>
                                 </browsing>
                                 """,
                    },
                ],
                DynamicProperties = [],
            };

            var storeServiceMock = new Mock<IStoreService>();
            storeServiceMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync([store]);

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

            return new AggregationConverter(
                new BrowseFilterService(storeServiceMock.Object),
                propertyServiceMock.Object,
                propDictSearchServiceMock.Object,
                categoryServiceMock.Object,
                storeServiceMock.Object,
                null);
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

        private static AggregationConverter GetAggregationConverter(
            string termValuesSortingType,
            IList<Property> properties = null,
            IList<PropertyDictionaryItem> dictionaryItems = null)
        {
            properties ??= [ColorProperty];
            dictionaryItems ??= DictinaryItems;

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
                .ReturnsAsync(properties);

            var propDictSearchServiceMock = new Mock<IPropertyDictionaryItemSearchService>();
            propDictSearchServiceMock
                .Setup(x => x.SearchAsync(It.IsAny<PropertyDictionaryItemSearchCriteria>(), It.IsAny<bool>()))
                .ReturnsAsync(new PropertyDictionaryItemSearchResult { Results = dictionaryItems, TotalCount = dictionaryItems.Count });

            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new List<Category>());

            return new AggregationConverter(browseFilterServiceMock.Object, propertyServiceMock.Object, propDictSearchServiceMock.Object, categoryServiceMock.Object, null, null);
        }
    }
}
