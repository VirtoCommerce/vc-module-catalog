using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.CatalogModule.Data.Caching;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CoreModule.Core.Seo;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class CatalogSeoBySlugResolver : ISeoBySlugResolver
    {
        private readonly IPlatformMemoryCache _platformMemoryCache;
        private readonly Func<ICatalogRepository> _repositoryFactory;

        public CatalogSeoBySlugResolver(Func<ICatalogRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache)
        {
            _repositoryFactory = repositoryFactory;
            _platformMemoryCache = platformMemoryCache;
        }

        #region ISeoBySlugResolver members
        public async Task<SeoInfo[]> FindSeoBySlugAsync(string slug)
        {

            var cacheKey = CacheKey.With(GetType(), nameof(FindSeoBySlugAsync), slug);
            return await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async cacheEntry =>
            {
                cacheEntry.AddExpirationToken(SeoInfoCacheRegion.CreateChangeToken());
                var result = new List<SeoInfo>();
                using (var repository = _repositoryFactory())
                {
                    // Find seo entries for specified keyword. Also add other seo entries related to found object.
                    result = (await repository.SeoInfos.Where(x => x.Keyword == slug)
                        .Join(repository.SeoInfos, x => new { x.ItemId, x.CategoryId }, y => new { y.ItemId, y.CategoryId }, (x, y) => y)
                        .ToArrayAsync()).Select(x => x.ToModel(AbstractTypeFactory<SeoInfo>.TryCreateInstance())).ToList();
                }

                return result.ToArray();
            });
        }
        #endregion
    }
}
