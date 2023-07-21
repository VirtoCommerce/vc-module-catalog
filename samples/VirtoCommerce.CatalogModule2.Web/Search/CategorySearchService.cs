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

namespace VirtoCommerce.CatalogModule2.Web.Search
{
    public class CategorySearchService2 : CategorySearchService
    {
        public CategorySearchService2(
            Func<ICatalogRepository> catalogRepositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            ICategoryService crudService,
            IOptions<CrudOptions> crudOptions)
            : base(catalogRepositoryFactory, platformMemoryCache, crudService, crudOptions)
        {
        }

        protected override IQueryable<CategoryEntity> BuildQuery(IRepository repository, CategorySearchCriteria criteria)
        {
            return base.BuildQuery(repository, criteria);
        }

        protected override IList<SortInfo> BuildSortExpression(CategorySearchCriteria criteria)
        {
            return base.BuildSortExpression(criteria);
        }
    }
}
