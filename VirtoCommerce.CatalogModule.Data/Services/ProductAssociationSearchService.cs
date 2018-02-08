using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Model.Search;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogModule.Data.Services
{
  public class ProductAssociationSearchService : IProductAssociationSearchService
  {
    private readonly Func<ICatalogRepository> _catalogRepositoryFactory;
    private readonly IItemService _itemService;
    private readonly ICategoryService _categoryService;

    private readonly Dictionary<string, string> _productSortingAliases = new Dictionary<string, string>();

    public ProductAssociationSearchService(Func<ICatalogRepository> catalogRepositoryFactory, IItemService itemService, ICategoryService categoryService)
    {
      _catalogRepositoryFactory = catalogRepositoryFactory;
      _itemService = itemService;
      _categoryService = categoryService;

      _productSortingAliases["sku"] = ReflectionUtility.GetPropertyName<CatalogProduct>(x => x.Code);
    }

    public GenericSearchResult<CatalogProduct> SearchProductAssociations(ProductAssociationSearchCriteria criteria)
    {
      var retVal = new GenericSearchResult<CatalogProduct>();
   
      var sortInfos = criteria.SortInfos;
      if (sortInfos.IsNullOrEmpty())
      {
        sortInfos = new[] { new SortInfo { SortColumn = "Priority", SortDirection = SortDirection.Descending }, new SortInfo { SortColumn = "Name", SortDirection = SortDirection.Ascending } };
      }
      //Try to replace sorting columns names
      TryTransformSortingInfoColumnNames(_productSortingAliases, sortInfos);
   
      var productResponseGroup = ItemResponseGroup.ItemInfo | ItemResponseGroup.ItemAssets | ItemResponseGroup.Links | ItemResponseGroup.Seo;
   
      var objectTypes = criteria.ObjectTypes.ToList();
      var objectIds = criteria.ObjectIds;
      var searchCategoryIds = new string[] { };
      var categoryIds = new string[] { };//new List<string>();
   
      var products = _itemService.GetByIds(objectIds.ToArray(), productResponseGroup)
                              .OrderBy(x => objectIds.IndexOf(x.Id)).ToList();   
    
      using (var repository = _catalogRepositoryFactory())
      {
        //Optimize performance and CPU usage
        repository.DisableChangesTracking();
       
        foreach (var product in products)
        {
          if (product.Associations.Count() > 0)
          {
             for(var i = 0; i < product.Associations.ToArray().Length; i++)
             {
               categoryIds[i] = product.CategoryId;
             }
          }
          //if (product.ReferencedAssociations.Count() > 0)
          //{
          //  for(var i = 0; i < product.ReferencedAssociations.ToArray().Length; i++)
          //  {
          //    categoryIds[i] = product.CategoryId;
          //  }
          //}
        }
       
        var query = repository.Items.Where(x => x.IsBuyable || x.IsActive);        
     
        if (!categoryIds.IsNullOrEmpty())
        {
          categoryIds = categoryIds.Concat(repository.GetAllChildrenCategoriesIds(categoryIds)).ToArray();
          //linked categories
          var allLinkedCategories = repository.CategoryLinks.Where(x => categoryIds.Contains(x.TargetCategoryId)).Select(x => x.SourceCategoryId).ToArray();
          categoryIds = categoryIds.Concat(allLinkedCategories).Distinct().ToArray();
     
          query = query.Where(x => categoryIds.Contains(x.CategoryId) || x.CategoryLinks.Any(link => categoryIds.Contains(link.CategoryId)));
        }
        
        if (criteria.ResponseGroup == SearchResponseGroup.WithProperties.ToString())
        {
          productResponseGroup |= ItemResponseGroup.ItemProperties;
        }
        if (criteria.ResponseGroup == SearchResponseGroup.WithVariations.ToString())
        {
          productResponseGroup |= ItemResponseGroup.Variations;
        }
        if (criteria.ResponseGroup == SearchResponseGroup.WithOutlines.ToString())
        {
          productResponseGroup |= ItemResponseGroup.Outlines;
        }
     
        query = query.OrderBySortInfos(sortInfos);
     
        var itemIds = query.Skip(criteria.Skip)
                           .Take(criteria.Take)
                           .Select(x => x.Id)
                           .ToList();
     
        retVal.TotalCount = query.Count();
        retVal.Results = _itemService.GetByIds(itemIds.ToArray(), productResponseGroup)
                                        .OrderBy(x => itemIds.IndexOf(x.Id)).ToList();
     
      }
    return retVal;
  }

    protected virtual void TryTransformSortingInfoColumnNames(IDictionary<string, string> transformationMap, SortInfo[] sortingInfos)
    {
      //Try to replace sorting columns names
      foreach (var sortInfo in sortingInfos)
      {
        string newColumnName;
        if (transformationMap.TryGetValue(sortInfo.SortColumn.ToLowerInvariant(), out newColumnName))
        {
          sortInfo.SortColumn = newColumnName;
        }
      }
    }
  }
}
