using System;
using VirtoCommerce.BulkActionsModule.Core.Models.BulkActions;
using VirtoCommerce.BulkActionsModule.Core.Services;
using VirtoCommerce.CatalogModule.BulkActions.Models;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.BulkActions.DataSources
{
    public class DataSourceFactory : IDataSourceFactory
    {
        private readonly IInternalListEntrySearchService _internalListEntrySearchService;
        private readonly Func<ICatalogRepository> _repositoryFactory;
        private readonly ICategoryService _categoryService;
        private readonly IPropertyService _propertyService;
        private readonly IItemService _itemService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceFactory"/> class.
        /// </summary>
        public DataSourceFactory(
            IInternalListEntrySearchService internalListEntrySearchService,
            Func<ICatalogRepository> repositoryFactory,
            ICategoryService categoryService,
            IPropertyService propertyService,
            IItemService itemService)
        {
            _internalListEntrySearchService = internalListEntrySearchService;
            _repositoryFactory = repositoryFactory;
            _categoryService = categoryService;
            _propertyService = propertyService;
            _itemService = itemService;
        }

        public IDataSource Create(BulkActionContext context)
        {
            IDataSource result = null;

            switch (context)
            {
                case CategoryChangeBulkActionContext categoryChangeContext:
                    result = new BaseDataSource(_internalListEntrySearchService, categoryChangeContext.DataQuery);
                    break;

                case PropertiesUpdateBulkActionContext propertiesUpdateContext when !propertiesUpdateContext.Properties.IsNullOrEmpty():
                    result = new ProductDataSource(_internalListEntrySearchService, propertiesUpdateContext.DataQuery);
                    break;

                case PropertiesUpdateBulkActionContext propertiesUpdateContext when propertiesUpdateContext.Properties.IsNullOrEmpty():
                    result = new PropertyDataSource(_repositoryFactory, _categoryService, _propertyService, _itemService, propertiesUpdateContext.DataQuery);
                    break;
            }

            var message = $"Unsupported bulk action query type: {context.GetType().Name}";
            return result ?? throw new ArgumentException(message);
        }
    }
}
