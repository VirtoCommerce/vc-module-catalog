using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.CatalogModule.Core.Search;

public interface IAutomaticLinkQuerySearchService : ISearchService<AutomaticLinkQuerySearchCriteria, AutomaticLinkQuerySearchResult, AutomaticLinkQuery>;
