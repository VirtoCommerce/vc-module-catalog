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

        public BulkUpdateActionFactory(ICatalogService catalogService,
            IListEntryMover<Category> categoryMover,
            IListEntryMover<CatalogProduct> productMover
            )
        {
            _catalogService = catalogService;
            _categoryMover = categoryMover;
            _productMover = productMover;
        }

        public IBulkUpdateAction Create(BulkUpdateActionContext context)
        {
            IBulkUpdateAction result = null;

            if (context is ChangeCategoryActionContext changeCategoryActionContext)
            {
                result = new ChangeCategoryBulkUpdateAction(_catalogService, _categoryMover, _productMover, changeCategoryActionContext);
            }

            return result ?? throw new ArgumentException($"Unsupported action type: {context.GetType().Name}");
        }
    }
}
