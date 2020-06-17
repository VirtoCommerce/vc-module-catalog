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

        public async Task<DynamicAssociationCondition> GetDynamicAssociationConditionAsync(DynamicAssociationsRuleEvaluationContext searchContext, CatalogProduct product)
        {
            var result = AbstractTypeFactory<DynamicAssociationCondition>.TryCreateInstance();

            var dynamicAssociationRules = (await _dynamicAssociationSearchService
                .SearchDynamicAssociationsAsync(new DynamicAssociationSearchCriteria
                {
                    Groups = new[] { searchContext.Group },
                    StoreIds = new[] { searchContext.StoreId },
                    Take = int.MaxValue,
                    SortInfos = { new SortInfo
                    {
                        SortColumn = nameof(DynamicAssociation.Priority),
                        SortDirection = SortDirection.Ascending,
                    }},
                    IsActive = true,
                }))
                .Results;

            var evaluationContext = AbstractTypeFactory<DynamicAssociationEvaluationContext>.TryCreateInstance();
            evaluationContext.Products.Add(product);

            foreach (var dynamicAssociationRule in dynamicAssociationRules)
            {
                var matchingRule = dynamicAssociationRule
                    .ExpressionTree.Children.OfType<BlockMatchingRules>().FirstOrDefault()
                    ?? throw new InvalidOperationException($"Block matching rules for dynamic association rule expression: {dynamicAssociationRule.Id}");

                if (matchingRule.IsSatisfiedBy(evaluationContext))
                {
                    var resultRule = dynamicAssociationRule
                        .ExpressionTree.Children.OfType<BlockResultingRules>().FirstOrDefault()
                        ?? throw new InvalidOperationException($"Block resulting rules for dynamic association rule expression: {dynamicAssociationRule.Id}");

                    result.PropertyValues = resultRule.GetPropertyValues();
                    result.CategoryIds = resultRule.GetCategoryIds();

                    break;
                }
            }

            return result;
        }
    }
}
