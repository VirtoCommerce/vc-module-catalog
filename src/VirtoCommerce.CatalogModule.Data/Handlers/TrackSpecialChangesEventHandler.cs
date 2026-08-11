using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogModule.Core.Events;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Jobs;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Jobs;

namespace VirtoCommerce.CatalogModule.Data.Handlers
{
    public sealed class TrackSpecialChangesEventHandler : IEventHandler<CategoryChangedEvent>
    {
        private readonly Func<ICatalogRepository> _catalogRepositoryFactory;
        private readonly IItemService _itemService;

        public TrackSpecialChangesEventHandler(Func<ICatalogRepository> catalogRepositoryFactory, IItemService itemService)
        {
            _catalogRepositoryFactory = catalogRepositoryFactory;
            _itemService = itemService;
        }

        public async Task Handle(CategoryChangedEvent message)
        {
            var categoryIds = message.ChangedEntries
                .Where(IsHierarchyOrVisibilityChanged)
                .Select(x => x.NewEntry.Id)
                .ToList();

            if (categoryIds.Count > 0)
            {
                var payload = AbstractTypeFactory<UpdateProductsJobPayload>.TryCreateInstance();
                payload.CategoryIds = categoryIds;

                // Event handlers are resolved from the root provider, so avoid capturing a scoped job service.
                await BackgroundJob.Enqueue<UpdateProductsJobHandler>(payload);
            }
        }

        private static bool IsHierarchyOrVisibilityChanged(GenericChangedEntry<Category> entry)
        {
            return entry.EntryState == EntryState.Modified
                && (entry.OldEntry?.CatalogId != entry.NewEntry?.CatalogId
                    || entry.OldEntry?.ParentId != entry.NewEntry?.ParentId
                    || entry.OldEntry?.Links?.Count != entry.NewEntry?.Links?.Count
                    || entry.OldEntry?.IsActive != entry.NewEntry?.IsActive);
        }

        /// <summary>
        /// Resave products to update ModifiedDate:
        /// a workaround to make ProductDocumentChangesProvider track changes in product hierarchy or visibility
        /// </summary>
        public async Task UpdateProductsAsync(List<string> categoryIds)
        {
            using var repository = _catalogRepositoryFactory();

            var childrenCategoryIds = await repository.GetAllChildrenCategoriesIdsAsync(categoryIds.ToArray());
            categoryIds.AddRange(childrenCategoryIds);
            var childrenProductIds = await repository.Items.Where(x => categoryIds.Contains(x.CategoryId)).Select(x => x.Id).ToListAsync();

            var products = await _itemService.GetAsync(childrenProductIds.ToList(), ItemResponseGroup.ItemInfo.ToString());
            await _itemService.SaveChangesAsync(products);
        }
    }
}
