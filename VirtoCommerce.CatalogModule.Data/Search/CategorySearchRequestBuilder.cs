using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class CategorySearchRequestBuilder : CatalogSearchRequestBuilder
    {
        public override string DocumentType { get; } = KnownDocumentTypes.Category;

        protected override IList<IFilter> GetFilters(SearchCriteria criteria)
        {
            var result = base.GetFilters(criteria);

            var categorySearchCriteria = criteria as CategorySearchCriteria;
            if (categorySearchCriteria != null)
            {
                if (!string.IsNullOrEmpty(categorySearchCriteria.Catalog))
                {
                    var catalog = categorySearchCriteria.Catalog.ToLowerInvariant();
                    result.Add(CreateTermFilter("catalog", new[] { catalog }));
                }

                if (!categorySearchCriteria.Outlines.IsNullOrEmpty())
                {
                    var outlines = categorySearchCriteria.Outlines.Select(o => o.TrimEnd('/', '*').ToLowerInvariant()).ToList();
                    result.Add(CreateTermFilter("__outline", outlines));
                }
            }

            return result;
        }

        protected IFilter CreateTermFilter(string fieldName, IList<string> values)
        {
            return new TermFilter
            {
                FieldName = fieldName,
                Values = values,
            };
        }
    }
}
