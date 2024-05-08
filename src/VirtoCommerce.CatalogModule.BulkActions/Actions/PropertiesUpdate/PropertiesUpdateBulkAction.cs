using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.BulkActionsModule.Core.Models.BulkActions;
using VirtoCommerce.BulkActionsModule.Core.Services;
using VirtoCommerce.CatalogModule.BulkActions.Models;
using VirtoCommerce.CatalogModule.BulkActions.Services;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.ListEntry;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.BulkActions.Actions.PropertiesUpdate
{
    public class PropertiesUpdateBulkAction : IBulkAction
    {
        private readonly PropertiesUpdateBulkActionContext _context;
        private readonly IProductService _productService;
        private readonly IBulkPropertyUpdateManager _bulkPropertyUpdateManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertiesUpdateBulkAction"/> class.
        /// </summary>
        /// <param name="lazyServiceProvider">
        /// The lazy service provider.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        public PropertiesUpdateBulkAction(PropertiesUpdateBulkActionContext context, IProductService productService, IBulkPropertyUpdateManager bulkPropertyUpdateManager)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _productService = productService;
            _bulkPropertyUpdateManager = bulkPropertyUpdateManager;
        }

        public BulkActionContext Context => _context;

        public async Task<BulkActionResult> ExecuteAsync(IEnumerable<IEntity> entities)
        {
            var entries = entities.Cast<ListEntryBase>().ToArray();

            if (entries.Any(entry => !entry.Type.EqualsInvariant(ProductListEntry.TypeName)))
            {
                throw new ArgumentException($"{GetType().Name} could be applied to product entities only.");
            }

            var productQuery = entries.Where(entry => entry.Type.EqualsInvariant(ProductListEntry.TypeName));
            var productIds = productQuery.Select(entry => entry.Id).ToList();
            var products = await _productService.GetAsync(productIds, (ItemResponseGroup.ItemInfo | ItemResponseGroup.ItemProperties).ToString());

            return await _bulkPropertyUpdateManager.UpdatePropertiesAsync(products?.ToArray(), _context.Properties);
        }

        public async Task<object> GetActionDataAsync()
        {
            var properties = await _bulkPropertyUpdateManager.GetPropertiesAsync(_context);

            return new { Properties = properties };
        }

        public Task<BulkActionResult> ValidateAsync()
        {
            return Task.FromResult(BulkActionResult.Success);
        }
    }
}
