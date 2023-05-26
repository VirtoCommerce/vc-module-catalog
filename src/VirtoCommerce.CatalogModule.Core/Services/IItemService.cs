using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    public interface IItemService : ICrudService<CatalogProduct>
    {
        Task<IList<CatalogProduct>> GetByCodes(string catalogId, IList<string> codes, string responseGroup);
        Task<IDictionary<string, string>> GetIdsByCodes(string catalogId, IList<string> codes);
        Task<CatalogProduct[]> GetByIdsAsync(string[] itemIds, string respGroup, string catalogId = null);
        Task<CatalogProduct> GetByIdAsync(string itemId, string responseGroup, string catalogId = null);
        Task SaveChangesAsync(CatalogProduct[] items);
        Task DeleteAsync(string[] itemIds);

        /// <summary>
        /// Returns data from the cache without cloning. This consumes less memory, but returned data must not be modified.
        /// </summary>
        Task<IList<CatalogProduct>> GetNoCloneAsync(IList<string> ids, string responseGroup);
    }
}
