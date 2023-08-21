using System;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.CatalogModule.Core.Search
{
    /// <summary>
    /// Represent abstraction for search property dictionary items  
    /// </summary>
    public interface IPropertyDictionaryItemSearchService : ISearchService<PropertyDictionaryItemSearchCriteria, PropertyDictionaryItemSearchResult, PropertyDictionaryItem>
    {
        [Obsolete("Use SearchAsync(PropertyDictionaryItemSearchCriteria searchCriteria, bool clone)", DiagnosticId = "VC0005", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
        Task<PropertyDictionaryItemSearchResult> SearchAsync(PropertyDictionaryItemSearchCriteria searchCriteria);
    }
}
