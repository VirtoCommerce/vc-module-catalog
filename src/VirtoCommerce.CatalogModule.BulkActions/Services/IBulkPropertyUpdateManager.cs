using System.Threading.Tasks;
using VirtoCommerce.BulkActionsModule.Core.Models.BulkActions;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogModule.BulkActions.Services
{
    public interface IBulkPropertyUpdateManager
    {
        Task<Property[]> GetPropertiesAsync(BulkActionContext context);

        Task<BulkActionResult> UpdatePropertiesAsync(CatalogProduct[] products, Property[] properties);
    }
}
