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
        public async Task GetStoreAggregations_ReadsFromStoreSettings_RoundTripsThroughSave()
        {
            var store = CreateStore();
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
            var store = CreateStore();
            var storeService = CreateStoreServiceMock(store);

            var service = new BrowseFilterService(storeService.Object);

            var result = await service.GetStoreAggregationsAsync(StoreId);

            Assert.Null(result);
        }

        [Fact]
        public async Task SaveStoreAggregations_WhenSettingMissing_AddsNewSetting()
        {
            var store = CreateStore();
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
        public async Task SaveStoreAggregations_WhenSettingExists_UpdatesValue()
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

        private static Store CreateStore()
        {
            return new Store
            {
                Id = StoreId,
                Settings = [new ObjectSettingEntry(ModuleConstants.Settings.Search.FilteredBrowsing)],
                DynamicProperties = [],
            };
        }

        private static Store CreateStoreWithSetting(string value)
        {
            var store = CreateStore();

            return store;
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
