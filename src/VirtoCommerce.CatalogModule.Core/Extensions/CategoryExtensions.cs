using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Extensions;
using VirtoCommerce.StoreModule.Core.Model;
using static VirtoCommerce.StoreModule.Core.ModuleConstants.Settings.SEO;

namespace VirtoCommerce.CatalogModule.Core.Extensions;

public static class CategoryExtensions
{
    public static IEnumerable<Category> FilterLinked(this IEnumerable<Category> source, Store store, Catalog catalog)
    {
        var isCollapsed = store.GetSeoLinksType() == SeoCollapsed && catalog.IsVirtual;
        if (isCollapsed)
        {
            return source.Where(x => x.Links.All(link => link.CatalogId != store.Catalog));
        }

        return source;
    }
}
