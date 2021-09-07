using System.Linq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.ExportModule.Data.Services;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.CatalogModule.Data.ExportImport
{
    public class CategoryExportPagedDataSource : ExportPagedDataSource<CategoryExportDataQuery, CategorySearchCriteria>
    {
        private readonly SearchService<CategorySearchCriteria, CategorySearchResult, Category, CategoryEntity> _categorySearchService;

        public CategoryExportPagedDataSource(ICategorySearchService categorySearchService, CategoryExportDataQuery dataQuery) : base(dataQuery)
        {
            _categorySearchService = (SearchService<CategorySearchCriteria, CategorySearchResult, Category, CategoryEntity>)categorySearchService;
        }

        protected override ExportableSearchResult FetchData(CategorySearchCriteria searchCriteria)
        {
            var searchResult = _categorySearchService.SearchAsync(searchCriteria).GetAwaiter().GetResult();

            return new ExportableSearchResult
            {
                TotalCount = searchResult.TotalCount,
                Results = searchResult.Results.ToList<IExportable>(),
            };
        }

        protected override CategorySearchCriteria BuildSearchCriteria(CategoryExportDataQuery exportDataQuery)
        {
            var result = base.BuildSearchCriteria(exportDataQuery);

            result.CatalogIds = exportDataQuery.CatalogIds;

            return result;
        }
    }
}
