using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class CategorySearch : CatalogSearch
    {
        /// <summary>
        /// CategoryResponseGroup
        /// </summary>
        public string ResponseGroup { get; set; }

        public override T AsCriteria<T>(string catalog)
        {
            var criteria = base.AsCriteria<T>(catalog);

            criteria.Sorting = GetSorting(catalog);

            return criteria;
        }

        protected virtual IList<SortingField> GetSorting(string catalog)
        {
            var result = new List<SortingField>();

            var categoryId = Outline.AsCategoryId();
            var sorts = Sort.AsSortInfoes();
            var priorityFieldName = StringsHelper.JoinNonEmptyStrings("_", "priority", catalog, categoryId).ToLowerInvariant();

            if (!sorts.IsNullOrEmpty())
            {
                foreach (var sortInfo in sorts)
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
            }

            if (!result.Any())
            {
                result.Add(new SortingField(priorityFieldName, true));
                result.Add(new SortingField("priority", true));
                result.Add(CatalogSearchCriteria.DefaultSortOrder);
            }

            return result;
        }
    }
}
