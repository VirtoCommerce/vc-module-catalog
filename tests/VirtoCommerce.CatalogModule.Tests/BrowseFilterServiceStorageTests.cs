using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Data.Search.BrowseFilters;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class BrowseFilterServiceStorageTests
    {
        private const string StoreId = "Store-Test";

        [Fact]
        public async Task GetStoreAggregations_RoundTripsThroughSave()
        {
            var store = CreateStoreWithSetting();
            var storeService = CreateStoreServiceMock(store);

            var service = new BrowseFilterService(storeService.Object);

            var filters = new List<IBrowseFilter>
            {
                new AttributeFilter { Key = "Brand", FacetSize = 5 },
            };

            await service.SaveStoreAggregationsAsync(StoreId, filters);
            var result = await service.GetStoreAggregationsAsync(StoreId);

            Assert.NotNull(result);
            var attribute = Assert.Single(result.OfType<AttributeFilter>());
            Assert.Equal("Brand", attribute.Key);
        }

        [Theory]
        [InlineData(ModuleConstants.TermValuesSortingTypeScore)]
        [InlineData(ModuleConstants.TermValuesSortingTypePriority)]
        [InlineData(ModuleConstants.TermValuesSortingTypePriorityAscending)]
        [InlineData(ModuleConstants.TermValuesSortingTypePriorityDescending)]
        [InlineData(ModuleConstants.TermValuesSortingTypeNameAscending)]
        [InlineData(ModuleConstants.TermValuesSortingTypeNameDescending)]
        [InlineData(ModuleConstants.TermValuesSortingTypeNumericAscending)]
        [InlineData(ModuleConstants.TermValuesSortingTypeNumericDescending)]
        public async Task GetStoreAggregations_RoundTripsTermValuesSortingTypeVerbatim(string sortingType)
        {
            // The sorting type is persisted as a free-form string, so every value has to survive
            // the save/load cycle byte for byte, including the legacy bare "Priority".
            var store = CreateStoreWithSetting();
            var storeService = CreateStoreServiceMock(store);

            var service = new BrowseFilterService(storeService.Object);

            var filters = new List<IBrowseFilter>
            {
                new AttributeFilter { Key = "Color", TermValuesSortingType = sortingType },
            };

            await service.SaveStoreAggregationsAsync(StoreId, filters);
            var result = await service.GetStoreAggregationsAsync(StoreId);

            var attribute = Assert.Single(result.OfType<AttributeFilter>());
            Assert.Equal(sortingType, attribute.TermValuesSortingType);
        }

        [Fact]
        public async Task GetStoreAggregations_LegacyXmlPriority_DeserializesVerbatim()
        {
            // Stores configured before the ascending/descending split can still hold the XML format
            // with a bare "Priority". It has to reach the aggregation converter unchanged.
            var store = CreateStoreWithSetting(
                """
                <browsing>
                  <attribute key="Color">
                    <termValuesSortingType>Priority</termValuesSortingType>
                  </attribute>
                </browsing>
                """);
            var storeService = CreateStoreServiceMock(store);

            var service = new BrowseFilterService(storeService.Object);

            var result = await service.GetStoreAggregationsAsync(StoreId);

            var attribute = Assert.Single(result.OfType<AttributeFilter>());
            Assert.Equal("Color", attribute.Key);
            Assert.Equal(ModuleConstants.TermValuesSortingTypePriority, attribute.TermValuesSortingType);
        }

        [Fact]
        public async Task GetStoreAggregations_WhenStoreMissing_ReturnsNull()
        {
            var storeService = new Mock<IStoreService>();
            // GetAsync returns empty so GetNoCloneAsync resolves to null.
            storeService
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new List<Store>());

            var service = new BrowseFilterService(storeService.Object);

            var result = await service.GetStoreAggregationsAsync(StoreId);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetStoreAggregations_WhenSettingValueEmpty_ReturnsNull()
        {
            var store = CreateStoreWithSetting();
            var storeService = CreateStoreServiceMock(store);

            var service = new BrowseFilterService(storeService.Object);

            var result = await service.GetStoreAggregationsAsync(StoreId);

            Assert.Null(result);
        }

        [Fact]
        public async Task SaveStoreAggregations_WritesValueToRegisteredSetting()
        {
            var store = CreateStoreWithSetting();
            var storeService = CreateStoreServiceMock(store);

            var service = new BrowseFilterService(storeService.Object);

            var filters = new List<IBrowseFilter>
            {
                new AttributeFilter { Key = "Brand", FacetSize = 5 },
            };

            await service.SaveStoreAggregationsAsync(StoreId, filters);

            var setting = store.Settings.Single(x =>
                x.Name == ModuleConstants.Settings.Search.FilteredBrowsing.Name);
            Assert.NotNull(setting.Value);
            Assert.Contains("Brand", setting.Value as string);
            storeService.Verify(x => x.SaveChangesAsync(It.Is<IList<Store>>(s => s.Count == 1 && s[0] == store)), Times.Once);
            Assert.Empty(store.DynamicProperties);
        }

        [Fact]
        public async Task SaveStoreAggregations_OverwritesExistingValue()
        {
            var store = CreateStoreWithSetting("legacy-value");
            var storeService = CreateStoreServiceMock(store);

            var service = new BrowseFilterService(storeService.Object);

            var filters = new List<IBrowseFilter>
            {
                new AttributeFilter { Key = "Color" },
            };

            await service.SaveStoreAggregationsAsync(StoreId, filters);

            var setting = store.Settings.Single(x =>
                x.Name == ModuleConstants.Settings.Search.FilteredBrowsing.Name);
            Assert.Contains("Color", setting.Value as string);
            Assert.DoesNotContain("legacy-value", setting.Value as string);
        }

        // Store with the FilteredBrowsing setting already registered (mirrors the runtime state where
        // settings are auto-created on registration). Pass a value to pre-populate it.
        private static Store CreateStoreWithSetting(string value = null)
        {
            return new Store
            {
                Id = StoreId,
                Settings =
                [
                    new ObjectSettingEntry(ModuleConstants.Settings.Search.FilteredBrowsing)
                    {
                        Value = value,
                    },
                ],
                DynamicProperties = [],
            };
        }

        private static Mock<IStoreService> CreateStoreServiceMock(Store store)
        {
            var mock = new Mock<IStoreService>();
            // GetByIdAsync / GetNoCloneAsync are extension methods that delegate to GetAsync.
            mock.Setup(x => x.GetAsync(
                    It.Is<IList<string>>(ids => ids.Count == 1 && ids[0] == StoreId),
                    It.IsAny<string>(),
                    It.IsAny<bool>()))
                .ReturnsAsync([store]);
            return mock;
        }
    }
}
