using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CoreModule.Data.Indexing.BackgroundJobs;
using VirtoCommerce.Domain.Catalog.Events;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.CatalogModule.Data.Handlers
{
    public class IndexProductChangedEventHandler : IEventHandler<ProductChangedEvent>
    {
        public Task Handle(ProductChangedEvent message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var indexEntries = message.ChangedEntries
                .Select(x => new IndexEntry { Id = x.OldEntry.Id, EntryState = x.EntryState, Type = KnownDocumentTypes.Product })
                .ToArray();

            IndexingJobs.EnqueueIndexAndDeleteDocuments(indexEntries);

            return Task.CompletedTask;
        }
    }
}
