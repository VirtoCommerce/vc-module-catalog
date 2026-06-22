namespace VirtoCommerce.CatalogModule.Core.Search.Sorting.Resolvers;

/// <summary>"Alphabetically, A-Z".</summary>
public class NameAscendingProductSearchOrderResolver : AbstractExpressionProductSearchOrderResolver
{
    public override string Code => ProductSearchOrderCodes.NameAscending;

    public override ProductSearchOrderInfo Info { get; } = new() { Name = "Alphabetically, A-Z", Order = 1 };

    protected override string DefaultExpression => "name:asc";
}
