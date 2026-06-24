using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using VirtoCommerce.CatalogModule.Data.Search.Sorting;
using VirtoCommerce.SearchModule.Core.Model;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class ProductSortableFieldServiceTests
    {
        [Fact]
        public async Task GetSortableFields_VirtualTokensFirst_ThenFilterableSingleValuedSchemaFields()
        {
            var schema = new IndexDocument("product");
            schema.Add(Field("brand", IndexDocumentFieldValueType.String, filterable: true, collection: false));
            schema.Add(Field("availability", IndexDocumentFieldValueType.String, filterable: true, collection: false));
            schema.Add(Field("description", IndexDocumentFieldValueType.String, filterable: false, collection: false)); // not filterable -> excluded
            schema.Add(Field("categories", IndexDocumentFieldValueType.String, filterable: true, collection: true));     // multi-valued -> excluded
            schema.Add(Field("__outline", IndexDocumentFieldValueType.String, filterable: true, collection: false));     // system field -> excluded
            schema.Add(Field("name", IndexDocumentFieldValueType.String, filterable: true, collection: false));          // same as a virtual token -> deduped

            var service = new TestProductSortableFieldService(schema);

            var fields = await service.GetSortableFieldsAsync("store");

            // Virtual tokens (in declared order) first, then schema fields sorted alphabetically; excluded/duplicate fields dropped.
            fields.Select(x => x.Name).Should().Equal("__score", "name", "price", "availability", "brand");
        }

        [Fact]
        public async Task GetSortableFields_EmptySchema_ReturnsOnlyVirtualTokens()
        {
            var service = new TestProductSortableFieldService(new IndexDocument("product"));

            var fields = await service.GetSortableFieldsAsync("store");

            fields.Select(x => x.Name).Should().Equal("__score", "name", "price");
            fields.Single(x => x.Name == "__score").DataType.Should().Be("Virtual");
        }

        private static IndexDocumentField Field(string name, IndexDocumentFieldValueType type, bool filterable, bool collection) =>
            new(name, type == IndexDocumentFieldValueType.String ? "x" : 0, type)
            {
                IsRetrievable = true,
                IsFilterable = filterable,
                IsCollection = collection,
            };

        // Stubs the index schema so the projection logic is tested without index configurations / schema builders.
        private sealed class TestProductSortableFieldService : ProductSortableFieldService
        {
            private readonly IndexDocument _schema;

            public TestProductSortableFieldService(IndexDocument schema)
                : base(new List<IndexDocumentConfiguration>())
            {
                _schema = schema;
            }

            protected override Task<IndexDocument> BuildProductSchemaAsync() => Task.FromResult(_schema);
        }
    }
}
