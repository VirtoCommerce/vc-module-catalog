using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Core.Outlines;
using VirtoCommerce.Seo.Core.Extensions;
using VirtoCommerce.Seo.Core.Models;
using VirtoCommerce.StoreModule.Core.Extensions;
using VirtoCommerce.StoreModule.Core.Model;
using static VirtoCommerce.StoreModule.Core.ModuleConstants.Settings.SEO;

namespace VirtoCommerce.CatalogModule.Core.Extensions;

public static class OutlineExtensions
{
    /// <summary>
    /// Returns SEO path if all outline items of the first outline for the store catalog have SEO keywords, otherwise returns default value.
    /// Path: GrandParentCategory/ParentCategory/ProductCategory/Product
    /// </summary>
    public static string GetSeoPath(this IHasOutlines hasOutlines, Store store, string language, string defaultValue = null, string seoLinksType = null)
    {
        return hasOutlines?.Outlines is null || store is null
            ? defaultValue
            : hasOutlines.Outlines.GetSeoPath(store, language, defaultValue, seoLinksType);
    }

    /// <summary>
    /// Returns SEO path if all outline items of the first outline for the store catalog have SEO keywords, otherwise returns default value.
    /// Path: GrandParentCategory/ParentCategory/ProductCategory/Product
    /// </summary>
    public static string GetSeoPath(this IEnumerable<Outline> outlines, Store store, string language, string defaultValue = null, string seoLinksType = null)
    {
        if (outlines is null || store is null || !outlines.TryGetOutlineForCatalog(store.Catalog, out var outline))
        {
            return defaultValue;
        }

        return outline.Items.GetSeoPath(store, language, defaultValue, seoLinksType);
    }

    /// <summary>
    /// Returns SEO path if all outline items have SEO keywords, otherwise returns default value.
    /// Path: GrandParentCategory/ParentCategory/ProductCategory/Product
    /// </summary>
    public static string GetSeoPath(this ICollection<OutlineItem> outlineItems, Store store, string language, string defaultValue = null, string seoLinksType = null)
    {
        if (outlineItems is null || store is null)
        {
            return defaultValue;
        }

        language ??= store.DefaultLanguage;
        seoLinksType ??= store.GetSeoLinksType();

        return outlineItems.GetSeoPath(seoLinksType, store.Id, store.DefaultLanguage, language, defaultValue);
    }

    /// <summary>
    /// Returns SEO path if all outline items have SEO keywords, otherwise returns default value.
    /// Path: GrandParentCategory/ParentCategory/ProductCategory/Product
    /// </summary>
    public static string GetSeoPath(this ICollection<OutlineItem> outlineItems, string seoLinksType, string storeId, string storeDefaultLanguage, string language, string defaultValue = null)
    {
        if (outlineItems is null || seoLinksType == SeoNone)
        {
            return defaultValue;
        }

        var pathSegments = new List<string>();

        switch (seoLinksType)
        {
            case SeoLong:
                pathSegments.AddRange(outlineItems
                    .Where(x => !x.IsCatalog())
                    .Select(GetBestMatchingSeoSlug));
                break;
            case SeoCollapsed:
                {
                    // If last item is a linked category, we cannot build the SEO path
                    var lastItem = outlineItems.Last();
                    if (!lastItem.IsLinkedCategory())
                    {
                        pathSegments.AddRange(outlineItems
                            .Where(x => !x.IsCatalog() && !x.IsLinkedCategory())
                            .Select(GetBestMatchingSeoSlug));
                    }
                    break;
                }
            default: // SeoShort
                {
                    var lastItem = outlineItems.LastOrDefault();
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
            return seoSupport.GetBestMatchingSeoInfo(storeId, storeDefaultLanguage, language)?.SemanticUrl;
        }
    }

    /// <summary>
    /// Returns the path for a first outline with the given catalog: CategoryId/CategoryId2.
    /// </summary>
    public static string GetOutlinePath(this IEnumerable<Outline> outlines, string catalogId)
    {
        if (outlines is null || !outlines.TryGetOutlineForCatalog(catalogId, out var outline))
        {
            return null;
        }

        return outline.Items.GetOutlinePath();
    }

    public static string GetOutlinePath(this IEnumerable<OutlineItem> outlineItems)
    {
        var ids = outlineItems
            ?.Where(x => !x.IsCatalog())
            .Select(x => x.Id)
            .ToList();

        if (ids is null || ids.Count == 0 || ids.Any(x => x is null))
        {
            return null;
        }

        return string.Join('/', ids);
    }


    public static bool TryGetOutlineForCatalog(this IEnumerable<Outline> outlines, string catalogId, out Outline outline)
    {
        // Find any outline for the given catalog
        outline = outlines?.FirstOrDefault(x => x.Items.ContainsCatalog(catalogId));

        return outline != null;
    }

    public static bool IsLinkedCategory(this OutlineItem item)
    {
        return item != null && item.HasVirtualParent && item.IsCategory();
    }
}
