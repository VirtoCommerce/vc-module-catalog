using System;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public sealed class CategoryIteratorFactory
    {
        private readonly Func<ICatalogRepository> _catalogRepositoryFactory;
        private readonly ISettingsManager _settingsManager;
        private readonly ICategoryIndexedSearchService _categoryIndexedSearchService;

        public CategoryIteratorFactory(
            Func<ICatalogRepository> catalogRepositoryFactory,
            ISettingsManager settingsManager,
            ICategoryIndexedSearchService categoryIndexedSearchService
            )
        {
            _catalogRepositoryFactory = catalogRepositoryFactory;
            _settingsManager = settingsManager;
            _categoryIndexedSearchService = categoryIndexedSearchService;
        }

        public CategoryIterator Create(int pageSize, string categoryId, string catalogId)
        {
            var result = new CategoryIterator(_catalogRepositoryFactory, _settingsManager,
                _categoryIndexedSearchService, pageSize, catalogId, categoryId);

            return result;
        }
    }
}
