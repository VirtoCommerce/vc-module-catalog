using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.CatalogModule.Core.Events;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

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

            var indexCategoryIds = message.ChangedEntries.Where(x => (x.EntryState == EntryState.Modified || x.EntryState == EntryState.Added) && x.OldEntry.Id != null)
                                                          .Select(x => x.OldEntry.Id)
                                                          .Distinct().ToArray();

            if (!indexCategoryIds.IsNullOrEmpty())
            {
                BackgroundJob.Enqueue(() => TryIndexCategoryBackgroundJob(indexCategoryIds));
            }

            var deleteIndexCategoryIds = message.ChangedEntries.Where(x => x.EntryState == EntryState.Deleted && x.OldEntry.Id != null)
                                                          .Select(x => x.OldEntry.Id)
                                                          .Distinct().ToArray();

            if (!deleteIndexCategoryIds.IsNullOrEmpty())
            {
                BackgroundJob.Enqueue(() => TryDeleteIndexCategoryBackgroundJob(deleteIndexCategoryIds));
            }

            return Task.CompletedTask;
        }

        [DisableConcurrentExecution(60 * 60 * 24)]
        public async Task TryIndexCategoryBackgroundJob(string[] indexCategoryIds)
        {
            await TryIndexCategory(indexCategoryIds);
        }

        [DisableConcurrentExecution(60 * 60 * 24)]
        public async Task TryDeleteIndexCategoryBackgroundJob(string[] indexCategoryIds)
        {
            await TryDeleteIndexCategory(indexCategoryIds);
        }


        protected virtual async Task TryIndexCategory(string[] indexCategoryIds)
        {
            await _indexingManager.IndexDocumentsAsync(KnownDocumentTypes.Category, indexCategoryIds);
        }

        protected virtual async Task TryDeleteIndexCategory(string[] indexCategoryIds)
        {
            await _indexingManager.DeleteDocumentsAsync(KnownDocumentTypes.Category, indexCategoryIds);
        }
    }
}
