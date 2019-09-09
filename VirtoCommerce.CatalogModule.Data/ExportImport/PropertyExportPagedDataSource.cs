using System.Linq;
using VirtoCommerce.CatalogModule.Data.Extensions;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.ExportModule.Data.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.ExportImport
{
    public class PropertyExportPagedDataSource : ExportPagedDataSource<PropertyExportDataQuery, PropertyExportSearchCriteria>
    {
        private readonly IPropertyService _propertyService;

        public PropertyExportPagedDataSource(
            IPropertyService propertyService,
            PropertyExportDataQuery dataQuery) : base(dataQuery)
        {
            _propertyService = propertyService;
        }

        protected override ExportableSearchResult FetchData(PropertyExportSearchCriteria searchCriteria)
        {
            var allproperties = _propertyService.GetAllProperties();
            var properties = allproperties.Where(x => searchCriteria.CatalogIds.Contains(x.CatalogId)).Skip(searchCriteria.Skip).Take(searchCriteria.Take);

            //Load property dictionary values and reset some props to decrease size of the resulting json 
            foreach (var property in properties)
            {
                property.ResetRedundantReferences();
            }

            return new ExportableSearchResult
            {
                TotalCount = allproperties.Count(),
                Results = properties.Select(y => AbstractTypeFactory<ExportableProperty>.TryCreateInstance().FromModel(y)).Cast<IExportable>().ToList()
            };
        }

        protected override PropertyExportSearchCriteria BuildSearchCriteria(PropertyExportDataQuery exportDataQuery)
        {
            var result = base.BuildSearchCriteria(exportDataQuery);
            result.CatalogIds = exportDataQuery.CatalogIds;
            return result;
        }
    }
}
