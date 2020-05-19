using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Events;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Data.BackgroundJobs;

namespace VirtoCommerce.CatalogModule.Data.Handlers
{
    public class IndexProductChangedEventHandler : IEventHandler<ProductChangedEvent>
    {
        private readonly ISettingsManager _settingsManager;

        public IndexProductChangedEventHandler(ISettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
        }

        public async Task Handle(ProductChangedEvent message)
        {
            if (await _settingsManager.GetValueAsync(ModuleConstants.Settings.General.EventBasedIndexation.Name, false))
            {
                if (message == null)
                {
                    throw new ArgumentNullException(nameof(message));
                }

                var indexEntries = message.ChangedEntries
                    .Select(x => new IndexEntry { Id = x.OldEntry.Id, EntryState = x.EntryState, Type = KnownDocumentTypes.Product })
                    .ToArray();

                IndexingJobs.EnqueueIndexAndDeleteDocuments(indexEntries);
            }
        }
    }
}
