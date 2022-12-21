using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Search;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule2.Data.Search
{
    public class PropertyDictionaryItemSearchService2 : PropertyDictionaryItemSearchService
    {
        public PropertyDictionaryItemSearchService2(Func<ICatalogRepository> repositoryFactory, IPropertyDictionaryItemService properyDictionaryItemService) : base(repositoryFactory, properyDictionaryItemService)
        {
        }
        protected override IQueryable<PropertyDictionaryItemEntity> BuildQuery(ICatalogRepository repository, PropertyDictionaryItemSearchCriteria criteria)
        {
            return base.BuildQuery(repository, criteria);
        }
        protected override IList<SortInfo> BuildSortExpression(PropertyDictionaryItemSearchCriteria criteria)
        {
            return base.BuildSortExpression(criteria);
        }
    }
}
