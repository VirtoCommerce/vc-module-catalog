using System;
using System.Linq;
using VirtoCommerce.Domain.Catalog.Model.Search;
using Xunit;

namespace VirtoCommerce.CatalogModule.Test
{
    [Trait("Category", "CI")]
    public class TermFilterBuilderTests : BrowseFiltersTestBase
    {
        [CLSCompliant(false)]
        [Theory]
        [InlineData(null, "", "")]
        [InlineData("", "", "")]
        [InlineData("Unknown:10", "Unknown:10", "")] // Unknown term produces permanent filter
        [InlineData("Color:Red", "", "Color:Red")] // Predefined attribute term produces removable filter
        [InlineData("Size:size1", "", "Size:[TO 10)")] // Predefined range term produces removable filter
        [InlineData("Unknown:10;Color:Red;Size:size1", "Unknown:10", "Color:Red;Size:[TO 10)")]
        [InlineData("Color:Pink", "", "Color:Pink")] // Predefined attribute term with unknown value produces a filter with unknown value
        [InlineData("Size:unknown", "", "ID:")] // Predefined range term with unknown value produces a filter which will not return any document
        public void TestSimpleTerms(string terms, string expectedPermanentFilters, string expectedRemovableFilters)
        {
            var criteria = new ProductSearchCriteria
            {
                Terms = terms?.Split(';'),
            };

            var termFilterBuilder = GetTermFilterBuilder();

            var filters = termFilterBuilder.GetTermFilters(criteria);

            var permanentFilters = string.Join(";", filters.PermanentFilters.Select(f => f.ToString()));
            Assert.Equal(expectedPermanentFilters, permanentFilters);

            var removableFilters = string.Join(";", filters.RemovableFilters.Select(kvp => kvp.Value.ToString()));
            Assert.Equal(expectedRemovableFilters, removableFilters);
        }

        [CLSCompliant(false)]
        [Theory]
        [InlineData("price:price1", null, null, "price:price1", "")] // Empty currency produces permanent attribute filter
        [InlineData("price:price1", "", null, "price:price1", "")] // Empty currency produces permanent attribute filter
        [InlineData("price:price1", "USD", null, "", "price_usd:[TO 100)")]
        [InlineData("price:price1", "USD", "", "", "price_usd:[TO 100)")]
        [InlineData("price:price1", "EUR", "", "", "price_eur:[TO 200)")] // For EUR we have different range bounds
        [InlineData("price:price1", "USD", "1", "", "price_usd_1:[TO 100)")]
        [InlineData("price:price1", "USD", "1;2", "", "(price_usd_1:[TO 100) OR (NOT(price_usd_1:(0 TO)) AND price_usd_2:[TO 100)))")]
        [InlineData("price:price1", "USD", "1;2;3", "", "(price_usd_1:[TO 100) OR (NOT(price_usd_1:(0 TO)) AND (price_usd_2:[TO 100) OR (NOT(price_usd_2:(0 TO)) AND price_usd_3:[TO 100)))))")]
        [InlineData("price:unknown", "USD", "1;2;3", "", "ID:")] // Unknown value produces a filter which will not return any document
        public void TestPriceTerms(string terms, string currency, string pricelists, string expectedPermanentFilters, string expectedRemovableFilters)
        {
            var criteria = new ProductSearchCriteria
            {
                Terms = terms?.Split(';'),
                Currency = currency,
                Pricelists = pricelists?.Split(';'),
            };

            var termFilterBuilder = GetTermFilterBuilder();

            var filters = termFilterBuilder.GetTermFilters(criteria);

            var permanentFilters = string.Join(";", filters.PermanentFilters.Select(f => f.ToString()));
            Assert.Equal(expectedPermanentFilters, permanentFilters);

            var removableFilters = string.Join(";", filters.RemovableFilters.Select(kvp => kvp.Value.ToString()));
            Assert.Equal(expectedRemovableFilters, removableFilters);
        }

        // Tags term produces permanent filters
        [CLSCompliant(false)]
        [Theory]
        [InlineData("tags:unknown", null, null, "ID:", "")] // Unknown value produces a filter which will not return any document
        [InlineData("tags:Red", null, null, "Color:Red", "")]
        [InlineData("tags:size1", null, null, "Size:[TO 10)", "")]
        [InlineData("tags:price1", null, null, "ID:", "")] // Price value with empty currency produces a filter which will not return any document
        [InlineData("tags:price1", "USD", null, "price_usd:[TO 100)", "")]
        [InlineData("tags:price1", "EUR", null, "price_eur:[TO 200)", "")] // For EUR we have different range bounds
        [InlineData("tags:price1", "USD", "1", "price_usd_1:[TO 100)", "")]
        [InlineData("tags:price1", "USD", "1;2", "(price_usd_1:[TO 100) OR (NOT(price_usd_1:(0 TO)) AND price_usd_2:[TO 100)))", "")]
        [InlineData("tags:price1", "USD", "1;2;3", "(price_usd_1:[TO 100) OR (NOT(price_usd_1:(0 TO)) AND (price_usd_2:[TO 100) OR (NOT(price_usd_2:(0 TO)) AND price_usd_3:[TO 100)))))", "")]
        public void TestTagsTerms(string terms, string currency, string pricelists, string expectedPermanentFilters, string expectedRemovableFilters)
        {
            var criteria = new ProductSearchCriteria
            {
                Terms = terms?.Split(';'),
                Currency = currency,
                Pricelists = pricelists?.Split(';'),
            };

            var termFilterBuilder = GetTermFilterBuilder();

            var filters = termFilterBuilder.GetTermFilters(criteria);

            var permanentFilters = string.Join(";", filters.PermanentFilters.Select(f => f.ToString()));
            Assert.Equal(expectedPermanentFilters, permanentFilters);

            var removableFilters = string.Join(";", filters.RemovableFilters.Select(kvp => kvp.Value.ToString()));
            Assert.Equal(expectedRemovableFilters, removableFilters);
        }
    }
}
