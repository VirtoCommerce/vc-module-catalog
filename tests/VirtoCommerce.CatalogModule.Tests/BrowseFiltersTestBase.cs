using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Data.Search.BrowseFilters;
using VirtoCommerce.CatalogModule.Data.Search.Indexing;

namespace VirtoCommerce.CatalogModule.Tests
{
    public abstract class BrowseFiltersTestBase
    {
        protected static IList<IBrowseFilter> BrowseFilters { get; } = new List<IBrowseFilter>
        {
            new AttributeFilter
            {
                Key = "Brand",
                FacetSize = 5,
            },
            new AttributeFilter
            {
                Key = "Color",
                Values = new[]
                {
                    new AttributeFilterValue { Id = "Red" },
                    new AttributeFilterValue { Id = "Green" },
                    new AttributeFilterValue { Id = "Blue" },
                },
            },
            new RangeFilter
            {
                Key = "Size",
                Values = new[]
                {
                    new RangeFilterValue
                    {
                        Id = "size1",
                        Lower = null,
                        Upper = "10",
                    },
                    new RangeFilterValue
                    {
                        Id = "size2",
                        Lower = "10",
                        Upper = null,
                    },
                }
            },
            new RangeFilter
            {
                Key = "Weight",
            },
            new PriceRangeFilter
            {
                Currency = "USD",
                Values = new[]
                {
                    new RangeFilterValue
                    {
                        Id = "price1",
                        Lower = null,
                        Upper = "100",
                    },
                    new RangeFilterValue
                    {
                        Id = "price2",
                        Lower = "100",
                        Upper = null,
                    },
                }
            },
            new PriceRangeFilter
            {
                Currency = "EUR",
                Values = new[]
                {
                    new RangeFilterValue
                    {
                        Id = "price1",
                        Lower = null,
                        Upper = "200",
                    },
                    new RangeFilterValue
                    {
                        Id = "price2",
                        Lower = "200",
                        Upper = null,
                    },
                }
            },
            new PriceRangeFilter
            {
                Currency = "RUR",
            },
        };


        protected static ITermFilterBuilder GetTermFilterBuilder()
        {
            return new TermFilterBuilder(GetBrowseFilterService(), null);
        }

        protected static IBrowseFilterService GetBrowseFilterService()
        {
            var mock = new Mock<BrowseFilterService>(null) { CallBase = true };

            mock
                .Setup(x => x.GetStoreAggregationsAsync(It.IsAny<string>()))
                .Returns<string>(GetStoreAggregations);

            return mock.Object;
        }

        protected static async Task<IList<IBrowseFilter>> GetStoreAggregations(string storeId)
        {
            return await Task.FromResult(BrowseFilters);
        }
    }
}
