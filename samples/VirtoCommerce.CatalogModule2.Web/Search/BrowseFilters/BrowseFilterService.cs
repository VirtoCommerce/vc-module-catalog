using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Data.Search.BrowseFilters;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.CatalogModule2.Data.Search.BrowseFilters
{
    public class BrowseFilterService2 : BrowseFilterService
    {
        public BrowseFilterService2(IStoreService storeService) : base(storeService)
        {
        }
        protected override Task<IList<IBrowseFilter>> GetAllAggregations(ProductIndexedSearchCriteria criteria)
        {
            return base.GetAllAggregations(criteria);
        }
        protected override Task<string> GetSerializedValue(string storeId)
        {
            return base.GetSerializedValue(storeId);
        }
        public override Task<IList<IBrowseFilter>> GetStoreAggregationsAsync(string storeId)
        {
            return base.GetStoreAggregationsAsync(storeId);
        }
        protected override Task SaveSerializedValue(string storeId, string serializedValue)
        {
            return base.SaveSerializedValue(storeId, serializedValue);
        }
        public override Task SaveStoreAggregationsAsync(string storeId, IList<IBrowseFilter> filters)
        {
            return base.SaveStoreAggregationsAsync(storeId, filters);
        }
    }
}
