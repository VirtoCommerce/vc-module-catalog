using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Extensions;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;
using static VirtoCommerce.SearchModule.Core.Extensions.IndexDocumentExtensions;

namespace VirtoCommerce.CatalogModule.Data.Search.Indexing
{
    public class CategorySearchRequestBuilder : ISearchRequestBuilder
    {
        private readonly ISearchPhraseParser _searchPhraseParser;

        public CategorySearchRequestBuilder(ISearchPhraseParser searchPhraseParser)
        {
            _searchPhraseParser = searchPhraseParser;
        }

        public virtual string DocumentType { get; } = KnownDocumentTypes.Category;

        public virtual Task<SearchRequest> BuildRequestAsync(SearchCriteriaBase criteria)
        {
            SearchRequest request = null;

            if (criteria is CategoryIndexedSearchCriteria categorySearchCriteria)
            {
                // GetFilters() modifies Keyword
                categorySearchCriteria = categorySearchCriteria.CloneTyped();
                var filters = GetFilters(categorySearchCriteria);

                request = new SearchRequest
                {
                    SearchKeywords = categorySearchCriteria.Keyword,
                    SearchFields = [ContentFieldName],
                    Filter = filters.And(),
                    Sorting = GetSorting(categorySearchCriteria),
                    Skip = criteria.Skip,
                    Take = criteria.Take,
                    IsFuzzySearch = categorySearchCriteria.IsFuzzySearch,
                };
            }

            return Task.FromResult(request);
        }


        protected virtual IList<IFilter> GetFilters(CategoryIndexedSearchCriteria criteria)
        {
            var result = new List<IFilter>();

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                var parseResult = _searchPhraseParser.Parse(criteria.Keyword);
                criteria.Keyword = parseResult.Keyword;
                result.AddRange(parseResult.Filters);
            }

            if (criteria.ObjectIds != null)
            {
                result.Add(new IdsFilter { Values = criteria.ObjectIds });
            }

            result.AddCatalogFilters(criteria);

            result.Add(FiltersHelper.CreateOutlineFilter(criteria));

            var terms = criteria.GetTerms();
            result.AddRange(terms.Select(term => FiltersHelper.CreateTermFilter(term.Key, term.Values)));

            return result;
        }

        protected virtual IList<SortingField> GetSorting(CategoryIndexedSearchCriteria criteria)
        {
            var result = new List<SortingField>();
            //For sorting by relevance have to keep sortInfo clear
            var needClearSortInfo = false;

            var priorityFields = criteria.GetPriorityFields();

            foreach (var sortInfo in criteria.SortInfos)
            {
                var fieldName = sortInfo.SortColumn.ToLowerInvariant();
                var isDescending = sortInfo.SortDirection == SortDirection.Descending;

                switch (fieldName)
                {
                    case "relevance":
                        needClearSortInfo = true;
                        break;
                    case "priority":
                        result.AddRange(priorityFields.Select(priorityField => new SortingField(priorityField, isDescending)));
                        break;
                    case "name":
                    case "title":
                        result.Add(new SortingField("name", isDescending));
                        break;
                    default:
                        result.Add(new SortingField(fieldName, isDescending));
                        break;
                }
            }

            if (!result.Any())
            {
                result.AddRange(priorityFields.Select(priorityField => new SortingField(priorityField, true)));
                result.Add(new SortingField("__sort"));
            }

            if (needClearSortInfo)
            {
                result.Clear();
            }
            return result;
        }
    }
}
