﻿using System;
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

    public ProductAssociationSearchService(Func<ICatalogRepository> catalogRepositoryFactory, IItemService itemService)
    {
      _catalogRepositoryFactory = catalogRepositoryFactory;
      _itemService = itemService;
    }

    public GenericSearchResult<CatalogProduct> SearchProductAssociations(ProductAssociationSearchCriteria criteria)
    {
      var retVal = new GenericSearchResult<CatalogProduct>();
      var categoryIds = new string[] { };

      var products = _itemService.GetByIds(criteria.ObjectIds.ToArray(), ItemResponseGroup.ItemAssociations).ToList();
      //Get all Ids of products category associations.
      var categoryAssociationIds = products.SelectMany(x => x.Associations.Where(a => a.AssociatedObjectType == "category")).Select(x => x.AssociatedObjectId).ToArray();
      categoryIds = categoryAssociationIds;

      using (var repository = _catalogRepositoryFactory())
      {
        //Optimize performance and CPU usage
        repository.DisableChangesTracking();

        var query = repository.Items.Where(x => x.IsBuyable || x.IsActive);

        if (!categoryIds.IsNullOrEmpty())
        {
          categoryIds = categoryIds.Concat(repository.GetAllChildrenCategoriesIds(categoryIds)).ToArray();
          //linked categories
          var allLinkedCategories = repository.CategoryLinks.Where(x => categoryIds.Contains(x.TargetCategoryId)).Select(x => x.SourceCategoryId).ToArray();
          categoryIds = categoryIds.Concat(allLinkedCategories).Distinct().ToArray();
     
          query = query.Where(x => categoryIds.Contains(x.CategoryId) || x.CategoryLinks.Any(link => categoryIds.Contains(link.CategoryId)));
        }

        var productResponseGroup = ItemResponseGroup.ItemInfo | ItemResponseGroup.ItemAssets | ItemResponseGroup.Links | ItemResponseGroup.Seo;
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

        var sortInfos = criteria.SortInfos;
        if (sortInfos.IsNullOrEmpty())
        {
          sortInfos = new[] { new SortInfo { SortColumn = "Priority", SortDirection = SortDirection.Descending }, new SortInfo { SortColumn = "Name", SortDirection = SortDirection.Ascending } };
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
  }
}
