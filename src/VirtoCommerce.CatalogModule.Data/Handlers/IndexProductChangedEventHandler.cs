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
    public class IndexProductChangedEventHandler : IEventHandler<ProductChangedEvent>
    {
        private readonly IIndexingManager _indexingManager;

        public IndexProductChangedEventHandler(IIndexingManager indexingManager)
        {
            _indexingManager = indexingManager;
        }

        public async Task Handle(ProductChangedEvent message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var indexProductIds = message.ChangedEntries.Where(x => (x.EntryState == EntryState.Modified || x.EntryState == EntryState.Added) && x.OldEntry.Id != null)
                                                          .Select(x => x.OldEntry.Id)
                                                          .Distinct().ToArray();

            if (!indexProductIds.IsNullOrEmpty())
            {
                await TryIndexIndexCategoryBackgroundJob(indexProductIds);
            }
        }


        [DisableConcurrentExecution(60 * 60 * 24)]
        public async Task TryIndexIndexCategoryBackgroundJob(string[] indexProductIds)
        {
            await TryIndexIndexCategory(indexProductIds);
        }


        protected virtual async Task TryIndexIndexCategory(string[] indexProductIds)
        {
            await _indexingManager.IndexDocumentsAsync(KnownDocumentTypes.Product, indexProductIds);
        }
    }
}
