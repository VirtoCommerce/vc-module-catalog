using System.Linq;
using VirtoCommerce.Domain.Catalog.Model.Search;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.ExportModule.Data.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.ExportImport
{
    public class PropertyDictionaryItemExportPagedDataSource : ExportPagedDataSource<PropertyDictionaryItemExportDataQuery, PropertyDictionaryItemExportSearchCriteria>
    {
        private readonly IProperyDictionaryItemSearchService _propertyDictionarySearchService;

        public PropertyDictionaryItemExportPagedDataSource(
            IProperyDictionaryItemSearchService propertyDictionarySearchService,
            PropertyDictionaryItemExportDataQuery dataQuery) : base(dataQuery)
        {
            _propertyDictionarySearchService = propertyDictionarySearchService;
        }

        protected override ExportableSearchResult FetchData(PropertyDictionaryItemExportSearchCriteria searchCriteria)
        {
            var criteria = new PropertyDictionaryItemSearchCriteria { Take = searchCriteria.Take, Skip = searchCriteria.Skip, CatalogIds = searchCriteria.CatalogIds };
            var searchResponse = _propertyDictionarySearchService.Search(criteria);
            return new ExportableSearchResult
            {
                TotalCount = searchResponse.TotalCount,
                Results = searchResponse.Results.Select(y => AbstractTypeFactory<ExportablePropertyDictionaryItem>.TryCreateInstance().FromModel(y)).Cast<IExportable>().ToList()
            };
        }

        protected override PropertyDictionaryItemExportSearchCriteria BuildSearchCriteria(PropertyDictionaryItemExportDataQuery exportDataQuery)
        {
            var result = base.BuildSearchCriteria(exportDataQuery);
            result.CatalogIds = exportDataQuery.CatalogIds;
            return result;
        }
    }
}
