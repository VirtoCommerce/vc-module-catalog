using System.Collections.Generic;
using System.Linq;
using Moq;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using Xunit;

namespace VirtoCommerce.CatalogModule.Test
{
    public class ReferencedAssociationsTest
    {
        private readonly IAssociationService _associationService;

        public ReferencedAssociationsTest()
        {
            var associations = GetTestAssociationEntities();

            var catalogRepository = new Mock<ICatalogRepository>();
            catalogRepository.Setup(r => r.Associations).Returns(associations);

            _associationService = new AssociationServiceImpl(() => catalogRepository.Object);
        }

        [Fact]
        public void GetCatalogItems_ByIds_WithReferencedAssociations_Test()
        {
            var owners = new CatalogProduct[]
            {
                new CatalogProduct { Id = "e7eee66223da43109502891b54bc33d3" }
            };

            _associationService.LoadReferencedAssociations(owners);

            var referencedAssociations = owners[0].ReferencedAssociations;

            Assert.NotNull(referencedAssociations);
            Assert.True(referencedAssociations.Count == 2);
        }

        private IQueryable<AssociationEntity> GetTestAssociationEntities()
        {
            var associations = new List<AssociationEntity>
            {
                new AssociationEntity
                {
                    ItemId = "9cbd8f316e254a679ba34a900fccb076",
                    AssociatedItemId = "bd8e58948c3648e8b08e1fb4ed4e01bb",
                    AssociationType = "Related Items",
                },
                new AssociationEntity
                {
                    ItemId = "b7fbcb4e4efb4b1bbe79482a20e80a3d",
                    AssociatedItemId = "e7eee66223da43109502891b54bc33d3",
                    AssociationType = "Related Items",
                    Quantity = 1
                },
                new AssociationEntity
                {
                    ItemId = "9cbd8f316e254a679ba34a900fccb076",
                    AssociatedItemId = "e7eee66223da43109502891b54bc33d3",
                    AssociationType = "Related Items",
                    Tags = "test1;test2"
                }
            };

            return associations.AsQueryable();
        }
    }
}