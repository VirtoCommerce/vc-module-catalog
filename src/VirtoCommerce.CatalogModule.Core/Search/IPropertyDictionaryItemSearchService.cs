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
    }
}
