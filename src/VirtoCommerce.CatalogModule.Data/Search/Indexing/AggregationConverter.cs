using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Common;
using VirtoCommerce.CatalogModule.Core.Extensions;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Outlines;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Search.BrowseFilters;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Extensions;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Extensions;
using VirtoCommerce.StoreModule.Core.Services;
using static VirtoCommerce.StoreModule.Core.ModuleConstants.Settings.SEO;
using Aggregation = VirtoCommerce.CatalogModule.Core.Model.Search.Aggregation;
using AggregationItem = VirtoCommerce.CatalogModule.Core.Model.Search.AggregationItem;
using AggregationLabel = VirtoCommerce.CatalogModule.Core.Model.Search.AggregationLabel;
using RangeFilter = VirtoCommerce.CatalogModule.Data.Search.BrowseFilters.RangeFilter;
using RangeFilterValue = VirtoCommerce.CatalogModule.Data.Search.BrowseFilters.RangeFilterValue;

namespace VirtoCommerce.CatalogModule.Data.Search.Indexing
{
    public class AggregationConverter : IAggregationConverter
    {
        private const string OutlineAggregationFieldName = "__outline";
        private const string OutlineNamedAggregationFieldName = "__outline_named";
        private const string PathAggregationFieldName = "__path";

        private readonly IBrowseFilterService _browseFilterService;
        private readonly IPropertyService _propertyService;
        private readonly IPropertyDictionaryItemSearchService _propDictItemsSearchService;
        private readonly ICategoryService _categoryService;
        private readonly IStoreService _storeService;
        private readonly ICatalogService _catalogService;

        public AggregationConverter(
            IBrowseFilterService browseFilterService,
            IPropertyService propertyService,
            IPropertyDictionaryItemSearchService propDictItemsSearchService,
            ICategoryService categoryService,
            IStoreService storeService,
            ICatalogService catalogService)
        {
            _browseFilterService = browseFilterService;
            _propertyService = propertyService;
            _propDictItemsSearchService = propDictItemsSearchService;
            _categoryService = categoryService;
            _storeService = storeService;
            _catalogService = catalogService;
        }

        #region Request converter

        public async Task<IList<AggregationRequest>> GetAggregationRequestsAsync(ProductIndexedSearchCriteria criteria, FiltersContainer allFilters)
        {
            var result = new List<AggregationRequest>();

            var browseFilters = await _browseFilterService.GetBrowseFiltersAsync(criteria);
            if (browseFilters != null)
            {
                foreach (var filter in browseFilters)
                {
                    var existingFilters = allFilters.GetFiltersExceptSpecified(filter.Key);

                    AggregationRequest aggregationRequest = null;
                    IList<AggregationRequest> aggregationRequests = null;

                    switch (filter)
                    {
                        case AttributeFilter attributeFilter:
                            aggregationRequest = GetAttributeFilterAggregationRequest(attributeFilter, existingFilters);
                            break;
                        case RangeFilter rangeFilter:
                            aggregationRequests = GetRangeFilterAggregationRequests(rangeFilter, existingFilters);
                            break;
                        case PriceRangeFilter priceRangeFilter when priceRangeFilter.Currency.EqualsIgnoreCase(criteria.Currency):
                            aggregationRequests = GetPriceRangeFilterAggregationRequests(priceRangeFilter, criteria, existingFilters);
                            break;
                    }

                    if (aggregationRequest != null)
                    {
                        result.Add(aggregationRequest);
                    }

                    if (aggregationRequests != null)
                    {
                        result.AddRange(aggregationRequests.Where(f => f != null));
                    }
                }
            }

            return result;
        }

        protected virtual AggregationRequest GetAttributeFilterAggregationRequest(AttributeFilter attributeFilter, IEnumerable<IFilter> existingFilters)
        {
            return new TermAggregationRequest
            {
                FieldName = attributeFilter.GetIndexFieldName(),
                Values = !attributeFilter.Values.IsNullOrEmpty() ? attributeFilter.Values.Select(v => v.Id).ToArray() : null,
                Filter = existingFilters.And(),
                Size = attributeFilter.FacetSize,
            };
        }

        protected virtual IList<AggregationRequest> GetRangeFilterAggregationRequests(RangeFilter rangeFilter, IList<IFilter> existingFilters)
        {
            var result = rangeFilter.Values?.Select(v => GetRangeFilterValueAggregationRequest(rangeFilter.GetIndexFieldName(), v, existingFilters)).ToList();
            return result;
        }

        protected virtual AggregationRequest GetRangeFilterValueAggregationRequest(string fieldName, RangeFilterValue value, IEnumerable<IFilter> existingFilters)
        {
            var valueFilter = FiltersHelper.CreateRangeFilter(fieldName, value.Lower, value.Upper, value.IncludeLower, value.IncludeUpper);

            var result = new TermAggregationRequest
            {
                Id = $"{fieldName}-{value.Id}",
                Filter = existingFilters.And(valueFilter),
            };

            return result;
        }

        protected virtual IList<AggregationRequest> GetPriceRangeFilterAggregationRequests(PriceRangeFilter priceRangeFilter, ProductIndexedSearchCriteria criteria, IList<IFilter> existingFilters)
        {
            var priceRangeFilters = priceRangeFilter.Values?.Select(v => GetPriceRangeFilterValueAggregationRequest(priceRangeFilter, v, existingFilters, criteria.Pricelists)).ToList();
            if (priceRangeFilters == null)
            {
                return new List<AggregationRequest>();
            }

            var commonFieldName = StringsHelper.JoinNonEmptyStrings("_", "price", priceRangeFilter.Currency).ToLowerInvariant();
            var ranges = priceRangeFilters.OfType<RangeAggregationRequest>().SelectMany(x => x.Values).ToList();
            var ids = string.Join('-', ranges.Select(x => x.Id));

            var result = new RangeAggregationRequest
            {
                Id = $"{priceRangeFilter.Key}-{ids}",
                Filter = existingFilters.And(),
                FieldName = commonFieldName,
                Values = ranges,
            };

            return new List<AggregationRequest> { result };
        }

        protected virtual AggregationRequest GetPriceRangeFilterValueAggregationRequest(PriceRangeFilter priceRangeFilter, RangeFilterValue value, IEnumerable<IFilter> existingFilters, IList<string> pricelists)
        {
            var result = new RangeAggregationRequest
            {
                Values = new List<RangeAggregationRequestValue>
                {
                    new RangeAggregationRequestValue
                    {
                        Id = value.Id,
                        IncludeLower = value.IncludeLower,
                        IncludeUpper = value.IncludeUpper,
                        Lower = value.Lower,
                        Upper = value.Upper,
                    },
                },
            };

            return result;
        }

        #endregion

        #region Response converter

        public virtual async Task<Aggregation[]> ConvertAggregationsAsync(IList<AggregationResponse> aggregationResponses, ProductIndexedSearchCriteria criteria)
        {
            if (aggregationResponses == null || !aggregationResponses.Any())
            {
                return [];
            }

            var result = new List<Aggregation>();

            var termNames = aggregationResponses.Select(r => r.Id).Distinct().ToList();

            var browseFilters = await _browseFilterService.GetBrowseFiltersAsync(criteria);
            if (browseFilters != null)
            {
                foreach (var filter in browseFilters)
                {
                    Aggregation aggregation = null;

                    switch (filter)
                    {
                        case AttributeFilter attributeFilter:
                            PreFilterOutlineAggregation(attributeFilter, aggregationResponses, criteria);
                            await FilterLinkedCategories(attributeFilter, aggregationResponses, criteria);
                            aggregation = GetAttributeAggregation(attributeFilter, aggregationResponses);
                            termNames.Remove(attributeFilter.GetIndexFieldName());
                            break;
                        case RangeFilter rangeFilter:
                            aggregation = GetRangeAggregation(rangeFilter, aggregationResponses);
                            RemoveRange(termNames, rangeFilter.GetIndexFieldName());
                            break;
                        case PriceRangeFilter priceRangeFilter:
                            aggregation = GetPriceRangeAggregation(priceRangeFilter, aggregationResponses);
                            RemoveRange(termNames, priceRangeFilter.Key);
                            break;
                    }

                    if (aggregation?.Items?.Any() == true)
                    {
                        result.Add(aggregation);
                    }

                    termNames.Remove(filter.Key);
                }
            }

            if (termNames.Any())
            {
                foreach (var fieldName in termNames)
                {
                    var aggregation = GetAttributeAggregation(new AttributeFilter { Key = fieldName }, aggregationResponses);

                    if (aggregation?.Items?.Any() == true)
                    {
                        result.Add(aggregation);
                    }
                }
            }

            // Add localized labels for names and values
            if (result.Any())
            {
                await AddLabelsAsync(result, criteria.CatalogId, browseFilters);
            }

            return result.ToArray();
        }

        private static void RemoveRange(List<string> terms, string fieldName)
        {
            terms.RemoveAll(term => term.StartsWith($"{fieldName}-", StringComparison.OrdinalIgnoreCase));
        }

        protected virtual Aggregation GetAttributeAggregation(AttributeFilter attributeFilter, IList<AggregationResponse> aggregationResponses)
        {
            Aggregation result = null;

            var fieldName = attributeFilter.GetIndexFieldName();
            var aggregationResponse = aggregationResponses.FirstOrDefault(a => a.Id.EqualsIgnoreCase(fieldName));

            if (aggregationResponse != null)
            {
                IList<AggregationResponseValue> aggregationResponseValues;

                if (attributeFilter.Values.IsNullOrEmpty())
                {
                    // Return all values
                    aggregationResponseValues = aggregationResponse.Values;
                }
                else
                {
                    // Return predefined values
                    aggregationResponseValues = attributeFilter.Values
                        .GroupBy(v => v.Id, StringComparer.OrdinalIgnoreCase)
                        .Select(g => aggregationResponse.Values.FirstOrDefault(v => v.Id.EqualsIgnoreCase(g.Key)))
                        .Where(v => v != null)
                        .ToArray();
                }

                if (aggregationResponseValues.Any())
                {
                    result = new Aggregation
                    {
                        AggregationType = "attr",
                        Field = attributeFilter.GetIndexFieldName(),
                        Items = GetAttributeAggregationItems(aggregationResponseValues).ToArray(),
                    };
                }
            }

            return result;
        }

        protected virtual Aggregation GetRangeAggregation(RangeFilter rangeFilter, IList<AggregationResponse> aggregationResponses)
        {
            var result = new Aggregation
            {
                AggregationType = "range",
                Field = rangeFilter.GetIndexFieldName(),
                Items = GetRangeAggregationItems(rangeFilter.GetIndexFieldName(), rangeFilter.Values, aggregationResponses).ToArray(),
            };

            return result;
        }

        protected virtual Aggregation GetPriceRangeAggregation(PriceRangeFilter priceRangeFilter, IList<AggregationResponse> aggregationResponses)
        {
            var rangeAggregations = new List<AggregationResponse>();

            // Merge All Virtual Price Ranges in rangeAggregations
            var priceFieldName = "price";

            var priceAggregations = aggregationResponses.Where(a => a.Id.StartsWith(priceFieldName)).ToList();
            var priceValues = priceAggregations.SelectMany(x => x.Values).ToArray();

            if (priceValues.Length == 0)
            {
                return null;
            }

            var matchIdRegEx = new Regex(@"^(?<left>[0-9*]+)-(?<right>[0-9*]+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            rangeAggregations.AddRange(priceValues.Select(x =>
            {
                var matchId = matchIdRegEx.Match(x.Id);
                var left = matchId.Groups["left"].Value;
                var right = matchId.Groups["right"].Value;
                x.Id = left == "*" ? $@"under-{right}" : x.Id;
                x.Id = right == "*" ? $@"over-{left}" : x.Id;
                return new AggregationResponse { Id = $@"{priceFieldName}-{x.Id}", Values = new List<AggregationResponseValue> { x } };
            }));

            var result = new Aggregation
            {
                AggregationType = "pricerange",
                Field = priceRangeFilter.Key,
                Items = GetRangeAggregationItems(priceRangeFilter.Key, priceRangeFilter.Values, rangeAggregations).ToArray(),
            };

            var aggregationStatistics = priceAggregations.FirstOrDefault(x => x.Statistics != null);
            if (aggregationStatistics != null)
            {
                result.Statistics = new Core.Model.Search.AggregationStatistics
                {
                    Min = aggregationStatistics.Statistics.Min,
                    Max = aggregationStatistics.Statistics.Max,
                };
            }

            return result;
        }

        protected virtual IList<AggregationItem> GetAttributeAggregationItems(IList<AggregationResponseValue> aggregationResponseValues)
        {
            var result = aggregationResponseValues
                .Select(v =>
                {
                    if (v.Id.Contains(OutlineString.NameDelimiter))
                    {
                        var aggregationIdName = v.Id.Split(OutlineString.NameDelimiter);
                        if (aggregationIdName.Length > 1)
                        {
                            return new AggregationItem
                            {
                                Value = v.Id,
                                Count = (int)v.Count,
                                Labels = [new AggregationLabel { Label = CreateLabel(aggregationIdName[1]) }],
                            };
                        }
                    }

                    return new AggregationItem
                    {
                        Value = v.Id,
                        Count = (int)v.Count,
                    };
                })
                .ToList();

            return result;
        }

        protected virtual string CreateLabel(string title)
        {
            return Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(title);
        }

        /// <summary>
        /// Prefilters __outline, __outline_named and __path based on the parentOutline
        /// </summary>
        /// <param name="attributeFilter"></param>
        /// <param name="aggregationResponses"></param>
        /// <param name="criteria"></param>
        protected virtual void PreFilterOutlineAggregation(AttributeFilter attributeFilter, IList<AggregationResponse> aggregationResponses, ProductIndexedSearchCriteria criteria)
        {
            var fieldName = attributeFilter.Key;
            var aggregationResponse = aggregationResponses.FirstOrDefault(a => a.Id.EqualsIgnoreCase(fieldName));

            if (aggregationResponse == null)
            {
                return;
            }

            var parentOutline = string.Empty;
            if (!string.IsNullOrEmpty(criteria.Outline))
            {
                parentOutline = criteria.Outline;
            }
            else if (!string.IsNullOrEmpty(criteria.CatalogId))
            {
                parentOutline = criteria.CatalogId;
            }

            switch (fieldName)
            {
                case OutlineAggregationFieldName:
                case OutlineNamedAggregationFieldName:
                    aggregationResponse.Values = FilterOutlineAggregationItems(aggregationResponse.Values, parentOutline, expandChild: false);
                    break;
                case PathAggregationFieldName:
                    aggregationResponse.Values = FilterOutlineAggregationItems(aggregationResponse.Values, parentOutline, expandChild: true);
                    break;
            }
        }

        protected virtual async Task FilterLinkedCategories(AttributeFilter attributeFilter, IList<AggregationResponse> aggregationResponses, ProductIndexedSearchCriteria criteria)
        {
            if (attributeFilter.Key != OutlineNamedAggregationFieldName)
            {
                return;
            }

            var aggregationResponse = aggregationResponses.FirstOrDefault(a => a.Id.EqualsIgnoreCase(OutlineNamedAggregationFieldName));
            if (aggregationResponse == null || string.IsNullOrEmpty(criteria.StoreId))
            {
                return;
            }

            var values = aggregationResponse.Values;

            if (values.IsNullOrEmpty())
            {
                return;
            }

            var store = await _storeService.GetByIdAsync(criteria.StoreId);
            if (store == null || store.GetSeoLinksType() != SeoCollapsed)
            {
                return;
            }

            var storeCatalog = await _catalogService.GetByIdAsync(store.Catalog);
            if (storeCatalog == null || !storeCatalog.IsVirtual)
            {
                return;
            }

            var outlines = values.ToDictionary(x => OutlineString.GetLastItemId(x.Id));
            var categories = await _categoryService.GetAsync(outlines.Keys.ToArray(), clone: false);

            var result = categories.FilterLinked(store, storeCatalog).Select(x => outlines[x.Id]);

            aggregationResponse.Values = result.ToArray();
        }

        /// <summary>
        /// Filter outline aggregation items by requested outline or catalog.
        /// </summary>
        protected virtual IList<AggregationResponseValue> FilterOutlineAggregationItems(IList<AggregationResponseValue> values, string parentOutline, bool expandChild)
        {
            if (string.IsNullOrEmpty(parentOutline) || values.IsNullOrEmpty())
            {
                return values;
            }

            // Exclude direct outlines: {CatalogId}/{CategoryId}
            var directOutlines = values
                .Select(value => value.Id.Split(OutlineString.ItemDelimiter))
                .Where(items => items.Length > 2)
                .Select(items => OutlineString.Build([items.First(), items.Last()]))
                .ToHashSet();

            var filteredValues = values
                .Where(x =>
                    IsChildOutline(x.Id, parentOutline, expandChild) &&
                    !directOutlines.Contains(x.Id));

            return filteredValues.ToArray();
        }

        protected virtual bool IsChildOutline(string outline, string parentOutline, bool expandChild)
        {
            if (string.IsNullOrEmpty(outline))
            {
                return false;
            }

            return outline.StartsWith(parentOutline + OutlineString.ItemDelimiter) &&
                (expandChild
                    ? outline.IndexOf(OutlineString.ItemDelimiter, parentOutline.Length + 1) != -1
                    : outline.IndexOf(OutlineString.ItemDelimiter, parentOutline.Length + 1) == -1);
        }

        protected virtual IList<AggregationItem> GetRangeAggregationItems(string aggregationId, IList<RangeFilterValue> values, IList<AggregationResponse> aggregationResponses)
        {
            var result = new List<AggregationItem>();

            if (values != null)
            {
                foreach (var group in values.GroupBy(v => v.Id, StringComparer.OrdinalIgnoreCase))
                {
                    var valueId = group.Key;
                    var aggregationValueId = $"{aggregationId}-{valueId}";
                    var aggregationResponse = aggregationResponses.FirstOrDefault(v => v.Id.EqualsIgnoreCase(aggregationValueId));

                    if (aggregationResponse?.Values?.Any() == true)
                    {
                        var value = aggregationResponse.Values.First();
                        var rangeValue = group.First();

                        var aggregationItem = new AggregationItem
                        {
                            Value = valueId,
                            Count = (int)value.Count,
                            RequestedLowerBound = !string.IsNullOrEmpty(rangeValue.Lower) ? rangeValue.Lower : null,
                            RequestedUpperBound = !string.IsNullOrEmpty(rangeValue.Upper) ? rangeValue.Upper : null,
                            IncludeLower = rangeValue.IncludeLower,
                            IncludeUpper = rangeValue.IncludeUpper,
                        };

                        result.Add(aggregationItem);
                    }
                }
            }

            return result;
        }

        protected virtual async Task AddLabelsAsync(IList<Aggregation> aggregations, string catalogId, IList<IBrowseFilter> browseFilters)
        {
            var allCatalogProperties = await _propertyService.GetAllCatalogPropertiesAsync(catalogId);
            var allProperties = allCatalogProperties as IList<Property> ?? allCatalogProperties.ToList();

            foreach (var aggregation in aggregations)
            {
                // Add Label For Outlines
                var labelAdded = await TryAddLabelsAsyncForOutline(aggregation);
                if (!labelAdded)
                {
                    await TryAddLabelsAsyncForProperty(allProperties, aggregation, browseFilters);
                }
            }

            var outlineNamedAggregations = aggregations.Where(x => x.Field == "__outline_named").ToList();
            await TryAddLabelsAsyncForOutlinesItems(outlineNamedAggregations);
        }

        private async Task TryAddLabelsAsyncForOutlinesItems(List<Aggregation> outlineAggregations)
        {
            var categoriesDictionary = new Dictionary<string, Category>();

            // Collect all category ids
            foreach (var outlineAggregation in outlineAggregations)
            {
                foreach (var aggregationItem in outlineAggregation.Items)
                {
                    var categoryId = OutlineString.GetLastItemId(aggregationItem.Value as string);
                    if (categoryId != null)
                    {
                        categoriesDictionary.TryAdd(categoryId, null);
                    }
                }
            }

            // Load categories
            var categories = await _categoryService.GetAsync(categoriesDictionary.Keys.ToArray(), clone: false);
            foreach (var category in categories)
            {
                categoriesDictionary[category.Id] = category;
            }

            // Add localized category names to labels
            foreach (var outlineAggregation in outlineAggregations)
            {
                foreach (var aggregationItem in outlineAggregation.Items)
                {
                    var categoryId = OutlineString.GetLastItemId(aggregationItem.Value as string);
                    if (categoryId != null &&
                        categoriesDictionary.TryGetValue(categoryId, out var category) &&
                        category != null &&
                        category.LocalizedName.Values.Count > 0)
                    {
                        var localizedLabels = category.LocalizedName.Values
                            .Select(x => new AggregationLabel { Language = x.Key, Label = !string.IsNullOrEmpty(x.Value) ? x.Value : category.Name }).ToList();

                        var defaultCategoryLabel = aggregationItem.Labels.FirstOrDefault(x => x.Language == null);
                        if (defaultCategoryLabel != null)
                        {
                            defaultCategoryLabel.Label = category.Name;
                            localizedLabels.Add(defaultCategoryLabel);
                        }

                        aggregationItem.Labels = localizedLabels.ToArray();
                    }
                }
            }
        }

        private async Task TryAddLabelsAsyncForProperty(IEnumerable<Property> allProperties, Aggregation aggregation, IList<IBrowseFilter> browseFilters)
        {
            IBrowseFilter filter = browseFilters.OfType<AttributeFilter>().FirstOrDefault(f => f.IndexFieldName.EqualsIgnoreCase(aggregation.Field));
            filter ??= browseFilters.OfType<RangeFilter>().FirstOrDefault(f => f.IndexFieldName.EqualsIgnoreCase(aggregation.Field));

            // There can be many properties with the same name
            var properties = allProperties.Where(p => p.Name.EqualsIgnoreCase(filter?.Key ?? aggregation.Field)).ToArray();

            if (properties.Length == 0)
            {
                return;
            }

            var allPropertyLabels = properties.SelectMany(p => p.DisplayNames)
                .Select(n => new AggregationLabel { Language = n.LanguageCode, Label = n.Name })
                .ToArray();

            aggregation.Labels = GetFirstLabelForEachLanguage(allPropertyLabels);

            var dictionaryItemsSearchResult = await _propDictItemsSearchService.SearchAsync(
                new PropertyDictionaryItemSearchCriteria { PropertyIds = properties.Select(x => x.Id).ToArray(), Take = int.MaxValue }, clone: true);
            var allDictItemsMap = dictionaryItemsSearchResult.Results.GroupBy(x => x.Alias, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.Key,
                    x => x.SelectMany(dictItem => dictItem.LocalizedValues)
                        .Select(localizedValue => new AggregationLabel { Language = localizedValue.LanguageCode, Label = localizedValue.Value })
                        .ToArray(),
                    StringComparer.OrdinalIgnoreCase);

            foreach (var aggregationItem in aggregation.Items)
            {
                var alias = aggregationItem.Value?.ToString();
                if (string.IsNullOrEmpty(alias) || !allDictItemsMap.ContainsKey(alias))
                {
                    continue;
                }

                var pair = allDictItemsMap.First(x => allDictItemsMap.Comparer.Equals(x.Key, alias));
                aggregationItem.Value = pair.Key;
                aggregationItem.Labels = GetFirstLabelForEachLanguage(pair.Value);
            }
        }

        protected virtual Task<bool> TryAddLabelsAsyncForOutline(Aggregation aggregation)
        {
            // Add Label For Outline Named
            if (aggregation.Field.EqualsIgnoreCase("__outline_named"))
            {
                aggregation.Labels = [new AggregationLabel { Label = "Categories" }];
                return Task.FromResult(true);
            }

            // Add Label For Outline and Path
            if (aggregation.Field.EqualsIgnoreCase("__outline") || aggregation.Field.EqualsIgnoreCase("__path"))
            {
                aggregation.Labels = [new AggregationLabel { Label = "Categories" }];
                foreach (var aggregationItem in aggregation.Items)
                {
                    aggregationItem.Labels = [new AggregationLabel { Label = OutlineString.GetLastItem((string)aggregationItem.Value) }];
                }
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        private static AggregationLabel[] GetFirstLabelForEachLanguage(AggregationLabel[] labels)
        {
            var result = labels
                .Where(x => !string.IsNullOrEmpty(x.Language) && !string.IsNullOrEmpty(x.Label))
                .GroupBy(x => x.Language, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.FirstOrDefault())
                .OrderBy(x => x?.Language)
                .ThenBy(x => x?.Label)
                .ToArray();

            return result.Any() ? result : null;
        }

        #endregion
    }
}
