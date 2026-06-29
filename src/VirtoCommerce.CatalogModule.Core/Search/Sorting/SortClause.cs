using System.Collections.Generic;
using System.Linq;

namespace VirtoCommerce.CatalogModule.Core.Search.Sorting;

/// <summary>
/// A single sort instruction: a logical field name and a direction.
/// "Logical" means the field is written as it would be in a sort expression (e.g. <c>price</c>, <c>name</c>,
/// <c>priority</c>) and is bound to the physical index field by the search-request builder
/// (e.g. <c>price</c> -> <c>price_usd</c>, <c>name</c> -> <c>name_en-us</c>).
/// </summary>
public class SortClause
{
    public string Field { get; set; }

    public bool IsDescending { get; set; }
}

public static class SortClauseExtensions
{
    /// <summary>
    /// Serializes a list of clauses into a platform sort expression (e.g. <c>__score:desc;priority:desc;id:asc</c>).
    /// The separator is <c>;</c> to match the platform <c>SortInfo</c> column separator.
    /// </summary>
    public static string ToSortExpression(this IEnumerable<SortClause> clauses)
    {
        if (clauses == null)
        {
            return null;
        }

        var parts = clauses
            .Where(x => !string.IsNullOrWhiteSpace(x?.Field))
            .Select(x => $"{x.Field.Trim()}:{(x.IsDescending ? "desc" : "asc")}")
            .ToList();

        return parts.Count > 0 ? string.Join(";", parts) : null;
    }
}
