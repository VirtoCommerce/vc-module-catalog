using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    public interface ICategoryService : ICrudService<Category>
    {
        Task<Category[]> GetByIdsAsync(string[] categoryIds, string responseGroup, string catalogId = null);
        Task SaveChangesAsync(Category[] categories);
        Task DeleteAsync(string[] categoryIds);

        /// <summary>
        /// Returns data from the cache without cloning. This consumes less memory, but returned data must not be modified.
        /// </summary>
        Task<IList<Category>> GetNoCloneAsync(IList<string> ids, string responseGroup = null);
    }
}
