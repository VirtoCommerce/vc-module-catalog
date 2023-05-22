using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Search.Indexed;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Data.Search.Indexing;

public class ProductSuggestionService : IProductSuggestionService
{
    private readonly ISearchProvider _searchProvider;

    protected virtual string ProductSuggestonField => nameof(CatalogProduct.Name);

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

        // product suggestion only works with the predefined Product field
        request.Fields = new List<string> { ProductSuggestonField };

        var result = await supportSuggestions.GetSuggestionsAsync(KnownDocumentTypes.Product, request);

        return result;
    }
}
