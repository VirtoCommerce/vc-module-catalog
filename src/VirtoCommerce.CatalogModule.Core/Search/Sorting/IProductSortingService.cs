using System.Collections.Generic;
using System.Threading.Tasks;

namespace VirtoCommerce.CatalogModule.Core.Search.Sorting;

/// <summary>
/// Composes the effective list of sort sortings for a store (code resolvers merged with the store-level admin
/// overrides) and resolves an incoming sort token into a concrete expression. This is the single entry point used
/// by the admin API and by x-catalog at query time.
/// </summary>
public interface IProductSortingService
{
    /// <summary>
    /// Returns all effective sortings for a store (including hidden ones, with their flags), ordered by position.
    /// Expressions are resolved using <paramref name="context"/>. Callers that target the storefront filter to
    /// <see cref="ProductSorting.IsVisible"/> themselves.
    /// </summary>
    Task<IList<ProductSorting>> GetSortingsAsync(ProductSortingContext context);

    /// <summary>
    /// Persists the admin-edited sortings for a store as a sparse delta (plus full entries for custom sortings).
    /// Validates codes are unique and non-empty before saving.
    /// </summary>
    Task SaveSortingsAsync(string storeId, IList<ProductSorting> sortings);
}
