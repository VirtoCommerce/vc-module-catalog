namespace VirtoCommerce.CatalogModule.Core.Search.Sorting.Resolvers;

/// <summary>"Price, high to low". <c>price</c> is bound to the currency-specific index field downstream.</summary>
public class PriceDescendingProductSortingResolver : AbstractExpressionProductSortingResolver
{
    public override string Code => ProductSortingConsts.PriceDescending;

    public override ProductSortingInfo Info { get; } = new() { Name = "Price, high to low", Order = ProductSortingConsts.PriceDescendingOrder };

    protected override string DefaultExpression => "price:desc";
}
