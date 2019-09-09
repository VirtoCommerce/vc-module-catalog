using System;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.ExportModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Data.ExportImport
{
    public class CatalogFullExportPagedDataSourceFactory : IPagedDataSourceFactory
    {
        private readonly CatalogExportPagedDataSourceFactory _catalogDataSourceFactory;
        private readonly CategoryExportPagedDataSourceFactory _categoryDataSourceFactory;
        private readonly PropertyExportPagedDataSourceFactory _propertyExportPagedDataSourceFactory;
        private readonly PropertyDictionaryItemExportPagedDataSourceFactory _propertyDictionaryItemExportPagedDataSourceFactory;

        public CatalogFullExportPagedDataSourceFactory(
            CatalogExportPagedDataSourceFactory catalogDataSourceFactory,
            CategoryExportPagedDataSourceFactory categoryDataSourceFactory,
            PropertyExportPagedDataSourceFactory propertyExportPagedDataSourceFactory,
            PropertyDictionaryItemExportPagedDataSourceFactory propertyDictionaryItemExportPagedDataSourceFactory)
        {
            _catalogDataSourceFactory = catalogDataSourceFactory;
            _categoryDataSourceFactory = categoryDataSourceFactory;
            _propertyExportPagedDataSourceFactory = propertyExportPagedDataSourceFactory;
            _propertyDictionaryItemExportPagedDataSourceFactory = propertyDictionaryItemExportPagedDataSourceFactory;
        }

        public IPagedDataSource Create(ExportDataQuery dataQuery)
        {
            var catalogFullExportDataQuery = dataQuery as CatalogFullExportDataQuery ?? throw new InvalidCastException($"Cannot cast dataQuery to type {typeof(CatalogFullExportDataQuery)}");

            return new CatalogFullExportPagedDataSource(_catalogDataSourceFactory, _categoryDataSourceFactory, _propertyDictionaryItemExportPagedDataSourceFactory, _propertyExportPagedDataSourceFactory, catalogFullExportDataQuery);
        }
    }
}
