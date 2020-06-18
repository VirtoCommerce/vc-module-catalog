using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.DynamicAssociations;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    public interface IDynamicAssociationConditionSelector
    {
        Task<DynamicAssociationConditionEvaluationRequest> GetDynamicAssociationConditionAsync(DynamicAssociationEvaluationContext context, CatalogProduct product);
    }
}
