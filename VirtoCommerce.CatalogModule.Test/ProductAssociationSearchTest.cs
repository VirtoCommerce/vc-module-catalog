using Moq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using Xunit;

namespace VirtoCommerce.CatalogModule.Test
{
  public class ProductAssociationSearchTest
  {
    [Fact]
    public void GetProducts_Test()
    {
      var products = GetTestProductEntities();
      var catalogRepository = new Mock<ICatalogRepository>();
      catalogRepository.Setup(r => r.Items).Returns(products);

      var expectingResult = new List<ItemEntity>
              {
                  new ItemEntity
                  {
                      Id = "source1",
                      Name = "3DR Solo Quadcopter (No Gimbal)",
                      Associations = new ObservableCollection<AssociationEntity>(new List<AssociationEntity>
                      {
                          new AssociationEntity { ItemId = "source1", AssociatedItemId = "target1" },
                          new AssociationEntity { ItemId = "source1", AssociatedItemId = "target2" },
                          new AssociationEntity { ItemId = "source1", AssociatedCategoryId = "target3" },
                          new AssociationEntity { ItemId = "source1", AssociatedCategoryId = "target4" }
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
      Assert.Equal(expectingResult.AsQueryable(), products);
    }

    [Fact]
    public void GetProduct_AssociationCategories_Ids_Test()
    {
      var products = GetTestProductEntities();
      var associations = GetTestAssociationCategoriesIds();

      var associationCategoriesIds = products.SelectMany(x => x.Associations.Where(a => a.AssociationType == "category")).Select(x => x.AssociatedCategoryId).AsQueryable();

      Assert.Equal(associationCategoriesIds, associations);
    }

    private IQueryable<AssociationEntity> GetTestProductAssociationEntities()
    {
      var products = GetTestProductEntities();
      return products.SelectMany(x => x.Associations.Where(a => a.AssociationType == "category")).AsQueryable();
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
                          new AssociationEntity { ItemId = "source1", AssociatedItemId = "target2" },
                          new AssociationEntity { ItemId = "source1", AssociatedCategoryId = "target3", AssociationType = "category" },
                          new AssociationEntity { ItemId = "source1", AssociatedCategoryId = "target4", AssociationType = "category" }
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

    private IQueryable<string> GetTestAssociationCategoriesIds()
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
                    ItemId = "source1",
                    AssociatedCategoryId = "target3",
                    AssociationType = "category",
                    Quantity = 1
                },
                new AssociationEntity
                {
                    ItemId = "source1",
                    AssociatedCategoryId = "target4",
                    AssociationType = "category",
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
      return associations.Where(x => x.AssociationType == "category").Select(x => x.AssociatedCategoryId).AsQueryable();
    }
  }
}
