using System;
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

        return query;
    }

    // protected override IList<SortInfo> BuildSortExpression(ProductConfigurationSearchCriteria criteria)
    // {
    //     var sortInfos = criteria.SortInfos;
    //
    //     if (sortInfos.IsNullOrEmpty())
    //     {
    //         sortInfos =
    //         [
    //             new SortInfo { SortColumn = nameof(CategoryEntity.Name) },
    //         ];
    //     }
    //
    //     return sortInfos;
    // }

    // protected override Task<ProductConfigurationSearchResult> ProcessSearchAsync(
    //     ProductConfigurationSearchCriteria criteria)
    // {
    //     throw new NotImplementedException();
    // }
    //
    // protected override Task<IList<ProductConfigurationEntity>> LoadEntities(IRepository repository, IList<string> ids,
    //     string responseGroup)
    // {
    //     throw new NotImplementedException();
    // }
}
