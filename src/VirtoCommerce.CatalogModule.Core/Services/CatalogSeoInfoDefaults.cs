using System.Linq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Seo.Core.Models;

namespace VirtoCommerce.CatalogModule.Core.Services;

// The SEO record of a catalog that has none: the catalogs API and the store setup create catalogs with it, and the JSON
// import gives it to every imported catalog without one. A solution changes it with
// AbstractTypeFactory<CatalogSeoInfoDefaults>.OverrideType.
public class CatalogSeoInfoDefaults
{
    // A catalog without an SEO record gets one in its default language: slug "catalog", title "Catalog".
    public virtual void EnsureSeoInfo(Catalog catalog)
    {
        if (!catalog.SeoInfos.IsNullOrEmpty())
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
