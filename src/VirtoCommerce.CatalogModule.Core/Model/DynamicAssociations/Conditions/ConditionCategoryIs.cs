using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.CatalogModule.Core.Model.DynamicAssociations.Conditions
{
    public class ConditionCategoryIs : ConditionTree
    {
        public ICollection<string> ExcludingCategoryIds { get; set; } = new List<string>();
        public ICollection<string> ExcludingProductIds { get; set; } = new List<string>();

        public string[] CategoryIds { get; set; }
        public string[] CategoryNames { get; set; }

        /// <summary>
        /// ((PromotionEvaluationContext)x).IsItemInCategory(CategoryId, ExcludingCategoryIds, ExcludingProductIds)
        /// </summary>
        public override bool IsSatisfiedBy(IEvaluationContext context)
        {
            var result = false;
            if (context is DynamicAssociationEvaluationContext evaluationContext)
            {
                result = evaluationContext.IsItemInCategory(CategoryIds, ExcludingCategoryIds.ToArray(), ExcludingProductIds.ToArray());
            }

            return result;
        }
    }
}
