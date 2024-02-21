using System;
using System.Linq;
using Microsoft.Extensions.Options;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class MeasureSearchService : SearchService<MeasureSearchCriteria, MeasureSearchResult, Measure, MeasureEntity>, IMeasureSearchService
    {
        public MeasureSearchService(
            Func<ICatalogRepository> repositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IMeasureService crudService,
            IOptions<CrudOptions> crudOptions)
            : base(repositoryFactory, platformMemoryCache, crudService, crudOptions)
        {
        }

        protected override IQueryable<MeasureEntity> BuildQuery(IRepository repository, MeasureSearchCriteria criteria)
        {
            var query = ((ICatalogRepository)repository).Measures;
            return query;
        }
    }
}
