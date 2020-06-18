using VirtoCommerce.CatalogModule.Core.Model.DynamicAssociations;

namespace VirtoCommerce.CatalogModule.Data.Extensions
{
    public static class DynamicAssociationConditionEvaluationRequestExtension
    {
        public static DynamicAssociationConditionEvaluationRequest PopulatePaging(
            this DynamicAssociationConditionEvaluationRequest conditionRequest,
            DynamicAssociationEvaluationContext evaluationContext)
        {
            conditionRequest.Take = evaluationContext.Take;
            conditionRequest.Skip = evaluationContext.Skip;

            return conditionRequest;
        }
    }
}
