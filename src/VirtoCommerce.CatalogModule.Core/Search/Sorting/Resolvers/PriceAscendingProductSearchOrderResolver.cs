namespace VirtoCommerce.CatalogModule.Core.Search.Sorting.Resolvers;

/// <summary>"Price, low to high". <c>price</c> is bound to the currency-specific index field downstream.</summary>
public class PriceAscendingProductSearchOrderResolver : AbstractExpressionProductSearchOrderResolver
{
    public override string Code => ProductSearchOrderCodes.PriceAscending;

    public override ProductSearchOrderInfo Info { get; } = new() { Name = "Price, low to high", Order = ProductSearchOrderCodes.DefaultOrder.PriceAscending };

    protected override string DefaultExpression => "price:asc";
}
