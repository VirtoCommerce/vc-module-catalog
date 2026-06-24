using System.Collections.Generic;
using System.Threading.Tasks;

namespace VirtoCommerce.CatalogModule.Core.Search.Sorting;

/// <summary>
/// Composes the effective list of sort orderings for a store (code resolvers merged with the store-level admin
/// overrides) and resolves an incoming sort token into a concrete expression. This is the single entry point used
/// by the admin API and by x-catalog at query time.
/// </summary>
public interface IProductSearchOrderService
{
    /// <summary>
    /// Returns all effective orderings for a store (including hidden ones, with their flags), ordered by position.
    /// Expressions are resolved using <paramref name="context"/>. Callers that target the storefront filter to
    /// <see cref="ProductSearchOrdering.IsVisible"/> themselves.
    /// </summary>
    Task<IList<ProductSearchOrdering>> GetOrderingsAsync(ProductSearchOrderContext context);

    /// <summary>
    /// Persists the admin-edited orderings for a store as a sparse delta (plus full entries for custom orderings).
    /// Validates codes are unique and non-empty before saving.
    /// </summary>
    Task SaveOrderingsAsync(string storeId, IList<ProductSearchOrdering> orderings);
}
