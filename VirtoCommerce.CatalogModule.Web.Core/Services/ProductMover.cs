using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Web.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Common;
using domain = VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogModule.Web.Services
{
    public class ProductMover : IListEntryMover<domain.CatalogProduct>
    {
        private readonly IItemService _itemService;

        public ProductMover(IItemService itemService)
        {
            _itemService = itemService;
        }

        public void ConfirmMove(IEnumerable<domain.CatalogProduct> entities)
        {
            if (entities.Any())
            {
                _itemService.Update(entities.ToArray());
            }
        }

        public List<domain.CatalogProduct> PrepareMove(MoveInfo moveInfo)
        {
            var result = new List<domain.CatalogProduct>();

            foreach (var listEntryProduct in moveInfo.ListEntries.Where(x => x.Type.EqualsInvariant(ListEntryProduct.TypeName)))
            {
                var product = _itemService.GetById(listEntryProduct.Id, domain.ItemResponseGroup.ItemLarge);
                if (product.CatalogId != moveInfo.Catalog)
                {
                    product.CatalogId = moveInfo.Catalog;
                    product.CategoryId = null;
                    foreach (var variation in product.Variations)
                    {
                        variation.CatalogId = moveInfo.Catalog;
                        variation.CategoryId = null;
                    }

                }
                if (product.CategoryId != moveInfo.Category)
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
