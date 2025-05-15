using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Services;
using static VirtoCommerce.CatalogModule.Core.ModuleConstants.Settings.General;

namespace VirtoCommerce.CatalogModule.Data.Services;

public class BrandSettingService : IBrandSettingService
{
    private readonly IStoreService _storeService;

    public BrandSettingService(IStoreService storeService)
    {
        _storeService = storeService;
    }

    public async Task<BrandStoreSetting> GetByStoreIdAsync(string storeId)
    {
        var store = await _storeService.GetNoCloneAsync(storeId);

        if (store == null)
        {
            return null;
        }

        var brandStoreSetting = AbstractTypeFactory<BrandStoreSetting>.TryCreateInstance();

        brandStoreSetting.StoreId = store.Id;

        brandStoreSetting.BrandsEnabled = store.Settings.GetValue<bool>(BrandsEnabled);
        brandStoreSetting.BrandCatalogId = store.Settings.GetValue<string>(BrandCatalogId);
        brandStoreSetting.BrandPropertyName = store.Settings.GetValue<string>(BrandPropertyName);

        return brandStoreSetting;
    }

    public async Task UpdateAsync(BrandStoreSetting brandStoreSetting)
    {
        var store = await _storeService.GetByIdAsync(brandStoreSetting.StoreId);

        if (store == null)
        {
            return;
        }

        var brandsEnabledSetting = store.Settings.FirstOrDefault(x => x.Name.EqualsIgnoreCase(BrandsEnabled.Name));
        if (brandsEnabledSetting == null)
        {
            brandsEnabledSetting = new ObjectSettingEntry(BrandsEnabled);
            store.Settings.Add(brandsEnabledSetting);
        }
        brandsEnabledSetting.Value = brandStoreSetting.BrandsEnabled;

        var brandCatalogIdSetting = store.Settings.FirstOrDefault(x => x.Name.EqualsIgnoreCase(BrandCatalogId.Name));
        if (brandCatalogIdSetting == null)
        {
            brandCatalogIdSetting = new ObjectSettingEntry(BrandCatalogId);
            store.Settings.Add(brandCatalogIdSetting);
        }
        brandCatalogIdSetting.Value = brandStoreSetting.BrandCatalogId;

        var brandPropertyNameSetting = store.Settings.FirstOrDefault(x => x.Name.EqualsIgnoreCase(BrandPropertyName.Name));
        if (brandPropertyNameSetting == null)
        {
            brandPropertyNameSetting = new ObjectSettingEntry(BrandPropertyName);
            store.Settings.Add(brandPropertyNameSetting);
        }
        brandPropertyNameSetting.Value = brandStoreSetting.BrandPropertyName;

        await _storeService.SaveChangesAsync([store]);
    }
}
