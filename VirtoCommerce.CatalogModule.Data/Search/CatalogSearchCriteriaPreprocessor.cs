using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class CatalogSearchCriteriaPreprocessor : ISearchCriteriaPreprocessor
    {
        public virtual void Process(SearchCriteria criteria)
        {
            var catalogSearchCriteria = criteria as CatalogSearchCriteria;
            if (catalogSearchCriteria != null)
            {
                AddCatalogFilters(catalogSearchCriteria);
            }
        }


        protected virtual void AddCatalogFilters(CatalogSearchCriteria criteria)
        {
            if (criteria != null)
            {
                if (!string.IsNullOrEmpty(criteria.Catalog))
                {
                    criteria.Filters.Add(FiltersHelper.CreateTermFilter("catalog", criteria.Catalog.ToLowerInvariant()));
                }

                if (!criteria.Outline.IsNullOrEmpty())
                {
                    criteria.Filters.Add(FiltersHelper.CreateTermFilter("__outline", criteria.Outline));
                }
            }
        }
    }
}
