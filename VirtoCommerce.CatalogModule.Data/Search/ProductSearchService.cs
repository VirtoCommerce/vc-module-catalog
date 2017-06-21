using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Data.Search.BrowseFilters;
using VirtoCommerce.CatalogModule.Web.Converters;
using VirtoCommerce.CatalogModule.Web.Model;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using Aggregation = VirtoCommerce.CatalogModule.Web.Model.Aggregation;
using AggregationItem = VirtoCommerce.CatalogModule.Web.Model.AggregationItem;
using AggregationLabel = VirtoCommerce.CatalogModule.Web.Model.AggregationLabel;
using RangeFilter = VirtoCommerce.CatalogModule.Data.Search.BrowseFilters.RangeFilter;
using RangeFilterValue = VirtoCommerce.CatalogModule.Data.Search.BrowseFilters.RangeFilterValue;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class ProductSearchService : CatalogSearchService2<Product, ProductSearchCriteria, ProductSearchResult>, IProductSearchService
    {
        private readonly IItemService _itemService;
        private readonly IBlobUrlResolver _blobUrlResolver;
        private readonly IBrowseFilterService _browseFilterService;

        public ProductSearchService(ISearchRequestBuilder[] searchRequestBuilders, ISearchProvider searchProvider, ISettingsManager settingsManager, IItemService itemService, IBlobUrlResolver blobUrlResolver, IBrowseFilterService browseFilterService)
            : base(searchRequestBuilders, searchProvider, settingsManager)
        {
            _itemService = itemService;
            _blobUrlResolver = blobUrlResolver;
            _browseFilterService = browseFilterService;
        }


        protected override IList<Product> LoadMissingItems(string[] missingItemIds, ProductSearchCriteria criteria)
        {
            var catalog = criteria.CatalogId;
            var responseGroup = GetResponseGroup(criteria);

            var products = _itemService.GetByIds(missingItemIds, responseGroup, catalog);

            var result = products.Select(p => p.ToWebModel(_blobUrlResolver)).ToArray();
            return result;
        }

        protected override void ReduceSearchResults(IEnumerable<Product> items, ProductSearchCriteria criteria)
        {
            var responseGroup = GetResponseGroup(criteria);

            foreach (var obj in items)
            {
                ReduceSearchResult(obj, responseGroup);
            }
        }


        protected virtual ItemResponseGroup GetResponseGroup(ProductSearchCriteria criteria)
        {
            var result = EnumUtility.SafeParse(criteria?.ResponseGroup, ItemResponseGroup.ItemLarge & ~ItemResponseGroup.ItemProperties);
            return result;
        }

        protected virtual void ReduceSearchResult(Product product, ItemResponseGroup responseGroup)
        {
            if (!responseGroup.HasFlag(ItemResponseGroup.ItemAssets))
            {
                product.Assets = null;
            }

            if (!responseGroup.HasFlag(ItemResponseGroup.ItemAssociations))
            {
                product.Associations = null;
            }

            if (!responseGroup.HasFlag(ItemResponseGroup.ItemEditorialReviews))
            {
                product.Reviews = null;
            }

            if (!responseGroup.HasFlag(ItemResponseGroup.ItemInfo))
            {
                product.Properties = null;
            }

            if (!responseGroup.HasFlag(ItemResponseGroup.Links))
            {
                product.Links = null;
            }

            if (!responseGroup.HasFlag(ItemResponseGroup.Outlines))
            {
                product.Outlines = null;
            }

            if (!responseGroup.HasFlag(ItemResponseGroup.Seo))
            {
                product.SeoInfos = null;
            }

            if (!responseGroup.HasFlag(ItemResponseGroup.Variations))
            {
                product.Variations = null;
            }
        }

        #region Aggregations

        protected override Aggregation[] ConvertAggregations(IList<AggregationResponse> aggregationResponses, ProductSearchCriteria criteria)
        {
            var result = new List<Aggregation>();

            var browseFilters = GetBrowseFilters(criteria);
            if (browseFilters != null && aggregationResponses?.Any() == true)
            {
                foreach (var filter in browseFilters)
                {
                    Aggregation aggregation = null;

                    var attributeFilter = filter as AttributeFilter;
                    var rangeFilter = filter as RangeFilter;
                    var priceRangeFilter = filter as PriceRangeFilter;

                    if (attributeFilter != null)
                    {
                        aggregation = GetAttributeAggregation(attributeFilter, aggregationResponses);
                    }
                    else if (rangeFilter != null)
                    {
                        aggregation = GetRangeAggregation(rangeFilter, aggregationResponses);
                    }
                    else if (priceRangeFilter != null)
                    {
                        aggregation = GetPriceRangeAggregation(priceRangeFilter, aggregationResponses);
                    }

                    if (aggregation?.Items?.Any() == true)
                    {
                        result.Add(aggregation);
                    }
                }
            }

            return result.ToArray();
        }

        protected virtual IList<IBrowseFilter> GetBrowseFilters(ProductSearchCriteria criteria)
        {
            var allFilters = _browseFilterService.GetFilters(criteria.StoreId);

            var result = allFilters
                ?.Where(f => !(f is PriceRangeFilter) || ((PriceRangeFilter)f).Currency.EqualsInvariant(criteria.Currency))
                .ToList();

            return result;
        }

        protected virtual Aggregation GetAttributeAggregation(AttributeFilter attributeFilter, IList<AggregationResponse> aggregationResponses)
        {
            var result = new Aggregation
            {
                AggregationType = "attr",
                Field = attributeFilter.Key,
                Labels = attributeFilter.DisplayNames
                    ?.Select(d => new AggregationLabel { Language = d.Language, Label = d.Name })
                    .ToArray(),
            };

            var aggregationId = attributeFilter.Key;
            var aggregationResponse = aggregationResponses.FirstOrDefault(a => a.Id.EqualsInvariant(aggregationId));

            if (aggregationResponse != null)
            {
                if (attributeFilter.Values == null)
                {
                    // Return all values
                    result.Items = aggregationResponse.Values.Select(v => new AggregationItem { Value = v.Id, Count = (int)v.Count }).ToArray();
                }
                else
                {
                    // Return predefined values with localization
                    var aggregationItems = new List<AggregationItem>();

                    foreach (var group in attributeFilter.Values.GroupBy(v => v.Id))
                    {
                        var value = aggregationResponse.Values.FirstOrDefault(v => v.Id.EqualsInvariant(group.Key));
                        if (value != null)
                        {
                            var valueLabels = group.GetValueLabels();
                            var aggregationItem = new AggregationItem { Value = value.Id, Count = (int)value.Count, Labels = valueLabels };
                            aggregationItems.Add(aggregationItem);
                        }
                    }

                    if (aggregationItems.Any())
                    {
                        result.Items = aggregationItems;
                    }
                }
            }

            return result;
        }

        protected virtual Aggregation GetRangeAggregation(RangeFilter rangeFilter, IList<AggregationResponse> aggregationResponses)
        {
            var result = new Aggregation
            {
                AggregationType = "range",
                Field = rangeFilter.Key,
                Items = GetRangeAggregationItems(rangeFilter.Key, rangeFilter.Values, aggregationResponses),
            };

            return result;
        }

        protected virtual Aggregation GetPriceRangeAggregation(PriceRangeFilter priceRangeFilter, IList<AggregationResponse> aggregationResponses)
        {
            var result = new Aggregation
            {
                AggregationType = "pricerange",
                Field = priceRangeFilter.Key,
                Items = GetRangeAggregationItems(priceRangeFilter.Key, priceRangeFilter.Values, aggregationResponses),
            };


            return result;
        }

        protected virtual IList<AggregationItem> GetRangeAggregationItems(string aggregationId, IList<RangeFilterValue> values, IList<AggregationResponse> aggregationResponses)
        {
            var result = new List<AggregationItem>();

            if (values != null)
            {
                foreach (var group in values.GroupBy(v => v.Id))
                {
                    var valueId = group.Key;
                    var aggregationValueId = $"{aggregationId}-{valueId}";
                    var aggregationResponse = aggregationResponses.FirstOrDefault(v => v.Id.EqualsInvariant(aggregationValueId));

                    if (aggregationResponse?.Values?.Any() == true)
                    {
                        var value = aggregationResponse.Values.First();
                        var valueLabels = group.GetValueLabels();
                        var aggregationItem = new AggregationItem { Value = valueId, Count = (int)value.Count, Labels = valueLabels };
                        result.Add(aggregationItem);
                    }
                }
            }

            return result;
        }

        #endregion
    }
}
