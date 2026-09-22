using System.Collections.Generic;
using System.Threading.Tasks;

namespace VirtoCommerce.CatalogModule.Core.Search.Barcodes;

public interface IBarcodeSearchConfigurationService
{
    Task<BarcodeSearchSettings> GetSettingsAsync(string storeId);

    Task SaveSettingsAsync(string storeId, BarcodeSearchSettings settings);

    Task<IList<BarcodeSearchField>> GetAvailableFieldsAsync(string storeId);
}
