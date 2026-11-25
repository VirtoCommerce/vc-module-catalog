using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Seo.Core.Models;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests;

[Trait("Category", "Unit")]
public class CatalogSeoInfoDefaultsTests
{
    [Fact]
    public void EnsureSeoInfo_ACatalogWithoutARecord_GetsTheDefaultOneInItsDefaultLanguage()
    {
        //Arrange
        var catalog = CreateCatalog();

        //Act
        new CatalogSeoInfoDefaults().EnsureSeoInfo(catalog);

        //Assert
        var seoInfo = Assert.Single(catalog.SeoInfos);
        Assert.Equal(("catalog", "Catalog", "de-DE"), (seoInfo.SemanticUrl, seoInfo.PageTitle, seoInfo.LanguageCode));
    }

    [Fact]
    public void EnsureSeoInfo_ACatalogWithARecord_KeepsIt()
    {
        //Arrange
        var sent = new SeoInfo { LanguageCode = "en-US", SemanticUrl = "eu" };
        var catalog = CreateCatalog();
        catalog.SeoInfos = [sent];

        //Act
        new CatalogSeoInfoDefaults().EnsureSeoInfo(catalog);

        //Assert
        Assert.Same(sent, Assert.Single(catalog.SeoInfos));
    }

    private static Catalog CreateCatalog()
    {
        return new Catalog
        {
            Id = "eu-catalog",
            Name = "EU catalog",
            Languages = [new CatalogLanguage { LanguageCode = "en-US" }, new CatalogLanguage { LanguageCode = "de-DE", IsDefault = true }],
        };
    }
}
