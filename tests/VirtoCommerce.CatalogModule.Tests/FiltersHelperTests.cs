using VirtoCommerce.CatalogModule.Data.Search.Indexing;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    [Trait("Category", "CI")]
    public class FiltersHelperTests
    {
        [Theory]
        [InlineData(null, null, "price:[TO 100)")]
        [InlineData("", null, "price:[TO 100)")]
        [InlineData("USD", null, "price_usd:[TO 100)")]
        [InlineData("USD", "", "price_usd:[TO 100)")]
        [InlineData("USD", "1", "price_usd_1:[TO 100)")]
        [InlineData("USD", "1;2", "(price_usd_1:[TO 100) OR (NOT(price_usd_1:(0 TO)) AND price_usd_2:[TO 100)))")]
        [InlineData("USD", "1;2;3", "(price_usd_1:[TO 100) OR (NOT(price_usd_1:(0 TO)) AND (price_usd_2:[TO 100) OR (NOT(price_usd_2:(0 TO)) AND price_usd_3:[TO 100)))))")]
        public void TestPriceFilters(string currency, string priceLists, string expectedFilterString)
        {
            var filter = FiltersHelper.CreatePriceRangeFilter(currency, priceLists?.Split(';'), null, 100, true, false);
            var filterString = filter.ToString();
            Assert.Equal(expectedFilterString, filterString);
        }
    }
}
