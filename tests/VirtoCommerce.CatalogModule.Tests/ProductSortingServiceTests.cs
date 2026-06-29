using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json;
using VirtoCommerce.CatalogModule.Core.Search.Sorting;
using VirtoCommerce.CatalogModule.Core.Search.Sorting.Resolvers;
using VirtoCommerce.CatalogModule.Data.Search.Sorting;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using Xunit;
using static VirtoCommerce.CatalogModule.Core.ModuleConstants.Settings.Search;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class ProductSortingServiceTests
    {
        private const string StoreId = "Store-Test";

        // ---- GetSortingsAsync: compose code defaults ⊕ stored deltas (stubbed at the stored-entries boundary) ----

        [Fact]
        public async Task GetOrderings_NoStoredEntries_ReturnsSevenBuiltInDefaultsInOrder()
        {
            var service = CreateService([]);

            var result = await service.GetSortingsAsync(Context());

            result.Select(x => x.Code).Should().Equal(
                ProductSortingConsts.Featured,
                ProductSortingConsts.NameAscending,
                ProductSortingConsts.NameDescending,
                ProductSortingConsts.PriceAscending,
                ProductSortingConsts.PriceDescending,
                ProductSortingConsts.CreatedDateDescending,
                ProductSortingConsts.CreatedDateAscending);

            result.Should().OnlyContain(x => x.IsVisible && !x.IsCustom);
            result.Count(x => x.IsDefault).Should().Be(1);
            result.Single(x => x.Code == ProductSortingConsts.Featured).IsDefault.Should().BeTrue();
            result.Single(x => x.Code == ProductSortingConsts.Featured).SortExpression
                .Should().Be("__score:desc;priority:desc;id:asc");
            result.Single(x => x.Code == ProductSortingConsts.PriceAscending).SortExpression.Should().Be("price:asc");
        }

        [Fact]
        public async Task GetOrderings_HiddenDelta_MarksBuiltInHidden_DefaultUnchanged()
        {
            var service = CreateService([
                new ProductSortingEntry { Code = ProductSortingConsts.PriceAscending, IsVisible = false },
            ]);

            var result = await service.GetSortingsAsync(Context());

            result.Single(x => x.Code == ProductSortingConsts.PriceAscending).IsVisible.Should().BeFalse();
            result.Single(x => x.Code == ProductSortingConsts.Featured).IsDefault.Should().BeTrue();
        }

        [Fact]
        public async Task GetOrderings_DefaultHidden_DefaultMovesToNextVisible()
        {
            var service = CreateService([
                new ProductSortingEntry { Code = ProductSortingConsts.Featured, IsVisible = false },
            ]);

            var result = await service.GetSortingsAsync(Context());

            result.Single(x => x.Code == ProductSortingConsts.Featured).IsDefault.Should().BeFalse();
            result.Single(x => x.Code == ProductSortingConsts.NameAscending).IsDefault.Should().BeTrue();
        }

        [Fact]
        public async Task GetOrderings_NameAndClauseOverride_AppliesOverride()
        {
            var service = CreateService([
                new ProductSortingEntry
                {
                    Code = ProductSortingConsts.NameAscending,
                    Name = "A to Z",
                    Clauses = [new SortClause { Field = "name", IsDescending = true }],
                },
            ]);

            var result = await service.GetSortingsAsync(Context());

            var nameAsc = result.Single(x => x.Code == ProductSortingConsts.NameAscending);
            nameAsc.Name.Should().Be("A to Z");
            nameAsc.SortExpression.Should().Be("name:desc");
        }

        [Fact]
        public async Task GetOrderings_CustomEntry_AppendedAfterBuiltIns()
        {
            var service = CreateService([
                new ProductSortingEntry
                {
                    Code = "foo",
                    Name = "Foo bar",
                    IsCustom = true,
                    Order = 7,
                    Clauses = [new SortClause { Field = "availability", IsDescending = true }],
                },
            ]);

            var result = await service.GetSortingsAsync(Context());

            result.Should().HaveCount(8);
            var foo = result.Single(x => x.Code == "foo");
            foo.IsCustom.Should().BeTrue();
            foo.SortExpression.Should().Be("availability:desc");
            result.Last().Code.Should().Be("foo");
        }

        [Fact]
        public async Task GetOrderings_OrphanAndClauselessCustomEntries_AreIgnored()
        {
            var service = CreateService([
                new ProductSortingEntry { Code = "orphan-no-resolver", IsCustom = false },
                new ProductSortingEntry { Code = "custom-without-clause", IsCustom = true, Clauses = null },
            ]);

            var result = await service.GetSortingsAsync(Context());

            result.Should().HaveCount(7);
            result.Should().NotContain(x => x.Code == "orphan-no-resolver");
            result.Should().NotContain(x => x.Code == "custom-without-clause");
        }

        [Fact]
        public async Task GetOrderings_MalformedStoredJson_FallsBackToDefaults()
        {
            // Real read path: a malformed setting value must not throw — Deserialize swallows it and returns defaults.
            var store = CreateStoreWithSetting("this is not valid json {{{");
            var service = new ProductSortingService(CreateRegistry(), CreateStoreServiceMock(store).Object,
                NullLogger<ProductSortingService>.Instance);

            var result = await service.GetSortingsAsync(Context());

            result.Should().HaveCount(7);
            result.First().Code.Should().Be(ProductSortingConsts.Featured);
        }

        // ---- Sort expression resolution (GetSortingsAsync + FindSelected, the path x-catalog applies) ----

        [Fact]
        public async Task ResolveExpression_EmptySort_ReturnsDefaultExpression()
        {
            var service = CreateService([]);

            var expression = await ResolveExpression(service, "");

            expression.Should().Be("__score:desc;priority:desc;id:asc");
        }

        [Theory]
        [InlineData("price-ascending", "price:asc")]
        [InlineData("PRICE-ASCENDING", "price:asc")] // code match is case-insensitive
        [InlineData("name-descending", "name:desc")]
        public async Task ResolveExpression_KnownCode_ReturnsThatExpression(string sort, string expected)
        {
            var service = CreateService([]);

            var expression = await ResolveExpression(service, sort);

            expression.Should().Be(expected);
        }

        [Fact]
        public async Task ResolveExpression_RawOrUnknownToken_PassesThrough()
        {
            var service = CreateService([]);

            var expression = await ResolveExpression(service, "rating:desc");

            expression.Should().Be("rating:desc");
        }

        // ---- Gate flags (AllowOverride / IsExpressionEditable) — the no-migration "kill switch" ----

        [Fact]
        public async Task GetOrderings_AllowOverrideFalse_IgnoresStoredDisplayDelta()
        {
            // A resolver with AllowOverride=false keeps its code-provided name/order/visibility even when the
            // store has stored overrides for them.
            var service = CreateService(
                [new FixedDisplayResolver()],
                [new ProductSortingEntry { Code = FixedDisplayResolver.ResolverCode, Name = "Hacked", Order = 99, IsVisible = false }]);

            var sorting = (await service.GetSortingsAsync(Context())).Single(x => x.Code == FixedDisplayResolver.ResolverCode);

            sorting.Name.Should().Be("Fixed");
            sorting.Order.Should().Be(50);
            sorting.IsVisible.Should().BeTrue();
        }

        [Fact]
        public async Task GetOrderings_ExpressionNotEditable_IgnoresStoredClauses()
        {
            // A resolver with IsExpressionEditable=false keeps its code expression, ignoring stored clauses.
            var service = CreateService(
                [new FixedExpressionResolver()],
                [new ProductSortingEntry { Code = FixedExpressionResolver.ResolverCode, Clauses = [new SortClause { Field = "name", IsDescending = true }] }]);

            var sorting = (await service.GetSortingsAsync(Context())).Single(x => x.Code == FixedExpressionResolver.ResolverCode);

            sorting.SortExpression.Should().Be("priority:desc");
        }

        [Fact]
        public async Task SaveOrderings_AllowOverrideFalse_DoesNotPersistDisplayDelta()
        {
            var store = CreateStoreWithSetting();
            var registry = new ProductSortingResolverRegistry([new FixedDisplayResolver()], NullLogger<ProductSortingResolverRegistry>.Instance);
            var service = new ProductSortingService(registry, CreateStoreServiceMock(store).Object, NullLogger<ProductSortingService>.Instance);

            var sortings = await service.GetSortingsAsync(Context());
            var sorting = sortings.Single(x => x.Code == FixedDisplayResolver.ResolverCode);
            sorting.IsVisible = false;
            sorting.Order = 99;

            await service.SaveSortingsAsync(StoreId, sortings);

            // AllowOverride=false -> the display change is not a delta -> no entries persisted (stored as "[]").
            ReadPersistedEntries(store).Should().BeEmpty();
        }

        // ---- SaveSortingsAsync: delta-only persistence ----

        [Fact]
        public async Task SaveOrderings_HiddenBuiltIn_PersistsOnlyThatDelta()
        {
            var store = CreateStoreWithSetting();
            var storeService = CreateStoreServiceMock(store);
            var service = new ProductSortingService(CreateRegistry(), storeService.Object,
                NullLogger<ProductSortingService>.Instance);

            // Start from the effective defaults, hide one built-in, then save.
            var sortings = await service.GetSortingsAsync(Context());
            sortings.Single(x => x.Code == ProductSortingConsts.PriceAscending).IsVisible = false;

            await service.SaveSortingsAsync(StoreId, sortings);

            var entry = ReadPersistedEntries(store).Should().ContainSingle().Which;
            entry.Code.Should().Be(ProductSortingConsts.PriceAscending);
            entry.IsVisible.Should().BeFalse();
            storeService.Verify(x => x.SaveChangesAsync(It.Is<IList<Store>>(s => s.Count == 1 && s[0] == store)), Times.Once);
        }

        [Fact]
        public async Task SaveOrderings_OrderAndLocalizedName_RoundTrip()
        {
            var store = CreateStoreWithSetting();
            var service = new ProductSortingService(CreateRegistry(), CreateStoreServiceMock(store).Object,
                NullLogger<ProductSortingService>.Instance);

            var sortings = await service.GetSortingsAsync(Context());
            var nameAsc = sortings.Single(x => x.Code == ProductSortingConsts.NameAscending);
            nameAsc.Order = 0;
            nameAsc.LocalizedNames = new Dictionary<string, string> { ["de-DE"] = "Alphabetisch" };

            await service.SaveSortingsAsync(StoreId, sortings);

            var persisted = ReadPersistedEntries(store).Single(x => x.Code == ProductSortingConsts.NameAscending);
            persisted.Order.Should().Be(0);
            persisted.LocalizedNames.Should().Contain("de-DE", "Alphabetisch");
        }

        [Fact]
        public async Task SaveOrderings_PristineDefaults_PersistsNothing()
        {
            var store = CreateStoreWithSetting();
            var service = new ProductSortingService(CreateRegistry(), CreateStoreServiceMock(store).Object,
                NullLogger<ProductSortingService>.Instance);

            var sortings = await service.GetSortingsAsync(Context());

            await service.SaveSortingsAsync(StoreId, sortings);

            // No overrides -> no delta entries persisted (untouched built-ins are never persisted; stored as "[]").
            ReadPersistedEntries(store).Should().BeEmpty();
        }

        [Fact]
        public async Task SaveOrderings_RevertLastOverrideToDefaults_OverwritesStoredValue_NotNull()
        {
            // Regression (the "stuck hidden" bug): reverting the LAST remaining override yields zero deltas. The save
            // must OVERWRITE the stored value (with "[]"), NOT set it to null — writing null does not overwrite the
            // previously-stored value at the platform settings layer, which left the last override un-clearable.
            var store = CreateStoreWithSetting(JsonConvert.SerializeObject(new[]
            {
                new ProductSortingEntry { Code = ProductSortingConsts.PriceDescending, IsVisible = false },
            }));
            var service = new ProductSortingService(CreateRegistry(), CreateStoreServiceMock(store).Object,
                NullLogger<ProductSortingService>.Instance);

            // Effective list reflects the stored hide; unhide it -> back to all code defaults (zero deltas).
            var sortings = await service.GetSortingsAsync(Context());
            sortings.Single(x => x.Code == ProductSortingConsts.PriceDescending).IsVisible = true;

            await service.SaveSortingsAsync(StoreId, sortings);

            var value = store.Settings.Single(x => x.Name == ProductSortings.Name).Value as string;
            value.Should().NotBeNull("the stale override must be overwritten, not left in place by a null write");
            ReadPersistedEntries(store).Should().BeEmpty();
        }

        // ---- SaveSortingsAsync: validation ----

        [Fact]
        public async Task SaveOrderings_DuplicateCodes_Throws()
        {
            var service = CreateValidationOnlyService();

            var sortings = new List<ProductSorting>
            {
                new() { Code = "dup", Name = "Dup", IsCustom = true, Clauses = [new SortClause { Field = "name" }] },
                new() { Code = "DUP", Name = "Dup", IsCustom = true, Clauses = [new SortClause { Field = "name" }] }, // case-insensitive dup
            };

            await FluentActions.Awaiting(() => service.SaveSortingsAsync(StoreId, sortings))
                .Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SaveOrderings_CustomWithoutClause_Throws()
        {
            var service = CreateValidationOnlyService();

            var sortings = new List<ProductSorting>
            {
                new() { Code = "no-clause", Name = "No clause", IsCustom = true, Clauses = [] },
            };

            await FluentActions.Awaiting(() => service.SaveSortingsAsync(StoreId, sortings))
                .Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SaveOrderings_EmptyCode_Throws()
        {
            var service = CreateValidationOnlyService();

            var sortings = new List<ProductSorting> { new() { Code = "" } };

            await FluentActions.Awaiting(() => service.SaveSortingsAsync(StoreId, sortings))
                .Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SaveOrderings_EmptyName_Throws()
        {
            var service = CreateValidationOnlyService();

            var sortings = new List<ProductSorting>
            {
                new() { Code = "no-name", IsCustom = true, Clauses = [new SortClause { Field = "name" }] },
            };

            await FluentActions.Awaiting(() => service.SaveSortingsAsync(StoreId, sortings))
                .Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SaveOrderings_CustomOrdering_PersistsFullEntry()
        {
            var store = CreateStoreWithSetting();
            var service = new ProductSortingService(CreateRegistry(), CreateStoreServiceMock(store).Object,
                NullLogger<ProductSortingService>.Instance);

            var sortings = new List<ProductSorting>
            {
                new()
                {
                    Code = "my-custom",
                    Name = "My Custom",
                    IsCustom = true,
                    Clauses = [new SortClause { Field = "availability", IsDescending = true }],
                },
            };

            await service.SaveSortingsAsync(StoreId, sortings);

            var entry = ReadPersistedEntries(store).Single(x => x.Code == "my-custom");
            entry.IsCustom.Should().BeTrue();
            entry.Name.Should().Be("My Custom");
            entry.Clauses.Should().ContainSingle().Which.Field.Should().Be("availability");
        }

        // ---- helpers ----

        private static ProductSortingContext Context() => new()
        {
            StoreId = StoreId,
            CatalogId = "catalog",
            CurrencyCode = "USD",
            CultureName = "en-US",
        };

        private static ProductSortingResolverRegistry CreateRegistry() =>
            new(BuiltInResolvers(), NullLogger<ProductSortingResolverRegistry>.Instance);

        private static IEnumerable<IProductSortingResolver> BuiltInResolvers() =>
        [
            new FeaturedProductSortingResolver(),
            new NameAscendingProductSortingResolver(),
            new NameDescendingProductSortingResolver(),
            new PriceAscendingProductSortingResolver(),
            new PriceDescendingProductSortingResolver(),
            new CreatedDateDescendingProductSortingResolver(),
            new CreatedDateAscendingProductSortingResolver(),
        ];

        // Service stubbed at the stored-entries boundary (GetStoredEntriesAsync is protected virtual), so the
        // merge/compose logic can be tested without an IStoreService/settings round-trip.
        private static StubbedProductSortingService CreateService(IList<ProductSortingEntry> storedEntries) =>
            new(CreateRegistry(), storedEntries);

        private static StubbedProductSortingService CreateService(
            IEnumerable<IProductSortingResolver> resolvers,
            IList<ProductSortingEntry> storedEntries) =>
            new(new ProductSortingResolverRegistry(resolvers, NullLogger<ProductSortingResolverRegistry>.Instance), storedEntries);

        // Mirrors how x-catalog resolves the applied expression: the selected sorting's expression, else passthrough.
        private static async Task<string> ResolveExpression(IProductSortingService service, string sort)
        {
            var sortings = await service.GetSortingsAsync(Context());
            return sortings.FindSelected(sort)?.SortExpression ?? sort;
        }

        // Validation runs before the store is loaded, so a no-op store service is enough.
        private static ProductSortingService CreateValidationOnlyService() =>
            new ProductSortingService(CreateRegistry(), Mock.Of<IStoreService>(), NullLogger<ProductSortingService>.Instance);

        private sealed class StubbedProductSortingService : ProductSortingService
        {
            private readonly IList<ProductSortingEntry> _entries;

            public StubbedProductSortingService(IProductSortingResolverRegistry registry, IList<ProductSortingEntry> entries)
                : base(registry, Mock.Of<IStoreService>(), NullLogger<ProductSortingService>.Instance)
            {
                _entries = entries;
            }

            protected override Task<IList<ProductSortingEntry>> GetStoredEntriesAsync(string storeId) =>
                Task.FromResult(_entries);
        }

        // AllowOverride=false: the admin cannot change name/order/visibility (code values win).
        private sealed class FixedDisplayResolver : IProductSortingResolver
        {
            public const string ResolverCode = "fixed-display";
            public string Code => ResolverCode;
            public ProductSortingInfo Info { get; } = new() { Name = "Fixed", Order = 50, AllowOverride = false };
            public string GetSortExpression(ProductSortingContext context) => "name:asc";
        }

        // IsExpressionEditable=false: the admin cannot change the clauses (resolver expression wins).
        private sealed class FixedExpressionResolver : IProductSortingResolver
        {
            public const string ResolverCode = "fixed-expression";
            public string Code => ResolverCode;
            public ProductSortingInfo Info { get; } = new() { Name = "Computed", Order = 51, IsExpressionEditable = false };
            public string GetSortExpression(ProductSortingContext context) => "priority:desc";
        }

        private static Store CreateStoreWithSetting(string value = null) => new()
        {
            Id = StoreId,
            Settings = [new ObjectSettingEntry(ProductSortings) { Value = value }],
            DynamicProperties = [],
        };

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

        private static List<ProductSortingEntry> ReadPersistedEntries(Store store)
        {
            var value = store.Settings.Single(x => x.Name == ProductSortings.Name).Value as string;
            return string.IsNullOrEmpty(value)
                ? []
                : JsonConvert.DeserializeObject<List<ProductSortingEntry>>(value);
        }
    }
}
