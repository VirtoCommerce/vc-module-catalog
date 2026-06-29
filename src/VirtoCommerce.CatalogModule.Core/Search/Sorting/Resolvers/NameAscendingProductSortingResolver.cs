namespace VirtoCommerce.CatalogModule.Core.Search.Sorting.Resolvers;

/// <summary>"Alphabetically, A-Z".</summary>
public class NameAscendingProductSortingResolver : AbstractExpressionProductSortingResolver
{
    public override string Code => ProductSortingConsts.NameAscending;

    public override ProductSortingInfo Info { get; } = new() { Name = "Alphabetically, A-Z", Order = ProductSortingConsts.NameAscendingOrder };

    protected override string DefaultExpression => "name:asc";
}
