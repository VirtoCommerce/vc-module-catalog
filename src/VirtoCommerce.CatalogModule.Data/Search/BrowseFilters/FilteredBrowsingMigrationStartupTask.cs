using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Model.Search;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Data.Search.BrowseFilters
{
    public class FilteredBrowsingMigrationStartupTask(
        IStoreSearchService storeSearchService,
        IStoreService storeService,
        ISettingsManager settingsManager)
    {
        private readonly IStoreSearchService _storeSearchService = storeSearchService;
        private readonly IStoreService _storeService = storeService;
        private readonly ISettingsManager _settingsManager = settingsManager;

        public virtual async Task RunAsync()
        {
            // Bail out if the migration was already completed in a previous run.
            if (await _settingsManager.GetValueAsync<bool>(ModuleConstants.Settings.Search.FilteredBrowsingMigrated))
            {
                return;
            }

            var settingDescriptor = ModuleConstants.Settings.Search.FilteredBrowsing;

#pragma warning disable VC0014 // Legacy constants kept for migration
            var legacyPropertyName = BrowseFilterService.FilteredBrowsingPropertyName;
#pragma warning restore VC0014

            var searchCriteria = new StoreSearchCriteria
            {
                ResponseGroup = (StoreResponseGroup.DynamicProperties | StoreResponseGroup.StoreInfo).ToString(),
            };

            await foreach (var batch in _storeSearchService.SearchBatchesNoCloneAsync(searchCriteria))
            {
                var storesToSave = new List<Store>();

                foreach (var store in batch.Results)
                {
                    var legacyValue = store.GetDynamicPropertyValue(legacyPropertyName, string.Empty);
                    if (string.IsNullOrEmpty(legacyValue))
                    {
                        continue;
                    }

                    store.Settings.First(x => x.Name.EqualsIgnoreCase(settingDescriptor.Name)).Value = legacyValue;

                    storesToSave.Add(store);
                }

                if (storesToSave.Count > 0)
                {
                    await _storeService.SaveChangesAsync(storesToSave);
                }
            }

            // Mark migration as complete so subsequent startups skip the scan entirely.
            await _settingsManager.SetValueAsync(ModuleConstants.Settings.Search.FilteredBrowsingMigrated.Name, true);
        }
    }
}
