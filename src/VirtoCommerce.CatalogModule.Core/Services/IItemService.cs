using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    public interface IItemService : ICrudService<CatalogProduct>
    {
        Task<CatalogProduct[]> GetByIdsAsync(string[] itemIds, string respGroup, string catalogId);
        Task<CatalogProduct> GetByIdAsync(string itemId, string responseGroup, string catalogId);
    }
}
