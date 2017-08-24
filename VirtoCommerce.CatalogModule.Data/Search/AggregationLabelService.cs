using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Common;
using Aggregation = VirtoCommerce.CatalogModule.Web.Model.Aggregation;
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

        public void AddLabels(IList<Aggregation> aggregations, string catalogId)
        {
            var allProperties = GetAllCatalogProperties(catalogId);

            foreach (var aggregation in aggregations)
            {
                // There can be many properties with the same name
                var properties = allProperties
                    .Where(p => p.Name.EqualsInvariant(aggregation.Field))
                    .ToArray();

                var allPropertyLabels = properties
                    .SelectMany(p => p.DisplayNames)
                    .Select(n => new AggregationLabel { Language = n.LanguageCode, Label = n.Name })
                    .ToArray();

                aggregation.Labels = GetDistinctLabels(allPropertyLabels);

                // Get distinct labels for each dictionary value alias
                var allValueLabels = properties
                    .Where(p => p.Dictionary && p.DictionaryValues != null && p.DictionaryValues.Any())
                    .SelectMany(p => p.DictionaryValues)
                    .GroupBy(v => v.Alias, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => GetDistinctLabels(g.Select(v => new AggregationLabel { Language = v.LanguageCode, Label = v.Value })), StringComparer.OrdinalIgnoreCase);

                foreach (var aggregationItem in aggregation.Items)
                {
                    var valueId = aggregationItem.Value.ToString();
                    aggregationItem.Labels = allValueLabels.ContainsKey(valueId) ? allValueLabels[valueId] : null;
                }
            }
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
