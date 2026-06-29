namespace VirtoCommerce.CatalogModule.Core.Search.Sorting.Resolvers;

/// <summary>"Price, low to high". <c>price</c> is bound to the currency-specific index field downstream.</summary>
public class PriceAscendingProductSortingResolver : AbstractExpressionProductSortingResolver
{
    public override string Code => ProductSortingConsts.PriceAscending;

    public override ProductSortingInfo Info { get; } = new() { Name = "Price, low to high", Order = ProductSortingConsts.PriceAscendingOrder };

    protected override string DefaultExpression => "price:asc";
}
