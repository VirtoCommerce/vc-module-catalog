namespace VirtoCommerce.CatalogModule.Core.Search.Sorting.Resolvers;

/// <summary>
/// "Featured" — relevance, then product priority, then id as a stable tie-breaker. The default store ordering.
/// <c>priority</c> is a logical field; the search-request builder binds it to the (per-category) priority index
/// field(s).
/// </summary>
public class FeaturedProductSearchOrderResolver : AbstractExpressionProductSearchOrderResolver
{
    public override string Code => ProductSearchOrderCodes.Featured;

    public override ProductSearchOrderInfo Info { get; } = new() { Name = "Featured", Order = 0 };

    protected override string DefaultExpression => "__score:desc;priority:desc;id:asc";
}
