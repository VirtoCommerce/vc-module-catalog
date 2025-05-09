using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.CatalogModule.Core.Services;

public interface IBrandStoreSettingSearchService : ISearchService<BrandStoreSettingSearchCriteria, BrandStoreSettingSearchResult, BrandStoreSetting>
{
}
