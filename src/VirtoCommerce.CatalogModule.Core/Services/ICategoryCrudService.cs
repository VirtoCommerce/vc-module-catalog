using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    public interface ICategoryCrudService : ICrudService<Category>
    {
        Task<Category> GetAsync(string id, string responseGroup, string catalogId);
        Task<IList<Category>> GetAsync(IList<string> ids, string responseGroup, string catalogId);
    }
}
