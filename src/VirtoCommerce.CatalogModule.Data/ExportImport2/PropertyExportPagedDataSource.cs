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
    public class PropertyExportPagedDataSource : ExportPagedDataSource<PropertyExportDataQuery, PropertySearchCriteria>
    {
        private readonly SearchService<PropertySearchCriteria, PropertySearchResult, Property, PropertyEntity> _propertySearchService;

        public PropertyExportPagedDataSource(IPropertySearchService propertySearchService, PropertyExportDataQuery dataQuery) : base(dataQuery)
        {
            _propertySearchService = (SearchService<PropertySearchCriteria, PropertySearchResult, Property, PropertyEntity>)propertySearchService;
        }

        protected override ExportableSearchResult FetchData(PropertySearchCriteria searchCriteria)
        {
            var searchResult = _propertySearchService.SearchAsync(searchCriteria).GetAwaiter().GetResult();

            return new ExportableSearchResult
            {
                TotalCount = searchResult.TotalCount,
                Results = searchResult.Results.ToList<IExportable>(),
            };
        }

        protected override PropertySearchCriteria BuildSearchCriteria(PropertyExportDataQuery exportDataQuery)
        {
            var result = base.BuildSearchCriteria(exportDataQuery);

            result.CatalogIds = exportDataQuery.CatalogIds;

            return result;
        }
    }
}
