namespace VirtoCommerce.CatalogModule.Core.Search.Sorting.Resolvers;

/// <summary>"Alphabetically, Z-A".</summary>
public class NameDescendingProductSearchOrderResolver : AbstractExpressionProductSearchOrderResolver
{
    public override string Code => ProductSearchOrderings.NameDescending;

    public override ProductSearchOrderInfo Info { get; } = new() { Name = "Alphabetically, Z-A", Order = ProductSearchOrderings.NameDescendingOrder };

    protected override string DefaultExpression => "name:desc";
}
