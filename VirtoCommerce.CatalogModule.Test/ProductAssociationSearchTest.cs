using Moq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Model.Search;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Common;
using Xunit;

namespace VirtoCommerce.CatalogModule.Test
{
  public class ProductAssociationSearchTest
  {
    private readonly IProductAssociationSearchService _productAssociationSearchService;

    public ProductAssociationSearchTest()
    {
      var catalogRepository = new Mock<ICatalogRepository>();
      var itemService = new Mock<IItemService>();
      _productAssociationSearchService = new ProductAssociationSearchService(() => catalogRepository.Object, itemService.Object);
    }

    //[Fact]
    //public void SearchProductAssociations_Test()
    //{
    //  var criteria = GetTestProductAssociationCriteria();
    //  var retVal = _productAssociationSearchService.SearchProductAssociations(criteria);

    //  var products = GetTestProductEntities();
    //  var catalogProducts = GetTestData().OfType<CatalogProduct>();

    //  Assert(expectedResult, retVal.Results);
    //}

    private ProductAssociationSearchCriteria GetTestProductAssociationCriteria()
    {
      var criteria = new ProductAssociationSearchCriteria() {
        ObjectIds = new string[] {  }.ToList(), //"source1", "source2", "target1", "target2"
        ResponseGroup = "None",
        Skip = 2,
        Take = 2
      };
      return criteria;
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
                      }),
                      IsActive = true,
                      IsBuyable = true
                  },
                  new ItemEntity
                  {
                      Id = "source2",
                      Name = "Parrot Jumping Sumo MiniDrone",
                      Associations = new ObservableCollection<AssociationEntity>(new List<AssociationEntity> {
                          new AssociationEntity { ItemId = "source2", AssociatedItemId = "target1" }
                      }),
                      IsActive = true,
                      IsBuyable = true
                  },
                  new ItemEntity
                  {
                      Id = "target1",
                      Name = "3DR X8-M Octocopter for Visual-Spectrum Aerial Maps 1 (915 MHz)",
                      IsActive = true,
                      IsBuyable = true
                  },
                  new ItemEntity
                  {
                      Id = "target2",
                      Name = "3DR X8-M Octocopter for Visual-Spectrum Aerial Maps 2 (915 MHz)",
                      IsActive = true,
                      IsBuyable = true
                  },
                  new ItemEntity
                  {
                      Id = "target5",
                      Name = "3DR X8-M Octocopter for Visual-Spectrum Aerial Maps 3 (915 MHz)",
                      CategoryId = "target3",
                      IsActive = true,
                      IsBuyable = true
                  },
                  new ItemEntity
                  {
                      Id = "target6",
                      Name = "3DR X8-M Octocopter for Visual-Spectrum Aerial  4 (915 MHz)",
                      CategoryId = "target4",
                      IsActive = true,
                      IsBuyable = true
                  }
              };
      return products.AsQueryable();
    }

    private static IEnumerable<IHasOutlines> GetTestData()
    {
      var c = new Catalog { Id = "c" };
      var v = new Catalog { Id = "v", IsVirtual = true };
      var c0 = new Category { CatalogId = c.Id, Catalog = c, Id = "c0" };
      var c1 = new Category { CatalogId = c.Id, Catalog = c, Id = "c1" };
      var c2 = new Category { CatalogId = c.Id, Catalog = c, Id = "c2", ParentId = c1.Id, Parents = new[] { c1 } };
      var c3 = new Category { CatalogId = c.Id, Catalog = c, Id = "c3", ParentId = c2.Id, Parents = new[] { c1, c2 } };
      var v1 = new Category { CatalogId = v.Id, Catalog = v, Id = "v1", IsVirtual = true };
      var v2 = new Category { CatalogId = v.Id, Catalog = v, Id = "v2", IsVirtual = true, ParentId = v1.Id, Parents = new[] { v1 } };
      var p01 = new CatalogProduct { CatalogId = c.Id, Id = "p0", Catalog = c };
      var p02 = new CatalogProduct { CatalogId = c.Id, Id = "p0", Catalog = c };
      var p03 = new CatalogProduct { CatalogId = c.Id, Id = "p0", Catalog = c, Associations = new List<ProductAssociation>
      { new ProductAssociation {AssociatedObjectId = "c3", AssociatedObjectType = "category"},
        new ProductAssociation {AssociatedObjectId = "p11", AssociatedObjectType = "product"} }
      };
      var p11 = new CatalogProduct { CatalogId = c.Id, Id = "p11", Catalog = c, CategoryId = c3.Id, Category = c3 };
      var p12 = new CatalogProduct { CatalogId = c.Id, Id = "p12", Catalog = c, CategoryId = c3.Id, Category = c3 };
      var p13 = new CatalogProduct { CatalogId = v.Id, Id = "p13", Catalog = v };

      c1.Links = new[] { new CategoryLink { CatalogId = v.Id, Catalog = v, CategoryId = v2.Id, Category = v2 } };
      c2.Links = new[] { new CategoryLink { CatalogId = v.Id, Catalog = v, CategoryId = v2.Id, Category = v2 } };
      c3.Links = new[] { new CategoryLink { CatalogId = v.Id, Catalog = v },
                         new CategoryLink { CatalogId = v.Id, Catalog = v, CategoryId = v2.Id, Category = v2 },
                       };
      p11.Links = new[] { new CategoryLink { CatalogId = v.Id, Catalog = v },
                         new CategoryLink { CatalogId = v.Id, Catalog = v, CategoryId = v2.Id, Category = v2 },
                       };

      return new IHasOutlines[] { c0, c1, c2, c3, v1, v2, p01, p02, p03, p11, p12, p13 };
    }
  }
}
