using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Web.Authorization;
using VirtoCommerce.CatalogModule.Web.Controllers.Api;
using VirtoCommerce.Platform.Core.PushNotifications;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.SearchModule.Core.BackgroundJobs;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests;

// The products and categories APIs prepare what they save through CatalogSeoInfoDefaults, as before it held the rules:
// a product's name is trimmed, and a new product or category without an SEO record gets the default one.
[Trait("Category", "Unit")]
public class CatalogModuleSaveDefaultsControllerTests
{
    // The slugs come from GenerateSlug, which needs the code pages the platform registers at its start.
    static CatalogModuleSaveDefaultsControllerTests()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    [Fact]
    public async Task SaveProduct_ANewProduct_IsSavedTrimmedWithTheDefaultSeoRecord()
    {
        // Arrange
        var itemService = new Mock<IItemService>();
        var controller = new CatalogModuleProductsController(itemService.Object, Mock.Of<ICategoryService>(), CreateCatalogService(), Mock.Of<ISkuGenerator>(),
            CreateAuthorizationService(), Mock.Of<IPropertyUpdateManager>(), Options.Create(new MvcNewtonsoftJsonOptions()));

        // Act
        await controller.SaveProduct(new CatalogProduct { Name = "  Laser printer  ", CatalogId = "eu-catalog" });

        // Assert
        itemService.Verify(x => x.SaveChangesAsync(It.Is<IList<CatalogProduct>>(products =>
            products.Single().Name == "Laser printer" &&
            products.Single().SeoInfos.Single().SemanticUrl == "laser-printer" &&
            products.Single().SeoInfos.Single().LanguageCode == "de-DE")));
    }

    [Fact]
    public async Task CreateOrUpdateCategory_ANewCategory_IsSavedWithTheDefaultSeoRecord()
    {
        // Arrange
        var categoryService = new Mock<ICategoryService>();
        var controller = new CatalogModuleCategoriesController(categoryService.Object, CreateCatalogService(), CreateAuthorizationService(),
            Mock.Of<ICategoryProductResolver>(), Mock.Of<IIndexingJobService>(), Mock.Of<IUserNameResolver>(), Mock.Of<IPushNotificationManager>());

        // Act
        await controller.CreateOrUpdateCategory(new Category { Name = "Laser printers", CatalogId = "eu-catalog" });

        // Assert
        categoryService.Verify(x => x.SaveChangesAsync(It.Is<IList<Category>>(categories =>
            categories.Single().SeoInfos.Single().SemanticUrl == "laser-printers" &&
            categories.Single().SeoInfos.Single().PageTitle == "Laser printers" &&
            categories.Single().SeoInfos.Single().LanguageCode == "de-DE")));
    }

    private static ICatalogService CreateCatalogService()
    {
        var catalog = new Catalog
        {
            Id = "eu-catalog",
            Languages = [new CatalogLanguage { LanguageCode = "en-US" }, new CatalogLanguage { LanguageCode = "de-DE", IsDefault = true }],
        };
        var catalogService = new Mock<ICatalogService>();
        catalogService
            .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync([catalog]);

        return catalogService.Object;
    }

    // Every request of the test is allowed.
    private static CatalogEntityAuthorizationService CreateAuthorizationService()
    {
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());

        return new CatalogEntityAuthorizationService(authorizationService.Object);
    }
}
