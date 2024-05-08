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
