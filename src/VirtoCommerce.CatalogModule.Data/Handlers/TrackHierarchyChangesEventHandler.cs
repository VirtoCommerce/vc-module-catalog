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
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.CatalogModule.Data.Handlers
{
    public class TrackHierarchyChangesEventHandler : IEventHandler<CategoryChangedEvent>
    {
        private readonly Func<ICatalogRepository> _catalogRepositoryFactory;
        private readonly IItemService _itemService;

        public TrackHierarchyChangesEventHandler(Func<ICatalogRepository> catalogRepositoryFactory, IItemService itemService)
        {
            _catalogRepositoryFactory = catalogRepositoryFactory;
            _itemService = itemService;
        }

        public Task Handle(CategoryChangedEvent message)
        {
            var categories = message.ChangedEntries
                .Where(x => x.OldEntry?.CatalogId != x.NewEntry?.CatalogId || x.OldEntry?.ParentId != x.NewEntry?.ParentId)
                .Select(x => x.NewEntry.Id)
                .ToList();

            BackgroundJob.Enqueue(() => UpdateProductsAsync(categories));

            return Task.CompletedTask;
        }

        [DisableConcurrentExecution(10)]
        public async Task UpdateProductsAsync(List<string> categoryIds)
        {
            using (var repository = _catalogRepositoryFactory())
            {
                var childrenCategoryIds = await repository.GetAllChildrenCategoriesIdsAsync(categoryIds.ToArray());
                categoryIds.AddRange(childrenCategoryIds);
                var childrenProductIds = await repository.Items.Where(x => categoryIds.Contains(x.CategoryId)).Select(x => x.Id).ToListAsync();

                var products = await _itemService.GetByIdsAsync(childrenProductIds.ToArray(), ItemResponseGroup.ItemInfo.ToString());
                await _itemService.SaveChangesAsync(products);
            }
        }
    }
}
