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
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Data.GenericCrud;


namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class PropertySearchService : SearchService<PropertySearchCriteria, PropertySearchResult, Property, PropertyEntity>, IPropertySearchService
    {
        public PropertySearchService(Func<ICatalogRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache, IPropertyService propertyService)
            : base(repositoryFactory, platformMemoryCache, (ICrudService<Property>)propertyService)
        {
        }

        protected override IQueryable<PropertyEntity> BuildQuery(IRepository repository, PropertySearchCriteria criteria)
        {
            var query = ((ICatalogRepository)repository).Properties;
            if (!criteria.CatalogIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.CatalogIds.Contains(x.CatalogId));
            }
            if (!criteria.CategoryId.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.CategoryId == x.CategoryId);
            }
            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                query = query.Where(x => x.Name.Contains(criteria.Keyword));
            }
            if (!criteria.PropertyNames.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.PropertyNames.Contains(x.Name));
            }
            return query;
        }

        protected override IList<SortInfo> BuildSortExpression(PropertySearchCriteria criteria)
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

        public async Task<PropertySearchResult> SearchPropertiesAsync(PropertySearchCriteria criteria)
        {
            return await base.SearchAsync(criteria);
        }
    }
}
