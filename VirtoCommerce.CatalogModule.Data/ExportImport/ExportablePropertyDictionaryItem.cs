using System;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.ExportModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Data.ExportImport
{
    public class ExportablePropertyDictionaryItem : PropertyDictionaryItem, IExportable
    {
        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
