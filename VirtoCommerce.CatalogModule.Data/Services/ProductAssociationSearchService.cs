using System;
using System.Linq;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Model.Search;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Exceptions;
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
            if (criteria.ObjectIds.IsNullOrEmpty())
            {
                throw new ArgumentNullException($"{ nameof(criteria.ObjectIds) } must be set");
            }
            var retVal = new GenericSearchResult<CatalogProduct>();
            using (var repository = _catalogRepositoryFactory())
            {
                //Optimize performance and CPU usage
                repository.DisableChangesTracking();

                var query = repository.Associations.Where(x => criteria.ObjectIds.Contains(x.ItemId));
                if (!string.IsNullOrEmpty(criteria.Group))
                {
                    query = query.Where(x => x.AssociationType == criteria.Group);
                }
                
                var associationCategoriesIds = query.Where(x => x.AssociatedCategoryId != null)
                                                    .Select(x => x.AssociatedCategoryId)
                                                    .ToArray();
                //Need to return all products from the associated categories (recursive)
                associationCategoriesIds = repository.GetAllChildrenCategoriesIds(associationCategoriesIds).Concat(associationCategoriesIds)
                                                    .Distinct()
                                                    .ToArray();                
                var itemsQuery = repository.Items.Join(query, item => item.Id, association => association.AssociatedItemId, (item, association) => item)                                           
                                           .Union(repository.Items.Where(x => associationCategoriesIds.Contains(x.CategoryId)));

                var sortInfos = criteria.SortInfos;
                if (sortInfos.IsNullOrEmpty())
                {
                    sortInfos = new[] { new SortInfo { SortColumn = "CreatedDate", SortDirection = SortDirection.Descending } };
                }
                //TODO: Sort by association priority
                itemsQuery = itemsQuery.OrderBySortInfos(sortInfos);

                retVal.TotalCount = itemsQuery.Count();
                var itemIds = itemsQuery
                              .Skip(criteria.Skip)
                              .Take(criteria.Take)
                              .Select(x => x.Id).ToList();

                retVal.Results = _itemService.GetByIds(itemIds.ToArray(), EnumUtility.SafeParse(criteria.ResponseGroup, ItemResponseGroup.ItemInfo))
                                             .OrderBy(x => itemIds.IndexOf(x.Id))
                                             .ToList();
            }
            return retVal;
        }
    }
}
