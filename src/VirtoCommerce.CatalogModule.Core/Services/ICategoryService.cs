using System;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    public interface ICategoryService
    {
        Task<Category[]> GetByIdsAsync(string[] categoryIds, string responseGroup, string catalogId = null);
        [Obsolete(@"Need to remove after inherit ICategoryService from ICrudService<Category>")]
        Task SaveChangesAsync(Category[] categories);
        Task DeleteAsync(string[] categoryIds);
    }
}
