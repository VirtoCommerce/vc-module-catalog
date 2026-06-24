using System.Collections.Generic;
using System.Threading.Tasks;
using MockQueryable;
using Moq;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Services;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    [Trait("Category", "CI")]
    public class CategoryProductResolverTests
    {
        private readonly Mock<ICatalogRepository> _repositoryMock = new();

        private CategoryProductResolver CreateService()
        {
            return new CategoryProductResolver(() => _repositoryMock.Object);
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
            _repositoryMock.Setup(x => x.Items).Returns(items.BuildMock());
            _repositoryMock.Setup(x => x.CategoryItemRelations).Returns(relations.BuildMock());
            _repositoryMock.Setup(x => x.CategoryLinks).Returns((categoryLinks ?? []).BuildMock());
        }

        [Fact]
        public async Task GetCategoryProductIds_DirectProductsOnly_ExcludesVariations()
        {
            // Arrange
            const string categoryId = "cat1";
            var items = new List<ItemEntity>
            {
                new() { Id = "p1", CategoryId = categoryId, ParentId = null },
                new() { Id = "p2", CategoryId = categoryId, ParentId = null },
                new() { Id = "v1", CategoryId = categoryId, ParentId = "p1" }, // variation — must be excluded
            };

            SetupRepository(categoryId, childIds: [], items: items, relations: []);
            var service = CreateService();

            // Act
            var result = await service.GetCategoryProductIds(categoryId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains("p1", result);
            Assert.Contains("p2", result);
            Assert.DoesNotContain("v1", result);
        }

        [Fact]
        public async Task GetCategoryProductIds_LinkedProductsOnly_ExcludesVariations()
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

            SetupRepository(categoryId, childIds: [], items: items, relations: relations);
            var service = CreateService();

            // Act
            var result = await service.GetCategoryProductIds(categoryId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains("p1", result);
            Assert.Contains("p2", result);
            Assert.DoesNotContain("v1", result);
        }

        [Fact]
        public async Task GetCategoryProductIds_DirectAndLinked_DeduplicatesOverlappingIds()
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

            SetupRepository(categoryId, childIds: [], items: items, relations: relations);
            var service = CreateService();

            // Act
            var result = await service.GetCategoryProductIds(categoryId);

            // Assert
            Assert.Equal(2, result.Count); // p1 deduplicated, p2 included
            Assert.Contains("p1", result);
            Assert.Contains("p2", result);
        }

        [Fact]
        public async Task GetCategoryProductIds_WithSubcategories_IncludesProductsFromAllCategories()
        {
            // Arrange
            const string rootId = "root";
            const string childId = "child1";
            var items = new List<ItemEntity>
            {
                new() { Id = "p1", CategoryId = rootId, ParentId = null },
                new() { Id = "p2", CategoryId = childId, ParentId = null },
            };

            SetupRepository(rootId, childIds: [childId], items: items, relations: []);
            var service = CreateService();

            // Act
            var result = await service.GetCategoryProductIds(rootId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains("p1", result);
            Assert.Contains("p2", result);
        }

        [Fact]
        public async Task GetCategoryProductIds_EmptyCategory_ReturnsEmpty()
        {
            // Arrange
            const string categoryId = "cat-empty";

            SetupRepository(categoryId, childIds: [], items: [], relations: []);
            var service = CreateService();

            // Act
            var result = await service.GetCategoryProductIds(categoryId);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCategoryProductIds_VirtualCategory_FindsProductsFromTargetPhysicalCategory()
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
                new() { SourceCategoryId = physCatId, TargetCategoryId = virtCatId },
            };

            SetupRepository(virtCatId, childIds: [], items: items, relations: [], categoryLinks: categoryLinks);
            _repositoryMock
                .Setup(x => x.GetAllChildrenCategoriesIdsAsync(It.Is<IList<string>>(ids => ids.Contains(physCatId))))
                .ReturnsAsync([]);

            var service = CreateService();

            // Act
            var result = await service.GetCategoryProductIds(virtCatId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains("p1", result);
            Assert.Contains("p2", result);
        }

        [Fact]
        public async Task GetCategoryProductIds_VirtualCategoryWithChildren_IncludesSubcategoryProducts()
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
                new() { SourceCategoryId = physCatId, TargetCategoryId = virtCatId },
            };

            SetupRepository(virtCatId, childIds: [], items: items, relations: [], categoryLinks: categoryLinks);
            _repositoryMock
                .Setup(x => x.GetAllChildrenCategoriesIdsAsync(It.Is<IList<string>>(ids => ids.Contains(physCatId))))
                .ReturnsAsync([childCatId]);

            var service = CreateService();

            // Act
            var result = await service.GetCategoryProductIds(virtCatId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains("p1", result);
            Assert.Contains("p2", result);
        }

        [Fact]
        public async Task GetCategoryProductIds_ChainedVirtualCategories_FollowsMultipleHops()
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
                new() { SourceCategoryId = virtCatBId, TargetCategoryId = virtCatAId },
                new() { SourceCategoryId = physCatCId, TargetCategoryId = virtCatBId },
            };

            SetupRepository(virtCatAId, childIds: [], items: items, relations: [], categoryLinks: categoryLinks);
            _repositoryMock
                .Setup(x => x.GetAllChildrenCategoriesIdsAsync(It.Is<IList<string>>(ids => ids.Contains(virtCatBId))))
                .ReturnsAsync([]);
            _repositoryMock
                .Setup(x => x.GetAllChildrenCategoriesIdsAsync(It.Is<IList<string>>(ids => ids.Contains(physCatCId))))
                .ReturnsAsync([]);

            var service = CreateService();

            // Act
            var result = await service.GetCategoryProductIds(virtCatAId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains("p1", result);
            Assert.Contains("p2", result);
        }
    }
}
