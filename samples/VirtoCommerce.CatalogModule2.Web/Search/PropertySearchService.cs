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
    public class PropertySearchService2 : PropertySearchService
    {
        public PropertySearchService2(Func<ICatalogRepository> repositoryFactory, IPropertyService propertyService) : base(repositoryFactory, propertyService)
        {
        }
        protected override IQueryable<PropertyEntity> BuildQuery(ICatalogRepository repository, PropertySearchCriteria criteria)
        {
            return base.BuildQuery(repository, criteria);
        }
        protected override IList<SortInfo> BuildSortExpression(PropertySearchCriteria criteria)
        {
            return base.BuildSortExpression(criteria);
        }
    }
}
