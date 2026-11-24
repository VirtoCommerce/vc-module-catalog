using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Moq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Web.Authorization;
using VirtoCommerce.CatalogModule.Web.Controllers.Api;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests;

[Trait("Category", "Unit")]
public class CatalogModuleCatalogsControllerTests
{
    // A catalog created without an SEO record is saved with the default one (CatalogSeoInfoDefaults).
    [Fact]
    public async Task CreateCatalog_WithoutAnSeoRecord_SavesTheDefaultOne()
    {
        //Arrange
        var catalogService = new Mock<ICatalogService>();
        var controller = new CatalogModuleCatalogsController(catalogService.Object, Mock.Of<ICatalogSearchService>(),
            new CatalogEntityAuthorizationService(Mock.Of<IAuthorizationService>()));
        var catalog = new Catalog { Name = "EU catalog", Languages = [new CatalogLanguage { LanguageCode = "de-DE", IsDefault = true }] };

        //Act
        await controller.CreateCatalog(catalog);

        //Assert
        catalogService.Verify(x => x.SaveChangesAsync(It.Is<IList<Catalog>>(catalogs =>
            catalogs.Single().SeoInfos.Single().SemanticUrl == "catalog" && catalogs.Single().SeoInfos.Single().LanguageCode == "de-DE")));
    }
}
