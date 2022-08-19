using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.CatalogModule.Core.Search
{
    public interface IVideoSearchService : ISearchService<VideoSearchCriteria, VideoSearchResult, Video>
    {
    }
}
