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
    public class CatalogExportPagedDataSource : ExportPagedDataSource<CatalogExportDataQuery, CatalogSearchCriteria>
    {
        private readonly SearchService<CatalogSearchCriteria, CatalogSearchResult, Catalog, CatalogEntity> _catalogSearchService;

        public CatalogExportPagedDataSource(ICatalogSearchService catalogSearchService, CatalogExportDataQuery dataQuery) : base(dataQuery)
        {
            _catalogSearchService = (SearchService<CatalogSearchCriteria, CatalogSearchResult, Catalog, CatalogEntity>)catalogSearchService;
        }

        protected override ExportableSearchResult FetchData(CatalogSearchCriteria searchCriteria)
        {
            var searchResult = _catalogSearchService.SearchAsync(searchCriteria).GetAwaiter().GetResult();

            return new ExportableSearchResult
            {
                TotalCount = searchResult.TotalCount,
                Results = searchResult.Results.ToList<IExportable>(),
            };
        }

        protected override CatalogSearchCriteria BuildSearchCriteria(CatalogExportDataQuery exportDataQuery)
        {
            var result = base.BuildSearchCriteria(exportDataQuery);

            result.CatalogIds = exportDataQuery.CatalogIds;
            result.ResponseGroup = CatalogResponseGroup.Info.ToString();

            return result;
        }
    }
}
