using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Search;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.CatalogModule2.Data.Search
{
    public class PropertyDictionaryItemSearchService2 : PropertyDictionaryItemSearchService
    {
        public PropertyDictionaryItemSearchService2(Func<ICatalogRepository> repositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IPropertyDictionaryItemService crudService,
            IOptions<CrudOptions> crudOptions)
            : base(repositoryFactory, platformMemoryCache, crudService, crudOptions)
        {
        }

        protected override IQueryable<PropertyDictionaryItemEntity> BuildQuery(IRepository repository, PropertyDictionaryItemSearchCriteria criteria)
        {
            return base.BuildQuery(repository, criteria);
        }

        protected override IList<SortInfo> BuildSortExpression(PropertyDictionaryItemSearchCriteria criteria)
        {
            return base.BuildSortExpression(criteria);
        }
    }
}
