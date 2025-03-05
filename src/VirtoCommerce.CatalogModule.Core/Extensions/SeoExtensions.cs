using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CoreModule.Core.Seo;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Core.Extensions;

public static class SeoExtensions
{
    /// <summary>
    /// Returns SEO record with the highest score
    /// </summary>
    public static SeoInfo GetBestMatchingSeoInfo(this ISeoSupport seoSupport, Store store, string language, string slug = null, string permalink = null)
    {
        return seoSupport?.SeoInfos?.GetBestMatchingSeoInfo(store, language, slug, permalink);
    }

    /// <summary>
    /// Returns SEO record with the highest score
    /// </summary>
    public static SeoInfo GetBestMatchingSeoInfo(this IEnumerable<SeoInfo> seoInfos, Store store, string language, string slug = null, string permalink = null)
    {
        return seoInfos?.GetBestMatchingSeoInfo(store?.Id, store?.DefaultLanguage, language ?? store?.DefaultLanguage, slug, permalink);
    }

    /// <summary>
    /// Returns SEO record with the highest score
    /// </summary>
    public static SeoInfo GetBestMatchingSeoInfo(this ISeoSupport seoSupport, string storeId, string storeDefaultLanguage, string language, string slug = null, string permalink = null)
    {
        return seoSupport?.SeoInfos?.GetBestMatchingSeoInfo(storeId, storeDefaultLanguage, language, slug, permalink);
    }

    /// <summary>
    /// Returns SEO record with the highest score
    /// </summary>
    public static SeoInfo GetBestMatchingSeoInfo(this IEnumerable<SeoInfo> seoInfos, string storeId, string storeDefaultLanguage, string language, string slug = null, string permalink = null)
    {
        if (string.IsNullOrEmpty(storeId) ||
            string.IsNullOrEmpty(storeDefaultLanguage) ||
            string.IsNullOrEmpty(language))
        {
            return null;
        }

        var result = seoInfos
            ?.Select(seoInfo => new
            {
                SeoRecord = seoInfo,
                Score = seoInfo.CalculateScore(storeId, storeDefaultLanguage, language, slug, permalink),
            })
            .OrderByDescending(x => x.Score)
            .Select(x => x.SeoRecord)
            .FirstOrDefault();

        return result;
    }


    private static int CalculateScore(this SeoInfo seoInfo, string storeId, string storeDefaultLanguage, string language, string slug, string permalink)
    {
        var score = new[]
            {
                seoInfo.IsActive,
                seoInfo.SemanticUrl.EqualsWithoutSlash(permalink),
                seoInfo.SemanticUrl.EqualsWithoutSlash(slug),
                seoInfo.StoreId.EqualsIgnoreCase(storeId),
                seoInfo.LanguageCode.EqualsIgnoreCase(language),
                seoInfo.LanguageCode.EqualsIgnoreCase(storeDefaultLanguage),
                seoInfo.LanguageCode.IsNullOrEmpty(),
            }
            .Reverse()
            .Select((valid, index) => valid ? 1 << index : 0)
            .Sum();

        return score;
    }

    private static bool EqualsWithoutSlash(this string a, string b)
    {
        return a.TrimStart('/').EqualsIgnoreCase(b?.TrimStart('/'));
    }
}
