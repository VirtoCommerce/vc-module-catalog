using System;
using System.Collections.Generic;
using System.Linq;
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

namespace VirtoCommerce.CatalogModule.Data.Search;

public class AutomaticLinkQuerySearchService(
    Func<ICatalogRepository> repositoryFactory,
    IPlatformMemoryCache platformMemoryCache,
    IAutomaticLinkQueryService crudService,
    IOptions<CrudOptions> crudOptions)
    : SearchService<AutomaticLinkQuerySearchCriteria, AutomaticLinkQuerySearchResult, AutomaticLinkQuery, AutomaticLinkQueryEntity>
        (repositoryFactory, platformMemoryCache, crudService, crudOptions),
        IAutomaticLinkQuerySearchService
{
    protected override IQueryable<AutomaticLinkQueryEntity> BuildQuery(IRepository repository, AutomaticLinkQuerySearchCriteria criteria)
    {
        var query = ((ICatalogRepository)repository).AutomaticLinkQueries;

        if (!criteria.ObjectIds.IsNullOrEmpty())
        {
            query = criteria.ObjectIds.Count == 1
                ? query.Where(x => x.Id == criteria.ObjectIds.First())
                : query.Where(x => criteria.ObjectIds.Contains(x.Id));
        }

        if (!criteria.TargetCategoryId.IsNullOrEmpty())
        {
            query = query.Where(x => x.TargetCategoryId == criteria.TargetCategoryId);
        }

        return query;
    }

    protected override IList<SortInfo> BuildSortExpression(AutomaticLinkQuerySearchCriteria criteria)
    {
        var sortInfos = criteria.SortInfos;

        if (sortInfos.IsNullOrEmpty())
        {
            sortInfos =
            [
                new SortInfo { SortColumn = nameof(AutomaticLinkQueryEntity.CreatedDate), SortDirection = SortDirection.Descending },
                new SortInfo { SortColumn = nameof(AutomaticLinkQueryEntity.Id) },
            ];
        }

        return sortInfos;
    }
}
