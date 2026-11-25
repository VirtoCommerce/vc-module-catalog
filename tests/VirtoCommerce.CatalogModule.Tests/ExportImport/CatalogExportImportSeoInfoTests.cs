using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json.Linq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests.ExportImport;

[Trait("Category", "Unit")]
public class CatalogExportImportSeoInfoTests
{
    // An imported catalog without an SEO record gets the one CatalogSeoInfoDefaults gives, so a solution's override of it
    // applies to imported catalogs too; a record sent keeps it, with its missing slug and title filled in.
    [Fact]
    public async Task DoImportAsync_ACatalogWithoutAnSeoRecord_GetsItFromCatalogSeoInfoDefaults()
    {
        //Arrange
        var fixture = new CatalogExportImportTestFixture();
        var saved = new List<Catalog>();
        fixture.CatalogService
            .Setup(x => x.SaveChangesAsync(It.IsAny<IList<Catalog>>()))
            .Callback((IList<Catalog> catalogs) => saved.AddRange(catalogs))
            .Returns(Task.CompletedTask);
        var catalogs = new JObject
        {
            ["Catalogs"] = new JArray(CreateCatalogJson("imported-without-seo", seoInfos: null),
                CreateCatalogJson("imported-with-seo", new JArray(new JObject { ["LanguageCode"] = "en-US" }))),
        };
        var package = CatalogPackageTestHelper.Build(catalogs);

        AbstractTypeFactory<CatalogSeoInfoDefaults>.OverrideType<CatalogSeoInfoDefaults, RecordingSeoInfoDefaults>();

        try
        {
            //Act
            await fixture.CreateSut().DoImportAsync(new MemoryStream(package), new ExportImportOptions(), fixture.CaptureProgress, CancellationToken.None);
        }
        finally
        {
            AbstractTypeFactory<CatalogSeoInfoDefaults>.OverrideType<RecordingSeoInfoDefaults, CatalogSeoInfoDefaults>();
        }

        //Assert
        Assert.Contains("imported-without-seo", RecordingSeoInfoDefaults.CatalogIds);
        var seoInfo = Assert.Single(saved.Single(x => x.Id == "imported-without-seo").SeoInfos);
        Assert.Equal(("catalog", "Catalog", "en-US"), (seoInfo.SemanticUrl, seoInfo.PageTitle, seoInfo.LanguageCode));
        var sent = Assert.Single(saved.Single(x => x.Id == "imported-with-seo").SeoInfos);
        Assert.Equal(("catalog", "Catalog", "en-US"), (sent.SemanticUrl, sent.PageTitle, sent.LanguageCode));
    }

    private static JObject CreateCatalogJson(string id, JArray seoInfos)
    {
        var catalog = new JObject
        {
            ["Id"] = id,
            ["Name"] = id,
            ["Languages"] = new JArray(new JObject { ["LanguageCode"] = "en-US", ["IsDefault"] = true }),
        };

        if (seoInfos != null)
        {
            catalog["SeoInfos"] = seoInfos;
        }

        return catalog;
    }
}
