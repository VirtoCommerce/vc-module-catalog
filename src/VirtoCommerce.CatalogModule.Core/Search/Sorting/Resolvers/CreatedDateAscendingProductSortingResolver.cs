namespace VirtoCommerce.CatalogModule.Core.Search.Sorting.Resolvers;

/// <summary>"Date, old to new".</summary>
public class CreatedDateAscendingProductSortingResolver : AbstractExpressionProductSortingResolver
{
    public override string Code => ProductSortingConsts.CreatedDateAscending;

    public override ProductSortingInfo Info { get; } = new() { Name = "Date, old to new", Order = ProductSortingConsts.CreatedDateAscendingOrder };

    protected override string DefaultExpression => "createddate:asc";
}
