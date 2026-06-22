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
}
