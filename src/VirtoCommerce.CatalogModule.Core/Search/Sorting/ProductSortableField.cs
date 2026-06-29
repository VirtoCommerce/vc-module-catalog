namespace VirtoCommerce.CatalogModule.Core.Search.Sorting;

/// <summary>
/// A field that can be used in a sort clause, offered to the admin clause editor. Derived from the product index
/// schema (single-valued filterable fields) plus a few logical tokens, never a hand-maintained list.
/// </summary>
public class ProductSortableField
{
    /// <summary>
    /// Logical field name to use in a clause (e.g. <c>name</c>, <c>priority</c>, <c>createddate</c>, <c>price</c>, <c>__score</c>).
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Field data type hint (the index field value type, or <c>Virtual</c> for logical tokens with no single physical field).
    /// </summary>
    public string DataType { get; set; }
}
