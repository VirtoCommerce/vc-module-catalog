using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.ExportImport
{
    public class CatalogFullExportSearchCriteria : SearchCriteriaBase
    {
        public string[] CatalogIds { get; set; }

        public virtual SearchCriteria ToCatalogSearchCriteria()
        {
            var result = AbstractTypeFactory<SearchCriteria>.TryCreateInstance();

            result.Keyword = SearchPhrase;
            result.Sort = Sort;
            result.Skip = Skip;
            result.Take = Take;
            result.WithHidden = true;

            result.CatalogIds = CatalogIds;

            return result;
        }
    }
}
