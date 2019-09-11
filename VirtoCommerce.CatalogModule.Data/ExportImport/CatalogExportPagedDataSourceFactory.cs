using System;
using System.Collections.Generic;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.ExportImport
{
    public class CatalogExportPagedDataSourceFactory : ICatalogExportPagedDataSourceFactory
    {
        private readonly IPropertyService _propertyService;
        private readonly IProperyDictionaryItemSearchService _propertyDictionaryItemSearchService;
        private readonly IBlobStorageProvider _blobStorageProvider;
        private readonly IItemService _itemService;
        private readonly ICatalogSearchService _catalogSearchService;
        private readonly IBlobUrlResolver _blobUrlResolver;
        private readonly ICategoryService _categoryService;

        public CatalogExportPagedDataSourceFactory(
            IPropertyService propertyService
            , IProperyDictionaryItemSearchService propertyDictionaryItemSearchService
            , IBlobStorageProvider blobStorageProvider
            , IItemService itemService
            , ICatalogSearchService catalogSearchService
            , IBlobUrlResolver blobUrlResolver
            , ICategoryService categoryService)
        {
            _propertyService = propertyService;
            _propertyDictionaryItemSearchService = propertyDictionaryItemSearchService;
            _blobStorageProvider = blobStorageProvider;
            _itemService = itemService;
            _catalogSearchService = catalogSearchService;
            _blobUrlResolver = blobUrlResolver;
            _categoryService = categoryService;
        }

        public virtual IPagedDataSource Create(ExportDataQuery dataQuery)
        {
            IPagedDataSource result = null;
            if (dataQuery is PropertyExportDataQuery propertyExportQuery)
            {
                result = new PropertyExportPagedDataSource(_propertyService, propertyExportQuery);
            }
            else if (dataQuery is PropertyDictionaryItemExportDataQuery propDictExportQuery)
            {
                result = new PropertyDictionaryItemExportPagedDataSource(_propertyDictionaryItemSearchService, propDictExportQuery);
            }
            else if (dataQuery is ProductFullExportDataQuery productFullExportQuery)
            {
                result = new ProductExportPagedDataSource(_blobStorageProvider, _itemService, _catalogSearchService, _blobUrlResolver, productFullExportQuery.ToProductExportDataQuery());
            }
            else if (dataQuery is CategoryExportDataQuery categoryExportQuery)
            {
                result = new CategoryExportPagedDataSource(_catalogSearchService, _categoryService, _blobStorageProvider, categoryExportQuery);
            }
            else if (dataQuery is CatalogExportDataQuery catalogExportQuery)
            {
                result = new CatalogExportPagedDataSource(_catalogSearchService, catalogExportQuery);
            }
            else if (dataQuery is ProductExportDataQuery productExportQuery)
            {
                result = new ProductExportPagedDataSource(_blobStorageProvider, _itemService, _catalogSearchService, _blobUrlResolver, productExportQuery);
            }
            else if (dataQuery is CatalogFullExportDataQuery catalogFullExportDataQuery)
            {
                result = new CatalogFullExportPagedDataSource(this, catalogFullExportDataQuery);
            }

            if (result == null)
            {
                throw new ArgumentException($"Unsupported export query type: {dataQuery.GetType().Name}");
            }
            return result;
        }

        public virtual IEnumerable<IPagedDataSource> GetAllFullExportPagedDataSources(CatalogFullExportDataQuery query)
        {
            yield return Create(AbstractTypeFactory<CatalogExportDataQuery>.TryCreateInstance().FromOther(query));
            yield return Create(AbstractTypeFactory<CategoryExportDataQuery>.TryCreateInstance().FromOther(query));
            yield return Create(AbstractTypeFactory<ProductFullExportDataQuery>.TryCreateInstance().FromOther(query));
            yield return Create(AbstractTypeFactory<PropertyExportDataQuery>.TryCreateInstance().FromOther(query));
            yield return Create(AbstractTypeFactory<PropertyDictionaryItemExportDataQuery>.TryCreateInstance().FromOther(query));
        }
    }
}
