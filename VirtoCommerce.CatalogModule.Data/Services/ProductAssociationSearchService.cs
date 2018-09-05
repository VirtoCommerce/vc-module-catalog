using System;
using System.Linq;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Model.Search;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Commerce.Model.Search;
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

        public GenericSearchResult<ProductAssociation> SearchProductAssociations(ProductAssociationSearchCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            var result = new GenericSearchResult<ProductAssociation>();
            using (var repository = _catalogRepositoryFactory())
            {
                //Optimize performance and CPU usage
                repository.DisableChangesTracking();

                var query = repository.Associations;

                if (!criteria.ObjectIds.IsNullOrEmpty())
                {
                    query = query.Where(x => criteria.ObjectIds.Contains(x.ItemId));
                }
                if (!string.IsNullOrEmpty(criteria.Group))
                {
                    query = query.Where(x => x.AssociationType == criteria.Group);
                }

                var sortInfos = criteria.SortInfos;
                if (sortInfos.IsNullOrEmpty())
                {
                    sortInfos = new[] { new SortInfo { SortColumn = "Priority", SortDirection = SortDirection.Descending } };
                }
                //TODO: Sort by association priority
                query = query.OrderBySortInfos(sortInfos);

                result.TotalCount = query.Count();
                result.Results = query.Skip(criteria.Skip).Take(criteria.Take)
                                   .ToArray().Select(x => x.ToModel(AbstractTypeFactory<ProductAssociation>.TryCreateInstance()))
                                   .ToList();
            }
            return result;
        }
    }
}
