using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.CatalogModule.Core.Model.DynamicAssociations.Conditions
{
    public class ConditionCategoryIs : ConditionTree
    {
        public string[] CategoryIds { get; set; }
        public string[] CategoryNames { get; set; }

        public override bool IsSatisfiedBy(IEvaluationContext context)
        {
            var result = false;
            if (context is DynamicAssociationExpressionEvaluationContext evaluationContext)
            {
                result = evaluationContext.AreItemsInCategory(CategoryIds);
            }

            return result;
        }
    }
}
