using System.Linq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Seo.Core.Models;

namespace VirtoCommerce.CatalogModule.Core.Services;

// The SEO record a new catalog gets when none is sent: the catalogs API and the store setup both create catalogs, and
// both take it from here. A solution changes it with AbstractTypeFactory<CatalogSeoInfoDefaults>.OverrideType.
public class CatalogSeoInfoDefaults
{
    // A catalog without an SEO record gets one in its default language: slug "catalog", title "Catalog".
    public virtual void EnsureSeoInfo(Catalog catalog)
    {
        if (catalog.SeoInfos != null && catalog.SeoInfos.Any())
        {
            return;
        }

        var seoInfo = AbstractTypeFactory<SeoInfo>.TryCreateInstance();
        seoInfo.LanguageCode = catalog.Languages.First(x => x.IsDefault).LanguageCode;
        seoInfo.SemanticUrl = "catalog";
        seoInfo.PageTitle = "Catalog";
        catalog.SeoInfos = [seoInfo];
    }
}
