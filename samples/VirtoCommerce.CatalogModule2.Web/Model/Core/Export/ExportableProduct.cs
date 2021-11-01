using System.Linq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Export;
using VirtoCommerce.CoreModule.Core.Outlines;
using VirtoCommerce.CoreModule.Core.Seo;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule2.Core.Model.Export
{
    public class ExportableProduct2 : ExportableProduct
    {
        public override ExportableProduct FromModel(CatalogProduct source)
        {
            return base.FromModel(source);
        }

        public override void Move(string catalogId, string categoryId)
        {
            base.Move(catalogId, categoryId);
        }

        public override void ReduceDetails(string responseGroup)
        {
            base.ReduceDetails(responseGroup);
        }

        
    }
}

