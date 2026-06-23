namespace VirtoCommerce.CatalogModule.Core.Search.Sorting.Resolvers;

/// <summary>"Price, high to low". <c>price</c> is bound to the currency-specific index field downstream.</summary>
public class PriceDescendingProductSearchOrderResolver : AbstractExpressionProductSearchOrderResolver
{
    public override string Code => ProductSearchOrderCodes.PriceDescending;

    public override ProductSearchOrderInfo Info { get; } = new() { Name = "Price, high to low", Order = ProductSearchOrderCodes.DefaultOrder.PriceDescending };

    protected override string DefaultExpression => "price:desc";
}
