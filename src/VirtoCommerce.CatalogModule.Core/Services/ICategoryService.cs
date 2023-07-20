using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    public interface ICategoryService : ICrudService<Category>
    {
        Task<IList<Category>> GetByIdsAsync(IList<string> ids, string responseGroup, string catalogId);
    }
}
