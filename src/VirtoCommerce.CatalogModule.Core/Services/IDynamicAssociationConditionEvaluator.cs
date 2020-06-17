using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model.DynamicAssociations;
using VirtoCommerce.CatalogModule.Core.Model.Search;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    public interface IDynamicAssociationConditionEvaluator
    {
        Task<string[]> EvaluateDynamicAssociationConditionAsync(DynamicAssociationsRuleEvaluationContext searchContext, DynamicAssociationCondition dynamicAssociationCondition);
    }
}
