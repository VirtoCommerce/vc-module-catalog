using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search.Indexed;
using VirtoCommerce.CatalogModule.Core.Search.Indexed;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Extensions;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Data.Search.Indexing;

public class ProductSuggestionService : IProductSuggestionService
{
    private readonly ISearchProvider _searchProvider;

    protected virtual string ProductSuggestionField => nameof(CatalogProduct.Name).ToLowerInvariant();

    public ProductSuggestionService(ISearchProvider searchProvider)
    {
        _searchProvider = searchProvider;
    }

    public async Task<SuggestionResponse> GetSuggestionsAsync(ProductSuggestionRequest request)
    {
        const string documentType = KnownDocumentTypes.Product;

        if (!_searchProvider.Is<ISupportSuggestions>(documentType, out var supportSuggestions))
        {
            return AbstractTypeFactory<SuggestionResponse>.TryCreateInstance();
        }

        var suggestionRequest = AbstractTypeFactory<SuggestionRequest>.TryCreateInstance();
        suggestionRequest.Query = request.Query;
        suggestionRequest.Size = request.Size;

        if (!string.IsNullOrWhiteSpace(request.CatalogId))
        {
            suggestionRequest.QueryContext = new Dictionary<string, object> { { "catalog", request.CatalogId } };
        }

        // product suggestion only works with the predefined Product field
        suggestionRequest.Fields = new List<string> { ProductSuggestionField };

        var result = await supportSuggestions.GetSuggestionsAsync(documentType, suggestionRequest);

        return result;
    }
}
