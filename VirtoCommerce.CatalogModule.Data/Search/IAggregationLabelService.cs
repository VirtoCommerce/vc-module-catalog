using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Web.Model;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public interface IAggregationLabelService
    {
        void AddLabels(IList<Aggregation> aggregations, string catalogId);
    }
}
