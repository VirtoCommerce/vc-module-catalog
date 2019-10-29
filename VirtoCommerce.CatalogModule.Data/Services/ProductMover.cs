using System.Collections.Generic;
using System.Linq;

using VirtoCommerce.CatalogModule.Web.Model;
using VirtoCommerce.CatalogModule.Web.Services;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Common;
using domain = VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class ProductMover : ListEntryMover<domain.CatalogProduct>
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

        public override void ConfirmMove(IEnumerable<domain.CatalogProduct> entities)
        {
            _itemService.Update(entities.ToArray());
        }

        public override List<domain.CatalogProduct> PrepareMove(MoveInfo moveInfo)
        {
            var result = new List<domain.CatalogProduct>();

            foreach (var listEntryProduct in moveInfo.ListEntries.Where(
                listEntry => listEntry.Type.EqualsInvariant(ListEntryProduct.TypeName)))
            {
                var product = _itemService.GetById(listEntryProduct.Id, domain.ItemResponseGroup.ItemLarge);
                if (product.CatalogId == moveInfo.Catalog)
                {
                    // idle
                }
                else
                {
                    product.CatalogId = moveInfo.Catalog;
                    product.CategoryId = null;
                    foreach (var variation in product.Variations)
                    {
                        variation.CatalogId = moveInfo.Catalog;
                        variation.CategoryId = null;
                    }
                }

                if (product.CategoryId == moveInfo.Category)
                {
                    // idle
                }
                else
                {
                    product.CategoryId = moveInfo.Category;
                    foreach (var variation in product.Variations)
                    {
                        variation.CategoryId = moveInfo.Category;
                    }
                }

                result.Add(product);
                result.AddRange(product.Variations);
            }

            return result;
        }
    }
}
