namespace VirtoCommerce.CatalogModule.Core.Search.Sorting;

/// <summary>
/// Base for orderings whose expression is a static logical expression (the common case). Subclasses supply the
/// code, the default info and the default expression. Per-context expansion of logical fields
/// (e.g. <c>price</c> -> <c>price_{currency}</c>, <c>priority</c> -> per-category fields) is the search-request
/// builder's job, not the resolver's — so these stay context-free.
/// </summary>
public abstract class AbstractExpressionProductSearchOrderResolver : IProductSearchOrderResolver
{
    public abstract string Code { get; }

    public abstract ProductSearchOrderInfo Info { get; }

    /// <summary>
    /// The default logical sort expression (e.g. <c>price:asc</c>).
    /// </summary>
    protected abstract string DefaultExpression { get; }

    public virtual string GetSortExpression(ProductSearchOrderContext context)
    {
        return DefaultExpression;
    }
}
