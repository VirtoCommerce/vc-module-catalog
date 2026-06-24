using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Search.Sorting;

public static class ProductSortingExtensions
{
    /// <summary>
    /// Picks the sorting selected by an incoming sort token: empty -> the default (first visible) sorting;
    /// a known code -> that sorting; otherwise <c>null</c> (the token is a raw expression / unknown code and is
    /// passed through to the engine unchanged).
    /// </summary>
    public static ProductSorting FindSelected(this IEnumerable<ProductSorting> sortings, string sort)
    {
        if (sortings == null)
        {
            return null;
        }

        var list = sortings as IList<ProductSorting> ?? sortings.ToList();

        if (string.IsNullOrWhiteSpace(sort))
        {
            return list.FirstOrDefault(x => x.IsDefault)
                ?? list.FirstOrDefault(x => x.IsVisible)
                ?? list.FirstOrDefault();
        }

        return list.FirstOrDefault(x => x.Code != null && x.Code.EqualsIgnoreCase(sort));
    }
}
