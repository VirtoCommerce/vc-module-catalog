using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.ExportModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Data.ExportImport
{
    public class ExportableCatalog : Catalog, IExportable
    {
        public new CatalogLanguage DefaultLanguage { get; set; }

        public static ExportableCatalog FromModel(Catalog catalog)
        {
            var result = new ExportableCatalog
            {
                Id = catalog.Id,
                Name = catalog.Name,
                IsVirtual = catalog.IsVirtual,
                DefaultLanguage = catalog.DefaultLanguage,
                Languages = catalog.Languages,
                Properties = catalog.Properties,
                PropertyValues = catalog.PropertyValues
            };
            return result;
        }
    }
}
