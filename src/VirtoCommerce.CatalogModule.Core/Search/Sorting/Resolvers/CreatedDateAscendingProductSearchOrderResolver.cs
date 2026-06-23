namespace VirtoCommerce.CatalogModule.Core.Search.Sorting.Resolvers;

/// <summary>"Date, old to new".</summary>
public class CreatedDateAscendingProductSearchOrderResolver : AbstractExpressionProductSearchOrderResolver
{
    public override string Code => ProductSearchOrderings.CreatedDateAscending;

    public override ProductSearchOrderInfo Info { get; } = new() { Name = "Date, old to new", Order = ProductSearchOrderings.CreatedDateAscendingOrder };

    protected override string DefaultExpression => "createddate:asc";
}
