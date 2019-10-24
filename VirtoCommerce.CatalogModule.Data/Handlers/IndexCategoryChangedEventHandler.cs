using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.Domain.Catalog.Events;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.CatalogModule.Data.Handlers
{
    public class IndexCategoryChangedEventHandler : IEventHandler<CategoryChangedEvent>
    {
        private readonly IIndexingManager _indexingManager;

        public IndexCategoryChangedEventHandler(IIndexingManager indexingManager)
        {
            _indexingManager = indexingManager;
        }

        public Task Handle(CategoryChangedEvent message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var indexedCategoryIds = message.ChangedEntries.Where(x => (x.EntryState == EntryState.Modified || x.EntryState == EntryState.Added) && x.OldEntry.Id != null)
                                                          .Select(x => x.OldEntry.Id)
                                                          .Distinct().ToArray();

            if (!indexedCategoryIds.IsNullOrEmpty())
            {
                BackgroundJob.Enqueue(() => TryIndexCategoryBackgroundJob(indexedCategoryIds));
            }

            var deletedCategoryIds = message.ChangedEntries.Where(x => x.EntryState == EntryState.Deleted && x.OldEntry.Id != null)
                                                          .Select(x => x.OldEntry.Id)
                                                          .Distinct().ToArray();

            if (!deletedCategoryIds.IsNullOrEmpty())
            {
                BackgroundJob.Enqueue(() => TryDeleteIndexCategoryBackgroundJob(deletedCategoryIds));
            }

            return Task.CompletedTask;
        }

        [DisableConcurrentExecution(60 * 60 * 24)]
        public async Task TryIndexCategoryBackgroundJob(string[] indexedCategoryIds)
        {
            await TryIndexCategory(indexedCategoryIds);
        }

        [DisableConcurrentExecution(60 * 60 * 24)]
        public async Task TryDeleteIndexCategoryBackgroundJob(string[] deletedCategoryIds)
        {
            await TryDeleteIndexCategory(deletedCategoryIds);
        }


        protected virtual async Task TryIndexCategory(string[] indexedCategoryIds)
        {
            await _indexingManager.IndexDocumentsAsync(KnownDocumentTypes.Category, indexedCategoryIds);
        }

        protected virtual async Task TryDeleteIndexCategory(string[] deletedCategoryIds)
        {
            await _indexingManager.DeleteDocumentsAsync(KnownDocumentTypes.Category, deletedCategoryIds);
        }
    }
}
