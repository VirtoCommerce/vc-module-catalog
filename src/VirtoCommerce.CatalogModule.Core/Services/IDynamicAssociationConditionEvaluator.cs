using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model.DynamicAssociations;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    public interface IDynamicAssociationConditionEvaluator
    {
        Task<string[]> EvaluateDynamicAssociationConditionAsync(DynamicAssociationEvaluationContext context, DynamicAssociationCondition dynamicAssociationCondition);
    }
}
