using System;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model.Search;

namespace VirtoCommerce.CatalogModule.Core.Search
{
    /// <summary>
    /// This interface should implement <![CDATA[<see cref="SearchService<PropertyDictionaryItem>"/>]]> without methods.
    /// Methods left for compatibility and should be removed after upgrade to inheritance
    /// </summary>
    public interface IPropertyDictionaryItemSearchService
    {
        [Obsolete(@"Need to remove after inherit IPropertyDictionaryItemSearchService from SearchService<PropertyDictionaryItem>")]
        Task<PropertyDictionaryItemSearchResult> SearchAsync(PropertyDictionaryItemSearchCriteria searchCriteria);
    }
}
