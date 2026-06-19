using System.Collections.Generic;
using System.Threading.Tasks;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    public interface ICategoryIndexingService
    {
        Task<IList<string>> GetProductIdsForIndexAsync(string categoryId);
    }
}
