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
    public class DynamicAssociationsValueFetcher : IDynamicAssociationsValueFetcher
    {
        private readonly IDynamicAssociationSearchService _dynamicAssociationSearchService;

        public DynamicAssociationsValueFetcher(IDynamicAssociationSearchService dynamicAssociationSearchService)
        {
            _dynamicAssociationSearchService = dynamicAssociationSearchService;
        }

        public virtual async Task<DynamicAssociationValue> GetDynamicAssociationValueAsync(string group, string storeId, CatalogProduct product)
        {
            var result = AbstractTypeFactory<DynamicAssociationValue>.TryCreateInstance();

            var dynamicAssociationRules = (await _dynamicAssociationSearchService
                .SearchDynamicAssociationsAsync(new DynamicAssociationSearchCriteria
                {
                    Groups = new[] { group },
                    StoreIds = new[] { storeId },
                    Take = int.MaxValue,
                    SortInfos = { new SortInfo
                    {
                        SortColumn = "Priority",
                        SortDirection = SortDirection.Ascending,
                    }},
                }))
                .Results;

            var evaluationContext = AbstractTypeFactory<DynamicAssociationEvaluationContext>.TryCreateInstance();
            evaluationContext.Products.Add(product);

            foreach (var dynamicAssociationRule in dynamicAssociationRules)
            {
                var matchingRule = dynamicAssociationRule.ExpressionTree.Children.OfType<BlockMatchingRules>().First();

                if (matchingRule.IsSatisfiedBy(evaluationContext))
                {
                    var resultRule = dynamicAssociationRule.ExpressionTree.Children.OfType<BlockResultingRules>().First();
                    result.PropertyValues = resultRule.GetPropertyValues();
                    result.CategoryIds = resultRule.GetCategoryIds();

                    break;
                }
            }

            return result;
        }
    }
}
