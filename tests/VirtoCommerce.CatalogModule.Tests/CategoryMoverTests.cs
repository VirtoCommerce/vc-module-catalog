using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Services;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class CategoryMoverTests
    {
        private const string OwnerCatalogId = "ownerCatalog";
        private const string SourceCatalogId = "srcCatalog";
        private const string TargetCatalogId = "dstCatalog";
        private const string MovedCategoryId = "movedCategory";

        private readonly Mock<ICategoryService> _categoryServiceMock = new();
        private readonly Mock<ICategorySearchService> _categorySearchServiceMock = new();
        private readonly Mock<IProductSearchService> _productSearchServiceMock = new();
        private readonly Mock<IItemService> _itemServiceMock = new();
        private readonly List<CatalogProduct> _savedProducts = [];

        public CategoryMoverTests()
        {
            _itemServiceMock
                .Setup(x => x.SaveChangesAsync(It.IsAny<IList<CatalogProduct>>()))
                .Callback<IList<CatalogProduct>>(_savedProducts.AddRange)
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task ConfirmMoveAsync_CrossCatalog_LinkedProductKeepsOwnerCatalog_LinkFollowsCategory()
        {
            // Arrange
            var linkedProduct = new CatalogProduct
            {
                Id = "linkedProduct",
                CatalogId = OwnerCatalogId,
                CategoryId = "ownerCategory",
                Links =
                [
                    new CategoryLink { ListEntryId = "linkedProduct", CatalogId = SourceCatalogId, CategoryId = MovedCategoryId },
                    new CategoryLink { ListEntryId = "linkedProduct", CatalogId = "otherCatalog", CategoryId = "otherCategory" },
                ],
            };
            SetupProductSearch(linkedProduct);

            // Act
            await CreateMover().ConfirmMoveAsync([MovedCategory()]);

            // Assert
            linkedProduct.CatalogId.Should().Be(OwnerCatalogId);
            linkedProduct.Links.Single(x => x.CategoryId == MovedCategoryId).CatalogId.Should().Be(TargetCatalogId);
            linkedProduct.Links.Single(x => x.CategoryId == "otherCategory").CatalogId.Should().Be("otherCatalog");
            _savedProducts.Should().ContainSingle().Which.Should().BeSameAs(linkedProduct);
        }

        [Fact]
        public async Task ConfirmMoveAsync_CrossCatalog_OwnedProductMovesToTargetCatalog()
        {
            // Arrange
            var ownedProduct = new CatalogProduct
            {
                Id = "ownedProduct",
                CatalogId = SourceCatalogId,
                CategoryId = MovedCategoryId,
                Links = [],
            };
            SetupProductSearch(ownedProduct);

            // Act
            await CreateMover().ConfirmMoveAsync([MovedCategory()]);

            // Assert
            ownedProduct.CatalogId.Should().Be(TargetCatalogId);
            _savedProducts.Should().ContainSingle().Which.Should().BeSameAs(ownedProduct);
        }

        [Fact]
        public async Task ConfirmMoveAsync_NothingToChange_DoesNotSaveProducts()
        {
            // Arrange
            var product = new CatalogProduct
            {
                Id = "product",
                CatalogId = TargetCatalogId,
                CategoryId = MovedCategoryId,
                Links = [new CategoryLink { ListEntryId = "product", CatalogId = TargetCatalogId, CategoryId = MovedCategoryId }],
            };
            SetupProductSearch(product);

            // Act
            await CreateMover().ConfirmMoveAsync([MovedCategory()]);

            // Assert
            _itemServiceMock.Verify(x => x.SaveChangesAsync(It.IsAny<IList<CatalogProduct>>()), Times.Never);
        }

        private CategoryMover CreateMover()
        {
            return new CategoryMover(
                _categoryServiceMock.Object,
                _categorySearchServiceMock.Object,
                _productSearchServiceMock.Object,
                _itemServiceMock.Object);
        }

        private static Category MovedCategory()
        {
            // PrepareMoveAsync has already set the target catalog on the moved category.
            return new Category { Id = MovedCategoryId, CatalogId = TargetCatalogId };
        }

        private void SetupProductSearch(params CatalogProduct[] products)
        {
            _productSearchServiceMock
                .Setup(x => x.SearchAsync(It.IsAny<ProductSearchCriteria>(), It.IsAny<bool>()))
                .ReturnsAsync((ProductSearchCriteria criteria, bool _) => new ProductSearchResult
                {
                    TotalCount = products.Length,
                    Results = criteria.Skip == 0 ? products.ToList() : [],
                });
        }
    }
}
