using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Model.Actions.ChangeCategory
{
    public class ChangeCategoryBulkUpdateAction : IBulkUpdateAction
    {
        private readonly IItemService _itemService;
        private readonly ICatalogService _catalogService;
        private readonly ChangeCategoryActionContext _context;

        public ChangeCategoryBulkUpdateAction(IItemService itemService, ICatalogService catalogService, ChangeCategoryActionContext context)
        {
            _itemService = itemService;
            _catalogService = catalogService;
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
            var result = BulkUpdateActionResult.Success;

            var products = new List<CatalogProduct>();
            //Move products
            foreach (var listEntryProduct in entities)
            {
                var product = _itemService.GetById(listEntryProduct.Id, ItemResponseGroup.ItemLarge);
                if (product.CatalogId != _context.CatalogId)
                {
                    product.CatalogId = _context.CatalogId;
                    product.CategoryId = null;
                    foreach (var variation in product.Variations)
                    {
                        variation.CatalogId = _context.CatalogId;
                        variation.CategoryId = null;
                    }

                }
                if (product.CategoryId != _context.CategoryId)
                {
                    product.CategoryId = _context.CategoryId;
                    foreach (var variation in product.Variations)
                    {
                        variation.CategoryId = _context.CategoryId;
                    }
                }
                products.Add(product);
                products.AddRange(product.Variations);
            }

            if (products.Any())
            {
                _itemService.Update(products.ToArray());
            }

            return result;
        }
    }
}
