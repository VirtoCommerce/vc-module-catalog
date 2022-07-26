using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    public interface ICategoryService : ICrudService<Category>
    {
        Task<Category[]> GetByIdsAsync(string[] categoryIds, string responseGroup, string catalogId);
    }
}
