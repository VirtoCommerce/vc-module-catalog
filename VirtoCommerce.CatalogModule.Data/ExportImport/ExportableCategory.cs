using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.ExportModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Data.ExportImport
{
    public class ExportableCategory : Category, IExportable
    {
        public object Clone()
        {
            throw new System.NotImplementedException();
        }
    }
}
