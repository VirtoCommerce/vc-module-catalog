using System;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class DynamicAssociationEvaluator : IDynamicAssociationEvaluator
    {
        private readonly IStoreService _storeService;
        private readonly IProductSearchService _productSearchService;
        private readonly IDynamicAssociationsValueFetcher _dynamicAssociationsValueFetcher;

        public DynamicAssociationEvaluator(
            IStoreService storeService,
            IProductSearchService productSearchService,
            IDynamicAssociationsValueFetcher dynamicAssociationsValueFetcher
            )
        {
            _storeService = storeService;
            _productSearchService = productSearchService;
            _dynamicAssociationsValueFetcher = dynamicAssociationsValueFetcher;
        }

        public async Task<string[]> EvaluateDynamicAssociationsAsync(ProductsToMatchSearchContext context)
        {
            if (context.ProductsToMatch.IsNullOrEmpty())
            {
                return new string[0];
            }

            var store = await _storeService.GetByIdAsync(context.StoreId);

            var products = (await _productSearchService.SearchProductsAsync(new ProductSearchCriteria
            {
                ObjectIds = context.ProductsToMatch,
                CatalogId = store.Catalog,
                Take = int.MaxValue,
                ResponseGroup = ItemResponseGroup.WithProperties.ToString(),
            })).Results;

            foreach (var product in products)
            {
                var dynamicAssociationValue = await _dynamicAssociationsValueFetcher.GetDynamicAssociationValueAsync(context.Group, context.StoreId, product);

                // TODO: query to Elasticsearch
            }
            throw new NotImplementedException();
        }
    }
}
