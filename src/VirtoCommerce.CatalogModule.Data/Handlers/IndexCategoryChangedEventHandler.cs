using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Events;
using VirtoCommerce.CatalogModule.Data.Search.Indexing;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.BackgroundJobs;
using VirtoCommerce.SearchModule.Core.Extensions;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Data.Handlers
{
    public class IndexCategoryChangedEventHandler : IEventHandler<CategoryChangedEvent>
    {
        private readonly ISettingsManager _settingsManager;
        private readonly IIndexingJobService _indexingJobService;
        private readonly IEnumerable<IndexDocumentConfiguration> _configurations;

        public IndexCategoryChangedEventHandler(
            ISettingsManager settingsManager,
            IIndexingJobService indexingJobService,
            IEnumerable<IndexDocumentConfiguration> configurations)
        {
            _settingsManager = settingsManager;
            _configurations = configurations;
            _indexingJobService = indexingJobService;
        }

        public async Task Handle(CategoryChangedEvent message)
        {
            if (await _settingsManager.GetValueAsync<bool>(ModuleConstants.Settings.General.EventBasedIndexation))
            {
                if (message == null)
                {
                    throw new ArgumentNullException(nameof(message));
                }

                var indexEntries = message.ChangedEntries
                    .Select(x => new IndexEntry { Id = x.OldEntry.Id, EntryState = x.EntryState, Type = KnownDocumentTypes.Category })
                    .ToArray();

                _indexingJobService.EnqueueIndexAndDeleteDocuments(indexEntries, JobPriority.Normal,
                    _configurations.GetDocumentBuilders(KnownDocumentTypes.Category, typeof(CategoryDocumentChangesProvider)).ToList());
            }
        }
    }
}
