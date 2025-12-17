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
        [InlineData("", null, null, "10")]
        [InlineData("Unknown:10", "Unknown:10", "Unknown:10", "10")]
        [InlineData("Unknown:10;Color:Red", "(Unknown:10 AND Color:Red)", "Unknown:10", "10")]
        [InlineData("Unknown:10;Color:Red;Size:size1", "(Unknown:10 AND Color:Red AND Size:[TO 10))", "(Unknown:10 AND Size:[TO 10))", "10")]
        [InlineData("Color:Pink", "Color:Pink", null, "10")]
        [InlineData("Size:unknown", "ID:", "ID:", "10")]
        public async Task TestSimpleAggregations(string terms, string expectedFilter1, string expectedFilter2, string expectedRange)
        {
            var criteria = new ProductIndexedSearchCriteria
            {
                Terms = terms?.Split(';'),
            };

            var allFilters = await GetTermFilterBuilder().GetTermFiltersAsync(criteria);

            var requests = await GetAggregationRequestBuilder().GetAggregationRequestsAsync(criteria, allFilters);

            Assert.Equal(3, requests.Count);

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

            Assert.IsType<RangeAggregationRequest>(requests[2]);
            var rangeRequest = (RangeAggregationRequest)requests[2];
            Assert.Equal("Size", rangeRequest.FieldName);

            var rangeRequestValue = rangeRequest.Values[0];
            Assert.Equal("size1", rangeRequestValue.Id);
            Assert.Equal(expectedRange, rangeRequestValue.Upper);
            Assert.Null(rangeRequestValue.Lower);

            rangeRequestValue = rangeRequest.Values[1];
            Assert.Equal("size2", rangeRequestValue.Id);
            Assert.Null(rangeRequestValue.Upper);
            Assert.Equal(expectedRange, rangeRequestValue.Lower);
        }

        private static IAggregationConverter GetAggregationRequestBuilder()
        {
            return new AggregationConverter(GetBrowseFilterService(), null, null, null, null, null);
        }
    }
}
