using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.ExportModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Data.ExportImport
{
    public class ExportablePropertyDictionaryItem : PropertyDictionaryItem, IExportable
    {
        public virtual ExportablePropertyDictionaryItem FromModel(PropertyDictionaryItem propertyDictionaryItem)
        {
            Id = propertyDictionaryItem.Id;
            PropertyId = propertyDictionaryItem.PropertyId;
            Alias = propertyDictionaryItem.Alias;
            SortOrder = propertyDictionaryItem.SortOrder;
            LocalizedValues = propertyDictionaryItem.LocalizedValues;

            return this;
        }
    }
}
