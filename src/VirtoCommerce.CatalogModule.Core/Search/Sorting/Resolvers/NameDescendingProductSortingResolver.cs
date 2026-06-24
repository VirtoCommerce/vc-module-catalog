namespace VirtoCommerce.CatalogModule.Core.Search.Sorting.Resolvers;

/// <summary>"Alphabetically, Z-A".</summary>
public class NameDescendingProductSortingResolver : AbstractExpressionProductSortingResolver
{
    public override string Code => ProductSortingConsts.NameDescending;

    public override ProductSortingInfo Info { get; } = new() { Name = "Alphabetically, Z-A", Order = ProductSortingConsts.NameDescendingOrder };

    protected override string DefaultExpression => "name:desc";
}
