using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model.DynamicAssociations;
using VirtoCommerce.CatalogModule.Core.Model.Search;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    public interface IDynamicAssociationsConditionEvaluator
    {
        Task<string[]> EvaluateDynamicAssociationConditionAsync(DynamicRuleAssociationsEvaluationContext searchContext, DynamicAssociationCondition dynamicAssociationCondition);
    }
}
