using System.Collections.Generic;
using VirtoCommerce.Domain.Catalog.Model.Search;
using VirtoCommerce.Domain.Search;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public interface IAggregationRequestBuilder
    {
        IList<AggregationRequest> GetAggregationRequests(ProductSearchCriteria criteria, FiltersContainer allFilters);
    }
}
