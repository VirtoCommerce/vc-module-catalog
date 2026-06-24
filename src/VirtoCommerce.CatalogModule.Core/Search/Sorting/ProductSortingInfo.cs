namespace VirtoCommerce.CatalogModule.Core.Search.Sorting;

/// <summary>
/// The code-provided defaults for a sort sorting. A resolver supplies concrete values (no nulls);
/// the store admin may override them at runtime depending on the two gate flags below.
/// </summary>
public class ProductSortingInfo
{
    /// <summary>
    /// Default display name (invariant). Localizations are an admin-only concern and are never set from code.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Default position in the storefront dropdown. The empty-sort default is the first visible sorting by <see cref="Order"/>.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Default visibility in the storefront dropdown.
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// When <c>false</c>, the admin cannot override <see cref="Name"/>/<see cref="Order"/>/<see cref="IsVisible"/> —
    /// the code values win and the admin UI shows them read-only. Acts as a no-migration runtime kill-switch.
    /// Orthogonal to <see cref="IsExpressionEditable"/>.
    /// </summary>
    public bool AllowOverride { get; set; } = true;

    /// <summary>
    /// When <c>false</c>, the admin cannot edit the sort clauses/expression — the resolver's expression always wins
    /// and the clause editor is read-only. Use for computed sortings (e.g. ML ranking) or to force an expression
    /// without a data migration. Orthogonal to <see cref="AllowOverride"/>.
    /// </summary>
    public bool IsExpressionEditable { get; set; } = true;
}
