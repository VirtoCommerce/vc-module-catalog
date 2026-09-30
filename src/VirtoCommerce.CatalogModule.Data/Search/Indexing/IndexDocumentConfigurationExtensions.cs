using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.SearchModule.Core.Extensions;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Data.Search.Indexing;

public static class IndexDocumentConfigurationExtensions
{
    // Composes the product index schema across every registered schema builder (the same set the indexer uses to create the index).
    public static async Task<IndexDocument> BuildProductSchemaAsync(this IEnumerable<IndexDocumentConfiguration> configurations)
    {
        var schema = new IndexDocument(Guid.NewGuid().ToString("N"));

        var schemaBuilders = configurations
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
