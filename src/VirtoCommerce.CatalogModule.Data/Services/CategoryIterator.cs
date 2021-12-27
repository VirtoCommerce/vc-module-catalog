using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public sealed class CategoryIterator
    {
        private readonly Func<ICatalogRepository> _catalogRepositoryFactory;
        private readonly ISettingsManager _settingsManager;
        private readonly ICategoryIndexedSearchService _categoryIndexedSearchService;

        private readonly int _pageSize;
        private readonly string _catalogId;
        private readonly string _categoryId;
        private readonly bool _isIndexedSearchEnabled;

        private List<string> _items = new List<string>();
        private long _totalCount;
        private int _currentPage;

        private int _indexedSearchRequestSkip = 0;
        private const int _indexedSearchRequestPageSize = 1000;
        private int _handledItemsCount = 0;

        public CategoryIterator(
            Func<ICatalogRepository> catalogRepositoryFactory,
            ISettingsManager settingsManager,
            ICategoryIndexedSearchService categoryIndexedSearchService,
            int pageSize,
            string catalogId,
            string categoryId)
        {
            _catalogRepositoryFactory = catalogRepositoryFactory;
            _settingsManager = settingsManager;
            _categoryIndexedSearchService = categoryIndexedSearchService;

            _pageSize = Math.Max(50, pageSize);
            _catalogId = catalogId;
            _categoryId = categoryId;

            _isIndexedSearchEnabled = _settingsManager.GetValue(ModuleConstants.Settings.Search.UseCatalogIndexedSearchInManager.Name, true);
        }

        public bool HasMoreResults { get; private set; } = true;

        public async Task<IReadOnlyCollection<string>> GetNextPageAsync()
        {
            await EnsureThatDataIsActualAsync();

            var result = _items.Skip(_pageSize * _currentPage++).Take(_pageSize);

            _handledItemsCount += result.Count();

            HasMoreResults = _handledItemsCount < _totalCount;

            return result.ToImmutableArray();
        }


        private async Task EnsureThatDataIsActualAsync()
        {
            var currentProgress = _currentPage * _pageSize;

            if ((currentProgress >= _items.Count && _totalCount > currentProgress) || _items.Count == 0)
            {
                if (_isIndexedSearchEnabled)
                {
                    var searchCriteria = AbstractTypeFactory<CategoryIndexedSearchCriteria>.TryCreateInstance();

                    searchCriteria.Take = _indexedSearchRequestPageSize;
                    searchCriteria.Skip = _indexedSearchRequestSkip++ * _indexedSearchRequestPageSize;
                    searchCriteria.Outline = GetOutline();

                    Rewind();

                    var categoriesIndexedSearchResult = await _categoryIndexedSearchService.SearchAsync(searchCriteria);

                    _items = categoriesIndexedSearchResult.Items.Select(x => x.Id).ToList();

                    _totalCount = categoriesIndexedSearchResult.TotalCount;
                }
                else
                {
                    using var catalogRepository = _catalogRepositoryFactory();

                    _items = (await catalogRepository.GetAllChildrenCategoriesIdsAsync(new[] { _categoryId })).ToList();
                    _totalCount = _items.Count;
                }

            }
        }

        private void Rewind()
        {
            _currentPage = 0;
        }

        private string GetOutline()
        {
            var result = string.IsNullOrEmpty(_categoryId)
                ? _catalogId
                : $"{_catalogId}/{_categoryId}";

            return result;
        }
    }
}
