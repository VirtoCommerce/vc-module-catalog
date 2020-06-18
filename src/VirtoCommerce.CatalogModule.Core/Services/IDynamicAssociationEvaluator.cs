using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model.DynamicAssociations;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    public interface IDynamicAssociationEvaluator
    {
        Task<string[]> EvaluateDynamicAssociationsAsync(DynamicAssociationEvaluationContext context);
    }
}
