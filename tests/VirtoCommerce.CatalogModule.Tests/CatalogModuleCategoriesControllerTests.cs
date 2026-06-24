using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Web.Authorization;
using VirtoCommerce.CatalogModule.Web.Controllers.Api;
using VirtoCommerce.Platform.Core.PushNotifications;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.SearchModule.Core.BackgroundJobs;
using VirtoCommerce.SearchModule.Core.Model;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    [Trait("Category", "CI")]
    public class CatalogModuleCategoriesControllerTests
    {
        private readonly Mock<ICategoryService> _categoryServiceMock = new();
        private readonly Mock<ICatalogService> _catalogServiceMock = new();
        private readonly Mock<IAuthorizationService> _authorizationServiceMock = new();
        private readonly Mock<ICategoryProductResolver> _categoryProductResolverMock = new();
        private readonly Mock<IIndexingJobService> _indexingJobServiceMock = new();
        private readonly Mock<IUserNameResolver> _userNameResolverMock = new();
        private readonly Mock<IPushNotificationManager> _pushNotifierMock = new();

        private CatalogModuleCategoriesController CreateController()
        {
            var authService = new CatalogEntityAuthorizationService(_authorizationServiceMock.Object);

            var controller = new CatalogModuleCategoriesController(
                _categoryServiceMock.Object,
                _catalogServiceMock.Object,
                authService,
                _categoryProductResolverMock.Object,
                _indexingJobServiceMock.Object,
                _userNameResolverMock.Object,
                _pushNotifierMock.Object);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "testuser")], "test"))
                }
            };

            return controller;
        }

        private void SetupAuthorizationSuccess()
        {
            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Success());
        }

        [Fact]
        public async Task IndexCategoryProducts_CategoryNotFound_ReturnsNotFound()
        {
            // Arrange
            _categoryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync([]);

            var controller = CreateController();

            // Act
            var result = await controller.IndexCategoryProducts("cat1");

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
            _indexingJobServiceMock.Verify(x => x.Enqueue(It.IsAny<string>(), It.IsAny<IndexingOptions[]>()), Times.Never);
        }

        [Fact]
        public async Task IndexCategoryProducts_AuthorizationDenied_ReturnsForbid()
        {
            // Arrange
            _categoryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync([new Category { Id = "cat1" }]);

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Failed());

            var controller = CreateController();

            // Act
            var result = await controller.IndexCategoryProducts("cat1");

            // Assert
            Assert.IsType<ForbidResult>(result.Result);
            _indexingJobServiceMock.Verify(x => x.Enqueue(It.IsAny<string>(), It.IsAny<IndexingOptions[]>()), Times.Never);
        }

        [Fact]
        public async Task IndexCategoryProducts_Success_EnqueuesProductIdsFromService()
        {
            // Arrange
            const string categoryId = "cat1";
            var productIds = new List<string> { "p1", "p2", "p3" };

            _categoryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync([new Category { Id = categoryId }]);

            SetupAuthorizationSuccess();

            _categoryProductResolverMock
                .Setup(x => x.GetCategoryProductIds(categoryId))
                .ReturnsAsync(productIds);

            _userNameResolverMock.Setup(x => x.GetCurrentUserName()).Returns("testuser");
            _indexingJobServiceMock
                .Setup(x => x.Enqueue(It.IsAny<string>(), It.IsAny<IndexingOptions[]>()))
                .Returns(new IndexProgressPushNotification("testuser"));

            var controller = CreateController();

            // Act
            var result = await controller.IndexCategoryProducts(categoryId);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
            _indexingJobServiceMock.Verify(x => x.Enqueue(
                "testuser",
                It.Is<IndexingOptions[]>(opts =>
                    opts.Length == 1 &&
                    opts[0].DocumentIds == productIds)),
                Times.Once);
            _pushNotifierMock.Verify(x => x.Send(It.IsAny<IndexProgressPushNotification>()), Times.Once);
        }

        [Fact]
        public async Task IndexCategoryProducts_EmptyCategory_EnqueuesEmptyList()
        {
            // Arrange
            const string categoryId = "cat-empty";

            _categoryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync([new Category { Id = categoryId }]);

            SetupAuthorizationSuccess();

            _categoryProductResolverMock
                .Setup(x => x.GetCategoryProductIds(categoryId))
                .ReturnsAsync([]);

            _userNameResolverMock.Setup(x => x.GetCurrentUserName()).Returns("testuser");
            _indexingJobServiceMock
                .Setup(x => x.Enqueue(It.IsAny<string>(), It.IsAny<IndexingOptions[]>()))
                .Returns(new IndexProgressPushNotification("testuser"));

            var controller = CreateController();

            // Act
            var result = await controller.IndexCategoryProducts(categoryId);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
            _indexingJobServiceMock.Verify(x => x.Enqueue(
                It.IsAny<string>(),
                It.Is<IndexingOptions[]>(opts => opts[0].DocumentIds.Count == 0)),
                Times.Once);
        }
    }
}
