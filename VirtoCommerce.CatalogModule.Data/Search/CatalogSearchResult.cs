using VirtoCommerce.CatalogModule.Web.Model;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public abstract class CatalogSearchResult<TItem>
    {
        public long TotalCount { get; set; }

        public TItem[] Items { get; set; }

        public Aggregation[] Aggregations { get; set; }
    }
}
