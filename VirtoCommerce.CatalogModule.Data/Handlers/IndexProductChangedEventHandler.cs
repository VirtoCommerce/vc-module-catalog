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
    public class IndexProductChangedEventHandler : IEventHandler<ProductChangedEvent>
    {
        private readonly IIndexingManager _indexingManager;

        public IndexProductChangedEventHandler(IIndexingManager indexingManager)
        {
            _indexingManager = indexingManager;
        }

        public Task Handle(ProductChangedEvent message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var indexedProductIds = message.ChangedEntries.Where(x => (x.EntryState == EntryState.Modified || x.EntryState == EntryState.Added) && x.OldEntry.Id != null)
                                                          .Select(x => x.OldEntry.Id)
                                                          .Distinct().ToArray();

            if (!indexedProductIds.IsNullOrEmpty())
            {
                BackgroundJob.Enqueue(() => TryIndexProductBackgroundJob(indexedProductIds));
            }

            var deletedProductIds = message.ChangedEntries.Where(x => x.EntryState == EntryState.Deleted && x.OldEntry.Id != null)
                                                          .Select(x => x.OldEntry.Id)
                                                          .Distinct().ToArray();

            if (!deletedProductIds.IsNullOrEmpty())
            {
                BackgroundJob.Enqueue(() => TryDeleteIndexProductBackgroundJob(deletedProductIds));
            }

            return Task.CompletedTask;
        }


        [DisableConcurrentExecution(60 * 60 * 24)]
        public Task TryIndexProductBackgroundJob(string[] indexedProductIds)
        {
            return TryIndexProduct(indexedProductIds);
        }

        [DisableConcurrentExecution(60 * 60 * 24)]
        public Task TryDeleteIndexProductBackgroundJob(string[] deletedProductIds)
        {
            return TryDeleteIndexProduct(deletedProductIds);
        }


        protected virtual Task TryIndexProduct(string[] indexedProductIds)
        {
            return _indexingManager.IndexDocumentsAsync(KnownDocumentTypes.Product, indexedProductIds);
        }

        protected virtual Task TryDeleteIndexProduct(string[] deletedProductIds)
        {
            return _indexingManager.DeleteDocumentsAsync(KnownDocumentTypes.Product, deletedProductIds);
        }
    }
}
