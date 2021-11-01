using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.Platform.Core.Settings;


namespace VirtoCommerce.CatalogModule2.Data.Services
{
    public class InternalListEntrySearchService2 : InternalListEntrySearchService
    {
        public InternalListEntrySearchService2(
            IProductIndexedSearchService productIndexedSearchService,
            ICategoryIndexedSearchService categoryIndexedSearchService,
            IListEntrySearchService listEntrySearchService,
            ISettingsManager settingsManager) : base(productIndexedSearchService, categoryIndexedSearchService, listEntrySearchService, settingsManager)
        {
        }
    }
}
