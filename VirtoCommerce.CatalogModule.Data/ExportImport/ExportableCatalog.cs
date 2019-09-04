using VirtoCommerce.CatalogModule.Web.Model;
using VirtoCommerce.ExportModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Data.ExportImport
{
    public class ExportableCatalog : Catalog, IExportable
    {
        public object Clone()
        {
            throw new System.NotImplementedException();
        }
    }
}
