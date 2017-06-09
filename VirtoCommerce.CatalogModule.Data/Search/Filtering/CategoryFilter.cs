using System.Collections.Generic;

namespace VirtoCommerce.CatalogModule.Data.Search.Filtering
{
    public class CategoryFilter : ISearchFilter
    {
        public string Key { get; set; }

        public IList<CategoryFilterValue> Values { get; set; }
    }
}
