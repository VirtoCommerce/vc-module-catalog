using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogModule.Core.Events;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;

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

        public Task Handle(CategoryChangedEvent message)
        {
            var categoryIds = message.ChangedEntries
                .Where(x =>
                    x.EntryState == EntryState.Modified &&
                    (x.OldEntry?.CatalogId != x.NewEntry?.CatalogId ||
                    x.OldEntry?.ParentId != x.NewEntry?.ParentId ||
                    x.OldEntry?.Links?.Count != x.NewEntry?.Links?.Count) ||
                    (x.OldEntry?.IsActive != x.NewEntry?.IsActive))
                .Select(x => x.NewEntry.Id)
                .ToList();

            if (categoryIds.Any())
            {
                BackgroundJob.Enqueue(() => UpdateProductsAsync(categoryIds));
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Resave products to update ModifiedDate:
        /// a workaround to make ProductDocumentChangesProvider track changes in product hierarchy or visibility
        /// </summary>
        [DisableConcurrentExecution(10)]
        public async Task UpdateProductsAsync(List<string> categoryIds)
        {
            using (var repository = _catalogRepositoryFactory())
            {
                var childrenCategoryIds = await repository.GetAllChildrenCategoriesIdsAsync(categoryIds.ToArray());
                categoryIds.AddRange(childrenCategoryIds);
                var childrenProductIds = await repository.Items.Where(x => categoryIds.Contains(x.CategoryId)).Select(x => x.Id).ToListAsync();

                var products = await _itemService.GetAsync(childrenProductIds.ToList(), ItemResponseGroup.ItemInfo.ToString());
                await _itemService.SaveChangesAsync(products);
            }
        }
    }
}
