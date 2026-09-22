using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Search.Barcodes;
using VirtoCommerce.CatalogModule.Data.Search.Barcodes;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using Xunit;
using static VirtoCommerce.CatalogModule.Core.ModuleConstants.Settings.Search;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class BarcodeSearchServiceTests
    {
        private const string StoreId = "Store-Test";

        // ---- settings ----

        [Fact]
        public async Task GetSettings_NoStoredSettings_ReturnsDefaults()
        {
            var service = CreateService(CreateStore());

            var settings = await service.GetSettingsAsync(StoreId);

            settings.ScannerEnabled.Should().BeTrue();
            settings.Fields.Should().BeEmpty();
        }

        [Fact]
        public async Task GetSettings_MalformedStoredJson_FallsBackToFullTextMode()
        {
            var store = CreateStore(new ObjectSettingEntry(BarcodeSearchFields) { Value = "not json" });
            var service = CreateService(store);

            var settings = await service.GetSettingsAsync(StoreId);

            settings.Fields.Should().BeEmpty();
        }

        [Fact]
        public async Task SaveSettings_ThenGetSettings_RoundTripsBothValues()
        {
            var store = CreateStore();
            var service = CreateService(store);

            await service.SaveSettingsAsync(StoreId, new BarcodeSearchSettings
            {
                ScannerEnabled = false,
                Fields = ["gtin", "barcode_ean"],
            });

            var settings = await service.GetSettingsAsync(StoreId);

            settings.ScannerEnabled.Should().BeFalse();
            settings.Fields.Should().Equal("gtin", "barcode_ean");
            StoredValue(store, BarcodeSearchFields).Should().Be("[\"gtin\",\"barcode_ean\"]");
        }

        [Fact]
        public async Task SaveSettings_NoFields_PersistsEmptyJsonArray()
        {
            var store = CreateStore(new ObjectSettingEntry(BarcodeSearchFields) { Value = "[\"gtin\"]" });
            var service = CreateService(store);

            await service.SaveSettingsAsync(StoreId, new BarcodeSearchSettings { Fields = [] });

            StoredValue(store, BarcodeSearchFields).Should().Be("[]");
            (await service.GetSettingsAsync(StoreId)).Fields.Should().BeEmpty();
        }

        [Fact]
        public async Task SaveSettings_UnknownField_ThrowsAndPersistsNothing()
        {
            var store = CreateStore();
            var service = CreateService(store);

            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                service.SaveSettingsAsync(StoreId, new BarcodeSearchSettings { Fields = ["gtin", "price"] }));

            exception.Errors.Should().ContainSingle().Which.ErrorMessage.Should().Contain("price");
            store.Settings.Should().BeEmpty();
        }

        [Fact]
        public async Task SaveSettings_FieldNotInIndex_IsRejected()
        {
            var service = CreateService(CreateStore());

            await Assert.ThrowsAsync<ValidationException>(() =>
                service.SaveSettingsAsync(StoreId, new BarcodeSearchSettings { Fields = ["notindexed"] }));
        }

        [Fact]
        public async Task SaveSettings_MixedCaseAndDuplicates_NormalizesAndDedupes()
        {
            var store = CreateStore();
            var service = CreateService(store);

            await service.SaveSettingsAsync(StoreId, new BarcodeSearchSettings
            {
                Fields = ["GTIN", " gtin ", "MANUFACTURERPARTNUMBER", "Barcode_EAN"],
            });

            (await service.GetSettingsAsync(StoreId)).Fields
                .Should().Equal("gtin", "manufacturerPartNumber", "barcode_ean");
        }

        [Fact]
        public async Task GetSettings_UnknownStore_ReturnsDefaults()
        {
            var service = CreateService(CreateStore());

            var settings = await service.GetSettingsAsync("Unknown-Store");

            settings.ScannerEnabled.Should().BeTrue();
            settings.Fields.Should().BeEmpty();
        }

        [Fact]
        public async Task SaveSettings_UnknownStore_ThrowsStoreNotFound()
        {
            var service = CreateService(CreateStore());

            await Assert.ThrowsAsync<BarcodeSearchStoreNotFoundException>(() =>
                service.SaveSettingsAsync("Unknown-Store", new BarcodeSearchSettings()));
        }

        [Fact]
        public async Task SaveSettings_UnknownStoreAndUnknownField_ReportsTheStoreFirst()
        {
            var service = CreateService(CreateStore());

            await Assert.ThrowsAsync<BarcodeSearchStoreNotFoundException>(() =>
                service.SaveSettingsAsync("Unknown-Store", new BarcodeSearchSettings { Fields = ["price"] }));
        }

        // ---- available fields ----

        [Fact]
        public async Task GetAvailableFields_BuiltInFieldsFirstThenPropertiesByName()
        {
            var service = CreateService(CreateStore());

            var fields = await service.GetAvailableFieldsAsync(StoreId);

            fields.Select(x => x.Name).Should().Equal("code", "gtin", "manufacturerPartNumber", "barcode_ean", "color");
            fields.Where(x => x.IsProductField).Select(x => x.Name).Should().Equal("code", "gtin", "manufacturerPartNumber");
            fields.Should().OnlyContain(x => x.DataType == "String");
            fields.Single(x => x.Name == "barcode_ean").IsCollection.Should().BeTrue();
            fields.Single(x => x.Name == "color").IsCollection.Should().BeFalse();
        }

        [Fact]
        public async Task GetAvailableFields_ExcludesSkuSystemNonStringAndNonIndexedFields()
        {
            var service = CreateService(CreateStore());

            var names = (await service.GetAvailableFieldsAsync(StoreId)).Select(x => x.Name).ToList();

            names.Should().NotContain("sku");            // alias of code
            names.Should().NotContain("__outline");      // system field
            names.Should().NotContain("name");           // in the index, but neither built-in nor a catalog property
            names.Should().NotContain("vendor");
            names.Should().NotContain("priority");       // not a string field
            names.Should().NotContain("description");    // long text property: searchable, not filterable
            names.Should().NotContain("notindexed");     // property that is not in the index schema
        }

        [Fact]
        public async Task GetAvailableFields_BuiltInFieldMissingFromSchema_IsNotOffered()
        {
            var schema = new IndexDocument("product");
            schema.Add(FilterableString("code"));
            var service = CreateService(CreateStore(), schema, []);

            var fields = await service.GetAvailableFieldsAsync(StoreId);

            fields.Select(x => x.Name).Should().Equal("code");
        }

        // ---- helpers ----

        private static TestBarcodeSearchService CreateService(Store store, IndexDocument schema = null, IList<Property> properties = null)
        {
            return new TestBarcodeSearchService(
                CreateStoreServiceMock(store).Object,
                CreatePropertySearchServiceMock(properties ?? DefaultProperties()).Object,
                schema ?? DefaultSchema());
        }

        // Mirrors the real product index: built-in fields, system fields, and the fields the catalog properties produce.
        private static IndexDocument DefaultSchema()
        {
            var schema = new IndexDocument("product");

            schema.Add(FilterableString("code"));
            schema.Add(FilterableString("sku"));
            schema.Add(FilterableString("gtin"));
            schema.Add(FilterableString("manufacturerPartNumber"));
            schema.Add(FilterableString("name"));
            schema.Add(FilterableString("vendor"));
            schema.Add(new IndexDocumentField("__outline", "x", IndexDocumentFieldValueType.String) { IsFilterable = true, IsCollection = true });
            schema.Add(new IndexDocumentField("priority", 0, IndexDocumentFieldValueType.Integer) { IsFilterable = true });
            schema.Add(new IndexDocumentField("barcode_ean", "x", IndexDocumentFieldValueType.String) { IsFilterable = true, IsCollection = true });
            schema.Add(FilterableString("color"));
            // Long text properties are searchable only, so they can never be matched exactly.
            schema.Add(new IndexDocumentField("description", "x", IndexDocumentFieldValueType.String) { IsSearchable = true });

            return schema;
        }

        private static IList<Property> DefaultProperties() =>
        [
            NewProperty("Barcode_EAN", PropertyValueType.ShortText, multivalue: true),
            NewProperty("Color", PropertyValueType.ShortText),
            NewProperty("Description", PropertyValueType.LongText),
            NewProperty("NotIndexed", PropertyValueType.ShortText),
            NewProperty("SKU", PropertyValueType.ShortText),
        ];

        private static Property NewProperty(string name, PropertyValueType valueType, bool multivalue = false) => new()
        {
            Name = name,
            Type = PropertyType.Product,
            ValueType = valueType,
            Multivalue = multivalue,
        };

        private static IndexDocumentField FilterableString(string name) =>
            new(name, "x", IndexDocumentFieldValueType.String) { IsRetrievable = true, IsFilterable = true };

        private static Store CreateStore(params ObjectSettingEntry[] settings) => new()
        {
            Id = StoreId,
            Settings = [.. settings],
            DynamicProperties = [],
        };

        private static string StoredValue(Store store, SettingDescriptor descriptor) =>
            store.Settings.Single(x => x.Name == descriptor.Name).Value as string;

        private static Mock<IStoreService> CreateStoreServiceMock(Store store)
        {
            var mock = new Mock<IStoreService>();
            // GetByIdAsync / GetNoCloneAsync are extension methods that delegate to GetAsync.
            mock.Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync([]);
            mock.Setup(x => x.GetAsync(
                    It.Is<IList<string>>(ids => ids.Count == 1 && ids[0] == StoreId),
                    It.IsAny<string>(),
                    It.IsAny<bool>()))
                .ReturnsAsync([store]);
            return mock;
        }

        private static Mock<IPropertySearchService> CreatePropertySearchServiceMock(IList<Property> properties)
        {
            var mock = new Mock<IPropertySearchService>();
            mock.Setup(x => x.SearchPropertiesAsync(It.IsAny<PropertySearchCriteria>()))
                .ReturnsAsync(new PropertySearchResult { Results = properties, TotalCount = properties.Count });
            return mock;
        }

        // Stubs the index schema so the projection logic is tested without index configurations / schema builders.
        private sealed class TestBarcodeSearchService : BarcodeSearchService
        {
            private readonly IndexDocument _schema;

            public TestBarcodeSearchService(IStoreService storeService, IPropertySearchService propertySearchService, IndexDocument schema)
                : base(new List<IndexDocumentConfiguration>(), propertySearchService, storeService, NullLogger<BarcodeSearchService>.Instance)
            {
                _schema = schema;
            }

            protected override Task<IndexDocument> BuildProductSchemaAsync() => Task.FromResult(_schema);
        }
    }
}
