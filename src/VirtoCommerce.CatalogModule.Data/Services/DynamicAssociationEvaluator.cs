using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Search.Indexing;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class DynamicAssociationEvaluator : IDynamicAssociationEvaluator
    {
        private readonly IStoreService _storeService;
        private readonly IDynamicAssociationsValueFetcher _dynamicAssociationsValueFetcher;
        private readonly IItemService _itemService;
        private readonly ISearchProvider _searchProvider;

        public DynamicAssociationEvaluator(
            IStoreService storeService,
            IDynamicAssociationsValueFetcher dynamicAssociationsValueFetcher,
            IItemService itemService,
            ISearchProvider searchProvider
            )
        {
            _storeService = storeService;
            _dynamicAssociationsValueFetcher = dynamicAssociationsValueFetcher;
            _itemService = itemService;
            _searchProvider = searchProvider;
        }

        public async Task<string[]> EvaluateDynamicAssociationsAsync(ProductsToMatchSearchContext context)
        {
            if (context.ProductsToMatch.IsNullOrEmpty())
            {
                return new string[0];
            }

            var store = await _storeService.GetByIdAsync(context.StoreId);

            var products = await _itemService.GetByIdsAsync(context.ProductsToMatch,
                $"{ItemResponseGroup.WithProperties|ItemResponseGroup.WithOutlines}", store.Catalog);

            var result = new HashSet<string>();

            foreach (var product in products)
            {
                var dynamicAssociationValue = await _dynamicAssociationsValueFetcher.GetDynamicAssociationValueAsync(context.Group, context.StoreId, product);

                var searchRequestBuilder = AbstractTypeFactory<DynamicAssociationSearchRequestBuilder>.TryCreateInstance();
                    searchRequestBuilder
                        .AddPropertySearch(dynamicAssociationValue.PropertyValues)
                        .AddOutletSearch(dynamicAssociationValue.CategoryIds)
                        .WithPaging(context.Skip, context.Take);
                
                var searchResult = await _searchProvider.SearchAsync(KnownDocumentTypes.Product, searchRequestBuilder.Build());
                result.AddRange(searchResult.Documents.Select(x => x.Id));
            }

            return result.Distinct().ToArray();
        }
    }
}
