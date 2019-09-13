using System;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Model;
using VirtoCommerce.Domain.Catalog.Services;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Services
{
    public class BulkUpdateDataSourceFactory : IPagedDataSourceFactory
    {
        private readonly ICatalogSearchService _searchService;
        private readonly IItemService _itemService;

        public BulkUpdateDataSourceFactory(ICatalogSearchService searchService, IItemService itemService)
        {
            _searchService = searchService;
            _itemService = itemService;
        }

        public IPagedDataSource Create(BulkUpdateDataQuery dataQuery)
        {
            IPagedDataSource result = null;

            if (dataQuery is ProductBulkUpdateDataQuery productDataQuery)
            {
                result = new ProductDataSource(_searchService, _itemService, productDataQuery);
            }

            return result ?? throw new ArgumentException($"Unsupported bulk update query type: {dataQuery.GetType().Name}");
        }
    }
}
