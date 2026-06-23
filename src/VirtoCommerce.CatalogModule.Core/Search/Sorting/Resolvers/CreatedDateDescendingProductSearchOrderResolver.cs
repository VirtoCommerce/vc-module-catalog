namespace VirtoCommerce.CatalogModule.Core.Search.Sorting.Resolvers;

/// <summary>"Date, new to old".</summary>
public class CreatedDateDescendingProductSearchOrderResolver : AbstractExpressionProductSearchOrderResolver
{
    public override string Code => ProductSearchOrderCodes.CreatedDateDescending;

    public override ProductSearchOrderInfo Info { get; } = new() { Name = "Date, new to old", Order = ProductSearchOrderCodes.DefaultOrder.CreatedDateDescending };

    protected override string DefaultExpression => "createddate:desc";
}
