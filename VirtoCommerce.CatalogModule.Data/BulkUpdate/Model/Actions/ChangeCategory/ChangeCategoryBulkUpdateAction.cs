using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Web.Model;
using VirtoCommerce.CatalogModule.Web.Services;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Common;
using domain = VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Model.Actions.ChangeCategory
{
    public class ChangeCategoryBulkUpdateAction : IBulkUpdateAction
    {
        private readonly IListEntryMover<domain.Category> _categoryMover;
        private readonly IListEntryMover<CatalogProduct> _productMover;
        private readonly ICatalogService _catalogService;


        public ChangeCategoryBulkUpdateAction(ICatalogService catalogService,
            IListEntryMover<domain.Category> categoryMover,
            IListEntryMover<CatalogProduct> productMover,
            BulkUpdateActionContext context)
        {
            _catalogService = catalogService;
            _categoryMover = categoryMover;
            _productMover = productMover;
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public BulkUpdateActionContext Context { get; protected set; }
        public IBulkUpdateActionData GetActionData()
        {
            return null;
        }

        public BulkUpdateActionResult Validate()
        {
            var result = BulkUpdateActionResult.Success;
            if (Context is ChangeCategoryActionContext changeCategoryContext)
            {
                var dstCatalog = _catalogService.GetById(changeCategoryContext.CatalogId);
                if (dstCatalog.IsVirtual)
                {
                    result.Succeeded = false;
                    result.Errors.Add("Unable to move in virtual catalog");
                }
            }
            else
            {
                throw new InvalidCastException(nameof(ChangeCategoryActionContext));
            }
            return result;
        }

        public BulkUpdateActionResult Execute(IEnumerable<IEntity> entities)
        {
            var changeCategoryContext = Context as ChangeCategoryActionContext ?? throw new InvalidCastException(nameof(ChangeCategoryActionContext));
            var listEntries = entities.Cast<ListEntry>().ToArray();

            var result = BulkUpdateActionResult.Success;
            var moveInfo = new MoveInfo()
            {
                Catalog = changeCategoryContext.CatalogId,
                Category = changeCategoryContext.CategoryId,
                ListEntries = listEntries,
            };

            var categories = _categoryMover.PrepareMove(moveInfo);
            var products = _productMover.PrepareMove(moveInfo);

            _categoryMover.ConfirmMove(categories);
            _productMover.ConfirmMove(products);

            return result;
        }
    }
}
