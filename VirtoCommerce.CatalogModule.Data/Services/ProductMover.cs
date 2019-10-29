using System.Collections.Generic;
using System.Linq;

using VirtoCommerce.CatalogModule.Web.Model;
using VirtoCommerce.CatalogModule.Web.Services;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Common;
using VC = VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class ProductMover : ListEntryMover<VC.CatalogProduct>
    {
        private readonly IItemService _itemService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductMover"/> class.
        /// </summary>
        /// <param name="itemService">
        /// The item service.
        /// </param>
        public ProductMover(IItemService itemService)
        {
            _itemService = itemService;
        }

        public override void ConfirmMove(IEnumerable<VC.CatalogProduct> entities)
        {
            _itemService.Update(entities.ToArray());
        }

        public override List<VC.CatalogProduct> PrepareMove(MoveContext moveContext)
        {
            var result = new List<VC.CatalogProduct>();

            foreach (var listEntryProduct in moveContext.ListEntries.Where(
                listEntry => listEntry.Type.EqualsInvariant(ListEntryProduct.TypeName)))
            {
                var product = _itemService.GetById(listEntryProduct.Id, VC.ItemResponseGroup.ItemLarge);
                if (product.CatalogId == moveContext.Catalog)
                {
                    // idle
                }
                else
                {
                    product.CatalogId = moveContext.Catalog;
                    product.CategoryId = null;
                    foreach (var variation in product.Variations)
                    {
                        variation.CatalogId = moveContext.Catalog;
                        variation.CategoryId = null;
                    }
                }

                if (product.CategoryId == moveContext.Category)
                {
                    // idle
                }
                else
                {
                    product.CategoryId = moveContext.Category;
                    foreach (var variation in product.Variations)
                    {
                        variation.CategoryId = moveContext.Category;
                    }
                }

                result.Add(product);
                result.AddRange(product.Variations);
            }

            return result;
        }
    }
}
