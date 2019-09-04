using System;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.ExportModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Data.ExportImport
{
    public class CatalogFullExportPagedDataSourceFactory : IPagedDataSourceFactory
    {
        private readonly ICatalogService _catalogService;
        private readonly ICatalogSearchService _catalogSearchService;
        private readonly ICategoryService _categoryService;
        private readonly IItemService _itemService;
        private readonly IPropertyService _propertyService;
        private readonly IAssociationService _associationService;
        private readonly IProperyDictionaryItemSearchService _propertyDictionarySearchService;
        private readonly IProperyDictionaryItemService _propertyDictionaryService;

        public CatalogFullExportPagedDataSourceFactory(ICatalogSearchService catalogSearchService,
            ICatalogService catalogService,
            ICategoryService categoryService,
            IItemService itemService,
            IPropertyService propertyService,
            IAssociationService associationService,
            IProperyDictionaryItemSearchService propertyDictionarySearchService,
            IProperyDictionaryItemService propertyDictionaryService)
        {
            _catalogSearchService = catalogSearchService;
            _catalogService = catalogService;
            _categoryService = categoryService;
            _itemService = itemService;
            _propertyService = propertyService;
            _associationService = associationService;
            _propertyDictionarySearchService = propertyDictionarySearchService;
            _propertyDictionaryService = propertyDictionaryService;
        }

        public IPagedDataSource Create(ExportDataQuery dataQuery)
        {
            var catalogFullExportDataQuery = dataQuery as CatalogFullExportDataQuery ?? throw new InvalidCastException($"Cannot cast dataQuery to type {typeof(CatalogFullExportDataQuery)}");

            return new CatalogFullExportPagedDataSource(_catalogSearchService, _catalogService, _categoryService, _itemService, _propertyService, _associationService, _propertyDictionarySearchService, _propertyDictionaryService, catalogFullExportDataQuery);
        }
    }
}
