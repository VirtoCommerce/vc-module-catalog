using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.ListEntry;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;


namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class InternalListEntrySearchService : IInternalListEntrySearchService
    {
        private readonly IProductIndexedSearchService _productIndexedSearchService;
        private readonly ICategoryIndexedSearchService _categoryIndexedSearchService;
        private readonly IListEntrySearchService _listEntrySearchService;
        private readonly ISettingsManager _settingsManager;

        public InternalListEntrySearchService(
            IProductIndexedSearchService productIndexedSearchService,
            ICategoryIndexedSearchService categoryIndexedSearchService,
            IListEntrySearchService listEntrySearchService,
            ISettingsManager settingsManager)
        {
            _productIndexedSearchService = productIndexedSearchService;
            _categoryIndexedSearchService = categoryIndexedSearchService;
            _listEntrySearchService = listEntrySearchService;
            _settingsManager = settingsManager;
        }

        public async Task<ListEntrySearchResult> InnerListEntrySearchAsync(CatalogListEntrySearchCriteria criteria)
        {
            var result = new ListEntrySearchResult();
            var useIndexedSearch = _settingsManager.GetValue(ModuleConstants.Settings.Search.UseCatalogIndexedSearchInManager.Name, true);

            if (useIndexedSearch && !string.IsNullOrEmpty(criteria.Keyword))
            {
                // PT-5120: create outline for category, implement sorting
                if (criteria.ObjectTypes.IsNullOrEmpty() || criteria.ObjectTypes.Contains(nameof(Category)))
                {
                    var categoryIndexedSearchCriteria = AbstractTypeFactory<CategoryIndexedSearchCriteria>.TryCreateInstance().FromListEntryCriteria(criteria) as CategoryIndexedSearchCriteria;
                    const CategoryResponseGroup catResponseGroup = CategoryResponseGroup.Info | CategoryResponseGroup.WithOutlines;
                    categoryIndexedSearchCriteria.ResponseGroup = catResponseGroup.ToString();

                    var catIndexedSearchResult = await _categoryIndexedSearchService.SearchAsync(categoryIndexedSearchCriteria);
                    var totalCount = catIndexedSearchResult.TotalCount;
                    var skip = Math.Min(totalCount, criteria.Skip);
                    var take = Math.Min(criteria.Take, Math.Max(0, totalCount - criteria.Skip));

                    result.Results = catIndexedSearchResult.Items.Select(x => AbstractTypeFactory<CategoryListEntry>.TryCreateInstance().FromModel(x)).ToList();
                    result.TotalCount = (int)totalCount;

                    criteria.Skip -= (int)skip;
                    criteria.Take -= (int)take;
                }

                if (criteria.ObjectTypes.IsNullOrEmpty() || criteria.ObjectTypes.Contains(nameof(CatalogProduct)))
                {
                    const ItemResponseGroup itemResponseGroup = ItemResponseGroup.ItemInfo | ItemResponseGroup.Outlines;

                    var productIndexedSearchCriteria = AbstractTypeFactory<ProductIndexedSearchCriteria>.TryCreateInstance().FromListEntryCriteria(criteria) as ProductIndexedSearchCriteria;
                    productIndexedSearchCriteria.ResponseGroup = itemResponseGroup.ToString();

                    var indexedSearchResult = await _productIndexedSearchService.SearchAsync(productIndexedSearchCriteria);
                    result.TotalCount += (int)indexedSearchResult.TotalCount;
                    result.Results.AddRange(indexedSearchResult.Items.Select(x => AbstractTypeFactory<ProductListEntry>.TryCreateInstance().FromModel(x)));
                }
            }
            else
            {
                result = await _listEntrySearchService.SearchAsync(criteria);
            }

            return result;
        }
    }
}
