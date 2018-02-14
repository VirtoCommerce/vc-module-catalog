using Moq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Services;
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
    //   var criteria = GetTestProductAssociationCriteria(); 
    //   var retVal = _productAssociationSearchService.SearchProductAssociations(criteria);

    //   Assert(expectedResult, retVal.Results);
    //}

    private ProductAssociationSearchCriteria GetTestProductAssociationCriteria()
    {
      var criteria = new ProductAssociationSearchCriteria() {
        ObjectIds = new string[] { }.ToList(),
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
  }
}
