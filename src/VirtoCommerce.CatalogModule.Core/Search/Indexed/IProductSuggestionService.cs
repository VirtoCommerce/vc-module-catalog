using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model.Search.Indexed;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Core.Search.Indexed;

public interface IProductSuggestionService
{
    Task<SuggestionResponse> GetSuggestionsAsync(ProductSuggestionRequest request);
}
