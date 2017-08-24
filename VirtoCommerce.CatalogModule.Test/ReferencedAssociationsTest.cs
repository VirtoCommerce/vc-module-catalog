using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            var products = GetTestProductEntities();

            var catalogRepository = new Mock<ICatalogRepository>();
            catalogRepository.Setup(r => r.Items).Returns(products);
            catalogRepository.Setup(r => r.GetItemByIds(It.IsAny<string[]>(), It.IsAny<ItemResponseGroup>())).Returns((string[] ids, ItemResponseGroup responseGroup) =>
            {
                return products.Where(p => ids.Contains(p.Id)).ToArray();
            });
            catalogRepository.Setup(r => r.Associations).Returns(associations);

            _associationService = new AssociationServiceImpl(() => catalogRepository.Object);
        }

        [Fact]
        public void GetCatalogItems_ByIds_WithReferencedAssociations_Test()
        {
            // Each of association source items has an association with 'target1' item
            var associationSourceItems = new[]
            {
                new CatalogProduct { Id = "source1" },
                new CatalogProduct { Id = "source2" }
            };
        }

        private IQueryable<ItemEntity> GetTestProductEntities()
        {
            var products = new List<ItemEntity>
            {
                new ItemEntity
                {
                    Id = "source1",
                    Name = "3DR Solo Quadcopter (No Gimbal)",
                    Associations = new ObservableCollection<AssociationEntity>(new List<AssociationEntity>
                    {
                        new AssociationEntity { ItemId = "source1", AssociatedItemId = "target1" },
                        new AssociationEntity { ItemId = "source1", AssociatedItemId = "target2" }
                    })
                },
                new ItemEntity
                {
                    Id = "source2",
                    Name = "Parrot Jumping Sumo MiniDrone",
                    Associations = new ObservableCollection<AssociationEntity>(new List<AssociationEntity> {
                        new AssociationEntity { ItemId = "source2", AssociatedItemId = "target1" }
                    })
                },
                new ItemEntity
                {
                    Id = "target1",
                    Name = "3DR X8-M Octocopter for Visual-Spectrum Aerial Maps (915 MHz)"
                }
            };

            return products.AsQueryable();
        }

        private IQueryable<AssociationEntity> GetTestAssociationEntities()
        {
            var associations = new List<AssociationEntity>
            {
                new AssociationEntity
                {
                    ItemId = "source1",
                    AssociatedItemId = "target1",
                    AssociationType = "Related Items",
                },
                new AssociationEntity
                {
                    ItemId = "source1",
                    AssociatedItemId = "target2",
                    AssociationType = "Related Items",
                    Quantity = 1
                },
                new AssociationEntity
                {
                    ItemId = "source2",
                    AssociatedItemId = "target1",
                    AssociationType = "Related Items",
                    Tags = "test1;test2"
                }
            };

            return associations.AsQueryable();
        }
    }
}