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
    /// Returns SEO path if all outline items of the first outline have SEO keywords, otherwise returns default value.
    /// Path: GrandParentCategory/ParentCategory/ProductCategory/Product
    /// </summary>
    public static string GetSeoPath(this IHasOutlines hasOutlines, Store store, string language)
    {
        return hasOutlines?.Outlines.GetSeoPath(store, language);
    }

    /// <summary>
    /// Returns SEO path if all outline items of the first outline have SEO keywords, otherwise returns default value.
    /// Path: GrandParentCategory/ParentCategory/ProductCategory/Product
    /// </summary>
    public static string GetSeoPath(this IEnumerable<Outline> outlines, Store store, string language)
    {
        var outline = outlines?.GetOutlineForCatalog(store.Catalog);
        if (outline is null)
        {
            return null;
        }

        var seoLinksType = store.GetSeoLinksType();
        if (seoLinksType == SeoNone)
        {
            return null;
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

        if (pathSegments.Count > 0 && pathSegments.All(x => x != null))
        {
            return string.Join("/", pathSegments);
        }

        return null;

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
        var outline = outlines?.GetOutlineForCatalog(catalogId);
        if (outline is null)
        {
            return null;
        }

        var pathSegments = outline.Items
            .Where(x => !x.IsCatalog())
            .Select(x => x.Id)
            .ToList();

        if (pathSegments.Count > 0 && pathSegments.All(x => x != null))
        {
            return string.Join("/", pathSegments);
        }

        return null;
    }


    /// <summary>
    /// Returns first outline for the given catalog (if any)
    /// </summary>
    private static Outline GetOutlineForCatalog(this IEnumerable<Outline> outlines, string catalogId)
    {
        // Find any outline for the given catalog
        return outlines.FirstOrDefault(outline => outline.Items.Any(item => item.IsCatalog() && item.Id == catalogId));
    }

    private static bool IsCatalog(this OutlineItem item)
    {
        return item.SeoObjectType == "Catalog";
    }

    private static bool IsLinkedCategory(this OutlineItem item)
    {
        return item.SeoObjectType == "Category" && item.HasVirtualParent;
    }
}
