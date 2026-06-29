namespace VirtoCommerce.CatalogModule.Core.Search.Sorting.Resolvers;

/// <summary>"Date, new to old".</summary>
public class CreatedDateDescendingProductSortingResolver : AbstractExpressionProductSortingResolver
{
    public override string Code => ProductSortingConsts.CreatedDateDescending;

    public override ProductSortingInfo Info { get; } = new() { Name = "Date, new to old", Order = ProductSortingConsts.CreatedDateDescendingOrder };

    protected override string DefaultExpression => "createddate:desc";
}
