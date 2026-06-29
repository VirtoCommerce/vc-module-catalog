using System.Collections.Generic;

namespace VirtoCommerce.CatalogModule.Core.Search.Sorting;

/// <summary>
/// The persisted shape of a single sorting in the store-level setting (JSON array). It is intentionally a
/// sparse <b>delta</b> keyed by <see cref="Code"/>: only fields the admin actually changed from the code default
/// are stored, so future changes to a resolver's code defaults flow through for untouched fields. Entries with
/// <see cref="IsCustom"/> = true are full admin-authored definitions that have no backing code resolver.
/// </summary>
public class ProductSortingEntry
{
    public string Code { get; set; }

    /// <summary>Override of the display position (null = not overridden).</summary>
    public int? Order { get; set; }

    /// <summary>Override of visibility (null = not overridden).</summary>
    public bool? IsVisible { get; set; }

    /// <summary>Overridden display name (null = not overridden).</summary>
    public string Name { get; set; }

    /// <summary>Admin-defined localizations (always admin-owned).</summary>
    public IDictionary<string, string> LocalizedNames { get; set; }

    /// <summary>Overridden / custom-defined sort clauses (null or empty = not overridden, use the resolver default).</summary>
    public IList<SortClause> Clauses { get; set; }

    /// <summary>True when this entry is an admin-authored sorting with no backing code resolver.</summary>
    public bool IsCustom { get; set; }
}
