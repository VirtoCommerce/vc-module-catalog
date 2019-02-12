using System;
using System.Globalization;
using System.Linq;
using VirtoCommerce.CatalogModule.Data.Search;
using VirtoCommerce.Domain.Catalog.Model.Search;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Domain.Search;
using Xunit;

namespace VirtoCommerce.CatalogModule.Test
{
    [Trait("Category", "Unit")]
    [CLSCompliant(false)]
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
        public void TestPriceRangeFilter(string lower, string upper, string currency, string pricelists, string expectedFilter)
        {
            var criteria = new ProductSearchCriteria
            {
                PriceRange = new NumericRange
                {
                    Lower = ParseDecimal(lower),
                    Upper = ParseDecimal(upper),
                },
                Currency = currency,
                Pricelists = pricelists?.Split(';'),
            };

            var termFilterBuilder = GetSearchRequestBuilder();

            var searchRequest = termFilterBuilder.BuildRequest(criteria);

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
