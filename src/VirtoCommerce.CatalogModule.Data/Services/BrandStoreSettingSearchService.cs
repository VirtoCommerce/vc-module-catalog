using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.CatalogModule.Data.Services;

public class BrandStoreSettingSearchService : SearchService<BrandStoreSettingSearchCriteria, BrandStoreSettingSearchResult, BrandStoreSetting, BrandStoreSettingEntity>, IBrandStoreSettingSearchService
{
    public BrandStoreSettingSearchService(
        Func<ICatalogRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IBrandStoreSettingService crudService,
        IOptions<CrudOptions> crudOptions)
        : base(repositoryFactory, platformMemoryCache, crudService, crudOptions)
    {
    }

    protected override IQueryable<BrandStoreSettingEntity> BuildQuery(IRepository repository, BrandStoreSettingSearchCriteria criteria)
    {
        var query = ((ICatalogRepository)repository).BrandStoreSettings;

        if (!string.IsNullOrEmpty(criteria.StoreId))
        {
            query = query.Where(x => x.StoreId == criteria.StoreId);
        }

        return query;
    }

    protected override IList<SortInfo> BuildSortExpression(BrandStoreSettingSearchCriteria criteria)
    {
        var sortInfos = criteria.SortInfos;

        if (sortInfos.IsNullOrEmpty())
        {
            sortInfos =
            [
                new SortInfo { SortColumn = nameof(BrandStoreSetting.CreatedDate), SortDirection = SortDirection.Descending },
                new SortInfo { SortColumn = nameof(BrandStoreSetting.Id) },
            ];
        }

        return sortInfos;
    }
}

