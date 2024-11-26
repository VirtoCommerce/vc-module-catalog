using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using VirtoCommerce.CatalogModule.Core.Model.Configuration;
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

public class ProductConfigurationSearchService : SearchService<ProductConfigurationSearchCriteria, ProductConfigurationSearchResult, ProductConfiguration, ProductConfigurationEntity>, IProductConfigurationSearchService
{
    public ProductConfigurationSearchService(
        Func<ICatalogRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IProductConfigurationService crudService,
        IOptions<CrudOptions> crudOptions)
        : base(repositoryFactory, platformMemoryCache, crudService, crudOptions)
    {
    }

    protected override IQueryable<ProductConfigurationEntity> BuildQuery(IRepository repository, ProductConfigurationSearchCriteria criteria)
    {
        var query = ((ICatalogRepository)repository).ProductConfigurations;

        if (!string.IsNullOrEmpty(criteria.ProductId))
        {
            query = query.Where(x => x.ProductId == criteria.ProductId);
        }

        if (criteria.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == criteria.IsActive.Value);
        }

        return query;
    }

    protected override IList<SortInfo> BuildSortExpression(ProductConfigurationSearchCriteria criteria)
    {
        var sortInfos = criteria.SortInfos;
        if (sortInfos.IsNullOrEmpty())
        {
            sortInfos = [new SortInfo
            {
                SortColumn = nameof(ProductConfigurationEntity.CreatedDate)
            }];
        }
        return sortInfos;
    }
}
