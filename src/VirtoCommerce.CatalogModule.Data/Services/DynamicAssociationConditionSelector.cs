using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.DynamicAssociations;
using VirtoCommerce.CatalogModule.Core.Model.DynamicAssociations.Conditions;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class DynamicAssociationConditionsSelector : IDynamicAssociationConditionSelector
    {
        private readonly IDynamicAssociationSearchService _dynamicAssociationSearchService;

        public DynamicAssociationConditionsSelector(IDynamicAssociationSearchService dynamicAssociationSearchService)
        {
            _dynamicAssociationSearchService = dynamicAssociationSearchService;
        }

        public async Task<DynamicAssociationConditionEvaluationRequest> GetDynamicAssociationConditionAsync(DynamicAssociationEvaluationContext context, CatalogProduct product)
        {
            DynamicAssociationConditionEvaluationRequest result = null;

            var dynamicAssociationRules = (await _dynamicAssociationSearchService
                .SearchDynamicAssociationsAsync(new DynamicAssociationSearchCriteria
                {
                    Groups = new[] { context.Group },
                    StoreIds = new[] { context.StoreId },
                    Take = int.MaxValue,
                    SortInfos = { new SortInfo
                    {
                        SortColumn = nameof(DynamicAssociation.Priority),
                        SortDirection = SortDirection.Descending,
                    }},
                    IsActive = true,
                }))
                .Results;

            var expressionContext = AbstractTypeFactory<DynamicAssociationExpressionEvaluationContext>.TryCreateInstance();
            expressionContext.Products.Add(product);

            foreach (var dynamicAssociationRule in dynamicAssociationRules)
            {
                var matchingRule = dynamicAssociationRule.ExpressionTree.Children.OfType<BlockMatchingRules>().FirstOrDefault()
                    ?? throw new InvalidOperationException($"Matching rules block for \"{dynamicAssociationRule.Name}\" dynamic association rule is missing");

                if (matchingRule.IsSatisfiedBy(expressionContext))
                {
                    var resultRule = dynamicAssociationRule.ExpressionTree.Children.OfType<BlockResultingRules>().FirstOrDefault()
                        ?? throw new InvalidOperationException($"Resulting rules block for \"{dynamicAssociationRule.Name}\" dynamic association rule is missing");

                    var outputTuningBlock = dynamicAssociationRule.ExpressionTree.Children.OfType<BlockOutputTuning>().FirstOrDefault()
                        ?? throw new InvalidOperationException($"Output tuning block for \"{dynamicAssociationRule.Name}\" dynamic association rule is missing");

                    result = AbstractTypeFactory<DynamicAssociationConditionEvaluationRequest>.TryCreateInstance();
                    result.PropertyValues = resultRule.GetPropertyValues();
                    result.CategoryIds = resultRule.GetCategoryIds();
                    result.Sort = outputTuningBlock.Sort;
                    result.Skip = context.Skip;
                    result.Take = Math.Min(Math.Max(outputTuningBlock.OutputLimit - context.Skip, 0), context.Take);

                    break;
                }
            }

            return result;
        }
    }
}
