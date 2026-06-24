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
    public class ProductSearchOrderServiceTests
    {
        private const string StoreId = "Store-Test";

        // ---- GetOrderingsAsync: compose code defaults ⊕ stored deltas (stubbed at the stored-entries boundary) ----

        [Fact]
        public async Task GetOrderings_NoStoredEntries_ReturnsSevenBuiltInDefaultsInOrder()
        {
            var service = CreateService([]);

            var result = await service.GetOrderingsAsync(Context());

            result.Select(x => x.Code).Should().Equal(
                ProductSearchOrderings.Featured,
                ProductSearchOrderings.NameAscending,
                ProductSearchOrderings.NameDescending,
                ProductSearchOrderings.PriceAscending,
                ProductSearchOrderings.PriceDescending,
                ProductSearchOrderings.CreatedDateDescending,
                ProductSearchOrderings.CreatedDateAscending);

            result.Should().OnlyContain(x => x.IsVisible && !x.IsCustom);
            result.Count(x => x.IsDefault).Should().Be(1);
            result.Single(x => x.Code == ProductSearchOrderings.Featured).IsDefault.Should().BeTrue();
            result.Single(x => x.Code == ProductSearchOrderings.Featured).SortExpression
                .Should().Be("__score:desc;priority:desc;id:asc");
            result.Single(x => x.Code == ProductSearchOrderings.PriceAscending).SortExpression.Should().Be("price:asc");
        }

        [Fact]
        public async Task GetOrderings_HiddenDelta_MarksBuiltInHidden_DefaultUnchanged()
        {
            var service = CreateService([
                new ProductSearchOrderingEntry { Code = ProductSearchOrderings.PriceAscending, IsVisible = false },
            ]);

            var result = await service.GetOrderingsAsync(Context());

            result.Single(x => x.Code == ProductSearchOrderings.PriceAscending).IsVisible.Should().BeFalse();
            result.Single(x => x.Code == ProductSearchOrderings.Featured).IsDefault.Should().BeTrue();
        }

        [Fact]
        public async Task GetOrderings_DefaultHidden_DefaultMovesToNextVisible()
        {
            var service = CreateService([
                new ProductSearchOrderingEntry { Code = ProductSearchOrderings.Featured, IsVisible = false },
            ]);

            var result = await service.GetOrderingsAsync(Context());

            result.Single(x => x.Code == ProductSearchOrderings.Featured).IsDefault.Should().BeFalse();
            result.Single(x => x.Code == ProductSearchOrderings.NameAscending).IsDefault.Should().BeTrue();
        }

        [Fact]
        public async Task GetOrderings_NameAndClauseOverride_AppliesOverride()
        {
            var service = CreateService([
                new ProductSearchOrderingEntry
                {
                    Code = ProductSearchOrderings.NameAscending,
                    Name = "A to Z",
                    Clauses = [new SortClause { Field = "name", IsDescending = true }],
                },
            ]);

            var result = await service.GetOrderingsAsync(Context());

            var nameAsc = result.Single(x => x.Code == ProductSearchOrderings.NameAscending);
            nameAsc.Name.Should().Be("A to Z");
            nameAsc.SortExpression.Should().Be("name:desc");
        }

        [Fact]
        public async Task GetOrderings_CustomEntry_AppendedAfterBuiltIns()
        {
            var service = CreateService([
                new ProductSearchOrderingEntry
                {
                    Code = "foo",
                    Name = "Foo bar",
                    IsCustom = true,
                    Order = 7,
                    Clauses = [new SortClause { Field = "availability", IsDescending = true }],
                },
            ]);

            var result = await service.GetOrderingsAsync(Context());

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
                new ProductSearchOrderingEntry { Code = "orphan-no-resolver", IsCustom = false },
                new ProductSearchOrderingEntry { Code = "custom-without-clause", IsCustom = true, Clauses = null },
            ]);

            var result = await service.GetOrderingsAsync(Context());

            result.Should().HaveCount(7);
            result.Should().NotContain(x => x.Code == "orphan-no-resolver");
            result.Should().NotContain(x => x.Code == "custom-without-clause");
        }

        [Fact]
        public async Task GetOrderings_MalformedStoredJson_FallsBackToDefaults()
        {
            // Real read path: a malformed setting value must not throw — Deserialize swallows it and returns defaults.
            var store = CreateStoreWithSetting("this is not valid json {{{");
            var service = new ProductSearchOrderService(CreateRegistry(), CreateStoreServiceMock(store).Object,
                NullLogger<ProductSearchOrderService>.Instance);

            var result = await service.GetOrderingsAsync(Context());

            result.Should().HaveCount(7);
            result.First().Code.Should().Be(ProductSearchOrderings.Featured);
        }

        // ---- Sort expression resolution (GetOrderingsAsync + FindSelected, the path x-catalog applies) ----

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
                [new ProductSearchOrderingEntry { Code = FixedDisplayResolver.ResolverCode, Name = "Hacked", Order = 99, IsVisible = false }]);

            var ordering = (await service.GetOrderingsAsync(Context())).Single(x => x.Code == FixedDisplayResolver.ResolverCode);

            ordering.Name.Should().Be("Fixed");
            ordering.Order.Should().Be(50);
            ordering.IsVisible.Should().BeTrue();
        }

        [Fact]
        public async Task GetOrderings_ExpressionNotEditable_IgnoresStoredClauses()
        {
            // A resolver with IsExpressionEditable=false keeps its code expression, ignoring stored clauses.
            var service = CreateService(
                [new FixedExpressionResolver()],
                [new ProductSearchOrderingEntry { Code = FixedExpressionResolver.ResolverCode, Clauses = [new SortClause { Field = "name", IsDescending = true }] }]);

            var ordering = (await service.GetOrderingsAsync(Context())).Single(x => x.Code == FixedExpressionResolver.ResolverCode);

            ordering.SortExpression.Should().Be("priority:desc");
        }

        [Fact]
        public async Task SaveOrderings_AllowOverrideFalse_DoesNotPersistDisplayDelta()
        {
            var store = CreateStoreWithSetting();
            var registry = new ProductSearchOrderResolverRegistry([new FixedDisplayResolver()], NullLogger<ProductSearchOrderResolverRegistry>.Instance);
            var service = new ProductSearchOrderService(registry, CreateStoreServiceMock(store).Object, NullLogger<ProductSearchOrderService>.Instance);

            var orderings = await service.GetOrderingsAsync(Context());
            var ordering = orderings.Single(x => x.Code == FixedDisplayResolver.ResolverCode);
            ordering.IsVisible = false;
            ordering.Order = 99;

            await service.SaveOrderingsAsync(StoreId, orderings);

            // AllowOverride=false -> the display change is not a delta -> nothing persisted.
            (store.Settings.Single(x => x.Name == SortDefinitions.Name).Value as string).Should().BeNullOrEmpty();
        }

        // ---- SaveOrderingsAsync: delta-only persistence ----

        [Fact]
        public async Task SaveOrderings_HiddenBuiltIn_PersistsOnlyThatDelta()
        {
            var store = CreateStoreWithSetting();
            var storeService = CreateStoreServiceMock(store);
            var service = new ProductSearchOrderService(CreateRegistry(), storeService.Object,
                NullLogger<ProductSearchOrderService>.Instance);

            // Start from the effective defaults, hide one built-in, then save.
            var orderings = await service.GetOrderingsAsync(Context());
            orderings.Single(x => x.Code == ProductSearchOrderings.PriceAscending).IsVisible = false;

            await service.SaveOrderingsAsync(StoreId, orderings);

            var entry = ReadPersistedEntries(store).Should().ContainSingle().Which;
            entry.Code.Should().Be(ProductSearchOrderings.PriceAscending);
            entry.IsVisible.Should().BeFalse();
            storeService.Verify(x => x.SaveChangesAsync(It.Is<IList<Store>>(s => s.Count == 1 && s[0] == store)), Times.Once);
        }

        [Fact]
        public async Task SaveOrderings_OrderAndLocalizedName_RoundTrip()
        {
            var store = CreateStoreWithSetting();
            var service = new ProductSearchOrderService(CreateRegistry(), CreateStoreServiceMock(store).Object,
                NullLogger<ProductSearchOrderService>.Instance);

            var orderings = await service.GetOrderingsAsync(Context());
            var nameAsc = orderings.Single(x => x.Code == ProductSearchOrderings.NameAscending);
            nameAsc.Order = 0;
            nameAsc.LocalizedNames = new Dictionary<string, string> { ["de-DE"] = "Alphabetisch" };

            await service.SaveOrderingsAsync(StoreId, orderings);

            var persisted = ReadPersistedEntries(store).Single(x => x.Code == ProductSearchOrderings.NameAscending);
            persisted.Order.Should().Be(0);
            persisted.LocalizedNames.Should().Contain("de-DE", "Alphabetisch");
        }

        [Fact]
        public async Task SaveOrderings_PristineDefaults_PersistsNothing()
        {
            var store = CreateStoreWithSetting();
            var service = new ProductSearchOrderService(CreateRegistry(), CreateStoreServiceMock(store).Object,
                NullLogger<ProductSearchOrderService>.Instance);

            var orderings = await service.GetOrderingsAsync(Context());

            await service.SaveOrderingsAsync(StoreId, orderings);

            // No overrides -> delta-only store stays empty (untouched built-ins are never persisted).
            var setting = store.Settings.Single(x => x.Name == SortDefinitions.Name);
            (setting.Value as string).Should().BeNullOrEmpty();
        }

        // ---- SaveOrderingsAsync: validation ----

        [Fact]
        public async Task SaveOrderings_DuplicateCodes_Throws()
        {
            var service = CreateValidationOnlyService();

            var orderings = new List<ProductSearchOrdering>
            {
                new() { Code = "dup", Name = "Dup", IsCustom = true, Clauses = [new SortClause { Field = "name" }] },
                new() { Code = "DUP", Name = "Dup", IsCustom = true, Clauses = [new SortClause { Field = "name" }] }, // case-insensitive dup
            };

            await FluentActions.Awaiting(() => service.SaveOrderingsAsync(StoreId, orderings))
                .Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SaveOrderings_CustomWithoutClause_Throws()
        {
            var service = CreateValidationOnlyService();

            var orderings = new List<ProductSearchOrdering>
            {
                new() { Code = "no-clause", Name = "No clause", IsCustom = true, Clauses = [] },
            };

            await FluentActions.Awaiting(() => service.SaveOrderingsAsync(StoreId, orderings))
                .Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SaveOrderings_EmptyCode_Throws()
        {
            var service = CreateValidationOnlyService();

            var orderings = new List<ProductSearchOrdering> { new() { Code = "" } };

            await FluentActions.Awaiting(() => service.SaveOrderingsAsync(StoreId, orderings))
                .Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SaveOrderings_EmptyName_Throws()
        {
            var service = CreateValidationOnlyService();

            var orderings = new List<ProductSearchOrdering>
            {
                new() { Code = "no-name", IsCustom = true, Clauses = [new SortClause { Field = "name" }] },
            };

            await FluentActions.Awaiting(() => service.SaveOrderingsAsync(StoreId, orderings))
                .Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SaveOrderings_CustomOrdering_PersistsFullEntry()
        {
            var store = CreateStoreWithSetting();
            var service = new ProductSearchOrderService(CreateRegistry(), CreateStoreServiceMock(store).Object,
                NullLogger<ProductSearchOrderService>.Instance);

            var orderings = new List<ProductSearchOrdering>
            {
                new()
                {
                    Code = "my-custom",
                    Name = "My Custom",
                    IsCustom = true,
                    Clauses = [new SortClause { Field = "availability", IsDescending = true }],
                },
            };

            await service.SaveOrderingsAsync(StoreId, orderings);

            var entry = ReadPersistedEntries(store).Single(x => x.Code == "my-custom");
            entry.IsCustom.Should().BeTrue();
            entry.Name.Should().Be("My Custom");
            entry.Clauses.Should().ContainSingle().Which.Field.Should().Be("availability");
        }

        // ---- helpers ----

        private static ProductSearchOrderContext Context() => new()
        {
            StoreId = StoreId,
            CatalogId = "catalog",
            CurrencyCode = "USD",
            CultureName = "en-US",
        };

        private static ProductSearchOrderResolverRegistry CreateRegistry() =>
            new(BuiltInResolvers(), NullLogger<ProductSearchOrderResolverRegistry>.Instance);

        private static IEnumerable<IProductSearchOrderResolver> BuiltInResolvers() =>
        [
            new FeaturedProductSearchOrderResolver(),
            new NameAscendingProductSearchOrderResolver(),
            new NameDescendingProductSearchOrderResolver(),
            new PriceAscendingProductSearchOrderResolver(),
            new PriceDescendingProductSearchOrderResolver(),
            new CreatedDateDescendingProductSearchOrderResolver(),
            new CreatedDateAscendingProductSearchOrderResolver(),
        ];

        // Service stubbed at the stored-entries boundary (GetStoredEntriesAsync is protected virtual), so the
        // merge/compose logic can be tested without an IStoreService/settings round-trip.
        private static StubbedProductSearchOrderService CreateService(IList<ProductSearchOrderingEntry> storedEntries) =>
            new(CreateRegistry(), storedEntries);

        private static StubbedProductSearchOrderService CreateService(
            IEnumerable<IProductSearchOrderResolver> resolvers,
            IList<ProductSearchOrderingEntry> storedEntries) =>
            new(new ProductSearchOrderResolverRegistry(resolvers, NullLogger<ProductSearchOrderResolverRegistry>.Instance), storedEntries);

        // Mirrors how x-catalog resolves the applied expression: the selected ordering's expression, else passthrough.
        private static async Task<string> ResolveExpression(IProductSearchOrderService service, string sort)
        {
            var orderings = await service.GetOrderingsAsync(Context());
            return orderings.FindSelected(sort)?.SortExpression ?? sort;
        }

        // Validation runs before the store is loaded, so a no-op store service is enough.
        private static ProductSearchOrderService CreateValidationOnlyService() =>
            new ProductSearchOrderService(CreateRegistry(), Mock.Of<IStoreService>(), NullLogger<ProductSearchOrderService>.Instance);

        private sealed class StubbedProductSearchOrderService : ProductSearchOrderService
        {
            private readonly IList<ProductSearchOrderingEntry> _entries;

            public StubbedProductSearchOrderService(IProductSearchOrderResolverRegistry registry, IList<ProductSearchOrderingEntry> entries)
                : base(registry, Mock.Of<IStoreService>(), NullLogger<ProductSearchOrderService>.Instance)
            {
                _entries = entries;
            }

            protected override Task<IList<ProductSearchOrderingEntry>> GetStoredEntriesAsync(string storeId) =>
                Task.FromResult(_entries);
        }

        // AllowOverride=false: the admin cannot change name/order/visibility (code values win).
        private sealed class FixedDisplayResolver : IProductSearchOrderResolver
        {
            public const string ResolverCode = "fixed-display";
            public string Code => ResolverCode;
            public ProductSearchOrderInfo Info { get; } = new() { Name = "Fixed", Order = 50, AllowOverride = false };
            public string GetSortExpression(ProductSearchOrderContext context) => "name:asc";
        }

        // IsExpressionEditable=false: the admin cannot change the clauses (resolver expression wins).
        private sealed class FixedExpressionResolver : IProductSearchOrderResolver
        {
            public const string ResolverCode = "fixed-expression";
            public string Code => ResolverCode;
            public ProductSearchOrderInfo Info { get; } = new() { Name = "Computed", Order = 51, IsExpressionEditable = false };
            public string GetSortExpression(ProductSearchOrderContext context) => "priority:desc";
        }

        private static Store CreateStoreWithSetting(string value = null) => new()
        {
            Id = StoreId,
            Settings = [new ObjectSettingEntry(SortDefinitions) { Value = value }],
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

        private static List<ProductSearchOrderingEntry> ReadPersistedEntries(Store store)
        {
            var value = store.Settings.Single(x => x.Name == SortDefinitions.Name).Value as string;
            return string.IsNullOrEmpty(value)
                ? []
                : JsonConvert.DeserializeObject<List<ProductSearchOrderingEntry>>(value);
        }
    }
}
