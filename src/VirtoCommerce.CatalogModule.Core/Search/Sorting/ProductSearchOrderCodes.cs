namespace VirtoCommerce.CatalogModule.Core.Search.Sorting;

/// <summary>
/// Stable codes for the built-in orderings. They deliberately match the storefront's historical sort ids so the
/// feature is backwards compatible: even an old backend parses e.g. <c>price-ascending</c> as <c>price asc</c>
/// (the platform <c>SortInfo</c> treats <c>-</c> as a direction separator).
/// </summary>
public static class ProductSearchOrderCodes
{
    public const string Featured = "featured";
    public const string NameAscending = "name-ascending";
    public const string NameDescending = "name-descending";
    public const string PriceAscending = "price-ascending";
    public const string PriceDescending = "price-descending";
    public const string CreatedDateDescending = "createddate-descending";
    public const string CreatedDateAscending = "createddate-ascending";

    /// <summary>
    /// Default display positions for the built-in orderings. Admins can re-order freely; these are only the
    /// out-of-the-box positions surfaced when there is no stored override.
    /// </summary>
    public static class DefaultOrder
    {
        public const int Featured = 0;
        public const int NameAscending = 1;
        public const int NameDescending = 2;
        public const int PriceAscending = 3;
        public const int PriceDescending = 4;
        public const int CreatedDateDescending = 5;
        public const int CreatedDateAscending = 6;
    }
}
