using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class CategorySearchRequestBuilder : ISearchRequestBuilder
    {
        private readonly ISearchPhraseParser _searchPhraseParser;

        public CategorySearchRequestBuilder(ISearchPhraseParser searchPhraseParser)
        {
            _searchPhraseParser = searchPhraseParser;
        }

        public string DocumentType { get; } = KnownDocumentTypes.Category;

        public SearchRequest BuildRequest(SearchCriteriaBase criteria)
        {
            SearchRequest request = null;

            var categorySearchCriteria = criteria as CategorySearchCriteria;
            if (categorySearchCriteria != null)
            {
                // Getting filters modifies search phrase
                var filters = GetFilters(categorySearchCriteria);

                request = new SearchRequest
                {
                    SearchKeywords = categorySearchCriteria.SearchPhrase,
                    SearchFields = new[] { "__content" },
                    Filter = filters.And(),
                    Sorting = GetSorting(categorySearchCriteria),
                    Skip = criteria.Skip,
                    Take = criteria.Take,
                };
            }

            return request;
        }


        protected virtual IList<IFilter> GetFilters(CategorySearchCriteria criteria)
        {
            var result = new List<IFilter>();

            if (!string.IsNullOrEmpty(criteria.SearchPhrase))
            {
                var parseResult = _searchPhraseParser.Parse(criteria.SearchPhrase);
                criteria.SearchPhrase = parseResult.SearchPhrase;
                result.AddRange(parseResult.Filters);
            }

            if (criteria.Ids != null)
            {
                result.Add(new IdsFilter { Values = criteria.Ids });
            }

            if (!string.IsNullOrEmpty(criteria.CatalogId))
            {
                result.Add(FiltersHelper.CreateTermFilter("catalog", criteria.CatalogId.ToLowerInvariant()));
            }

            if (!criteria.Outline.IsNullOrEmpty())
            {
                var outline = string.Join("/", criteria.CatalogId, criteria.Outline).TrimEnd('/', '*').ToLowerInvariant();
                result.Add(FiltersHelper.CreateTermFilter("__outline", outline));
            }

            return result;
        }

        protected virtual IList<SortingField> GetSorting(CategorySearchCriteria criteria)
        {
            var result = new List<SortingField>();

            var categoryId = criteria.Outline.AsCategoryId();
            var priorityFieldName = StringsHelper.JoinNonEmptyStrings("_", "priority", criteria.CatalogId, categoryId).ToLowerInvariant();

            foreach (var sortInfo in criteria.SortInfos)
            {
                var fieldName = sortInfo.SortColumn.ToLowerInvariant();
                var isDescending = sortInfo.SortDirection == SortDirection.Descending;

                switch (fieldName)
                {
                    case "priority":
                        result.Add(new SortingField(priorityFieldName, isDescending));
                        result.Add(new SortingField("priority", isDescending));
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
                result.Add(new SortingField(priorityFieldName, true));
                result.Add(new SortingField("priority", true));
                result.Add(new SortingField("__sort"));
            }

            return result;
        }
    }
}
