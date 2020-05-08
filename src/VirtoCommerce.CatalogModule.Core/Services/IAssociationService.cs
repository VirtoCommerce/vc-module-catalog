using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    public interface IAssociationService
    {
        Task LoadAssociationsAsync(IHasAssociations[] owners);
        Task<ProductAssociation[]> GetAssociationsAsync(string[] ownerIds);
        Task UpdateAssociationSetAsync(string ownerId, ProductAssociation[] associations);
        Task UpdateAssociationAsync(string ownerId, ProductAssociation association);
        Task SaveChangesAsync(IHasAssociations[] owners);
        Task DeleteAssociationAsync(string[] ids);
    }
}
