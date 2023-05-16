using System.Threading.Tasks;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Core.Search.Indexed;

public interface IProductSuggestionService
{
    Task<SuggestionResponse> GetSuggestionsAsync(SuggestionRequest request);
}
