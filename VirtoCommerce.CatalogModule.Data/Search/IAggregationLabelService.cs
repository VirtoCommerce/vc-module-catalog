using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Web.Model;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public interface IAggregationLabelService
    {
        IList<AggregationLabel> GetPropertyLabels(string catalogId, string propertyName);
        IDictionary<string, IList<AggregationLabel>> GetPropertyValueLabels(string catalogId, string propertyName);
    }
}
