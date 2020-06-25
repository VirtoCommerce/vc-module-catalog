using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Data.Search.Indexing;
using VirtoCommerce.SearchModule.Core.Model;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class DynamicAssociationSearchRequestBuilderTests
    {
        [Fact]
        public void Builder_Full_Succeded()
        {
            // Arrange
            var keyword = "testKeyword";
            var take = 9;
            var skip = 8;
            var outline = new[] { "testOutline" };

            var builder = new DynamicAssociationSearchRequestBuilder()
                .AddPropertySearch(new Dictionary<string, string[]>
                {
                    {"testProperty", new [] { "testValue1", "testValue2" }},
                })
                .AddKeywordSearch(keyword)
                .AddOutlineSearch(outline)
                .AddSortInfo("property1:asc;property2:desc")
                .WithPaging(skip, take);

            // Act
            var result = builder.Build();
            var propertyFilter = ((AndFilter) result.Filter).ChildFilters.OfType<TermFilter>().FirstOrDefault();
            var sortProperty1 = result.Sorting.FirstOrDefault(x => x.FieldName == "property1");
            var sortProperty2 = result.Sorting.FirstOrDefault(x => x.FieldName == "property2");
            var outlineFilter = ((AndFilter)result.Filter).ChildFilters.OfType<OrFilter>().FirstOrDefault()?.ChildFilters.FirstOrDefault();

            // Assert
            Assert.Equal("testProperty:testValue1,testValue2", propertyFilter?.ToString());

            Assert.Equal("__outline:*testOutline", outlineFilter?.ToString());

            Assert.Equal(keyword, result.SearchKeywords);
            Assert.Equal("__content", result.SearchFields.FirstOrDefault());

            Assert.Equal(skip, result.Skip);
            Assert.Equal(take, result.Take);

            Assert.False(sortProperty1?.IsDescending);
            Assert.True(sortProperty2?.IsDescending);
        }

        [Fact]
        public void Builder_All_Null_Succeded()
        {
            // Arrange
            var builder = new DynamicAssociationSearchRequestBuilder()
                .AddPropertySearch(null)
                .AddKeywordSearch(null)
                .AddOutlineSearch(null)
                .AddSortInfo(null);

            // Act
            var result = builder.Build();
            var outlineFilter = ((AndFilter) result.Filter).ChildFilters.OfType<OrFilter>().FirstOrDefault();
            var sorting = result.Sorting.FirstOrDefault();
            var propertyFilter = ((AndFilter)result.Filter).ChildFilters.OfType<TermFilter>().FirstOrDefault();

            // Assert
            Assert.Null(result.SearchKeywords);
            Assert.Null(result.SearchFields);
            Assert.Null(outlineFilter);
            Assert.Equal("__sort", sorting?.FieldName);
            Assert.Null(propertyFilter);
        }
    }
}
