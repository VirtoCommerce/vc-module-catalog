using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Data.Search;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Model.Search;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using SearchCriteria = VirtoCommerce.Domain.Catalog.Model.SearchCriteria;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    /// <summary>
    /// Another implementation for ICatalogSearchService. Combines indexed and DB search providers.
    /// </summary>
    public class CatalogSearchServiceDecorator : ICatalogSearchService
    {
        private readonly CatalogSearchServiceImpl _catalogSearchService;
        private readonly IItemService _itemService;
        private readonly IProductSearchService _productSearchService;
        private readonly ISettingsManager _settingsManager;

        public CatalogSearchServiceDecorator(
            CatalogSearchServiceImpl catalogSearchService,
            IItemService itemService,
            IProductSearchService productSearchService,
            ISettingsManager settingsManager)
        {
            _catalogSearchService = catalogSearchService;
            _itemService = itemService;
            _productSearchService = productSearchService;
            _settingsManager = settingsManager;
        }

        public SearchResult Search(SearchCriteria criteria)
        {
            SearchResult result;

            var useIndexedSearch = _settingsManager.GetValue("VirtoCommerce.SearchApi.UseCatalogIndexedSearchInManager", true);
            var searchProducts = criteria.ResponseGroup.HasFlag(SearchResponseGroup.WithProducts);

            if (useIndexedSearch && searchProducts && !string.IsNullOrEmpty(criteria.Keyword))
            {
                result = new SearchResult();

                // TODO: create outline for category
                // TODO: implement sorting

                const ItemResponseGroup responseGroup = ItemResponseGroup.ItemInfo | ItemResponseGroup.Outlines;

                var serviceCriteria = AbstractTypeFactory<ProductSearchCriteria>.TryCreateInstance();

                serviceCriteria.ObjectType = KnownDocumentTypes.Product;
                serviceCriteria.SearchPhrase = criteria.Keyword;
                serviceCriteria.CatalogId = criteria.CatalogId;
                serviceCriteria.Outline = criteria.CategoryId;
                serviceCriteria.WithHidden = criteria.WithHidden;
                serviceCriteria.Skip = criteria.Skip;
                serviceCriteria.Take = criteria.Take;
                serviceCriteria.ResponseGroup = responseGroup.ToString();

                SearchItems(result, serviceCriteria, responseGroup);
            }
            else
            {
                // use original implementation from catalog module
                result = _catalogSearchService.Search(criteria);
            }

            return result;
        }


        private void SearchItems(SearchResult result, ProductSearchCriteria criteria, ItemResponseGroup responseGroup)
        {
            Task.Run(() => SearchItemsAsync(result, criteria, responseGroup)).GetAwaiter().GetResult();
        }

        private async Task SearchItemsAsync(SearchResult result, ProductSearchCriteria criteria, ItemResponseGroup responseGroup)
        {
            var items = new List<CatalogProduct>();
            // Search using criteria, it will only return IDs of the items
            var searchResults = await _productSearchService.SearchAsync(criteria);            
             var itemIds = searchResults.Items.Select(x => x.Id.ToString()).ToList();
            // Now load items from repository with preserve order
            result.Products = _itemService.GetByIds(itemIds.ToArray(), responseGroup, criteria.CatalogId)
                                           .OrderBy(i => itemIds.IndexOf(i.Id)).ToArray();
            result.ProductsTotalCount = (int)searchResults.TotalCount;
        }
    }
}
