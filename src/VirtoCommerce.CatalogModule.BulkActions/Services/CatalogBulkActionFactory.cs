using System;
using VirtoCommerce.BulkActionsModule.Core.Models.BulkActions;
using VirtoCommerce.BulkActionsModule.Core.Services;
using VirtoCommerce.CatalogModule.BulkActions.Actions.CategoryChange;
using VirtoCommerce.CatalogModule.BulkActions.Actions.PropertiesUpdate;
using VirtoCommerce.CatalogModule.BulkActions.Models;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Services;

namespace VirtoCommerce.CatalogModule.BulkActions.Services
{
    public class CatalogBulkActionFactory : IBulkActionFactory
    {
        private readonly ICatalogService _catalogService;
        private readonly IProductService _productService;
        private readonly IBulkPropertyUpdateManager _bulkPropertyUpdateManager;
        private readonly ListEntryMover<Category> _categoryListEntryMover;
        private readonly ListEntryMover<CatalogProduct> _productListEntryMover;

        /// <summary>
        /// Initializes a new instance of the <see cref="CatalogBulkActionFactory"/> class.
        /// </summary>
        public CatalogBulkActionFactory(ICatalogService catalogService,
            IProductService productService,
            IBulkPropertyUpdateManager bulkPropertyUpdateManager,
            ListEntryMover<Category> categoryListEntryMover,
            ListEntryMover<CatalogProduct> productListEntryMover)
        {
            _catalogService = catalogService;
            _productService = productService;
            _bulkPropertyUpdateManager = bulkPropertyUpdateManager;
            _categoryListEntryMover = categoryListEntryMover;
            _productListEntryMover = productListEntryMover;
        }

        [Obsolete($"Use the overload that accepts {nameof(IProductService)}")]
        public CatalogBulkActionFactory(ICatalogService catalogService,
            IItemService itemService,
            IBulkPropertyUpdateManager bulkPropertyUpdateManager,
            ListEntryMover<Category> categoryListEntryMover,
            ListEntryMover<CatalogProduct> productListEntryMover)
            : this(catalogService, (IProductService)itemService, bulkPropertyUpdateManager, categoryListEntryMover, productListEntryMover)
        {
        }

        [Obsolete($"This constructor is intended to be used by a DI container only")]
        public CatalogBulkActionFactory(ICatalogService catalogService,
            IProductService productService,
            // ReSharper disable once UnusedParameter.Local
            IItemService itemService,
            IBulkPropertyUpdateManager bulkPropertyUpdateManager,
            ListEntryMover<Category> categoryListEntryMover,
            ListEntryMover<CatalogProduct> productListEntryMover)
            : this(catalogService, productService, bulkPropertyUpdateManager, categoryListEntryMover, productListEntryMover)
        {
        }

        public IBulkAction Create(BulkActionContext context)
        {
            IBulkAction result = null;

            switch (context)
            {
                case CategoryChangeBulkActionContext changeCategoryActionContext:
                    result = new CategoryChangeBulkAction(changeCategoryActionContext, _catalogService, _categoryListEntryMover, _productListEntryMover);
                    break;

                case PropertiesUpdateBulkActionContext updatePropertiesActionContext:
                    result = new PropertiesUpdateBulkAction(updatePropertiesActionContext, _productService, _bulkPropertyUpdateManager);
                    break;
            }

            return result ?? throw new ArgumentException($"Unsupported action type: {context.GetType().Name}");
        }
    }
}
