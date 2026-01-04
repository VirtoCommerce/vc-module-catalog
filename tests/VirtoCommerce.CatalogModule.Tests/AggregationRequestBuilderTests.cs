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
        [InlineData("", null, null, null)]
        [InlineData("Unknown:10", "Unknown:10", "Unknown:10", "Unknown:10")]
        [InlineData("Unknown:10;Color:Red", "(Unknown:10 AND Color:Red)", "Unknown:10", "(Unknown:10 AND Color:Red)")]
        [InlineData("Unknown:10;Color:Red;Size:size1", "(Unknown:10 AND Color:Red AND Size:[TO 10))", "(Unknown:10 AND Size:[TO 10))", "(Unknown:10 AND Color:Red)")]
        [InlineData("Color:Pink", "Color:Pink", null, "Color:Pink")]
        [InlineData("Size:unknown", "ID:", "ID:", null)]
        public async Task TestSimpleAggregations(string terms, string expectedFilter1, string expectedFilter2, string expectedFilter3)
        {
            // Arrange
            var criteria = new ProductIndexedSearchCriteria
            {
                Terms = terms?.Split(';'),
            };

            var allFilters = await GetTermFilterBuilder().GetTermFiltersAsync(criteria);

            // Act
            var requests = await GetAggregationRequestBuilder().GetAggregationRequestsAsync(criteria, allFilters);

            // Assert
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
            Assert.Equal(expectedFilter3, rangeRequest.Filter?.ToString());
            Assert.Equal("Size-size1-size2", rangeRequest.Id);
            Assert.Equal(2, rangeRequest.Values.Count);

            var rangeRequestValue = rangeRequest.Values[0];
            Assert.Equal("size1", rangeRequestValue.Id);
            Assert.Null(rangeRequestValue.Lower);
            Assert.True(rangeRequestValue.IncludeLower);
            Assert.Equal("10", rangeRequestValue.Upper);
            Assert.False(rangeRequestValue.IncludeUpper);

            rangeRequestValue = rangeRequest.Values[1];
            Assert.Equal("size2", rangeRequestValue.Id);
            Assert.Equal("10", rangeRequestValue.Lower);
            Assert.True(rangeRequestValue.IncludeLower);
            Assert.Null(rangeRequestValue.Upper);
            Assert.False(rangeRequestValue.IncludeUpper);
        }

        private static IAggregationConverter GetAggregationRequestBuilder()
        {
            return new AggregationConverter(GetBrowseFilterService(), null, null, null, null, null);
        }
    }
}
