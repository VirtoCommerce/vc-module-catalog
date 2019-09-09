using System;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.ExportModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Data.ExportImport
{
    public class PropertyExportPagedDataSourceFactory : IPagedDataSourceFactory
    {
        private readonly IPropertyService _propertyService;

        public PropertyExportPagedDataSourceFactory(IPropertyService propertyService)
        {
            _propertyService = propertyService;
        }

        public virtual IPagedDataSource Create(ExportDataQuery dataQuery)
        {
            var propertyExportDataQuery = dataQuery as PropertyExportDataQuery ?? throw new InvalidCastException($"Cannot cast dataQuery to type {typeof(PropertyExportDataQuery)}");

            return new PropertyExportPagedDataSource(_propertyService, propertyExportDataQuery);
        }
    }
}
