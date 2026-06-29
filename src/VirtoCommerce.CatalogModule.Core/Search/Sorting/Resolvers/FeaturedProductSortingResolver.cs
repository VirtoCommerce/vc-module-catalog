namespace VirtoCommerce.CatalogModule.Core.Search.Sorting.Resolvers;

/// <summary>
/// "Featured" — relevance, then product priority, then id as a stable tie-breaker. The default store sorting.
/// <c>priority</c> is a logical field; the search-request builder binds it to the (per-category) priority index
/// field(s).
/// </summary>
public class FeaturedProductSortingResolver : AbstractExpressionProductSortingResolver
{
    public override string Code => ProductSortingConsts.Featured;

    public override ProductSortingInfo Info { get; } = new() { Name = "Featured", Order = ProductSortingConsts.FeaturedOrder };

    protected override string DefaultExpression => "__score:desc;priority:desc;id:asc";
}
