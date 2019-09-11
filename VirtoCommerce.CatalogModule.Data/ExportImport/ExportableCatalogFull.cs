using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.ExportModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Data.ExportImport
{
    public class ExportableCatalogFull : IExportable
    {
        public string Id { get; set; }

        public object Clone()
        {
            var result = MemberwiseClone() as ExportableCatalogFull;
            result.Catalogs = Catalogs?.Select(x => x.Clone() as ExportableCatalog).ToList();
            result.Categories = Categories?.Select(x => x.Clone() as ExportableCategory).ToList();
            result.Properties = Properties?.Select(x => x.Clone() as ExportableProperty).ToList();
            result.PropertyDictionaryItems = PropertyDictionaryItems?.Select(x => x.Clone() as ExportablePropertyDictionaryItem).ToList();
            result.CatalogProducts = CatalogProducts?.Select(x => x.Clone() as ExportableCatalogProduct).ToList();
            return result;
        }

        public ICollection<ExportableCatalog> Catalogs { get; set; }
        public ICollection<ExportableCategory> Categories { get; set; }
        public ICollection<ExportableProperty> Properties { get; set; }
        public ICollection<ExportablePropertyDictionaryItem> PropertyDictionaryItems { get; set; }
        public ICollection<ExportableCatalogProduct> CatalogProducts { get; set; }
    }
}
