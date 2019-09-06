using System;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.ExportModule.Core.Services;
using VirtoCommerce.Platform.Core.Assets;

namespace VirtoCommerce.CatalogModule.Data.ExportImport
{
    public class CategoryExportPagedDataSourceFactory : IPagedDataSourceFactory
    {
        private readonly ICatalogSearchService _catalogSearchService;
        private readonly ICategoryService _categoryService;
        private readonly IBlobStorageProvider _blobStorageProvider;

        public CategoryExportPagedDataSourceFactory(
            ICatalogSearchService catalogSearchService,
            ICategoryService categoryService,
            IBlobStorageProvider blobStorageProvider)
        {
            _catalogSearchService = catalogSearchService;
            _categoryService = categoryService;
            _blobStorageProvider = blobStorageProvider;
        }

        public virtual IPagedDataSource Create(ExportDataQuery dataQuery)
        {
            var categoryExportDataQuery = dataQuery as CategoryExportDataQuery ?? throw new InvalidCastException($"Cannot cast dataQuery to type {typeof(CategoryExportDataQuery)}");

            return new CategoryExportPagedDataSource(_catalogSearchService, _categoryService, _blobStorageProvider, categoryExportDataQuery);
        }
    }
}
