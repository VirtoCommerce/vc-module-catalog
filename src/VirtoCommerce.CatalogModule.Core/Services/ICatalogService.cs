using System;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    public interface ICatalogService
    {
        [Obsolete(@"Need to remove after inheriting ICatalogService from ICrudService<Catalog>.")]
        Task<Catalog[]> GetByIdsAsync(string[] catalogIds, string responseGroup = null);
        [Obsolete(@"Need to remove after inheriting ICatalogService from ICrudService<Catalog>.")]
        Task SaveChangesAsync(Catalog[] catalogs);
        [Obsolete(@"Need to remove after inheriting ICatalogService from ICrudService<Catalog>.")]
        Task DeleteAsync(string[] catalogIds);
    }
}
