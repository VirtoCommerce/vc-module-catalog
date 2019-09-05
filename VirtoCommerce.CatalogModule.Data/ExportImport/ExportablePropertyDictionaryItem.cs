using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.ExportModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Data.ExportImport
{
    public class ExportablePropertyDictionaryItem : PropertyDictionaryItem, IExportable
    {
        public static ExportablePropertyDictionaryItem FromModel(PropertyDictionaryItem propertyDictionaryItem)
        {
            var result = new ExportablePropertyDictionaryItem
            {
                Id = propertyDictionaryItem.Id,
                PropertyId = propertyDictionaryItem.PropertyId,
                Alias = propertyDictionaryItem.Alias,
                SortOrder = propertyDictionaryItem.SortOrder,
                LocalizedValues = propertyDictionaryItem.LocalizedValues
            };
            return result;
        }
    }
}
