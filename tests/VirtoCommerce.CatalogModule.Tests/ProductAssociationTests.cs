using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.Moq;
using Moq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.Platform.Core.Domain;
using Xunit;
using dataModel = VirtoCommerce.CatalogModule.Data.Model;

namespace VirtoCommerce.CatalogModule.Tests
{
    [Trait("Category", "CI")]
    public class ProductAssociationTests
    {
        Mock<ICatalogRepository> _catalogRepositoryMock = new Mock<ICatalogRepository>();

        public static object[][] UpdateTestData => new object[][]
        {
            new object[]
            {
                new dataModel.AssociationEntity()
                {
                    Id = "originalId",
                    ItemId = "originalItemId",
                    AssociatedItemId = "originalAssociatedId",
                    AssociatedCategoryId = null,
                    AssociationType = "product",
                    Priority = 0
                },
            },
        };

        [Theory]
        [MemberData(nameof(UpdateTestData))]
        public async Task UpdateAssociationsAsync_UpdateAssociation_Changed(dataModel.AssociationEntity entity)
        {

            // Arrange
            var associationServiceMock = CreateProductAssociationServiceMock(new[] { entity });

            var productAssociation = new ProductAssociation()
            {
                Id = entity.Id,
                ItemId = "new_Item_ID",
                AssociatedObjectId = "new_object_Id",
                AssociatedObjectType = entity.AssociationType,
            };
            // Act

            await associationServiceMock.UpdateAssociationsAsync(new[] { productAssociation });
            // Assert
            Assert.Equal(productAssociation.ItemId, entity.ItemId);
            _catalogRepositoryMock.Verify(x => x.Add(It.IsAny<dataModel.AssociationEntity>()), Times.Never);
        }

        [Theory]
        [MemberData(nameof(UpdateTestData))]
        public async Task UpdateAssociationsAsync_AddAssociation_Added(dataModel.AssociationEntity entity)
        {
            // Arrange
            var associationServiceMock = CreateProductAssociationServiceMock(new[] { entity });

            var productAssociation = new ProductAssociation()
            {
                Id = null,
                ItemId = entity.ItemId,
                AssociatedObjectId = "NEW_AssociatedItemId",
                AssociatedObjectType = entity.AssociationType,
                Type = entity.AssociationType,
                Priority = 10
            };
            // Act
            await associationServiceMock.UpdateAssociationsAsync(new[] { productAssociation });

            // Assert
            _catalogRepositoryMock.Verify(x => x.Add(It.Is<dataModel.AssociationEntity>(q => q.AssociatedItemId == "NEW_AssociatedItemId")), Times.Once);
        }

        [Theory]
        [MemberData(nameof(UpdateTestData))]
        public async Task SaveChangesAsync_UpdateExisting_Changed(dataModel.AssociationEntity association)
        {

            var associationServiceMock = CreateProductAssociationServiceMock(new List<dataModel.AssociationEntity>() { association });

            var product = new CatalogProduct()
            {
                Id = "originalItemId",
                Name = "Owner object",
                Associations = new List<ProductAssociation>()
                {
                    new ProductAssociation()
                    {
                        Id = "originalId",
                        ItemId = "originalItemId",
                        AssociatedObjectId = association.AssociatedItemId,
                        AssociatedObjectType = association.AssociationType,
                        Type = association.AssociationType,

                        Priority = 66
                    }
                }
            };
            // Act
            await associationServiceMock.SaveChangesAsync(new IHasAssociations[] { product });
            // Assert
            Assert.Equal(association.Priority, product.Associations.First().Priority);
            _catalogRepositoryMock.Verify(x => x.Add(It.IsAny<dataModel.AssociationEntity>()), Times.Never);
        }

        [Theory]
        [MemberData(nameof(UpdateTestData))]
        public async Task SaveChangesAsync_UpdateExistingTransientAssociation_Changed(dataModel.AssociationEntity association)
        {

            var associationServiceMock = CreateProductAssociationServiceMock(new List<dataModel.AssociationEntity>() { association });

            var product = new CatalogProduct()
            {
                Id = "originalItemId",
                Name = "Owner object",
                Associations = new List<ProductAssociation>()
                {
                    new ProductAssociation()
                    {
                        Id = null,
                        ItemId = "originalItemId",
                        AssociatedObjectId = association.AssociatedItemId,
                        AssociatedObjectType = association.AssociationType,
                        Type = association.AssociationType,
                        Priority = 66
                    }
                }
            };
            // Act
            await associationServiceMock.SaveChangesAsync(new IHasAssociations[] { product });
            // Assert
            Assert.Equal(association.Priority, product.Associations.First().Priority);
            _catalogRepositoryMock.Verify(x => x.Add(It.IsAny<dataModel.AssociationEntity>()), Times.Never);
        }

        [Theory]
        [MemberData(nameof(UpdateTestData))]
        public async Task SaveChangesAsync_AddNotExistingTransientAssociation_Added(dataModel.AssociationEntity association)
        {

            var associationServiceMock = CreateProductAssociationServiceMock(new List<dataModel.AssociationEntity>() { association });

            var product = new CatalogProduct()
            {
                Id = "originalItemId",
                Name = "Owner object",
                Associations = new List<ProductAssociation>()
                {
                    new ProductAssociation()
                    {
                        Id = null,
                        ItemId = "originalItemId",
                        AssociatedObjectId = "NEW_AssociatedObject_Id",
                        AssociatedObjectType = association.AssociationType,
                        Type = association.AssociationType,
                        Priority = 66
                    }
                }
            };
            // Act
            await associationServiceMock.SaveChangesAsync(new IHasAssociations[] { product });
            // Assert
            _catalogRepositoryMock.Verify(x => x.Add(It.Is<dataModel.AssociationEntity>(q => q.AssociatedItemId == "NEW_AssociatedObject_Id")), Times.Once);
        }

        [Theory]
        [MemberData(nameof(UpdateTestData))]
        public async Task SaveChangesAsync_SetEmptyAssociationCollections_ExistingDeleted(dataModel.AssociationEntity association)
        {
            // Arrange
            var associationServiceMock = CreateProductAssociationServiceMock(new List<dataModel.AssociationEntity>() { association });

            var product = new CatalogProduct()
            {
                Id = "originalItemId",
                Name = "Owner object",
                Associations = new List<ProductAssociation>() { }

            };
            // Act
            await associationServiceMock.SaveChangesAsync(new IHasAssociations[] { product });
            // Assert
            _catalogRepositoryMock.Verify(x => x.Remove(It.Is<dataModel.AssociationEntity>(q => q.Id == "originalId")), Times.Once);
        }

        public static object[][] EqualEntities => new object[][]
        {
            new object[]
            {
                new dataModel.AssociationEntity()
                {
                    Id = "originalId",
                    ItemId = "originalItemId",
                    AssociatedItemId = "originalAssociatedId",
                    AssociatedCategoryId = null,
                    AssociationType = "product",
                    Priority = 0
                },
                new dataModel.AssociationEntity()
                {
                    Id = "originalId",
                    ItemId = "originalItemId",
                    AssociatedItemId = "originalAssociatedId",
                    AssociatedCategoryId = null,
                    AssociationType = "product",
                    Priority = 0
                },
            },
            new object[]
            {
                new dataModel.AssociationEntity()
                {
                    Id = "originalId",
                    ItemId = "originalItemId",
                    AssociatedItemId = "originalAssociatedId",
                    AssociatedCategoryId = null,
                    AssociationType = "product",
                    Priority = 0
                },
                new dataModel.AssociationEntity()
                {
                    Id = null,
                    ItemId = "originalItemId",
                    AssociatedItemId = "originalAssociatedId",
                    AssociatedCategoryId = null,
                    AssociationType = "product",
                    Priority = 0
                },

            },
            new object[]
            {
                new dataModel.AssociationEntity()
                {
                    Id = "originalId",
                    ItemId = "originalItemId",
                    AssociatedItemId = "originalAssociatedId",
                    AssociatedCategoryId = null,
                    AssociationType = "product",
                    Priority = 0
                },
                new dataModel.AssociationEntity()
                {
                    Id = "originalId",
                    ItemId = "originalItemId_1",
                    AssociatedItemId = "originalAssociatedId_1",
                    AssociatedCategoryId = null,
                    AssociationType = "product",
                    Priority = 0
                },

            },


        };

        [Theory]
        [MemberData(nameof(EqualEntities))]
        public void CompareEqualAssociation_Equal(dataModel.AssociationEntity x, dataModel.AssociationEntity y)
        {
            var result = new dataModel.AssociationEntityComparer().Equals(x, y);
            Assert.True(result);
        }

        public static object[][] NotEqualEntities => new[]
        {
            new object[]
            {
                new dataModel.AssociationEntity()
                {
                    Id = "originalId", ItemId = "originalItemId", AssociatedItemId = "originalAssociatedId", AssociationType = "product",
                },
                new dataModel.AssociationEntity()
                {
                    Id = "originalId_1", ItemId = "originalItemId", AssociatedItemId = "originalAssociatedId", AssociationType = "product",
                },
            },
            new object[]
            {
                new dataModel.AssociationEntity()
                {
                    Id = "originalId", ItemId = "originalItemId", AssociatedItemId = "originalAssociatedId", AssociationType = "product",
                },
                new dataModel.AssociationEntity()
                {
                    Id = null, ItemId = "originalItemId_1", AssociatedItemId = "originalAssociatedId", AssociatedCategoryId = null, AssociationType = "product",
                },

            },
            new object[]
            {
                new dataModel.AssociationEntity()
                {
                    Id = "originalId", ItemId = "originalItemId", AssociatedItemId = "originalAssociatedId", AssociationType = "product",

                },
                new dataModel.AssociationEntity()
                {
                    Id = "originalId_1", ItemId = "originalItemId_1", AssociatedItemId = "originalAssociatedId_1", AssociationType = "product",
                },
            },


        };

        [Theory]
        [MemberData(nameof(NotEqualEntities))]
        public void Compare_NotEqualAssociation_NotEqual(dataModel.AssociationEntity x, dataModel.AssociationEntity y)
        {

            var result = new dataModel.AssociationEntityComparer().Equals(x, y);

            Assert.False(result);
        }

        private AssociationService CreateProductAssociationServiceMock(IEnumerable<dataModel.AssociationEntity> entities)
        {
            var catalogRepositoryFactory = CreateRepositoryMock(entities);
            var productAssociationsService = new AssociationService(catalogRepositoryFactory);
            return productAssociationsService;
        }

        private Func<ICatalogRepository> CreateRepositoryMock(IEnumerable<dataModel.AssociationEntity> entities)
        {
            var entitiesMock = entities.AsQueryable().BuildMock();

            _catalogRepositoryMock.SetupGet(x => x.UnitOfWork).Returns(new Mock<IUnitOfWork>().Object);

            _catalogRepositoryMock.Setup(x => x.Associations)
                .Returns(entitiesMock.Object);
            _catalogRepositoryMock.Setup(x => x.GetAssociationsByIdsAsync(It.IsAny<string[]>()))
                .Returns<string[]>(ids =>
                    Task.FromResult(entities.Where(x => ids.Contains(x.Id)).ToArray()));

            ICatalogRepository func() => _catalogRepositoryMock.Object;

            return func;
        }
    }
}
