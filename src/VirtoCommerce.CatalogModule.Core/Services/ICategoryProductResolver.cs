using System.Collections.Generic;
using System.Threading.Tasks;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    public interface ICategoryProductResolver
    {
        Task<IList<string>> GetCategoryProductIds(string categoryId);
    }
}
