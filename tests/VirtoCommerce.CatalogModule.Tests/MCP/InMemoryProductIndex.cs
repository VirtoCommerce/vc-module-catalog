using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Tests.MCP;

// The search index of the test data's products: it answers the filters the tools send as the search providers answer
// them, and fails a request that would read more than one page, as no tool may read a catalog's products in full.
public class InMemoryProductIndex(CatalogToolsTestData data) : ISearchProvider
{
    // The most documents one request may ask for: a page of a list, or the products of one check.
    public const int MaxTake = 100;

    public List<SearchRequest> Requests { get; } = [];

    public Task<SearchResponse> SearchAsync(string documentType, SearchRequest request)
    {
        Requests.Add(request);

        if (request.Take > MaxTake)
        {
            throw new InvalidOperationException($"A request asked the index for {request.Take} documents.");
        }

        var documents = data.Products
            .Where(x => !data.NotIndexed.Contains(x.Id))
            .Select(x => (Id: x.Id, Fields: data.IndexedDocuments.TryGetValue(x.Id, out var indexed) ? indexed : data.GetIndexDocument(x)))
            .Where(x => Matches(request.Filter, x.Id, x.Fields))
            .Where(x => string.IsNullOrEmpty(request.SearchKeywords) || x.Fields["__content"].Any(v => v.ToString().Contains(request.SearchKeywords, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        foreach (var sorting in (request.Sorting ?? []).Reverse())
        {
            documents = (sorting.IsDescending
                ? documents.OrderByDescending(x => x.Fields[sorting.FieldName].FirstOrDefault())
                : documents.OrderBy(x => x.Fields[sorting.FieldName].FirstOrDefault())).ToList();
        }

        var response = new SearchResponse
        {
            TotalCount = documents.Count,
            Documents = documents.Skip(request.Skip).Take(request.Take).Select(x => ToSearchDocument(x.Id, x.Fields, request.IncludeFields)).ToList(),
        };

        return Task.FromResult(response);
    }

    public Task DeleteIndexAsync(string documentType)
    {
        throw new NotSupportedException();
    }

    public Task<IndexingResult> IndexAsync(string documentType, IList<IndexDocument> documents)
    {
        throw new NotSupportedException();
    }

    public Task<IndexingResult> RemoveAsync(string documentType, IList<IndexDocument> documents)
    {
        throw new NotSupportedException();
    }

    private static bool Matches(IFilter filter, string id, Dictionary<string, List<object>> fields)
    {
        return filter switch
        {
            null => true,
            AndFilter and => and.ChildFilters.All(x => Matches(x, id, fields)),
            OrFilter or => or.ChildFilters.Any(x => Matches(x, id, fields)),
            NotFilter not => !Matches(not.ChildFilter, id, fields),
            IdsFilter ids => ids.Values.Contains(id, StringComparer.OrdinalIgnoreCase),
            // The index keeps outlines in lower case and compares them as they are; other values ignore case.
            TermFilter term => fields.TryGetValue(term.FieldName, out var values) &&
                values.Any(v => term.Values.Contains(v.ToString(), term.FieldName == "__outline" ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase)),
            RangeFilter range => fields.TryGetValue(range.FieldName, out var values) && values.Any(v => range.Values.Any(r => InRange(v, r))),
            _ => throw new NotSupportedException($"The test index does not answer {filter.GetType().Name}."),
        };
    }

    private static bool InRange(object value, RangeFilterValue range)
    {
        var date = (DateTime)value;
        var lower = Parse(range.Lower);
        var upper = Parse(range.Upper);

        return (lower == null || (range.IncludeLower ? date >= lower : date > lower))
            && (upper == null || (range.IncludeUpper ? date <= upper : date < upper));
    }

    private static DateTime? Parse(string value)
    {
        return string.IsNullOrEmpty(value) ? null : DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
    }

    // Dates come back as the Lucene provider returns them: round-trip text in UTC.
    private static SearchDocument ToSearchDocument(string id, Dictionary<string, List<object>> fields, IList<string> includeFields)
    {
        var document = new SearchDocument { Id = id };

        foreach (var field in includeFields ?? fields.Keys.ToList())
        {
            if (fields.TryGetValue(field, out var values))
            {
                document[field] = values.Count == 1 ? ToText(values[0]) : values.Select(ToText).ToArray();
            }
        }

        return document;
    }

    private static object ToText(object value)
    {
        return value is DateTime date ? date.ToString("O", CultureInfo.InvariantCulture) : value;
    }
}
