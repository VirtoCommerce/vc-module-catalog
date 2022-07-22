using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Search;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule2.Data.Search
{
    public class VideoSearchService2 : VideoSearchService
    {
        public VideoSearchService2(Func<ICatalogRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache, IVideoService videoService)
            : base(repositoryFactory, platformMemoryCache, videoService)
        {
        }

        protected override IQueryable<VideoEntity> BuildQuery(IRepository repository, VideoSearchCriteria criteria)
        {
            return base.BuildQuery(repository, criteria);
        }

        protected override IList<SortInfo> BuildSortExpression(VideoSearchCriteria criteria)
        {
            return base.BuildSortExpression(criteria);
        }

        protected override Task<VideoSearchResult> ProcessSearchResultAsync(VideoSearchResult result, VideoSearchCriteria criteria)
        {
            return base.ProcessSearchResultAsync(result, criteria);
        }

        public override Task<VideoSearchResult> SearchAsync(VideoSearchCriteria criteria)
        {
            return base.SearchAsync(criteria);
        }
    }
}
