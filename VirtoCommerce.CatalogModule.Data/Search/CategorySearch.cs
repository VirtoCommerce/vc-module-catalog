using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class CategorySearch
    {
        /// <summary>
        /// CategoryResponseGroup
        /// </summary>
        public string ResponseGroup { get; set; }
        /// <summary>
        /// CategoryId/CategoryId
        /// </summary>
        public string Outline { get; set; }

        public string[] Sort { get; set; }

        public int Skip { get; set; }

        public int Take { get; set; }

        public virtual T AsCriteria<T>(string catalog)
            where T : CategorySearchCriteria, new()
        {
            var criteria = AbstractTypeFactory<T>.TryCreateInstance();

            criteria.Skip = Skip;
            criteria.Take = Take;

            // Add outline
            var outline = string.IsNullOrEmpty(Outline) ? $"{catalog}" : $"{catalog}{Outline}";
            criteria.Outlines.Add(outline);

            #region Sorting

            var categoryId = Outline.AsCategoryId();
            var sorts = Sort.AsSortInfoes();
            var sortFields = new List<SortingField>();
            var priorityFieldName = string.Format(CultureInfo.InvariantCulture, "priority_{0}_{1}", catalog, categoryId).ToLower();

            if (!sorts.IsNullOrEmpty())
            {
                foreach (var sortInfo in sorts)
                {
                    var fieldName = sortInfo.SortColumn.ToLowerInvariant();
                    var isDescending = sortInfo.SortDirection == SortDirection.Descending;

                    switch (fieldName)
                    {
                        case "priority":
                            sortFields.Add(new SortingField(priorityFieldName, isDescending));
                            sortFields.Add(new SortingField("priority", isDescending));
                            break;
                        case "name":
                        case "title":
                            sortFields.Add(new SortingField("name", isDescending));
                            break;
                        default:
                            sortFields.Add(new SortingField(fieldName, isDescending));
                            break;
                    }
                }
            }

            if (!sortFields.Any())
            {
                sortFields.Add(new SortingField(priorityFieldName, true));
                sortFields.Add(new SortingField("priority", true));
                sortFields.Add(CategorySearchCriteria.DefaultSortOrder);
            }

            criteria.Sorting = sortFields;

            #endregion

            return criteria;
        }
    }
}
