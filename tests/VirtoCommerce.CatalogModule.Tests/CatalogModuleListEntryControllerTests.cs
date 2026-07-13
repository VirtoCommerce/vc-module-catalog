using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Authorization;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.CatalogModule.Web.Authorization;
using VirtoCommerce.CatalogModule.Web.Controllers.Api;
using VirtoCommerce.CatalogModule.Web.Model;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    [Trait("Category", "CI")]
    public class CatalogModuleListEntryControllerTests
    {
        private readonly Mock<IInternalListEntrySearchService> _internalListEntrySearchServiceMock = new();
        private readonly Mock<ILinkSearchService> _linkSearchServiceMock = new();
        private readonly Mock<ICategoryService> _categoryServiceMock = new();
        private readonly Mock<IItemService> _itemServiceMock = new();
        private readonly Mock<ICatalogService> _catalogServiceMock = new();
        private readonly Mock<IAuthorizationService> _authorizationServiceMock = new();

        private CatalogModuleListEntryController CreateController()
        {
            var authorizationService = new CatalogEntityAuthorizationService(_authorizationServiceMock.Object);

            var controller = new CatalogModuleListEntryController(
                _internalListEntrySearchServiceMock.Object,
                _linkSearchServiceMock.Object,
                _categoryServiceMock.Object,
                _itemServiceMock.Object,
                _catalogServiceMock.Object,
                authorizationService,
                new ListEntryMover<Category>(),
                new ListEntryMover<CatalogProduct>());

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "testuser")], "test"))
                }
            };

            return controller;
        }

        // Only requests that carry the exact required permission succeed; everything else (including the
        // legacy CategoriesUpdate/ProductsUpdate permissions) is denied, since there is no fallback.
        private void SetupAuthorization(params string[] grantedPermissions)
        {
            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Failed());

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<object>(),
                    It.Is<IEnumerable<IAuthorizationRequirement>>(requirements => RequestsAnyOf(requirements, grantedPermissions))))
                .ReturnsAsync(AuthorizationResult.Success());
        }

        private static bool RequestsAnyOf(IEnumerable<IAuthorizationRequirement> requirements, string[] grantedPermissions)
        {
            foreach (var requirement in requirements)
            {
                if (requirement is CatalogAuthorizationRequirement car && System.Array.IndexOf(grantedPermissions, car.Permission) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        [Fact]
        public async Task CreateLinks_CategoryEntryWithoutCategoriesLinkPermission_ReturnsForbid()
        {
            // Arrange
            var category = new Category { Id = "cat1", CatalogId = "catalog1", Links = new List<CategoryLink>() };
            _categoryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync([category]);
            _itemServiceMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync([]);

            SetupAuthorization(); // no permissions granted at all

            var controller = CreateController();
            var links = new[] { new CategoryLink { ListEntryId = "cat1", ListEntryType = "category", CategoryId = "target" } };

            // Act
            var result = await controller.CreateLinks(links);

            // Assert
            Assert.IsType<ForbidResult>(result);
            _categoryServiceMock.Verify(x => x.SaveChangesAsync(It.IsAny<IList<Category>>()), Times.Never);
        }

        [Fact]
        public async Task CreateLinks_CategoryEntryHavingOnlyLegacyCategoriesUpdatePermission_StillReturnsForbid()
        {
            // Arrange: the legacy "catalog:categories:update" permission must NOT authorize linking a category
            // (this is the intentional breaking change replacing it with the dedicated "catalog:categories:link").
            var category = new Category { Id = "cat1", CatalogId = "catalog1", Links = new List<CategoryLink>() };
            _categoryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync([category]);
            _itemServiceMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync([]);

            SetupAuthorization("catalog:categories:update");

            var controller = CreateController();
            var links = new[] { new CategoryLink { ListEntryId = "cat1", ListEntryType = "category", CategoryId = "target" } };

            // Act
            var result = await controller.CreateLinks(links);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task CreateLinks_CategoryEntryWithCategoriesLinkPermission_CreatesLinkAndReturnsOk()
        {
            // Arrange
            var category = new Category { Id = "cat1", CatalogId = "catalog1", Links = new List<CategoryLink>() };
            _categoryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync([category]);
            _itemServiceMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync([]);

            SetupAuthorization("catalog:categories:link");

            var controller = CreateController();
            var links = new[] { new CategoryLink { ListEntryId = "cat1", ListEntryType = "category", CategoryId = "target" } };

            // Act
            var result = await controller.CreateLinks(links);

            // Assert
            Assert.IsType<OkResult>(result);
            Assert.Single(category.Links);
            _categoryServiceMock.Verify(x => x.SaveChangesAsync(It.Is<IList<Category>>(c => System.Linq.Enumerable.Contains(c, category))), Times.Once);
        }

        [Fact]
        public async Task CreateLinks_ProductEntryWithoutProductsLinkPermission_ReturnsForbid()
        {
            // Arrange
            var product = new CatalogProduct { Id = "prod1", CatalogId = "catalog1", Links = new List<CategoryLink>() };
            _itemServiceMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync([product]);
            _categoryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync([]);

            SetupAuthorization("catalog:categories:link"); // wrong permission for a product entry

            var controller = CreateController();
            var links = new[] { new CategoryLink { ListEntryId = "prod1", ListEntryType = "product", CategoryId = "target" } };

            // Act
            var result = await controller.CreateLinks(links);

            // Assert
            Assert.IsType<ForbidResult>(result);
            _itemServiceMock.Verify(x => x.SaveChangesAsync(It.IsAny<IList<CatalogProduct>>()), Times.Never);
        }

        [Fact]
        public async Task CreateLinks_ProductEntryWithProductsLinkPermission_CreatesLinkAndReturnsOk()
        {
            // Arrange
            var product = new CatalogProduct { Id = "prod1", CatalogId = "catalog1", Links = new List<CategoryLink>() };
            _itemServiceMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync([product]);
            _categoryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync([]);

            SetupAuthorization("catalog:products:link");

            var controller = CreateController();
            var links = new[] { new CategoryLink { ListEntryId = "prod1", ListEntryType = "product", CategoryId = "target" } };

            // Act
            var result = await controller.CreateLinks(links);

            // Assert
            Assert.IsType<OkResult>(result);
            Assert.Single(product.Links);
        }

        [Fact]
        public async Task BulkCreateLinks_MissingCatalogId_ReturnsBadRequest()
        {
            // Arrange
            var controller = CreateController();
            var request = new BulkLinkCreationRequest
            {
                SearchCriteria = new CatalogListEntrySearchCriteria(),
                CategoryId = "target",
                CatalogId = null,
            };

            // Act
            var result = await controller.BulkCreateLinks(request, TestContext.Current.CancellationToken);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task BulkCreateLinks_NoPermissionsGranted_ReturnsForbid()
        {
            // Arrange
            SetupAuthorization(); // no permissions granted at all

            var controller = CreateController();
            var request = new BulkLinkCreationRequest
            {
                SearchCriteria = new CatalogListEntrySearchCriteria(),
                CategoryId = "target",
                CatalogId = "catalog1",
            };

            // Act
            var result = await controller.BulkCreateLinks(request, TestContext.Current.CancellationToken);

            // Assert
            Assert.IsType<ForbidResult>(result);
            _internalListEntrySearchServiceMock.Verify(
                x => x.InnerListEntrySearchAsync(It.IsAny<CatalogListEntrySearchCriteria>()), Times.Never);
        }
    }
}
