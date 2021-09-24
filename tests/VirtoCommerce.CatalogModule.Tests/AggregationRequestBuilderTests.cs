using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Data.Search.Indexing;
using VirtoCommerce.SearchModule.Core.Model;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    [Trait("Category", "CI")]
    public class AggregationRequestBuilderTests : BrowseFiltersTestBase
    {
        [Theory]
        [InlineData("", null, null, "Size:[TO 10)", "Size:[10 TO)")]
        [InlineData("Unknown:10", "Unknown:10", "Unknown:10", "(Unknown:10 AND Size:[TO 10))", "(Unknown:10 AND Size:[10 TO))")]
        [InlineData("Unknown:10;Color:Red", "(Unknown:10 AND Color:Red)", "Unknown:10", "(Unknown:10 AND Color:Red AND Size:[TO 10))", "(Unknown:10 AND Color:Red AND Size:[10 TO))")]
        [InlineData("Unknown:10;Color:Red;Size:size1", "(Unknown:10 AND Color:Red AND Size:[TO 10))", "(Unknown:10 AND Size:[TO 10))", "(Unknown:10 AND Color:Red AND Size:[TO 10))", "(Unknown:10 AND Color:Red AND Size:[10 TO))")]
        [InlineData("Color:Pink", "Color:Pink", null, "(Color:Pink AND Size:[TO 10))", "(Color:Pink AND Size:[10 TO))")]
        [InlineData("Size:unknown", "ID:", "ID:", "Size:[TO 10)", "Size:[10 TO)")]
        public async Task TestSimpleAggregations(string terms, string expectedFilter1, string expectedFilter2, string expectedFilter3, string expectedFilter4)
        {
            var criteria = new ProductIndexedSearchCriteria
            {
                Terms = terms?.Split(';'),
            };

            var allFilters = await GetTermFilterBuilder().GetTermFiltersAsync(criteria);

            var requests = await GetAggregationRequestBuilder().GetAggregationRequestsAsync(criteria, allFilters);

            Assert.Equal(4, requests.Count);

            Assert.IsType<TermAggregationRequest>(requests[0]);
            var request = (TermAggregationRequest)requests[0];
            Assert.Equal("Brand", request.FieldName);
            Assert.Equal(expectedFilter1, request.Filter?.ToString());
            Assert.Null(request.Id);
            Assert.Equal(5, request.Size);
            Assert.Null(request.Values);

            Assert.IsType<TermAggregationRequest>(requests[1]);
            request = (TermAggregationRequest)requests[1];
            Assert.Equal("Color", request.FieldName);
            Assert.Equal(expectedFilter2, request.Filter?.ToString());
            Assert.Null(request.Id);
            Assert.Null(request.Size);
            Assert.Equal("Red,Green,Blue", string.Join(",", request.Values));

            Assert.IsType<TermAggregationRequest>(requests[2]);
            request = (TermAggregationRequest)requests[2];
            Assert.Null(request.FieldName);
            Assert.Equal(expectedFilter3, request.Filter?.ToString());
            Assert.Equal("Size-size1", request.Id);
            Assert.Null(request.Size);
            Assert.Null(request.Values);

            Assert.IsType<TermAggregationRequest>(requests[3]);
            request = (TermAggregationRequest)requests[3];
            Assert.Null(request.FieldName);
            Assert.Equal(expectedFilter4, request.Filter?.ToString());
            Assert.Equal("Size-size2", request.Id);
            Assert.Null(request.Size);
            Assert.Null(request.Values);
        }

        private static IAggregationConverter GetAggregationRequestBuilder()
        {
            return new AggregationConverter(GetBrowseFilterService(), null, null);
        }
    }
}
