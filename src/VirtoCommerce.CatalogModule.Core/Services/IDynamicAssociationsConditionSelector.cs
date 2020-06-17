using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.DynamicAssociations;
using VirtoCommerce.CatalogModule.Core.Model.Search;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    public interface IDynamicAssociationsConditionSelector
    {
        Task<DynamicAssociationCondition> GetDynamicAssociationConditionAsync(DynamicRuleAssociationsEvaluationContext evaluationContext, CatalogProduct product);
    }
}
