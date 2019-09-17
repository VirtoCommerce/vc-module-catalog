using System;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Model;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Model.Actions.ChangeCategory;
using VirtoCommerce.CatalogModule.Web.Services;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Services
{
    public class BulkUpdateActionFactory : IBulkUpdateActionFactory
    {
        private readonly ICatalogService _catalogService;
        private readonly IListEntryMover<Category> _categoryMover;
        private readonly IListEntryMover<CatalogProduct> _productMover;
        private readonly IBulkUpdatePropertyManager _bulkUpdatePropertyManager;

        public BulkUpdateActionFactory(ICatalogService catalogService,
            IListEntryMover<Category> categoryMover,
            IListEntryMover<CatalogProduct> productMover
            )
        {
            _catalogService = catalogService;
            _categoryMover = categoryMover;
            _productMover = productMover;
            _bulkUpdatePropertyManager = bulkUpdatePropertyManager;
        }

        public IBulkUpdateAction Create(BulkUpdateActionContext context)
        {
            IBulkUpdateAction result = null;

            if (context is ChangeCategoryActionContext changeCategoryActionContext)
            {
                result = new ChangeCategoryBulkUpdateAction(_catalogService, _categoryMover, _productMover, changeCategoryActionContext);
            }
            else if (context is UpdatePropertiesActionContext updatePropertiesActionContext)
            {
                result = new UpdatePropertiesBulkUpdateAction(_bulkUpdatePropertyManager, updatePropertiesActionContext);
            }

            return result ?? throw new ArgumentException($"Unsupported action type: {context.GetType().Name}");
        }
    }
}
