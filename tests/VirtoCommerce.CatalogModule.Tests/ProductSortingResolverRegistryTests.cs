using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using VirtoCommerce.CatalogModule.Core.Search.Sorting;
using VirtoCommerce.CatalogModule.Core.Search.Sorting.Resolvers;
using VirtoCommerce.CatalogModule.Data.Search.Sorting;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class ProductSortingResolverRegistryTests
    {
        [Fact]
        public void GetAllResolvers_OrderedByInfoOrder()
        {
            // Registered out of declaration order; the registry must sort by Info.Order.
            var registry = new ProductSortingResolverRegistry(
                [
                    new CreatedDateAscendingProductSortingResolver(), // order 6
                    new FeaturedProductSortingResolver(),             // order 0
                    new PriceAscendingProductSortingResolver(),       // order 3
                ],
                NullLogger<ProductSortingResolverRegistry>.Instance);

            registry.GetAllResolvers().Select(x => x.Code).Should().Equal(
                ProductSortingConsts.Featured,
                ProductSortingConsts.PriceAscending,
                ProductSortingConsts.CreatedDateAscending);
        }

        [Fact]
        public void GetResolver_IsCaseInsensitive_AndNullForUnknown()
        {
            var registry = new ProductSortingResolverRegistry(
                [new FeaturedProductSortingResolver()],
                NullLogger<ProductSortingResolverRegistry>.Instance);

            registry.GetResolver("FEATURED").Should().NotBeNull();
            registry.GetResolver(ProductSortingConsts.Featured).Should().NotBeNull();
            registry.GetResolver("does-not-exist").Should().BeNull();
        }

        [Fact]
        public void DuplicateCode_FirstRegistrationWins_AndDoesNotThrow()
        {
            var first = new FeaturedProductSortingResolver();
            var shadow = new FeaturedProductSortingResolver(); // same Code -> ignored, logged

            var registry = new ProductSortingResolverRegistry(
                [first, shadow],
                NullLogger<ProductSortingResolverRegistry>.Instance);

            registry.GetAllResolvers().Should().ContainSingle();
            registry.GetResolver(ProductSortingConsts.Featured).Should().BeSameAs(first);
        }

        [Fact]
        public void BlankCodeResolvers_AreSkipped()
        {
            var registry = new ProductSortingResolverRegistry(
                [new FeaturedProductSortingResolver(), new BlankCodeResolver()],
                NullLogger<ProductSortingResolverRegistry>.Instance);

            registry.GetAllResolvers().Should().ContainSingle();
        }

        private sealed class BlankCodeResolver : IProductSortingResolver
        {
            public string Code => "  ";
            public ProductSortingInfo Info { get; } = new() { Name = "Blank", Order = 99 };
            public string GetSortExpression(ProductSortingContext context) => "id:asc";
        }
    }
}
