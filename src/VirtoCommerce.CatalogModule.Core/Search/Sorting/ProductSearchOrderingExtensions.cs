using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Search.Sorting;

public static class ProductSearchOrderingExtensions
{
    /// <summary>
    /// Picks the ordering selected by an incoming sort token: empty -> the default (first visible) ordering;
    /// a known code -> that ordering; otherwise <c>null</c> (the token is a raw expression / unknown code and is
    /// passed through to the engine unchanged).
    /// </summary>
    public static ProductSearchOrdering FindSelected(this IEnumerable<ProductSearchOrdering> orderings, string sort)
    {
        if (orderings == null)
        {
            return null;
        }

        var list = orderings as IList<ProductSearchOrdering> ?? orderings.ToList();

        if (string.IsNullOrWhiteSpace(sort))
        {
            return list.FirstOrDefault(x => x.IsDefault)
                ?? list.FirstOrDefault(x => x.IsVisible)
                ?? list.FirstOrDefault();
        }

        return list.FirstOrDefault(x => x.Code != null && x.Code.EqualsIgnoreCase(sort));
    }
}
