using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Core.Services;

public interface IBrandSettingService
{
    Task<BrandStoreSetting> GetByStoreIdAsync(string storeId);
    Task UpdateAsync(BrandStoreSetting brandStoreSetting);
}
