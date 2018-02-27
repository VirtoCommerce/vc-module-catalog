using Moq;
using System;
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

      var products = GetTestProductEntities();
      var associations = GetTestAssociationEntities();
      var categories = GetTestCategories();

      catalogRepository.Setup(r => r.Items).Returns(products);
      catalogRepository.Setup(r => r.Associations).Returns(associations);
      catalogRepository.Setup(r => r.Categories).Returns(categories);
      //itemService.Setup(x => x.GetByIds(It.IsAny<string[]>(), ItemResponseGroup.ItemInfo, null)).Returns((CatalogProduct[] prod) =>
      //{
      //  return GetTestData().OfType<CatalogProduct>().Where(p => prod.Select(x => x.Id).Contains(p.Id)).ToArray();
      //});

      _productAssociationSearchService = new ProductAssociationSearchService(() => catalogRepository.Object, itemService.Object);
    }

    [Fact]
    public void GetCriteria_InvalidArguments_ThrowsArgumentNullException()
    {
      var criteria = GetTestProductAssociationCriteria_Empty();
      Assert.Throws<ArgumentNullException>(() => _productAssociationSearchService.SearchProductAssociations(criteria));
    }

    [Fact]
    public void SearchProductAssociations_Test()
    {
      var catalogRepository = new Mock<ICatalogRepository>();
      var itemService = new Mock<IItemService>();

      var criteria = new ProductAssociationSearchCriteria()
      {
        ObjectIds = new string[] { "p01", "p02" }.ToList(),
        ResponseGroup = "ItemInfo",
        Skip = 0,
        Take = 10
      };

      var products = GetTestProductEntities();
      var associations = GetTestAssociationEntities();
      var categories = GetTestCategories();

      catalogRepository.Setup(r => r.Items).Returns(products);
      catalogRepository.Setup(r => r.Associations).Returns(associations);
      catalogRepository.Setup(r => r.Categories).Returns(categories);

      var assoc = new List<AssociationEntity>();
      for (var i = 0; i < criteria.ObjectIds.ToArray().Length; i++)
        assoc.AddRange(catalogRepository.Object.Associations.Where(x => x.ItemId == criteria.ObjectIds.ToArray()[i]));

      var associationCategoriesIds = assoc
        .Where(x => x.AssociatedCategoryId != null)
        .Select(x => x.AssociatedCategoryId).ToArray();

      associationCategoriesIds = catalogRepository.Object.GetAllChildrenCategoriesIds(associationCategoriesIds).Concat(associationCategoriesIds).Distinct().ToArray();

      var itemsQuery = catalogRepository.Object.Items.Join(catalogRepository.Object.Associations, item => item.Id, association => association.AssociatedItemId, (item, association) => item)
                                           .Union(catalogRepository.Object.Items.Where(x => associationCategoriesIds.Contains(x.CategoryId)));
      var sortInfos = criteria.SortInfos;
      if (sortInfos.IsNullOrEmpty())
      {
        sortInfos = new[] { new SortInfo { SortColumn = "CreatedDate", SortDirection = SortDirection.Descending } };
      }
      itemsQuery = itemsQuery.OrderBySortInfos(sortInfos);

      var itemIds = itemsQuery.Skip(criteria.Skip)
                              .Take(criteria.Take)
                              .Select(x => x.Id).ToList();

      var provResult = GetByIds(itemIds);

      itemService.Setup(x => x.GetByIds(itemIds.ToArray(), EnumUtility.SafeParse(criteria.ResponseGroup, ItemResponseGroup.ItemInfo), null)).Returns(provResult);
      var expectedResult = itemService.Object.GetByIds(itemIds.ToArray(), EnumUtility.SafeParse(criteria.ResponseGroup, ItemResponseGroup.ItemInfo))
                                             .OrderBy(x => itemIds.IndexOf(x.Id))
                                             .ToList();
      var retVal = _productAssociationSearchService.SearchProductAssociations(criteria);
      Assert.Equal(expectedResult, retVal.Results);
    }

    private ProductAssociationSearchCriteria GetTestProductAssociationCriteria_Empty()
    {
      var criteria = new ProductAssociationSearchCriteria() {
        ObjectIds = new string[] {  }.ToList(), 
        ResponseGroup = "ItemInfo",
        Skip = 0,
        Take = 10
      };
      return criteria;
    }

    private CatalogProduct[] GetByIds(List<string> ids)
    {
      var catalog = GetTestData().OfType<CatalogProduct>();
      var result = new List<CatalogProduct>();
      foreach (var id in ids)
        result.Add(catalog.Single(x => x.Id == id));
      return result.ToArray();
    }

    private ProductAssociationSearchCriteria GetTestProductAssociationCriteria()
    {
      var criteria = new ProductAssociationSearchCriteria()
      {
        ObjectIds = new string[] { "source1" }.ToList(),
        ResponseGroup = "ItemInfo",
        Skip = 0,
        Take = 10
      };
      return criteria;
    }

    private static IEnumerable<IHasOutlines> GetTestData()
    {
      var c = new Catalog { Id = "c" };
      var v = new Catalog { Id = "v", IsVirtual = true };
      var c0 = new Category { CatalogId = c.Id, Catalog = c, Id = "c0" };
      var c1 = new Category { CatalogId = c.Id, Catalog = c, Id = "c1" };
      var c2 = new Category { CatalogId = c.Id, Catalog = c, Id = "c2", ParentId = c1.Id, Parents = new[] { c1 } };
      var c3 = new Category { CatalogId = c.Id, Catalog = c, Id = "c3", ParentId = c1.Id, Parents = new[] { c1 } };
      c1.Children = new List<Category> { c2, c3 };
      var v1 = new Category { CatalogId = v.Id, Catalog = v, Id = "v1", IsVirtual = true };
      var v2 = new Category { CatalogId = v.Id, Catalog = v, Id = "v2", IsVirtual = true, ParentId = v1.Id, Parents = new[] { v1 } };
      v1.Children = new List<Category> { v2 };
      var p01 = new CatalogProduct { CatalogId = c.Id, Id = "p01", Catalog = c, CategoryId = c1.Id, Category = c1, Name = "p01Name", IsActive = true, IsBuyable = true,
        Associations = new List<ProductAssociation>
                      {
                          new ProductAssociation { AssociatedObjectId = "p11", AssociatedObjectType = "product" },
                          new ProductAssociation { AssociatedObjectId = "p12", AssociatedObjectType = "product" },
                          new ProductAssociation { AssociatedObjectId = "c0", AssociatedObjectType = "category" },
                          new ProductAssociation { AssociatedObjectId = "c1", AssociatedObjectType = "category" }
                      }
      };
      var p02 = new CatalogProduct { CatalogId = c.Id, Id = "p02", Catalog = c, CategoryId = c2.Id, Category = c2, Name = "p02Name", IsActive = true, IsBuyable = true };
      var p03 = new CatalogProduct { CatalogId = c.Id, Id = "p03", Catalog = c, CategoryId = c3.Id, Category = c3, Name = "p03Name", IsActive = true, IsBuyable = true,
        Associations = new List<ProductAssociation>
                     { new ProductAssociation {AssociatedObjectId = "c3", AssociatedObjectType = "category"},
                       new ProductAssociation {AssociatedObjectId = "p13", AssociatedObjectType = "product"} }
                     };
      var p11 = new CatalogProduct { CatalogId = c.Id, Id = "p11", Catalog = c, CategoryId = c3.Id, Category = c3, Name = "p11Name", IsActive = true, IsBuyable = true };
      var p12 = new CatalogProduct { CatalogId = c.Id, Id = "p12", Catalog = c, CategoryId = c3.Id, Category = c3, Name = "p12Name", IsActive = true, IsBuyable = true };
      var p13 = new CatalogProduct { CatalogId = v.Id, Id = "p13", Catalog = v, CategoryId = v2.Id, Category = v2, Name = "p13Name", IsActive = true, IsBuyable = true };

      c1.Links = new[] { new CategoryLink { CatalogId = v.Id, Catalog = v, CategoryId = v2.Id, Category = v2 } };
      c2.Links = new[] { new CategoryLink { CatalogId = v.Id, Catalog = v, CategoryId = v2.Id, Category = v2 } };
      c3.Links = new[] { new CategoryLink { CatalogId = v.Id, Catalog = v },
                         new CategoryLink { CatalogId = v.Id, Catalog = v, CategoryId = v2.Id, Category = v2 } };

      return new IHasOutlines[] { c0, c1, c2, c3, v1, v2, p01, p02, p03, p11, p12, p13 };
    }

    private IQueryable<CategoryEntity> GetTestCategories()
    {
      var categories = new List<CategoryEntity>
              {
                  new CategoryEntity
                  {
                    Id = "c0",
                    CatalogId = "c"
                  },
                  new CategoryEntity
                  {
                    Id = "c1",
                    CatalogId = "c", 
                    OutgoingLinks = new ObservableCollection<CategoryRelationEntity> {
                      new CategoryRelationEntity { TargetCatalogId = "v", TargetCategoryId = "v2" } }
                  },
                  new CategoryEntity
                  {
                    Id = "c2",
                    CatalogId = "c", 
                    ParentCategoryId = "c1",
                    OutgoingLinks = new ObservableCollection<CategoryRelationEntity> {
                      new CategoryRelationEntity { TargetCatalogId = "v", TargetCategoryId = "v2" } }
                  },
                  new CategoryEntity
                  {
                    Id = "c3",
                    CatalogId = "c",
                    ParentCategoryId = "c1",
                    OutgoingLinks = new ObservableCollection<CategoryRelationEntity> {
                      new CategoryRelationEntity { TargetCatalogId = "v" },
                      new CategoryRelationEntity { TargetCatalogId = "v", TargetCategoryId = "v2" } }
                  },
                  new CategoryEntity
                  {
                    Id = "v1",
                    CatalogId = "v"
                  },
                  new CategoryEntity
                  {
                    Id = "v2",
                    CatalogId = "v",
                    ParentCategoryId = "v1"
                  }
      };
      return categories.AsQueryable();
    }

    private IQueryable<ItemEntity> GetTestProductEntities()
    {
      var products = new List<ItemEntity>
              {
                  new ItemEntity
                  {
                      Id = "p01",
                      Name = "p01Name",
                      Associations = new ObservableCollection<AssociationEntity>(new List<AssociationEntity>
                      {
                          new AssociationEntity { ItemId = "p01", AssociatedItemId = "p11", AssociationType = "product" },
                          new AssociationEntity { ItemId = "p01", AssociatedItemId = "p12", AssociationType = "product" },
                          new AssociationEntity { ItemId = "p01", AssociatedCategoryId = "c0", AssociationType = "category" },
                          new AssociationEntity { ItemId = "p01", AssociatedCategoryId = "c1", AssociationType = "category" }
                      }),
                      IsActive = true,
                      IsBuyable = true,
                      CatalogId = "c",
                      CategoryId = "c1"
                  },
                  new ItemEntity
                  {
                      Id = "p02",
                      Name = "p02Name",
                      IsActive = true,
                      IsBuyable = true,
                      CatalogId = "c",
                      CategoryId = "c2"
                  },
                  new ItemEntity
                  {
                      Id = "p03",
                      Name = "p03Name",
                      Associations = new ObservableCollection<AssociationEntity>(new List<AssociationEntity> {
                          new AssociationEntity { ItemId = "p03", AssociatedCategoryId = "c3", AssociationType = "category" },
                          new AssociationEntity { ItemId = "p03", AssociatedItemId = "p13", AssociationType = "product" }
                      }),
                      IsActive = true,
                      IsBuyable = true,
                      CatalogId = "c",
                      CategoryId = "c3"
                  },
                  new ItemEntity
                  {
                      Id = "p11",
                      Name = "p11Name",
                      IsActive = true,
                      IsBuyable = true,
                      CatalogId = "c",
                      CategoryId = "c3"
                  },
                  new ItemEntity
                  {
                      Id = "p12",
                      Name = "p12Name",
                      IsActive = true,
                      IsBuyable = true,
                      CatalogId = "c",
                      CategoryId = "c3"
                  },
                  new ItemEntity
                  {
                      Id = "p13",
                      Name = "p13Name",
                      IsActive = true,
                      IsBuyable = true,
                      CatalogId = "v",
                      CategoryId = "v2"
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
                    ItemId = "p01",
                    AssociatedItemId = "p11",
                    AssociationType = "product",
                },
                new AssociationEntity
                {
                    ItemId = "p01",
                    AssociatedItemId = "p12",
                    AssociationType = "product"
                },
                new AssociationEntity
                {
                    ItemId = "p01",
                    AssociatedCategoryId = "c0",
                    AssociationType = "category" 
                },
                new AssociationEntity
                {
                    ItemId = "p01",
                    AssociatedCategoryId = "c1",
                    AssociationType = "category"
                },
                new AssociationEntity
                {
                    ItemId = "p03",
                    AssociatedCategoryId = "c3",
                    AssociationType = "category"
                },
                new AssociationEntity
                {
                    ItemId = "p03",
                    AssociatedItemId = "p13",
                    AssociationType = "product"
                }
            };
      return associations.AsQueryable();
    }
  }
}
