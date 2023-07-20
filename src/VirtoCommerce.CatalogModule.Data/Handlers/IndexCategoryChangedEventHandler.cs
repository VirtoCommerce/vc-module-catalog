using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Events;
using VirtoCommerce.CatalogModule.Data.Repositories;
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
    public class IndexCategoryChangedEventHandler : IEventHandler<CategoryChangedEvent>
    {
        private readonly ISettingsManager _settingsManager;
        private readonly IEnumerable<IndexDocumentConfiguration> _configurations;
        private readonly Func<ICatalogRepository> _catalogRepositoryFactory;

        public IndexCategoryChangedEventHandler(Func<ICatalogRepository> catalogRepositoryFactory, ISettingsManager settingsManager, IEnumerable<IndexDocumentConfiguration> configurations)
        {
            _catalogRepositoryFactory = catalogRepositoryFactory;
            _settingsManager = settingsManager;
            _configurations = configurations;
        }

        public async Task Handle(CategoryChangedEvent message)
        {
            if (await _settingsManager.GetValueAsync(ModuleConstants.Settings.General.EventBasedIndexation.Name, false))
            {
                if (message == null)
                {
                    throw new ArgumentNullException(nameof(message));
                }

                var indexEntries = message.ChangedEntries
                    .Select(x => new IndexEntry { Id = x.OldEntry.Id, EntryState = x.EntryState, Type = KnownDocumentTypes.Category })
                    .ToArray();

                var visibilityChangedCategoryIds = message.ChangedEntries
                    .Where(x =>
                        x.EntryState == EntryState.Modified &&
                        x.OldEntry?.IsActive != x.NewEntry?.IsActive)
                    .Select(x => x.NewEntry.Id)
                    .ToArray();

                if (visibilityChangedCategoryIds.Any())
                {
                    using var repository = _catalogRepositoryFactory();

                    var childrenCategoryIds = await repository.GetAllChildrenCategoriesIdsAsync(visibilityChangedCategoryIds);
                    var childrenCategoryEntireis = childrenCategoryIds.Select(x => new IndexEntry { Id = x, EntryState = EntryState.Modified, Type = KnownDocumentTypes.Category });
                    indexEntries = indexEntries
                        .Concat(childrenCategoryEntireis)
                        .ToArray();
                }

                IndexingJobs.EnqueueIndexAndDeleteDocuments(indexEntries,
                    JobPriority.Normal, _configurations.GetBuildersForProvider(typeof(CategoryDocumentChangesProvider)).ToList());
            }
        }
    }
}
