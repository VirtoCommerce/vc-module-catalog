using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Search.Sorting;
using VirtoCommerce.SearchModule.Core.Extensions;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Data.Search.Sorting;

public class ProductSortableFieldService : IProductSortableFieldService
{
    // Logical tokens that are not single literal index fields: __score is relevance; price is bound to the
    // currency-specific field (price_{currency}) by the search-request builder.
    private static readonly ProductSortableField[] _virtualFields =
    [
        new() { Name = "__score", DataType = "Virtual" },
        new() { Name = "name", DataType = "String" },
        new() { Name = "price", DataType = "Decimal" },
    ];

    private readonly IEnumerable<IndexDocumentConfiguration> _configurations;

    public ProductSortableFieldService(IEnumerable<IndexDocumentConfiguration> configurations)
    {
        _configurations = configurations;
    }

    public async Task<IList<ProductSortableField>> GetSortableFieldsAsync(string storeId)
    {
        var result = new List<ProductSortableField>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Logical tokens first; they take precedence over any same-named index field (their names are statically distinct).
        result.AddRange(_virtualFields);
        seen.UnionWith(_virtualFields.Select(x => x.Name));

        // Compose the product index schema across every registered schema builder (the same set the indexer uses to
        // create the index), then offer single-valued filterable fields. Excludes internal "__" system fields.
        var schema = await BuildProductSchemaAsync();

        foreach (var field in schema.Fields
                     .Where(x => x.IsFilterable && !x.IsCollection && !string.IsNullOrEmpty(x.Name))
                     .Where(x => !x.Name.StartsWith("__", StringComparison.Ordinal))
                     .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
        {
            var isNew = seen.Add(field.Name);
            if (isNew)
            {
                result.Add(new ProductSortableField { Name = field.Name, DataType = field.ValueType.ToString() });
            }
        }

        return result;
    }

    protected virtual async Task<IndexDocument> BuildProductSchemaAsync()
    {
        var schema = new IndexDocument(Guid.NewGuid().ToString("N"));

        var schemaBuilders = _configurations
            .GetDocumentSources(KnownDocumentTypes.Product)
            .Select(x => x.DocumentBuilder)
            .OfType<IIndexSchemaBuilder>();

        foreach (var schemaBuilder in schemaBuilders)
        {
            await schemaBuilder.BuildSchemaAsync(schema);
        }

        return schema;
    }
}
