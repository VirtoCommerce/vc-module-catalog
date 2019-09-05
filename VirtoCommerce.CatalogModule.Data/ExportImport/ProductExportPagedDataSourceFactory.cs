using System;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.ExportModule.Core.Services;
using VirtoCommerce.Platform.Core.Assets;

namespace VirtoCommerce.CatalogModule.Data.ExportImport
{
    public class ProductExportPagedDataSourceFactory : IPagedDataSourceFactory
    {
        private readonly IBlobStorageProvider _blobStorageProvider;
        private readonly ICatalogSearchService _catalogSearchService;
        private readonly IItemService _itemService;
        private readonly IBlobUrlResolver _blobUrlResolver;

        public ProductExportPagedDataSourceFactory(IBlobStorageProvider blobStorageProvider, ICatalogSearchService catalogSearchService, IItemService itemService, IBlobUrlResolver blobUrlResolver)
        {
            _blobStorageProvider = blobStorageProvider;
            _catalogSearchService = catalogSearchService;
            _itemService = itemService;
            _blobUrlResolver = blobUrlResolver;
        }

        public IPagedDataSource Create(ExportDataQuery dataQuery)
        {
            var priceExportDataQuery = dataQuery as ProductExportDataQuery ?? throw new InvalidCastException($"Cannot cast dataQuery to type {typeof(ProductExportDataQuery)}");

            return new ProductExportPagedDataSource(_blobStorageProvider, _itemService, _catalogSearchService, _blobUrlResolver, priceExportDataQuery);

        }
    }
}
