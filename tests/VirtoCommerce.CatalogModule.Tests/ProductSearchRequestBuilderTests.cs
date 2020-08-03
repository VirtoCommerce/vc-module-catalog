using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Data.Search.Indexing;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    [Trait("Category", "Unit")]
    public class ProductSearchRequestBuilderTests : BrowseFiltersTestBase
    {
        [Theory]
        [InlineData(null, null, null, null, "price:")]
        [InlineData(null, null, "", null, "price:")]
        [InlineData("0", null, "", null, "price:(0 TO)")]
        [InlineData(null, "100", "", null, "price:(TO 100)")]
        [InlineData("0", "100", "", null, "price:(0 TO 100)")]
        [InlineData("100", "200", "", null, "price:(100 TO 200)")]
        [InlineData("100", "200", "USD", null, "price_usd:(100 TO 200)")]
        [InlineData("100", "200", "USD", "1", "price_usd_1:(100 TO 200)")]
        [InlineData("100", "200", "USD", "1;2", "(price_usd_1:(100 TO 200) OR (NOT(price_usd_1:(0 TO)) AND price_usd_2:(100 TO 200)))")]
        [InlineData("100", "200", "USD", "1;2;3", "(price_usd_1:(100 TO 200) OR (NOT(price_usd_1:(0 TO)) AND (price_usd_2:(100 TO 200) OR (NOT(price_usd_2:(0 TO)) AND price_usd_3:(100 TO 200)))))")]
        public async Task TestPriceRangeFilter(string lower, string upper, string currency, string priceLists, string expectedFilter)
        {
            var criteria = new ProductIndexedSearchCriteria
            {
                PriceRange = new NumericRange
                {
                    Lower = ParseDecimal(lower),
                    Upper = ParseDecimal(upper),
                },
                Currency = currency,
                Pricelists = priceLists?.Split(';'),
            };

            var termFilterBuilder = GetSearchRequestBuilder();
            var searchRequest = await termFilterBuilder.BuildRequestAsync(criteria);

            var priceFilter = (searchRequest.Filter as AndFilter)?.ChildFilters.Last().ToString();
            Assert.Equal(expectedFilter, priceFilter);
        }

        private static decimal? ParseDecimal(string str)
        {
            decimal? result = null;

            if (decimal.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
            {
                result = value;
            }

            return result;
        }

        private static ISearchRequestBuilder GetSearchRequestBuilder()
        {
            return new ProductSearchRequestBuilder(null, GetTermFilterBuilder(), null);
        }
    }
}
