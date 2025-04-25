using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CoreModule.Core.Seo;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Extensions;

public static class SeoExtensions
{
    public const string SeoCatalog = "Catalog";
    public const string SeoCategory = "Category";
    public const string SeoProduct = "CatalogProduct";

    public static bool ContainsCatalog(this IEnumerable<ISeoSupport> items, string catalogId)
    {
        return items != null && items.Any(x => x.IsCatalog() && x.Id.EqualsIgnoreCase(catalogId));
    }

    public static bool ContainsCategory(this IEnumerable<ISeoSupport> items, string categoryId)
    {
        return items != null && items.Any(x => x.IsCategory() && x.Id.EqualsIgnoreCase(categoryId));
    }

    public static bool IsCatalog(this ISeoSupport item)
    {
        return item.SeoObjectType.EqualsIgnoreCase(SeoCatalog);
    }

    public static bool IsCategory(this ISeoSupport item)
    {
        return item.SeoObjectType.EqualsIgnoreCase(SeoCategory);
    }

    public static bool IsProduct(this ISeoSupport item)
    {
        return item.SeoObjectType.EqualsIgnoreCase(SeoProduct);
    }
}
