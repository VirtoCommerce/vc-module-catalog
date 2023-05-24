using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    public interface ICatalogService : ICrudService<Catalog>
    {
        Task<Catalog[]> GetByIdsAsync(string[] catalogIds, string responseGroup = null);
        Task SaveChangesAsync(Catalog[] catalogs);
        Task DeleteAsync(string[] catalogIds);

        /// <summary>
        /// Returns data from the cache without cloning, which consumes less memory, but returned data must not be modified.
        /// </summary>
        Task<IList<Catalog>> GetNoCloneAsync(IList<string> ids, string responseGroup = null);
    }
}
