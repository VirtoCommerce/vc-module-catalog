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

        public ChangeCategoryBulkUpdateAction(IItemService itemService, ICatalogService catalogService, BulkUpdateActionContext context)
        {
            _itemService = itemService;
            _catalogService = catalogService;
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
            var result = BulkUpdateActionResult.Success;

            var changeCategoryContext = Context as ChangeCategoryActionContext;

            if (changeCategoryContext == null)
            {
                throw new InvalidCastException(nameof(ChangeCategoryActionContext));
            }

            var products = new List<CatalogProduct>();
            //Move products
            foreach (var listEntryProduct in entities)
            {
                var product = _itemService.GetById(listEntryProduct.Id, ItemResponseGroup.ItemLarge);
                if (product.CatalogId != changeCategoryContext.CatalogId)
                {
                    product.CatalogId = changeCategoryContext.CatalogId;
                    product.CategoryId = null;
                    foreach (var variation in product.Variations)
                    {
                        variation.CatalogId = changeCategoryContext.CatalogId;
                        variation.CategoryId = null;
                    }

                }
                if (product.CategoryId != changeCategoryContext.CategoryId)
                {
                    product.CategoryId = changeCategoryContext.CategoryId;
                    foreach (var variation in product.Variations)
                    {
                        variation.CategoryId = changeCategoryContext.CategoryId;
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
