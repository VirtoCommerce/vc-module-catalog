using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Search.Indexed;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Data.Search.Indexing;

public class ProductSuggestionService : IProductSuggestionService
{
    private readonly ISearchProvider _searchProvider;

    public ProductSuggestionService(ISearchProvider searchProvider)
    {
        _searchProvider = searchProvider;
    }

    public async Task<SuggestionResponse> GetSuggestionsAsync(SuggestionRequest request)
    {
        if (_searchProvider is not ISupportSuggestions supportSuggestions)
        {
            return AbstractTypeFactory<SuggestionResponse>.TryCreateInstance();
        }

        var result = await supportSuggestions.GetSuggestionsAsync(KnownDocumentTypes.Product, request);

        return result;
    }
}
