using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public abstract class CatalogSearchRequestBuilder : ISearchRequestBuilder
    {
        public abstract string DocumentType { get; }

        public virtual void BuildRequest(SearchRequest request, SearchCriteria criteria)
        {
            if (criteria?.DocumentType.EqualsInvariant(DocumentType) == true)
            {
                request.Filter = GetFilters(criteria).And();
                request.Sorting = GetSorting(criteria);
                request.Skip = criteria.Skip;
                request.Take = criteria.Take;
            }
        }


        protected virtual IList<IFilter> GetFilters(SearchCriteria criteria)
        {
            var filters = new List<IFilter>();

            if (criteria.Ids?.Any() == true)
            {
                filters.Add(new IdsFilter { Values = criteria.Ids });
            }

            return filters;
        }

        protected virtual IList<SortingField> GetSorting(SearchCriteria criteria)
        {
            return criteria.Sorting;
        }
    }
}
