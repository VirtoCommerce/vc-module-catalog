using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Export;

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

