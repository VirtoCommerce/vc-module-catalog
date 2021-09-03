using System;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    public interface IItemService
    {
        Task<CatalogProduct[]> GetByIdsAsync(string[] itemIds, string respGroup, string catalogId = null);
        Task<CatalogProduct> GetByIdAsync(string itemId, string responseGroup, string catalogId = null);
        [Obsolete(@"Need to remove after inherit IItemService from ICrudService<CatalogProduct>")]
        Task SaveChangesAsync(CatalogProduct[] items);
        [Obsolete(@"Need to remove after inherit IItemService from ICrudService<CatalogProduct>")]
        Task DeleteAsync(string[] itemIds);
    }
}
