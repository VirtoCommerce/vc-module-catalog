using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Web.Core.Model;

namespace VirtoCommerce.CatalogModule.Web.Services
{
    public interface IAggregationPropertyService
    {
        AggregationProperty[] GetAllCatalogProperties(string catalogId, IEnumerable<string> currencies);
    }
}
