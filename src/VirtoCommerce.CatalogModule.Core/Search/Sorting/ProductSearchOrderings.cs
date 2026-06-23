namespace VirtoCommerce.CatalogModule.Core.Search.Sorting;

/// <summary>
/// Stable identifiers and default display positions for the built-in product search orderings. The codes
/// deliberately match the storefront's historical sort ids so the feature stays backwards compatible: even an
/// old backend parses e.g. <c>price-ascending</c> as <c>price asc</c> (the platform <c>SortInfo</c> treats
/// <c>-</c> as a direction separator). Admins can re-order freely; the <c>*Order</c> values are only the
/// out-of-the-box positions surfaced when there is no stored override.
/// </summary>
public static class ProductSearchOrderings
{
    public const string Featured = "featured";
    public const string NameAscending = "name-ascending";
    public const string NameDescending = "name-descending";
    public const string PriceAscending = "price-ascending";
    public const string PriceDescending = "price-descending";
    public const string CreatedDateDescending = "createddate-descending";
    public const string CreatedDateAscending = "createddate-ascending";

    public const int FeaturedOrder = 0;
    public const int NameAscendingOrder = 1;
    public const int NameDescendingOrder = 2;
    public const int PriceAscendingOrder = 3;
    public const int PriceDescendingOrder = 4;
    public const int CreatedDateDescendingOrder = 5;
    public const int CreatedDateAscendingOrder = 6;
}
