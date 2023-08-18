using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
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
        public PropertyDictionaryItemSearchService(Func<ICatalogRepository> repositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IPropertyDictionaryItemService crudService,
            IOptions<CrudOptions> crudOptions)
            : base(repositoryFactory, platformMemoryCache, crudService, crudOptions)
        {
        }

        [Obsolete("Use SearchAsync(PropertyDictionaryItemSearchCriteria searchCriteria, bool clone)", DiagnosticId = "VC0005", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
        public async Task<PropertyDictionaryItemSearchResult> SearchAsync(PropertyDictionaryItemSearchCriteria criteria)
        {
            return await base.SearchAsync(criteria);
        }

        [Obsolete("Use BuildQuery(IRepository repository, PropertyDictionaryItemSearchCriteria criteria)", DiagnosticId = "VC0005", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
        protected virtual IQueryable<PropertyDictionaryItemEntity> BuildQuery(ICatalogRepository repository, PropertyDictionaryItemSearchCriteria criteria)
        {
            return BuildQuery(repository as IRepository, criteria);
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
