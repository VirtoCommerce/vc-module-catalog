namespace VirtoCommerce.CatalogModule.Core.Search.Sorting;

/// <summary>
/// Request-time context handed to resolvers and the sorting service. Carries the query parameters a resolver
/// may need to compute a context-dependent expression (e.g. per-category "featured"). Built-in resolvers
/// that return a static expression simply ignore it.
/// </summary>
public class ProductSortingContext
{
    public string StoreId { get; set; }

    public string CatalogId { get; set; }

    /// <summary>
    /// Current category id when browsing a category (null on catalog root / keyword search).
    /// </summary>
    public string CategoryId { get; set; }

    /// <summary>
    /// Current category outline when available (e.g. <c>catalogId/parentCategoryId/categoryId</c>).
    /// </summary>
    public string Outline { get; set; }

    public string CurrencyCode { get; set; }

    public string CultureName { get; set; }

    /// <summary>
    /// The raw incoming sort token from the caller (a resolver code, a raw expression, or empty for the store default).
    /// </summary>
    public string Sort { get; set; }

    // The current search-operation inputs, so a (complex) resolver can react to the user's query — e.g. pick a
    // different sorting when a particular filter is applied. Built-in expression resolvers ignore them.

    /// <summary>Search keyword/phrase of the current request.</summary>
    public string Keyword { get; set; }

    /// <summary>Raw user filter expression of the current request (the filters the shopper applied).</summary>
    public string Filter { get; set; }

    /// <summary>Raw facet expression of the current request.</summary>
    public string Facet { get; set; }
}
