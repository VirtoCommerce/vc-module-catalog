using System;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.ExportModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Data.ExportImport
{
    public class PropertyDictionaryItemExportPagedDataSourceFactory : IPagedDataSourceFactory
    {
        private readonly IProperyDictionaryItemSearchService _propertyDictionarySearchService;

        public PropertyDictionaryItemExportPagedDataSourceFactory(IProperyDictionaryItemSearchService propertyDictionarySearchService)
        {
            _propertyDictionarySearchService = propertyDictionarySearchService;
        }

        public virtual IPagedDataSource Create(ExportDataQuery dataQuery)
        {
            var propertyDictionaryItemExportDataQuery = dataQuery as PropertyDictionaryItemExportDataQuery ?? throw new InvalidCastException($"Cannot cast dataQuery to type {typeof(PropertyDictionaryItemExportDataQuery)}");

            return new PropertyDictionaryItemExportPagedDataSource(_propertyDictionarySearchService, propertyDictionaryItemExportDataQuery);
        }
    }
}
