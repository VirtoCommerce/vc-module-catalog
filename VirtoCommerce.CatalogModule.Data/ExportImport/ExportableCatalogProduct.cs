using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.ExportModule.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtoCommerce.CatalogModule.Data.ExportImport
{
    public class ExportableCatalogProduct: CatalogProduct, IExportable
    {
    }
}
