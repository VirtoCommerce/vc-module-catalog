using System.Linq;
using VirtoCommerce.CatalogModule.Core.Extensions;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Seo.Core.Models;

namespace VirtoCommerce.CatalogModule.Core.Services;

// The SEO records of catalogs, categories and products that have none, and what the products API does to a product
// before saving it. The catalogs, categories and products APIs, the store setup, the catalog manager tools and the JSON
// import use it. A solution changes it with AbstractTypeFactory<CatalogSeoInfoDefaults>.OverrideType.
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

    // What the products API does to a product before saving it: trims its name, and gives a new product without an SEO
    // record a default one. catalog: the product's catalog, or null when it was not found.
    public virtual void PrepareProduct(CatalogProduct product, Catalog catalog, bool isNew)
    {
        if (!product.Name.IsNullOrEmpty())
        {
            product.Name = product.Name.Trim();
        }

        if (isNew)
        {
            EnsureSeoInfo(product, catalog);
        }
    }

    // A product without an SEO record gets one in its catalog's default language: the slug of its name. A name that gives
    // no slug gives no record.
    public virtual void EnsureSeoInfo(CatalogProduct product, Catalog catalog)
    {
        if (!product.SeoInfos.IsNullOrEmpty())
        {
            return;
        }

        var slug = GetSlug(product);

        if (slug.IsNullOrEmpty())
        {
            return;
        }

        var seoInfo = AbstractTypeFactory<SeoInfo>.TryCreateInstance();
        seoInfo.LanguageCode = catalog?.Languages.First(x => x.IsDefault).LanguageCode;
        seoInfo.SemanticUrl = slug;
        product.SeoInfos = [seoInfo];
    }

    // A category without an SEO record gets one in its catalog's default language: the slug of its name, and its name as
    // the page title, cut to the longest title the catalog takes. A name that gives no slug gives no record.
    public virtual void EnsureSeoInfo(Category category, Catalog catalog)
    {
        if (!category.SeoInfos.IsNullOrEmpty())
        {
            return;
        }

        var slug = category.Name.GenerateSlug();

        if (slug.IsNullOrEmpty())
        {
            return;
        }

        var seoInfo = AbstractTypeFactory<SeoInfo>.TryCreateInstance();
        seoInfo.LanguageCode = catalog?.Languages.First(x => x.IsDefault).LanguageCode;
        seoInfo.SemanticUrl = slug;
        seoInfo.PageTitle = category.Name.SoftTruncate(ModuleConstants.MaxSEOTitleLength);
        category.SeoInfos = [seoInfo];
    }

    // The slug of a product's default SEO record: the slug of its name.
    protected virtual string GetSlug(CatalogProduct product)
    {
        return (product.Name ?? string.Empty).GenerateSlug();
    }
}
