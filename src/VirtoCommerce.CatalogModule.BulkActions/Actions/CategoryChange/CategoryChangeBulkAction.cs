using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.BulkActionsModule.Core.Models.BulkActions;
using VirtoCommerce.BulkActionsModule.Core.Services;
using VirtoCommerce.CatalogModule.BulkActions.Models;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.ListEntry;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.BulkActions.Actions.CategoryChange
{
    public class CategoryChangeBulkAction : IBulkAction
    {
        private readonly CategoryChangeBulkActionContext _context;

        private readonly ICatalogService _catalogService;
        private readonly ListEntryMover<Category> _categoryListEntryMover;
        private readonly ListEntryMover<CatalogProduct> _productListEntryMover;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryChangeBulkAction"/> class.
        /// </summary>
        /// <param name="lazyServiceProvider">
        /// The service provider.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        public CategoryChangeBulkAction(
            CategoryChangeBulkActionContext context,
            ICatalogService catalogService,
            ListEntryMover<Category> categoryListEntryMover,
            ListEntryMover<CatalogProduct> productListEntryMover)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _catalogService = catalogService;
            _categoryListEntryMover = categoryListEntryMover;
            _productListEntryMover = productListEntryMover;
        }

        public BulkActionContext Context => _context;

        public async Task<BulkActionResult> ExecuteAsync(IEnumerable<IEntity> entities)
        {
            var entries = entities.Cast<ListEntryBase>().ToArray();
            var moveInfo = new ListEntriesMoveRequest
            {
                Catalog = _context.CatalogId,
                Category = _context.CategoryId,
                ListEntries = entries
            };

            ValidateMoveInfo(moveInfo);

            var categories = await _categoryListEntryMover.PrepareMoveAsync(moveInfo);
            var products = await _productListEntryMover.PrepareMoveAsync(moveInfo);

            await _categoryListEntryMover.ConfirmMoveAsync(categories);
            await _productListEntryMover.ConfirmMoveAsync(products);

            return BulkActionResult.Success;
        }

        public Task<object> GetActionDataAsync()
        {
            return Task.FromResult(default(object));
        }

        public async Task<BulkActionResult> ValidateAsync()
        {
            var result = BulkActionResult.Success;

            var dstCatalog = (await _catalogService.GetByIdsAsync(new [] { _context.CatalogId })).FirstOrDefault();
            if (dstCatalog != null && dstCatalog.IsVirtual)
            {
                result.Succeeded = false;
                result.Errors.Add("Unable to move in virtual catalog");
            }

            return result;
        }

        private static bool IsEqual(string a, string b)
        {
            if (a == b)
            {
                return true;
            }

            return false;
        }

        private static void ValidateMoveInfo(ListEntriesMoveRequest moveInfo)
        {
            var exception = new Exception("Cannot be moved to a subcategory or into the same category");
            foreach (var entry in moveInfo.ListEntries)
            {
                if (IsEqual(moveInfo.Category, entry.Id))
                {
                    throw exception;
                }
            }
        }
    }
}
