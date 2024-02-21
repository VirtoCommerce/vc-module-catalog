using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    public interface IMeasureSearchService : ISearchService<MeasureSearchCriteria, MeasureSearchResult, Measure>
    {
    }
}
