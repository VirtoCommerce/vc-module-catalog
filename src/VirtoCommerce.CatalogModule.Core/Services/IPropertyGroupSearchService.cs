using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.CatalogModule.Core.Services;

public interface IPropertyGroupSearchService : ISearchService<PropertyGroupSearchCriteria, PropertyGroupSearchResult, PropertyGroup>
{
}

