using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using VirtoCommerce.CatalogModule.Core.Search.Sorting;
using VirtoCommerce.CatalogModule.Core.Search.Sorting.Resolvers;
using VirtoCommerce.CatalogModule.Data.Search.Sorting;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class ProductSearchOrderResolverRegistryTests
    {
        [Fact]
        public void GetAllResolvers_OrderedByInfoOrder()
        {
            // Registered out of declaration order; the registry must sort by Info.Order.
            var registry = new ProductSearchOrderResolverRegistry(
                [
                    new CreatedDateAscendingProductSearchOrderResolver(), // order 6
                    new FeaturedProductSearchOrderResolver(),             // order 0
                    new PriceAscendingProductSearchOrderResolver(),       // order 3
                ],
                NullLogger<ProductSearchOrderResolverRegistry>.Instance);

            registry.GetAllResolvers().Select(x => x.Code).Should().Equal(
                ProductSearchOrderings.Featured,
                ProductSearchOrderings.PriceAscending,
                ProductSearchOrderings.CreatedDateAscending);
        }

        [Fact]
        public void GetResolver_IsCaseInsensitive_AndNullForUnknown()
        {
            var registry = new ProductSearchOrderResolverRegistry(
                [new FeaturedProductSearchOrderResolver()],
                NullLogger<ProductSearchOrderResolverRegistry>.Instance);

            registry.GetResolver("FEATURED").Should().NotBeNull();
            registry.GetResolver(ProductSearchOrderings.Featured).Should().NotBeNull();
            registry.GetResolver("does-not-exist").Should().BeNull();
        }

        [Fact]
        public void DuplicateCode_FirstRegistrationWins_AndDoesNotThrow()
        {
            var first = new FeaturedProductSearchOrderResolver();
            var shadow = new FeaturedProductSearchOrderResolver(); // same Code -> ignored, logged

            var registry = new ProductSearchOrderResolverRegistry(
                [first, shadow],
                NullLogger<ProductSearchOrderResolverRegistry>.Instance);

            registry.GetAllResolvers().Should().ContainSingle();
            registry.GetResolver(ProductSearchOrderings.Featured).Should().BeSameAs(first);
        }

        [Fact]
        public void BlankCodeResolvers_AreSkipped()
        {
            var registry = new ProductSearchOrderResolverRegistry(
                [new FeaturedProductSearchOrderResolver(), new BlankCodeResolver()],
                NullLogger<ProductSearchOrderResolverRegistry>.Instance);

            registry.GetAllResolvers().Should().ContainSingle();
        }

        private sealed class BlankCodeResolver : IProductSearchOrderResolver
        {
            public string Code => "  ";
            public ProductSearchOrderInfo Info { get; } = new() { Name = "Blank", Order = 99 };
            public string GetSortExpression(ProductSearchOrderContext context) => "id:asc";
        }
    }
}
