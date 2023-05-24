using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search.Indexed;
using VirtoCommerce.CatalogModule.Core.Search.Indexed;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Data.Search.Indexing;

public class ProductSuggestionService : IProductSuggestionService
{
    private readonly ISearchProvider _searchProvider;

    protected virtual string ProductSuggestonField => nameof(CatalogProduct.Name).ToLowerInvariant();

    public ProductSuggestionService(ISearchProvider searchProvider)
    {
        _searchProvider = searchProvider;
    }

    public async Task<SuggestionResponse> GetSuggestionsAsync(ProductSuggestionRequest request)
    {
        if (_searchProvider is not ISupportSuggestions supportSuggestions)
        {
            return AbstractTypeFactory<SuggestionResponse>.TryCreateInstance();
        }

        var searchSuggestionResuest = AbstractTypeFactory<SuggestionRequest>.TryCreateInstance();
        searchSuggestionResuest.Query = request.Query;
        searchSuggestionResuest.Size = request.Size;

        if (!string.IsNullOrWhiteSpace(request.CatalogId))
        {
            searchSuggestionResuest.QueryContext = new Dictionary<string, object> { { "catalog", request.CatalogId } };
        }

        // product suggestion only works with the predefined Product field
        searchSuggestionResuest.Fields = new List<string> { ProductSuggestonField };

        var result = await supportSuggestions.GetSuggestionsAsync(KnownDocumentTypes.Product, searchSuggestionResuest);

        return result;
    }
}
