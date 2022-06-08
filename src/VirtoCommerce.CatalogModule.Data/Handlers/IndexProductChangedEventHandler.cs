using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Events;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Search.Indexing;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Data.BackgroundJobs;
using VirtoCommerce.SearchModule.Data.Services;

namespace VirtoCommerce.CatalogModule.Data.Handlers
{
    public class IndexProductChangedEventHandler : IEventHandler<ProductChangedEvent>
    {
        private readonly ISettingsManager _settingsManager;
        private readonly IEnumerable<IndexDocumentConfiguration> _configurations;

        public IndexProductChangedEventHandler(ISettingsManager settingsManager, IEnumerable<IndexDocumentConfiguration> configurations)
        {
            _settingsManager = settingsManager;
            _configurations = configurations;
        }

        public async Task Handle(ProductChangedEvent message)
        {
            if (!await _settingsManager.GetValueAsync(ModuleConstants.Settings.General.EventBasedIndexation.Name, false))
            {
                return;
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var indexEntries = new List<IndexEntry>();

            foreach (var changedEntry in message.ChangedEntries)
            {
                var changedProduct = changedEntry.OldEntry;

                indexEntries.Add(new IndexEntry
                {
                    Id = IsVariation(changedProduct) ? changedProduct.MainProductId : changedProduct.Id,
                    EntryState = IsVariation(changedProduct) ? EntryState.Added : changedEntry.EntryState, // Full indexation of main product once variation was modified
                    Type = KnownDocumentTypes.Product,
                });

                if (IsVariationCreatedOrDeleted(changedProduct, changedEntry.EntryState))
                {
                    indexEntries.Add(new IndexEntry
                    {
                        Id = changedProduct.Id,
                        EntryState = changedEntry.EntryState,
                        Type = KnownDocumentTypes.Product,
                    });
                }
            }

            IndexingJobs.EnqueueIndexAndDeleteDocuments(indexEntries.ToArray(), JobPriority.Normal, _configurations.GetBuildersForProvider(typeof(ProductDocumentChangesProvider)).ToList());
        }

        private static bool IsVariationCreatedOrDeleted(CatalogProduct catalogProduct, EntryState entryState)
            => IsVariation(catalogProduct) && entryState is EntryState.Added or EntryState.Deleted;

        private static bool IsVariation(CatalogProduct catalogProduct) => !string.IsNullOrEmpty(catalogProduct.MainProductId);
    }
}
