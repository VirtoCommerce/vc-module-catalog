namespace VirtoCommerce.CatalogModule.Core.Search.Sorting;

/// <summary>
/// A resolver materialized at runtime from an admin-authored ordering stored in the store setting (an entry with
/// no backing code resolver). It simply returns the admin-defined expression. Instances are created by
/// <see cref="IProductSearchOrderService"/>; this type is not registered in DI.
/// </summary>
public class GenericExpressionProductSearchOrderResolver : IProductSearchOrderResolver
{
    private readonly string _expression;

    public GenericExpressionProductSearchOrderResolver(string code, ProductSearchOrderInfo info, string expression)
    {
        Code = code;
        Info = info;
        _expression = expression;
    }

    public string Code { get; }

    public ProductSearchOrderInfo Info { get; }

    public string GetSortExpression(ProductSearchOrderContext context)
    {
        return _expression;
    }
}
