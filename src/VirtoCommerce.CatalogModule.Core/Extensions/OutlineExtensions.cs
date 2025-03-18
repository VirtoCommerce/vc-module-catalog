using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CoreModule.Core.Outlines;
using VirtoCommerce.CoreModule.Core.Seo;
using VirtoCommerce.StoreModule.Core.Extensions;
using VirtoCommerce.StoreModule.Core.Model;
using static VirtoCommerce.StoreModule.Core.ModuleConstants.Settings.SEO;

namespace VirtoCommerce.CatalogModule.Core.Extensions;

public static class OutlineExtensions
{
    /// <summary>
    /// Returns SEO path if all outline items of the first outline for the store catalog have SEO keywords, otherwise returns null.
    /// Path: GrandParentCategory/ParentCategory/ProductCategory/Product
    /// </summary>
    public static string GetSeoPath(this IHasOutlines hasOutlines, Store store, string language, string defaultValue = null, string seoLinksType = null)
    {
        return hasOutlines?.Outlines?.GetSeoPath(store, language, defaultValue, seoLinksType);
    }

    /// <summary>
    /// Returns SEO path if all outline items of the first outline for the store catalog have SEO keywords, otherwise returns null.
    /// Path: GrandParentCategory/ParentCategory/ProductCategory/Product
    /// </summary>
    public static string GetSeoPath(this IEnumerable<Outline> outlines, Store store, string language, string defaultValue = null, string seoLinksType = null)
    {
        if (store is null || !TryGetOutlineForCatalog(outlines, store.Catalog, out var outline))
        {
            return defaultValue;
        }

        seoLinksType ??= store.GetSeoLinksType();
        if (seoLinksType == SeoNone)
        {
            return defaultValue;
        }

        var pathSegments = new List<string>();

        switch (seoLinksType)
        {
            case SeoLong:
                pathSegments.AddRange(outline.Items
                    .Where(x => !x.IsCatalog())
                    .Select(GetBestMatchingSeoSlug));
                break;
            case SeoCollapsed:
                {
                    // If last item is a linked category, we cannot build the SEO path
                    var lastItem = outline.Items.Last();
                    if (!lastItem.IsLinkedCategory())
                    {
                        pathSegments.AddRange(outline.Items
                            .Where(x => !x.IsCatalog() && !x.IsLinkedCategory())
                            .Select(GetBestMatchingSeoSlug));
                    }
                    break;
                }
            default: // SeoShort
                {
                    var lastItem = outline.Items.LastOrDefault();
                    if (lastItem != null)
                    {
                        pathSegments.Add(GetBestMatchingSeoSlug(lastItem));
                    }
                    break;
                }
        }

        if (pathSegments.Count == 0 || pathSegments.Any(x => x is null))
        {
            return defaultValue;
        }

        return string.Join('/', pathSegments);

        string GetBestMatchingSeoSlug(ISeoSupport seoSupport)
        {
            return seoSupport.GetBestMatchingSeoInfo(store, language)?.SemanticUrl;
        }
    }

    /// <summary>
    /// Returns best matching outline path for the given catalog: CategoryId/CategoryId2.
    /// </summary>
    public static string GetOutlinePath(this IEnumerable<Outline> outlines, string catalogId)
    {
        if (!TryGetOutlineForCatalog(outlines, catalogId, out var outline))
        {
            return null;
        }

        var pathSegments = outline.Items
            .Where(x => !x.IsCatalog())
            .Select(x => x.Id)
            .ToList();

        if (pathSegments.Count == 0 || pathSegments.Any(x => x is null))
        {
            return null;
        }

        return string.Join('/', pathSegments);
    }


    public static bool TryGetOutlineForCatalog(IEnumerable<Outline> outlines, string catalogId, out Outline outline)
    {
        // Find any outline for the given catalog
        outline = outlines?.FirstOrDefault(x => x.Items.ContainsCatalog(catalogId));

        return outline != null;
    }

    public static bool IsLinkedCategory(this OutlineItem item)
    {
        return item.HasVirtualParent && item.IsCategory();
    }
}
