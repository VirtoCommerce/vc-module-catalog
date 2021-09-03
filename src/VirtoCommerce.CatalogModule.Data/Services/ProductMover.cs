using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.ListEntry;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class ProductMover : ListEntryMover<CatalogProduct>
    {
        private readonly IItemService _itemService;
        private readonly ICrudService<CatalogProduct> _itemServiceCrud;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductMover"/> class.
        /// </summary>
        /// <param name="itemService">
        /// The item service.
        /// </param>
        public ProductMover(IItemService itemService)
        {
            _itemService = itemService;
            _itemServiceCrud = (ICrudService<CatalogProduct>)itemService;
        }

        public override Task ConfirmMoveAsync(IEnumerable<CatalogProduct> entities)
        {
            return _itemServiceCrud.SaveChangesAsync(entities.ToArray());
        }

        public override async Task<List<CatalogProduct>> PrepareMoveAsync(ListEntriesMoveRequest moveInfo)
        {
            var result = new List<CatalogProduct>();

            foreach (var listEntryProduct in moveInfo.ListEntries.Where(
                listEntry => listEntry.Type.EqualsInvariant(ProductListEntry.TypeName)))
            {
                var product = await _itemService.GetByIdAsync(listEntryProduct.Id, ItemResponseGroup.ItemLarge.ToString());
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
