using System;
using System.Collections.Generic;
using System.Linq;
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
    public class PropertyDictionaryItemSearchService : SearchService<PropertyDictionaryItemSearchCriteria, PropertyDictionaryItemSearchResult, PropertyDictionaryItem, PropertyDictionaryItemEntity>, IPropertyDictionaryItemSearchService
    {
        public PropertyDictionaryItemSearchService(Func<ICatalogRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache, IPropertyDictionaryItemService properyDictionaryItemService)
             : base(repositoryFactory, platformMemoryCache, (ICrudService<PropertyDictionaryItem>)properyDictionaryItemService)
        {
        }

        protected override IQueryable<PropertyDictionaryItemEntity> BuildQuery(IRepository repository, PropertyDictionaryItemSearchCriteria criteria)
        {
            var query = ((ICatalogRepository)repository).PropertyDictionaryItems;
            if (!criteria.CatalogIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.CatalogIds.Contains(x.Property.CatalogId));
            }
            if (!criteria.PropertyIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.PropertyIds.Contains(x.PropertyId));
            }
            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                query = query.Where(x => x.Alias.Contains(criteria.Keyword));
            }

            return query;
        }

        protected override IList<SortInfo> BuildSortExpression(PropertyDictionaryItemSearchCriteria criteria)
        {
            var sortInfos = criteria.SortInfos;
            if (sortInfos.IsNullOrEmpty())
            {
                sortInfos = new[]
                {
                    new SortInfo { SortColumn = nameof(PropertyDictionaryItemEntity.SortOrder) },
                    new SortInfo { SortColumn = nameof(PropertyDictionaryItemEntity.Alias) }
               };
            }
            return sortInfos;
        }
    }
}
