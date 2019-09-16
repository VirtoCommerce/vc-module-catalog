using System;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Model;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Model.Actions.ChangeCategory;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Model.Actions.UpdateProperties;
using VirtoCommerce.Domain.Catalog.Services;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Services
{
    public class BulkUpdateActionFactory : IBulkUpdateActionFactory
    {
        private readonly IItemService _itemService;
        private readonly ICatalogService _catalogService;
        private readonly IBulkUpdatePropertyManager _bulkUpdatePropertyManager;

        public BulkUpdateActionFactory(IItemService itemService, ICatalogService catalogService, IBulkUpdatePropertyManager bulkUpdatePropertyManager)
        {
            _itemService = itemService;
            _catalogService = catalogService;
            _bulkUpdatePropertyManager = bulkUpdatePropertyManager;
        }

        public IBulkUpdateAction Create(BulkUpdateActionContext context)
        {
            IBulkUpdateAction result = null;

            if (context is ChangeCategoryActionContext changeCategoryActionContext)
            {
                result = new ChangeCategoryBulkUpdateAction(_itemService, _catalogService, changeCategoryActionContext);
            }
            else if (context is UpdatePropertiesActionContext updatePropertiesActionContext)
            {
                result = new UpdatePropertiesBulkUpdateAction(_bulkUpdatePropertyManager, updatePropertiesActionContext);
            }

            return result ?? throw new ArgumentException($"Unsupported action type: {context.GetType().Name}");
        }
    }
}
