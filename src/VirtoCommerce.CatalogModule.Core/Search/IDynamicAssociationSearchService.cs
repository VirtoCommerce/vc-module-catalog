using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model.Search;

namespace VirtoCommerce.CatalogModule.Core.Search
{
    public interface IDynamicAssociationSearchService
    {
        Task<DynamicAssociationSearchResult> SearchDynamicAssociationsAsync(DynamicAssociationSearchCriteria criteria);
    }
}
