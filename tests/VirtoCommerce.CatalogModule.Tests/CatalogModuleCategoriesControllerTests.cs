using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MockQueryable;
using Moq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
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
        private readonly Mock<ICatalogRepository> _repositoryMock = new();
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
                () => _repositoryMock.Object,
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
        public async Task IndexCategoryProducts_DirectProductsOnly_EnqueuesCorrectIds()
        {
            // Arrange
            const string categoryId = "cat1";
            var items = new List<ItemEntity>
            {
                new() { Id = "p1", CategoryId = categoryId, ParentId = null },
                new() { Id = "p2", CategoryId = categoryId, ParentId = null },
                new() { Id = "v1", CategoryId = categoryId, ParentId = "p1" }, // variation — must be excluded
            };

            SetupCategory(categoryId);
            SetupAuthorizationSuccess();
            SetupRepository(categoryId, childIds: [], items: items, relations: []);
            SetupEnqueue();

            var controller = CreateController();

            // Act
            var result = await controller.IndexCategoryProducts(categoryId);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
            var captured = CaptureEnqueuedIds();
            Assert.Equal(2, captured.Count);
            Assert.Contains("p1", captured);
            Assert.Contains("p2", captured);
            Assert.DoesNotContain("v1", captured);
        }

        [Fact]
        public async Task IndexCategoryProducts_LinkedProductsOnly_EnqueuesCorrectIds()
        {
            // Arrange
            const string categoryId = "cat1";
            var items = new List<ItemEntity>
            {
                new() { Id = "p1", CategoryId = "other-cat", ParentId = null },
                new() { Id = "p2", CategoryId = "other-cat", ParentId = null },
                new() { Id = "v1", CategoryId = "other-cat", ParentId = "p1" }, // variation linked to cat1 — must be excluded
            };
            var relations = new List<CategoryItemRelationEntity>
            {
                new() { CategoryId = categoryId, ItemId = "p1" },
                new() { CategoryId = categoryId, ItemId = "p2" },
                new() { CategoryId = categoryId, ItemId = "v1" }, // variation — must be excluded
            };

            SetupCategory(categoryId);
            SetupAuthorizationSuccess();
            SetupRepository(categoryId, childIds: [], items: items, relations: relations);
            SetupEnqueue();

            var controller = CreateController();

            // Act
            var result = await controller.IndexCategoryProducts(categoryId);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
            var captured = CaptureEnqueuedIds();
            Assert.Equal(2, captured.Count);
            Assert.Contains("p1", captured);
            Assert.Contains("p2", captured);
            Assert.DoesNotContain("v1", captured);
        }

        [Fact]
        public async Task IndexCategoryProducts_DirectAndLinked_DeduplicatesOverlappingIds()
        {
            // Arrange
            const string categoryId = "cat1";
            var items = new List<ItemEntity>
            {
                new() { Id = "p1", CategoryId = categoryId, ParentId = null }, // direct
                new() { Id = "p2", CategoryId = "other-cat", ParentId = null }, // linked only
            };
            var relations = new List<CategoryItemRelationEntity>
            {
                new() { CategoryId = categoryId, ItemId = "p1" }, // p1 is both direct AND linked
                new() { CategoryId = categoryId, ItemId = "p2" },
            };

            SetupCategory(categoryId);
            SetupAuthorizationSuccess();
            SetupRepository(categoryId, childIds: [], items: items, relations: relations);
            SetupEnqueue();

            var controller = CreateController();

            // Act
            var result = await controller.IndexCategoryProducts(categoryId);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
            var captured = CaptureEnqueuedIds();
            Assert.Equal(2, captured.Count); // p1 deduplicated, p2 included
            Assert.Contains("p1", captured);
            Assert.Contains("p2", captured);
        }

        [Fact]
        public async Task IndexCategoryProducts_WithSubcategories_IncludesProductsFromAllCategories()
        {
            // Arrange
            const string rootId = "root";
            const string childId = "child1";
            var items = new List<ItemEntity>
            {
                new() { Id = "p1", CategoryId = rootId, ParentId = null },
                new() { Id = "p2", CategoryId = childId, ParentId = null },
            };

            SetupCategory(rootId);
            SetupAuthorizationSuccess();
            SetupRepository(rootId, childIds: [childId], items: items, relations: []);
            SetupEnqueue();

            var controller = CreateController();

            // Act
            var result = await controller.IndexCategoryProducts(rootId);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
            var captured = CaptureEnqueuedIds();
            Assert.Equal(2, captured.Count);
            Assert.Contains("p1", captured);
            Assert.Contains("p2", captured);
        }

        [Fact]
        public async Task IndexCategoryProducts_EmptyCategory_EnqueuesEmptyList()
        {
            // Arrange
            const string categoryId = "cat-empty";

            SetupCategory(categoryId);
            SetupAuthorizationSuccess();
            SetupRepository(categoryId, childIds: [], items: [], relations: []);
            SetupEnqueue();

            var controller = CreateController();

            // Act
            var result = await controller.IndexCategoryProducts(categoryId);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
            var captured = CaptureEnqueuedIds();
            Assert.Empty(captured);
        }

        [Fact]
        public async Task IndexCategoryProducts_VirtualCategory_FindsProductsFromTargetPhysicalCategory()
        {
            // Arrange
            const string virtCatId = "virt-cat";
            const string physCatId = "phys-cat";

            var items = new List<ItemEntity>
            {
                new() { Id = "p1", CategoryId = physCatId, ParentId = null },
                new() { Id = "p2", CategoryId = physCatId, ParentId = null },
            };
            var categoryLinks = new List<CategoryRelationEntity>
            {
                new() { SourceCategoryId = virtCatId, TargetCategoryId = physCatId },
            };

            SetupCategory(virtCatId);
            SetupAuthorizationSuccess();
            SetupRepository(virtCatId, childIds: [], items: items, relations: [], categoryLinks: categoryLinks);

            _repositoryMock
                .Setup(x => x.GetAllChildrenCategoriesIdsAsync(It.Is<IList<string>>(ids => ids.Contains(physCatId))))
                .ReturnsAsync([]);

            SetupEnqueue();

            var controller = CreateController();

            // Act
            var result = await controller.IndexCategoryProducts(virtCatId);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
            var captured = CaptureEnqueuedIds();
            Assert.Equal(2, captured.Count);
            Assert.Contains("p1", captured);
            Assert.Contains("p2", captured);
        }

        [Fact]
        public async Task IndexCategoryProducts_VirtualCategoryWithChildren_IncludesSubcategoryProducts()
        {
            // Arrange
            const string virtCatId = "virt-cat";
            const string physCatId = "phys-cat";
            const string childCatId = "child-cat";

            var items = new List<ItemEntity>
            {
                new() { Id = "p1", CategoryId = physCatId, ParentId = null },
                new() { Id = "p2", CategoryId = childCatId, ParentId = null },
            };
            var categoryLinks = new List<CategoryRelationEntity>
            {
                new() { SourceCategoryId = virtCatId, TargetCategoryId = physCatId },
            };

            SetupCategory(virtCatId);
            SetupAuthorizationSuccess();
            SetupRepository(virtCatId, childIds: [], items: items, relations: [], categoryLinks: categoryLinks);

            _repositoryMock
                .Setup(x => x.GetAllChildrenCategoriesIdsAsync(It.Is<IList<string>>(ids => ids.Contains(physCatId))))
                .ReturnsAsync([childCatId]);

            SetupEnqueue();

            var controller = CreateController();

            // Act
            var result = await controller.IndexCategoryProducts(virtCatId);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
            var captured = CaptureEnqueuedIds();
            Assert.Equal(2, captured.Count);
            Assert.Contains("p1", captured);
            Assert.Contains("p2", captured);
        }

        [Fact]
        public async Task IndexCategoryProducts_ChainedVirtualCategories_FollowsMultipleHops()
        {
            // Arrange
            const string virtCatAId = "virt-cat-a";
            const string virtCatBId = "virt-cat-b";
            const string physCatCId = "phys-cat-c";

            var items = new List<ItemEntity>
            {
                new() { Id = "p1", CategoryId = physCatCId, ParentId = null },
                new() { Id = "p2", CategoryId = physCatCId, ParentId = null },
            };
            var categoryLinks = new List<CategoryRelationEntity>
            {
                new() { SourceCategoryId = virtCatAId, TargetCategoryId = virtCatBId },
                new() { SourceCategoryId = virtCatBId, TargetCategoryId = physCatCId },
            };

            SetupCategory(virtCatAId);
            SetupAuthorizationSuccess();
            SetupRepository(virtCatAId, childIds: [], items: items, relations: [], categoryLinks: categoryLinks);

            _repositoryMock
                .Setup(x => x.GetAllChildrenCategoriesIdsAsync(It.Is<IList<string>>(ids => ids.Contains(virtCatBId))))
                .ReturnsAsync([]);
            _repositoryMock
                .Setup(x => x.GetAllChildrenCategoriesIdsAsync(It.Is<IList<string>>(ids => ids.Contains(physCatCId))))
                .ReturnsAsync([]);

            SetupEnqueue();

            var controller = CreateController();

            // Act
            var result = await controller.IndexCategoryProducts(virtCatAId);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
            var captured = CaptureEnqueuedIds();
            Assert.Equal(2, captured.Count);
            Assert.Contains("p1", captured);
            Assert.Contains("p2", captured);
        }

        private void SetupCategory(string id)
        {
            _categoryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync([new Category { Id = id }]);
        }

        private void SetupRepository(
            string categoryId,
            IList<string> childIds,
            IList<ItemEntity> items,
            IList<CategoryItemRelationEntity> relations,
            IList<CategoryRelationEntity> categoryLinks = null)
        {
            _repositoryMock
                .Setup(x => x.GetAllChildrenCategoriesIdsAsync(It.Is<IList<string>>(ids => ids.Contains(categoryId))))
                .ReturnsAsync(childIds);

            _repositoryMock
                .Setup(x => x.Items)
                .Returns(items.BuildMock());

            _repositoryMock
                .Setup(x => x.CategoryItemRelations)
                .Returns(relations.BuildMock());

            _repositoryMock
                .Setup(x => x.CategoryLinks)
                .Returns((categoryLinks ?? []).BuildMock());
        }

        private void SetupEnqueue()
        {
            _userNameResolverMock.Setup(x => x.GetCurrentUserName()).Returns("testuser");

            _indexingJobServiceMock
                .Setup(x => x.Enqueue(It.IsAny<string>(), It.IsAny<IndexingOptions[]>()))
                .Returns(new IndexProgressPushNotification("testuser"));
        }

        private List<string> CaptureEnqueuedIds()
        {
            var call = _indexingJobServiceMock.Invocations
                .FirstOrDefault(i => i.Method.Name == nameof(IIndexingJobService.Enqueue));

            if (call == null)
            {
                return [];
            }

            var options = (IndexingOptions[])call.Arguments[1];
            return options[0].DocumentIds?.ToList() ?? [];
        }
    }
}
