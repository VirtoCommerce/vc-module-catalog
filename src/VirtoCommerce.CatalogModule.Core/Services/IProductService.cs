using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    public interface IProductService : ICrudService<CatalogProduct>
    {
        Task<IList<CatalogProduct>> GetByCodes(string catalogId, IList<string> codes, string responseGroup);
        Task<IDictionary<string, string>> GetIdsByCodes(string catalogId, IList<string> codes);
        Task<CatalogProduct> GetByIdAsync(string itemId, string responseGroup, string catalogId);
        Task<IList<CatalogProduct>> GetByIdsAsync(IList<string> ids, string responseGroup, string catalogId);
    }

    // Left for backward compatibility only - new code should use IProductService which has consistent naming scheme
    // (compare IProductSearchService, ICategoryService/ICategorySearchService and ICatalogService/ICatalogSearchService)
    [Obsolete($"Use {nameof(IProductService)} instead")]
    public interface IItemService : IProductService;
}
