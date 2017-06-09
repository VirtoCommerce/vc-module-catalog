using VirtoCommerce.CatalogModule.Web.Model;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class ProductSearchResult
    {
        public long TotalCount { get; set; }

        public Product[] Products { get; set; }

        public Aggregation[] Aggregations { get; set; }
    }
}
