using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    public interface IProductCrudService : ICrudService<CatalogProduct>
    {
        Task<CatalogProduct> GetAsync(string id, string responseGroup, string catalogId);
        Task<IList<CatalogProduct>> GetAsync(IList<string> ids, string responseGroup, string catalogId);
    }
}
