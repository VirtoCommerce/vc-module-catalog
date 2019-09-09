using System.Linq;
using VirtoCommerce.CatalogModule.Data.Extensions;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.ExportModule.Data.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.ExportImport
{
    public class CatalogExportPagedDataSource : ExportPagedDataSource<CatalogExportDataQuery, CatalogExportSearchCriteria>
    {
        private readonly ICatalogSearchService _catalogSearchService;

        public CatalogExportPagedDataSource(
            ICatalogSearchService catalogSearchService,
            CatalogExportDataQuery dataQuery) : base(dataQuery)
        {
            _catalogSearchService = catalogSearchService;
        }

        protected override ExportableSearchResult FetchData(CatalogExportSearchCriteria searchCriteria)
        {
            var catalogSearchCriteria = searchCriteria.ToCatalogSearchCriteria();
            catalogSearchCriteria.ResponseGroup = SearchResponseGroup.WithCatalogs;

            var searchResult = _catalogSearchService.Search(catalogSearchCriteria);

            var catalogs = searchResult.Catalogs.Skip(catalogSearchCriteria.Skip).Take(catalogSearchCriteria.Take).ToList();

            foreach (var catalog in catalogs)
            {
                catalog.ResetRedundantReferences();
            }

            return new ExportableSearchResult
            {
                TotalCount = searchResult.CatalogsTotalCount,
                Results = catalogs.Select(x => AbstractTypeFactory<ExportableCatalog>.TryCreateInstance().FromModel(x)).Cast<IExportable>().ToList(),
            };
        }

        protected override CatalogExportSearchCriteria BuildSearchCriteria(CatalogExportDataQuery exportDataQuery)
        {
            var result = base.BuildSearchCriteria(exportDataQuery);

            result.CatalogIds = exportDataQuery.CatalogIds;

            return result;
        }
    }
}
