using System.Collections.Generic;
using VirtoCommerce.Domain.Search;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public abstract class CatalogSearchRequestBuilder : ISearchRequestBuilder
    {
        public abstract string DocumentType { get; }

        public virtual SearchRequest BuildRequest(SearchCriteria criteria)
        {
            var request = new SearchRequest
            {
                SearchKeywords = criteria.SearchPhrase,
                SearchFields = new[] { "__content" },
                Filter = GetFilters(criteria).And(),
                Sorting = GetSorting(criteria),
                Skip = criteria.Skip,
                Take = criteria.Take
            };

            return request;
        }


        protected virtual IList<IFilter> GetFilters(SearchCriteria criteria)
        {
            var filters = new List<IFilter>();

            if (criteria.Filters != null)
            {
                filters.AddRange(criteria.Filters);
            }

            return filters;
        }

        protected virtual IList<SortingField> GetSorting(SearchCriteria criteria)
        {
            return criteria.Sorting;
        }
    }
}
