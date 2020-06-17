using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model.Search;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    public interface IDynamicAssociationEvaluator
    {
        Task<string[]> EvaluateDynamicAssociationsAsync(DynamicAssociationsRuleEvaluationContext context);
    }
}
