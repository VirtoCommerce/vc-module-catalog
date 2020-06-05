using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    public interface IDynamicAssociationService
    {
        Task<DynamicAssociation[]> GetByIdsAsync(string[] itemIds);
        Task SaveChangesAsync(DynamicAssociation[] items);
        Task DeleteAsync(string[] itemIds);
    }
}
