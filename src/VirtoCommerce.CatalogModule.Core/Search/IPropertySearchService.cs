using System;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model.Search;

namespace VirtoCommerce.CatalogModule.Core.Search
{
    public interface IPropertySearchService
    {
        [Obsolete(@"Need to remove after inheriting IPropertySearchService from SearchService.")]
        Task<PropertySearchResult> SearchPropertiesAsync(PropertySearchCriteria criteria);
    }
}
