using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.DynamicAssociations;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    public interface IDynamicAssociationsValueFetcher
    {
        Task<DynamicAssociationValue> GetDynamicAssociationValueAsync(string group, string storeId, CatalogProduct product);
    }
}
