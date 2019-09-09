using VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogModule.Data.ExportImport
{
    public class ProductExportSearchCriteria : CatalogFullExportSearchCriteria
    {
        public bool SearchInVariations { get; set; }
        public string[] CategoryIds { get; set; }
        public override SearchCriteria ToCatalogSearchCriteria()
        {
            var result = base.ToCatalogSearchCriteria();

            result.SearchInVariations = SearchInVariations;
            result.CategoryIds = CategoryIds;
            result.SearchInChildren = true;
            result.ResponseGroup = SearchResponseGroup.WithProducts;

            return result;
        }
    }
}
