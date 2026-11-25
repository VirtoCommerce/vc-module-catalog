using System.Text;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Seo.Core.Models;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests;

[Trait("Category", "Unit")]
public class CatalogSeoInfoDefaultsTests
{
    // The slugs come from GenerateSlug, which needs the code pages the platform registers at its start.
    static CatalogSeoInfoDefaultsTests()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

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

    [Fact]
    public void PrepareProduct_ANewProduct_IsTrimmedAndGetsTheSlugOfItsName()
    {
        //Arrange
        var product = new CatalogProduct { Name = "  Laser printer  " };

        //Act
        new CatalogSeoInfoDefaults().PrepareProduct(product, CreateCatalog(), isNew: true);

        //Assert
        Assert.Equal("Laser printer", product.Name);
        var seoInfo = Assert.Single(product.SeoInfos);
        Assert.Equal(("laser-printer", "de-DE"), (seoInfo.SemanticUrl, seoInfo.LanguageCode));
    }

    [Fact]
    public void PrepareProduct_AnExistingProduct_IsTrimmedOnly()
    {
        //Arrange
        var product = new CatalogProduct { Name = " Laser printer " };

        //Act
        new CatalogSeoInfoDefaults().PrepareProduct(product, CreateCatalog(), isNew: false);

        //Assert
        Assert.Equal("Laser printer", product.Name);
        Assert.Null(product.SeoInfos);
    }

    [Fact]
    public void EnsureSeoInfo_AProductWithARecord_KeepsIt()
    {
        //Arrange
        var sent = new SeoInfo { LanguageCode = "en-US", SemanticUrl = "printer" };
        var product = new CatalogProduct { Name = "Laser printer", SeoInfos = [sent] };

        //Act
        new CatalogSeoInfoDefaults().EnsureSeoInfo(product, CreateCatalog());

        //Assert
        Assert.Same(sent, Assert.Single(product.SeoInfos));
    }

    [Fact]
    public void EnsureSeoInfo_AProductWhoseNameGivesNoSlug_GetsNoRecord()
    {
        //Arrange
        var product = new CatalogProduct { Name = "!!!" };

        //Act
        new CatalogSeoInfoDefaults().EnsureSeoInfo(product, CreateCatalog());

        //Assert
        Assert.Null(product.SeoInfos);
    }

    [Fact]
    public void EnsureSeoInfo_ACategory_GetsTheSlugAndTheCutTitleOfItsName()
    {
        //Arrange
        var name = "Printers " + new string('x', ModuleConstants.MaxSEOTitleLength);
        var category = new Category { Name = name };

        //Act
        new CatalogSeoInfoDefaults().EnsureSeoInfo(category, CreateCatalog());

        //Assert
        var seoInfo = Assert.Single(category.SeoInfos);
        Assert.Equal("de-DE", seoInfo.LanguageCode);
        Assert.StartsWith("printers-", seoInfo.SemanticUrl);
        Assert.True(seoInfo.PageTitle.Length <= ModuleConstants.MaxSEOTitleLength);
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
