namespace VirtoCommerce.CatalogModule.Core.Search.Sorting;

/// <summary>
/// Extension point that contributes a product search sorting ("sort by" option). Register implementations in DI;
/// they are discovered through <see cref="IProductSortingResolverRegistry"/>. The built-in sortings (Featured,
/// A-Z, Z-A, price, date) are themselves resolvers, so they are available everywhere without seeding a store setting.
/// A resolver only provides its <b>code default</b> (info + expression); the store-level override and the
/// composition/precedence are handled by <see cref="IProductSortingService"/>.
/// </summary>
public interface IProductSortingResolver
{
    /// <summary>
    /// Stable, unique key (e.g. <c>featured</c>, <c>price-ascending</c>). Used in the API and the storefront URL.
    /// </summary>
    string Code { get; }

    /// <summary>
    /// Code-provided defaults (name, order, visibility) and the two override gate flags.
    /// </summary>
    ProductSortingInfo Info { get; }

    /// <summary>
    /// Returns the default logical sort expression (e.g. <c>__score:desc;priority:desc;id:asc</c>). Static sortings
    /// return a constant and ignore <paramref name="context"/>; computed sortings may use it (currency, culture,
    /// current category). The expression is bound to physical index fields downstream by the search-request builder.
    /// </summary>
    string GetSortExpression(ProductSortingContext context);
}
