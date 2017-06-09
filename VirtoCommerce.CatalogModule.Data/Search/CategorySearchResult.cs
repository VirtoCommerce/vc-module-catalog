using VirtoCommerce.CatalogModule.Web.Model;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class CategorySearchResult
    {
        public long TotalCount { get; set; }

        public Category[] Categories { get; set; }
    }
}
