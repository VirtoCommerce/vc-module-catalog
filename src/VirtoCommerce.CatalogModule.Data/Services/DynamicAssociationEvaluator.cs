using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class DynamicAssociationEvaluator : IDynamicAssociationEvaluator
    {
        private readonly IStoreService _storeService;
        private readonly IDynamicAssociationsConditionSelector _dynamicAssociationsConditionSelector;
        private readonly IItemService _itemService;
        private readonly IDynamicAssociationsConditionEvaluator _dynamicAssociationsConditionEvaluator;

        public DynamicAssociationEvaluator(
            IStoreService storeService,
            IDynamicAssociationsConditionSelector dynamicAssociationsConditionSelector,
            IItemService itemService,
            IDynamicAssociationsConditionEvaluator dynamicAssociationsConditionEvaluator
            )
        {
            _storeService = storeService;
            _dynamicAssociationsConditionSelector = dynamicAssociationsConditionSelector;
            _itemService = itemService;
            _dynamicAssociationsConditionEvaluator = dynamicAssociationsConditionEvaluator;
        }

        public async Task<string[]> EvaluateDynamicAssociationsAsync(DynamicRuleAssociationsEvaluationContext context)
        {
            if (context.ProductsToMatch.IsNullOrEmpty())
            {
                return Array.Empty<string>();
            }

            var store = await _storeService.GetByIdAsync(context.StoreId);

            var products = await _itemService.GetByIdsAsync(context.ProductsToMatch,
                $"{ItemResponseGroup.WithProperties|ItemResponseGroup.WithOutlines}", store.Catalog);

            var result = new HashSet<string>();

            foreach (var product in products)
            {
                
                var dynamicAssociationCondition = await _dynamicAssociationsConditionSelector.GetDynamicAssociationConditionAsync(context, product);
                var searchResult = await _dynamicAssociationsConditionEvaluator.EvaluateDynamicAssociationConditionAsync(context, dynamicAssociationCondition);

                result.AddRange(searchResult);

            }

            return result.Distinct().ToArray();
        }
    }
}
