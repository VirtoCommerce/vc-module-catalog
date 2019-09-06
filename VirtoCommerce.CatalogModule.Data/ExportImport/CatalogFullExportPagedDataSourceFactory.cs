using System;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.ExportModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Data.ExportImport
{
    public class CatalogFullExportPagedDataSourceFactory : IPagedDataSourceFactory
    {
        private readonly CatalogExportPagedDataSourceFactory _catalogDataSourceFactory;
        private readonly CategoryExportPagedDataSourceFactory _categoryDataSourceFactory;

        public CatalogFullExportPagedDataSourceFactory(
            CatalogExportPagedDataSourceFactory catalogDataSourceFactory,
            CategoryExportPagedDataSourceFactory categoryDataSourceFactory)
        {
            _catalogDataSourceFactory = catalogDataSourceFactory;
            _categoryDataSourceFactory = categoryDataSourceFactory;
        }

        public IPagedDataSource Create(ExportDataQuery dataQuery)
        {
            var catalogFullExportDataQuery = dataQuery as CatalogFullExportDataQuery ?? throw new InvalidCastException($"Cannot cast dataQuery to type {typeof(CatalogFullExportDataQuery)}");

            return new CatalogFullExportPagedDataSource(_catalogDataSourceFactory, _categoryDataSourceFactory, catalogFullExportDataQuery);
        }
    }
}
