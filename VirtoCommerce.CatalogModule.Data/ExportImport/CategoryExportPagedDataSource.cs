using System.Linq;
using VirtoCommerce.CatalogModule.Data.Extensions;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.ExportModule.Data.Services;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.ExportImport
{
    public class CategoryExportPagedDataSource : ExportPagedDataSource<CategoryExportDataQuery, CategoryExportSearchCriteria>
    {
        private readonly ICatalogSearchService _catalogSearchService;
        private readonly ICategoryService _categoryService;
        private readonly IBlobStorageProvider _blobStorageProvider;

        public CategoryExportPagedDataSource(
            ICatalogSearchService catalogSearchService,
            ICategoryService categoryService,
            IBlobStorageProvider blobStorageProvider,
            CategoryExportDataQuery dataQuery)
            : base(dataQuery)
        {
            _catalogSearchService = catalogSearchService;
            _categoryService = categoryService;
            _blobStorageProvider = blobStorageProvider;
        }

        protected override ExportableSearchResult FetchData(CategoryExportSearchCriteria searchCriteria)
        {
            var categorySearchCriteria = searchCriteria.ToCatalogSearchCriteria();
            categorySearchCriteria.ResponseGroup = SearchResponseGroup.WithCategories;

            var searchResult = _catalogSearchService.Search(categorySearchCriteria);
            var categoryIds = searchResult.Categories
                .Skip(categorySearchCriteria.Skip)
                .Take(categorySearchCriteria.Take)
                .Select(x => x.Id)
                .ToArray();

            Category[] categories;

            if (DataQuery.IsPreview)
            {
                categories = _categoryService.GetByIds(categoryIds, CategoryResponseGroup.Info);
            }
            else
            {
                categories = _categoryService.GetByIds(categoryIds, CategoryResponseGroup.Full);
                categories.LoadImages(_blobStorageProvider);

                foreach (var category in categories)
                {
                    category.ResetRedundantReferences();
                }
            }

            return new ExportableSearchResult
            {
                TotalCount = searchResult.CategoriesTotalCount,
                Results = categories.Select(x => AbstractTypeFactory<ExportableCategory>.TryCreateInstance().FromModel(x)).Cast<IExportable>().ToList(),
            };
        }

        protected override CategoryExportSearchCriteria BuildSearchCriteria(CategoryExportDataQuery exportDataQuery)
        {
            var result = base.BuildSearchCriteria(exportDataQuery);

            result.CatalogIds = exportDataQuery.CatalogIds;

            return result;
        }
    }
}
