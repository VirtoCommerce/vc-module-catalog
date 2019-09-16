using System;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Model;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Model.Actions.ChangeCategory;
using VirtoCommerce.Domain.Catalog.Services;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Services
{
    public class BulkUpdateActionFactory : IBulkUpdateActionFactory
    {
        private readonly IItemService _itemService;
        private readonly ICatalogService _catalogService;

        public BulkUpdateActionFactory(IItemService itemService, ICatalogService catalogService)
        {
            _itemService = itemService;
            _catalogService = catalogService;
        }

        public IBulkUpdateAction Create(BulkUpdateActionContext context)
        {
            IBulkUpdateAction result = null;

            if (context is ChangeCategoryActionContext changeCategoryActionContext)
            {
                result = new ChangeCategoryBulkUpdateAction(_itemService, _catalogService, changeCategoryActionContext);
            }

            return result ?? throw new ArgumentException($"Unsupported action type: {context.GetType().Name}");
        }
    }
}
