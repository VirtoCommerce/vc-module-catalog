using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class PropertySearchService : IPropertySearchService
    {
        private readonly Func<ICatalogRepository> _repositoryFactory;
        private readonly IPropertyService _propertyService;
        public PropertySearchService(Func<ICatalogRepository> repositoryFactory, IPropertyService propertyService)
        {
            _repositoryFactory = repositoryFactory;
            _propertyService = propertyService;
        }

        public async Task<PropertySearchResult> SearchPropertiesAsync(PropertySearchCriteria criteria)
        {
            var result = AbstractTypeFactory<PropertySearchResult>.TryCreateInstance();

            using var repository = _repositoryFactory();
            var query = BuildQuery(repository, criteria);
            var needExecuteCount = criteria.Take == 0;

            if (criteria.Take > 0)
            {
                var ids = await query
                    .OrderBySortInfos(BuildSortExpression(criteria))
                    .ThenBy(x => x.Id)
                    .Select(x => x.Id)
                    .Skip(criteria.Skip)
                    .Take(criteria.Take)
                    .ToListAsync();

                result.TotalCount = ids.Count;

                if (ids.Any())
                {
                    result.Results = (await _propertyService.GetByIdsAsync(ids))
                        .OrderBy(x => ids.IndexOf(x.Id))
                        .ToList();
                }

                if (criteria.Skip > 0 || result.TotalCount == criteria.Take)
                {
                    needExecuteCount = true;
                }
            }

            if (needExecuteCount)
            {
                result.TotalCount = await query.CountAsync();
            }

            result.Results ??= Array.Empty<Property>();

            return result;
        }


        protected virtual IQueryable<PropertyEntity> BuildQuery(ICatalogRepository repository, PropertySearchCriteria criteria)
        {
            var query = repository.Properties;

            if (!criteria.CatalogIds.IsNullOrEmpty())
            {
                query = criteria.CatalogIds.Length == 1
                    ? query.Where(x => x.CatalogId == criteria.CatalogIds.First())
                    : query.Where(x => criteria.CatalogIds.Contains(x.CatalogId));
            }

            if (!string.IsNullOrEmpty(criteria.CategoryId))
            {
                query = query.Where(x => criteria.CategoryId == x.CategoryId);
            }

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                query = query.Where(x => x.Name.Contains(criteria.Keyword));
            }

            if (!criteria.PropertyNames.IsNullOrEmpty())
            {
                query = criteria.PropertyNames.Count == 1
                    ? query.Where(x => x.Name == criteria.PropertyNames.First())
                    : query.Where(x => criteria.PropertyNames.Contains(x.Name));
            }

            if (!criteria.PropertyTypes.IsNullOrEmpty())
            {
                query = criteria.PropertyTypes.Count == 1
                    ? query.Where(x => x.TargetType == criteria.PropertyTypes.First())
                    : query.Where(x => criteria.PropertyTypes.Contains(x.TargetType));
            }

            return query;
        }

        protected virtual IList<SortInfo> BuildSortExpression(PropertySearchCriteria criteria)
        {
            var sortInfos = criteria.SortInfos;
            if (sortInfos.IsNullOrEmpty())
            {
                sortInfos = new[]
                {
                    new SortInfo { SortColumn = nameof(PropertyEntity.Name) }
                };
            }
            return sortInfos;
        }
    }
}
