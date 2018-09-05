using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.Domain.Catalog.Model.Search;
using VirtoCommerce.Domain.Catalog.Services;
using Xunit;
using dataModel = VirtoCommerce.CatalogModule.Data.Model;
using domainModel = VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogModule.Test
{
    [Trait("Category", "CI")]
    public class ProductAssociationSearchServiceTests
    {
        //[Fact]
        //public void SearchAssociatedProducts_ReturnsProducts()
        //{
        //    // Arrange
        //    var verifySet = new[] { "prod-1", "prod-2", "prod-3", "prod-4" }.Select(x => new domainModel.ProductAssociation { AssociatedObjectId = x }).ToArray();
        //    var criteria = new ProductAssociationSearchCriteria
        //    {
        //        ObjectIds = new[] { "prod-1" },
        //        ResponseGroup = domainModel.ItemResponseGroup.ItemInfo.ToString(),
        //        Sort = "Id"
        //    };
        //    var catalogRepository = new Mock<ICatalogRepository>();
        //    catalogRepository.Setup(x => x.Associations).Returns(TestAssociationEntities.AsQueryable());
        //    catalogRepository.Setup(x => x.Items).Returns(TestItemEntities.AsQueryable());
        //    catalogRepository.Setup(x => x.GetAllChildrenCategoriesIds(It.Is<string[]>(ids => ids.SequenceEqual(new[] { "cat-1" })))).Returns(TestChildCategories);

        //    var itemService = new Mock<IItemService>();
        //    itemService.Setup(x => x.GetByIds(It.Is<string[]>(ids => ids.SequenceEqual(verifySet.Select(p => p.Id))), It.IsIn(domainModel.ItemResponseGroup.ItemInfo), It.IsAny<string>()))
        //               .Returns(verifySet);
        //    var sut = new ProductAssociationSearchService(() => catalogRepository.Object, itemService.Object);
        //    // Act
        //    var result = sut.SearchProductAssociations(criteria);
        //    // Assert
        //    Assert.True(result.TotalCount == 4);
        //    Assert.Equal(result.Results, verifySet);
        //}

        //[Fact]
        //public void GetCriteria_InvalidArguments_ThrowsArgumentNullException()
        //{
        //    var criteria = new ProductAssociationSearchCriteria()
        //    {
        //        ObjectIds = new string[] { }.ToList()
        //    };
        //    var catalogRepository = new Mock<ICatalogRepository>();
        //    var itemService = new Mock<IItemService>();
        //    var sut = new ProductAssociationSearchService(() => catalogRepository.Object, itemService.Object);
        //    Assert.Throws<ArgumentNullException>(() => sut.SearchProductAssociations(criteria));
        //}

        private static string[] TestChildCategories
        {
            get
            {
                return new string[] { "cat-1", "cat-1-2", "cat-1-2-3" };
            }
        }
        private static IEnumerable<dataModel.AssociationEntity> TestAssociationEntities
        {
            get
            {
                yield return new dataModel.AssociationEntity { Id = "assoc-1", ItemId = "prod-1", AssociatedCategoryId = "cat-1" };
                yield return new dataModel.AssociationEntity { Id = "assoc-2", ItemId = "prod-1", AssociatedItemId = "prod-4" };
            }
        }
        private static IEnumerable<dataModel.ItemEntity> TestItemEntities
        {
            get
            {
                yield return new dataModel.ItemEntity { Id = "prod-1", CategoryId = "cat-1" };
                yield return new dataModel.ItemEntity { Id = "prod-2", CategoryId = "cat-1-2" };
                yield return new dataModel.ItemEntity { Id = "prod-3", CategoryId = "cat-1-2-3" };
                yield return new dataModel.ItemEntity { Id = "prod-4", CategoryId = "cat-2" };
                yield return new dataModel.ItemEntity { Id = "prod-5", CategoryId = "cat-3" };
            }
        }
    }
}
