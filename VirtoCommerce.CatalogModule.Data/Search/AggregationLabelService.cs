using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Common;
using AggregationLabel = VirtoCommerce.CatalogModule.Web.Model.AggregationLabel;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class AggregationLabelService : IAggregationLabelService
    {
        private readonly IPropertyService _propertyService;

        public AggregationLabelService(IPropertyService propertyService)
        {
            _propertyService = propertyService;
        }

        public IList<AggregationLabel> GetPropertyLabels(string catalogId, string propertyName)
        {
            var allProperties = GetAllCatalogProperties(catalogId);

            // There can be many properties with the same name
            var properties = allProperties
                .Where(p => p.Name.EqualsInvariant(propertyName))
                .ToArray();

            var allLabels = properties
                .SelectMany(p => p.DisplayNames)
                .Select(n => new AggregationLabel { Language = n.LanguageCode, Label = n.Name })
                .ToArray();

            var result = GetDistinctLabels(allLabels);
            return result;
        }

        public IDictionary<string, IList<AggregationLabel>> GetPropertyValueLabels(string catalogId, string propertyName)
        {
            var allProperties = GetAllCatalogProperties(catalogId);

            // There can be many properties with the same name
            var properties = allProperties
                .Where(p => p.Name.EqualsInvariant(propertyName))
                .ToArray();

            // Get distinct labels for each alias
            var result = properties
                .Where(p => p.Dictionary && p.DictionaryValues != null && p.DictionaryValues.Any())
                .SelectMany(p => p.DictionaryValues)
                .GroupBy(v => v.Alias, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => GetDistinctLabels(g.Select(v => new AggregationLabel { Language = v.LanguageCode, Label = v.Value })), StringComparer.OrdinalIgnoreCase);

            return result.Any() ? result : null;
        }


        /// <summary>
        /// Returns first label for each language
        /// </summary>
        /// <param name="labels"></param>
        /// <returns></returns>
        private static IList<AggregationLabel> GetDistinctLabels(IEnumerable<AggregationLabel> labels)
        {
            var result = labels
                .Where(x => !string.IsNullOrEmpty(x.Language) && !string.IsNullOrEmpty(x.Label))
                .GroupBy(x => x.Language, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.FirstOrDefault())
                .OrderBy(x => x?.Language)
                .ThenBy(x => x.Label)
                .ToArray();

            return result.Any() ? result : null;
        }

        private IList<Property> GetAllCatalogProperties(string catalogId)
        {
            var properties = _propertyService.GetAllCatalogProperties(catalogId);

            var result = properties
                .GroupBy(p => p.Id, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.FirstOrDefault())
                .OrderBy(p => p?.Name)
                .ToArray();

            return result;
        }
    }
}
