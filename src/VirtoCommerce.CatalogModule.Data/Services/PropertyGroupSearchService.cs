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

public class PropertyGroupSearchService : SearchService<PropertyGroupSearchCriteria, PropertyGroupSearchResult, PropertyGroup, PropertyGroupEntity>, IPropertyGroupSearchService
{
    public PropertyGroupSearchService(
        Func<ICatalogRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IPropertyGroupService crudService,
        IOptions<CrudOptions> crudOptions)
        : base(repositoryFactory, platformMemoryCache, crudService, crudOptions)
    {
    }

    protected override IQueryable<PropertyGroupEntity> BuildQuery(IRepository repository, PropertyGroupSearchCriteria criteria)
    {
        var query = ((ICatalogRepository)repository).PropertyGroups;

        if (!string.IsNullOrEmpty(criteria.CatalogId))
        {
            query = query.Where(x => x.CatalogId == criteria.CatalogId);
        }

        return query;
    }

    protected override IList<SortInfo> BuildSortExpression(PropertyGroupSearchCriteria criteria)
    {
        var sortInfos = criteria.SortInfos;

        if (sortInfos.IsNullOrEmpty())
        {
            sortInfos =
            [
                new SortInfo { SortColumn = nameof(PropertyGroupEntity.CreatedDate), SortDirection = SortDirection.Descending },
                new SortInfo { SortColumn = nameof(PropertyGroupEntity.Id) },
            ];
        }

        return sortInfos;
    }
}
