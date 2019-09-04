using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Web.Model;
using VirtoCommerce.ExportModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Data.ExportImport
{
    public class ExportableProperty : Property, IExportable
    {
        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
