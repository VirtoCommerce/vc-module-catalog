using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Web.Core.Model;
using VirtoCommerce.CatalogModule.Web.Services;
using VirtoCommerce.Domain.Catalog.Services;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class AggregationPropertyService : IAggregationPropertyService
    {
        private const string _attributeType = "Attribute";
        private const string _priceRangeType = "PriceRange";

        private readonly IPropertyService _propertyService;

        public AggregationPropertyService(IPropertyService propertyService)
        {
            _propertyService = propertyService;
        }

        public virtual AggregationProperty[] GetAllCatalogProperties(string catalogId, IEnumerable<string> currencies)
        {
            var result = _propertyService.GetAllCatalogProperties(catalogId)
                .Select(p => new AggregationProperty { Type = _attributeType, Name = p.Name })
                .ToList();

            result.AddRange(currencies.Select(c => new AggregationProperty { Type = _priceRangeType, Name = $"Price {c}", Currency = c }));

            result.Add(new AggregationProperty { Type = _attributeType, Name = $"__outline" });
            return result.ToArray();
        }
    }
}
