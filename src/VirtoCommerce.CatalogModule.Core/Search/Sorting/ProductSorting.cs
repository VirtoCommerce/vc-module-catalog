using System.Collections.Generic;

namespace VirtoCommerce.CatalogModule.Core.Search.Sorting;

/// <summary>
/// An effective, composed sort sorting: the code default merged with the store-level admin override.
/// This is the projection returned to the admin UI and (filtered to visible) to the storefront via GraphQL.
/// </summary>
public class ProductSorting
{
    /// <summary>
    /// Stable key used in the API and storefront URL (<c>?sort=&lt;code&gt;</c>).
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Effective invariant/base display name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Culture -> name. Admin-owned; the storefront resolves <c>LocalizedNames[culture] ?? Name</c>.
    /// </summary>
    public IDictionary<string, string> LocalizedNames { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Effective position in the dropdown.
    /// </summary>
    public int Order { get; set; }

    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// True for the sorting applied when the incoming sort is empty (the first visible sorting by <see cref="Order"/>). Computed.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Whether the clause editor is editable for this sorting (mirrors the resolver flag; always true for custom sortings).
    /// </summary>
    public bool IsExpressionEditable { get; set; } = true;

    /// <summary>
    /// Whether the admin may override name/order/visibility (mirrors the resolver flag; always true for custom sortings).
    /// </summary>
    public bool AllowOverride { get; set; } = true;

    /// <summary>
    /// True when this sorting was authored entirely in the admin UI (no backing code resolver). Only custom sortings can be deleted.
    /// </summary>
    public bool IsCustom { get; set; }

    /// <summary>
    /// Effective clauses (for the clause editor and the resolved-expression preview).
    /// </summary>
    public IList<SortClause> Clauses { get; set; } = [];

    /// <summary>
    /// Effective logical sort expression (e.g. <c>price:desc</c>). For clause-based sortings this equals the serialized
    /// <see cref="Clauses"/>; for computed resolvers it is the resolver output.
    /// </summary>
    public string SortExpression { get; set; }
}
