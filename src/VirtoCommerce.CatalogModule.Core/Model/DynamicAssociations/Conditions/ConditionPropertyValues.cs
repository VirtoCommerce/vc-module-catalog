using System.Collections.Generic;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.CatalogModule.Core.Model.DynamicAssociations.Conditions
{
    public class ConditionPropertyValues : ConditionTree
    {
        public Dictionary<string, string> PropertyNameValues { get; set; }

        public override bool IsSatisfiedBy(IEvaluationContext context)
        {
            var result = false;
            if (context is DynamicAssociationEvaluationContext evaluationContext)
            {
                result = evaluationContext.AreItemPropertyValuesEqual(PropertyNameValues);
            }

            return result;
        }
    }
}
