using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class ProductSearchCriteriaPreprocessor : ISearchCriteriaPreprocessor
    {
        public virtual void Process(SearchCriteria criteria)
        {
            var productSearchCriteria = criteria as ProductSearchCriteria;
            if (productSearchCriteria != null)
            {
                AddProductFilters(productSearchCriteria);
            }
        }


        protected virtual void AddProductFilters(ProductSearchCriteria criteria)
        {
            if (criteria != null)
            {
                criteria.Filters.Add(FiltersHelper.CreateDateRangeFilter("startdate", criteria.StartDateFrom, criteria.StartDate, false, true));

                if (criteria.EndDate != null)
                {
                    criteria.Filters.Add(FiltersHelper.CreateDateRangeFilter("enddate", criteria.EndDate, null, false, false));
                }

                if (!criteria.ClassTypes.IsNullOrEmpty())
                {
                    criteria.Filters.Add(FiltersHelper.CreateTermFilter("__type", criteria.ClassTypes));
                }

                if (!criteria.WithHidden)
                {
                    criteria.Filters.Add(FiltersHelper.CreateTermFilter("status", "visible"));
                }

                if (criteria.PriceRange != null)
                {
                    var range = criteria.PriceRange;
                    criteria.Filters.Add(FiltersHelper.CreatePriceRangeFilter(criteria.Currency, null, range.Lower, range.Upper, range.IncludeLower, range.IncludeUpper));
                }
            }
        }
    }
}
