using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class VideoSearchService : SearchService<VideoSearchCriteria, VideoSearchResult, Video, VideoEntity>, IVideoSearchService
    {
        public VideoSearchService(
            Func<ICatalogRepository> repositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IVideoService crudService,
            IOptions<CrudOptions> crudOptions)
            : base(repositoryFactory, platformMemoryCache, crudService, crudOptions)
        {
        }

        protected override IQueryable<VideoEntity> BuildQuery(IRepository repository, VideoSearchCriteria criteria)
        {
            var query = ((ICatalogRepository)repository).Videos;

            if (!criteria.OwnerIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.OwnerIds.Contains(x.OwnerId));
            }

            if (!criteria.OwnerType.IsNullOrEmpty())
            {
                query = query.Where(x => x.OwnerType == criteria.OwnerType);
            }

            if (!criteria.LanguageCode.IsNullOrEmpty())
            {
                // Video without language code is considered as a video with any language
                query = query.Where(x => x.LanguageCode == criteria.LanguageCode || x.LanguageCode == null);
            }

            if (!criteria.Keyword.IsNullOrEmpty())
            {
                query = query.Where(x => x.Name.Contains(criteria.Keyword));
            }

            return query;
        }

        protected override IList<SortInfo> BuildSortExpression(VideoSearchCriteria criteria)
        {
            var sortInfos = criteria.SortInfos;

            if (sortInfos.IsNullOrEmpty())
            {
                sortInfos = new[]
                {
                    new SortInfo { SortColumn = nameof(VideoEntity.SortOrder) },
                    new SortInfo { SortColumn = nameof(VideoEntity.Name) },
                };
            }

            return sortInfos;
        }
    }
}
