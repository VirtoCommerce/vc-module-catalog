using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Events;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Data.BackgroundJobs;

namespace VirtoCommerce.CatalogModule.Data.Handlers
{
    public class IndexCategoryChangedEventHandler : IEventHandler<CategoryChangedEvent>
    {
        public Task Handle(CategoryChangedEvent message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var indexEntries = message.ChangedEntries
                .Select(x => new IndexEntry { Id = x.OldEntry.Id, EntryState = x.EntryState, Type = KnownDocumentTypes.Category })
                .ToArray();

            IndexingJobs.EnqueueIndexAndDeleteDocuments(indexEntries);

            return Task.CompletedTask;
        }
    }
}
