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
    [Obsolete("Use VirtoCommerce.CatalogModule.Data.Services.CatalogSeoResolver", DiagnosticId = "VC0010", UrlFormat = "https://docs.virtocommerce.org/platform/user-guide/versions/virto3-products-versions/")]
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
        public Task<SeoInfo[]> FindSeoBySlugAsync(string slug)
        {
            var cacheKey = CacheKey.With(GetType(), nameof(FindSeoBySlugAsync), slug);
            return _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async cacheEntry =>
            {
                cacheEntry.AddExpirationToken(SeoInfoCacheRegion.CreateChangeToken());
                var result = new List<SeoInfo>();
                using (var repository = _repositoryFactory())
                {
                    // Find seo entries for specified keyword. Also add other seo entries related to found object.
                    result = (await repository.SeoInfos.Where(x => x.Keyword == slug)
                        .Join(repository.SeoInfos, x => new { x.ItemId, x.CategoryId }, y => new { y.ItemId, y.CategoryId }, (x, y) => y)
                        .ToArrayAsync()).Select(x =>
                    {
                        // this is obsolete code, but we need to keep it for backward compatibility
                        // the old ToModel method cannot be used

                        var seoInfoResult = AbstractTypeFactory<SeoInfo>.TryCreateInstance();

                        seoInfoResult.Id = x.Id;
                        seoInfoResult.CreatedBy = x.CreatedBy;
                        seoInfoResult.CreatedDate = x.CreatedDate;
                        seoInfoResult.ModifiedBy = x.ModifiedBy;
                        seoInfoResult.ModifiedDate = x.ModifiedDate;
                        seoInfoResult.LanguageCode = x.Language;
                        seoInfoResult.SemanticUrl = x.Keyword;
                        seoInfoResult.PageTitle = x.Title;
                        seoInfoResult.ImageAltDescription = x.ImageAltDescription;
                        seoInfoResult.IsActive = x.IsActive;
                        seoInfoResult.MetaDescription = x.MetaDescription;
                        seoInfoResult.MetaKeywords = x.MetaKeywords;
                        seoInfoResult.ObjectId = x.ItemId ?? x.CategoryId ?? x.CatalogId;
                        seoInfoResult.ObjectType = x.ItemId != null ? "CatalogProduct" : x.CategoryId != null ? "Category" : "Catalog";
                        seoInfoResult.StoreId = x.StoreId;

                        return seoInfoResult;
                    }).ToList();
                }

                return result.ToArray();
            });
        }
        #endregion
    }
}
