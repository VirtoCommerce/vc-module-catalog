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
        private readonly ChangeCategoryActionContext _context;


        public ChangeCategoryBulkUpdateAction(ICatalogService catalogService,
            IListEntryMover<domain.Category> categoryMover,
            IListEntryMover<CatalogProduct> productMover,
            ChangeCategoryActionContext context)
        {
            _catalogService = catalogService;
            _categoryMover = categoryMover;
            _productMover = productMover;
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public BulkUpdateActionContext Context => _context;
        public IBulkUpdateActionData GetActionData()
        {
            return null;
        }

        public BulkUpdateActionResult Validate()
        {
            var result = BulkUpdateActionResult.Success;

            var dstCatalog = _catalogService.GetById(_context.CatalogId);
            if (dstCatalog.IsVirtual)
            {
                result.Succeeded = false;
                result.Errors.Add("Unable to move in virtual catalog");
            }

            return result;
        }

        public BulkUpdateActionResult Execute(IEnumerable<IEntity> entities)
        {
            var listEntries = entities.Cast<ListEntry>().ToArray();

            var result = BulkUpdateActionResult.Success;
            var moveInfo = new MoveInfo()
            {
                Catalog = _context.CatalogId,
                Category = _context.CategoryId,
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
